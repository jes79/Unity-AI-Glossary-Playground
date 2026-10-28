#!/usr/bin/env perl
# 사무실 플러그인(OMP Studio)의 용어집 자료 glossary.json 을 저장소에서 다시 만든다.
# 사용법과 입력 목록은 README.md 참고.
#
#   perl build-glossary.pl [--out <glossary.json>] [--no-notion]
#
# 하나라도 비어 있거나 182개가 아니면 아무것도 쓰지 않고 실패(종료 코드 1)한다.
use strict;
use warnings;
use utf8;
use Encode qw(decode encode);
use File::Spec;
use File::Basename qw(dirname);
use JSON::PP;

binmode(STDOUT, ":encoding(UTF-8)");
binmode(STDERR, ":encoding(UTF-8)");

my $EXPECT = 182;
my $HERE   = dirname(File::Spec->rel2abs($0));
my $ROOT   = File::Spec->catdir($HERE, '..', '..');

my ($out, $no_notion) = ('', 0);
while (@ARGV) {
  my $a = shift @ARGV;
  if    ($a eq '--out')       { $out = shift @ARGV // die "--out 뒤에 경로가 필요해요\n" }
  elsif ($a eq '--no-notion') { $no_notion = 1 }
  else { die "알 수 없는 인자: $a\n" }
}
if (!$out) {
  die "--out 을 주거나 APPDATA 환경 변수가 있어야 해요\n" unless $ENV{APPDATA};
  $out = File::Spec->catfile($ENV{APPDATA}, 'omp-studio', 'plugins', 'sidebar-reorder', 'glossary.json');
}

my @problems;
sub bad { push @problems, join('', @_) }

# ---------- 파일 읽기 / C# 문자열 ----------
sub slurp {
  my ($path) = @_;
  open(my $fh, '<:raw', $path) or die "읽을 수 없어요: $path ($!)\n";
  local $/ = undef;
  my $b = <$fh>;
  close $fh;
  $b =~ s/^\xEF\xBB\xBF//;
  $b =~ s/\r\n/\n/g;
  return decode('UTF-8', $b, Encode::FB_CROAK);
}

my $LIT = qr/"(?:[^"\\]|\\.)*"/s;
sub unescape {
  my ($s) = @_;
  $s =~ s/\\(u[0-9a-fA-F]{4}|.)/
    my $c = $1;
    $c =~ m{^u} ? chr(hex(substr($c, 1))) : $c eq 'n' ? "\n" : $c eq 't' ? "\t" : $c eq 'r' ? "\r" : $c
  /gse;
  return $s;
}
# "a" + "b" … 이어 붙인 C# 문자열 → 한 문자열
sub lits {
  my ($raw) = @_;
  my $s = '';
  while ($raw =~ /"((?:[^"\\]|\\.)*)"/gs) { $s .= unescape($1) }
  return $s;
}

# ---------- 프로젝트 위치 ----------
my ($PROJECT) = grep { -f File::Spec->catfile($_, 'Assets', '01_Scripts', 'Common', 'GlossaryCatalog.Data.cs') }
                map  { File::Spec->catdir($ROOT, $_) }
                do { opendir(my $d, $ROOT) or die "저장소 폴더를 열 수 없어요: $ROOT\n"; sort grep { !/^\./ } readdir($d) };
die "Unity 프로젝트 폴더를 찾지 못했어요 ($ROOT 아래 Assets/01_Scripts/Common/GlossaryCatalog.Data.cs)\n" unless $PROJECT;
my $PROJECT_DIR = (File::Spec->splitdir($PROJECT))[-1];
my $COMMON = File::Spec->catdir($PROJECT, 'Assets', '01_Scripts', 'Common');

# ---------- GitHub 주소 ----------
my $remote = `git -C "$ROOT" remote get-url origin 2>&1`;
chomp $remote;
$remote =~ m{github\.com[:/]([^/]+/[^/]+?)(?:\.git)?$} or die "origin 이 GitHub 주소가 아니에요: $remote\n";
my $REPO   = $remote =~ s{^.*github\.com[:/]}{}r =~ s{\.git$}{}r;
my $BRANCH = `git -C "$ROOT" rev-parse --abbrev-ref HEAD 2>&1`;
chomp $BRANCH;
$BRANCH = 'master' if !$BRANCH || $BRANCH eq 'HEAD' || $BRANCH =~ /\s/;
my $GH = "https://github.com/$REPO";
sub pct {
  my ($s) = @_;
  my $b = encode('UTF-8', $s);
  $b =~ s/([^A-Za-z0-9\-._~\/%])/sprintf('%%%02X', ord($1))/ge;
  return $b;
}

# ---------- GlossaryCatalog.Data.cs : 분야 · 용어 목록 ----------
my $data = slurp(File::Spec->catfile($COMMON, 'GlossaryCatalog.Data.cs'));
my ($cat_block)    = $data =~ /Categories\s*=\s*\{(.*?)\};/s or die "Categories 를 찾지 못했어요\n";
my ($folder_block) = $data =~ /CategoryFolders\s*=\s*\{(.*?)\};/s or die "CategoryFolders 를 찾지 못했어요\n";
my @cat_names = map { unescape($_) } $cat_block   =~ /"((?:[^"\\]|\\.)*)"/g;
my @cat_dirs  = map { unescape($_) } $folder_block =~ /"((?:[^"\\]|\\.)*)"/g;
die "분야가 11개가 아니에요 (" . scalar(@cat_names) . ")\n" unless @cat_names == 11 && @cat_dirs == 11;

# 이 스크립트가 읽지 못하는 C# 문자열 문법(@"…" 축자 문자열, $"…" 보간 문자열)이 데이터 자리에 보이면 조용히 틀린 글자를 만들지 않고 실패한다
sub no_exotic {
  my ($label, $text) = @_;
  bad("$label 에 지원하지 않는 C# 문자열 문법(\@\"…\" · \$\"…\")이 있어요") if $text =~ /[\@\$]"/;
}
no_exotic('GlossaryCatalog.Data.cs', $data);
my $entry_calls = () = $data =~ /new GlossaryEntry\(/g;
my @entries;
my %by_id;
for my $line (split /\n/, $data) {
  next unless $line =~ /new GlossaryEntry\(/;
  unless ($line =~ /^\s*new GlossaryEntry\((.*)\)\s*,?\s*$/) { bad("용어 줄 모양이 달라요: $line"); next }
  my $args = $1;
  my @v;
  pos($args) = 0;
  while ($args =~ /\G\s*(?:"((?:[^"\\]|\\.)*)"|(\d+))\s*(?:,|$)/gc) { push @v, defined $1 ? unescape($1) : $2 + 0 }
  # 인자를 끝까지(닫는 괄호 앞까지) 다 읽었는지
  unless ((pos($args) // 0) == length($args) && @v == 7 && $v[0] =~ /^\d{4}$/) { bad("용어 줄을 끝까지 읽지 못했어요: $line"); next }
  my ($id, $cat, $sub, $term, $easy, $prompt, $scene) = @v;
  # 요청 예문은 바깥 큰따옴표를 뗀다 (Unity 화면에서는 따옴표째 보여 주지만, 복사·보드에는 문장만)
  $prompt =~ s/^"(.*)"$/$1/s;
  die "중복 id: $id\n" if $by_id{$id};
  my $e = { id => $id, cat => $cat, sub => $sub, term => $term, easy => $easy, prompt => $prompt, scene => $scene };
  push @entries, $e;
  $by_id{$id} = $e;
}
bad("GlossaryEntry 호출 $entry_calls 개 중 " . scalar(@entries) . "개만 읽었어요") unless $entry_calls == @entries;

# ---------- GlossaryCatalog.Guide.cs : 개념 · 확인 포인트 ----------
my $guide = slurp(File::Spec->catfile($COMMON, 'GlossaryCatalog.Guide.cs'));
my $CAT = qr/$LIT(?:\s*\+\s*$LIT)*/s;
my ($guide_block) = $guide =~ /\bGuide\s*=\s*new\(\)\s*\{(.*?)\n\s*\};/s or die "Guide 를 찾지 못했어요\n";
no_exotic('GlossaryCatalog.Guide.cs', $guide_block);
my $guide_n = () = $guide_block =~ /\["\d{4}"\]\s*=/g;
my $guide_got = 0;
while ($guide_block =~ /\["(\d{4})"\]\s*=\s*\(\s*($CAT)\s*,\s*($CAT)\s*\)\s*,/g) {
  my ($id, $c, $k) = ($1, $2, $3);
  $guide_got++;
  my $e = $by_id{$id} or do { bad("Guide 에만 있는 id: $id"); next };
  $e->{concept} = lits($c);
  $e->{check}   = lits($k);
}
bad("Guide 항목 $guide_n 개 중 $guide_got 개만 읽었어요") unless $guide_n == $guide_got;

# ---------- LectureRequestExamples.cs : 막연한 요청 · 분야별 채울 거리 ----------
my $rex = slurp(File::Spec->catfile($COMMON, 'LectureRequestExamples.cs'));
my ($vague_block) = $rex =~ /VagueById\s*=\s*new\(\)\s*\{(.*?)\n\s*\};/s or die "VagueById 를 찾지 못했어요\n";
no_exotic('LectureRequestExamples.cs(VagueById)', $vague_block);
my $vague_n = () = $vague_block =~ /\["\d{4}"\]\s*=/g;
my $vague_got = 0;
while ($vague_block =~ /\["(\d{4})"\]\s*=\s*($CAT)\s*,/g) {
  $vague_got++;
  my $e = $by_id{$1} or do { bad("VagueById 에만 있는 id: $1"); next };
  $e->{vague} = lits($2);
}
bad("VagueById 항목 $vague_n 개 중 $vague_got 개만 읽었어요") unless $vague_n == $vague_got;
my ($tgt_block) = $rex =~ /TargetExamples\s*=\s*\{(.*?)\};/s or die "TargetExamples 를 찾지 못했어요\n";
my ($num_block) = $rex =~ /NumberExamples\s*=\s*\{(.*?)\};/s or die "NumberExamples 를 찾지 못했어요\n";
no_exotic('LectureRequestExamples.cs(채울 거리)', $tgt_block . $num_block);
my @tgt = map { unescape($_) } $tgt_block =~ /"((?:[^"\\]|\\.)*)"/g;
my @num = map { unescape($_) } $num_block =~ /"((?:[^"\\]|\\.)*)"/g;
die "분야별 채울 거리가 11개씩이 아니에요\n" unless @tgt == 11 && @num == 11;

# ---------- LecturePresenters.cs : 분야 → 담당자 · 분야 소개 ----------
my $pres = slurp(File::Spec->catfile($COMMON, 'LecturePresenters.cs'));
my %presenter;
my ($pres_data) = $pres =~ /^(.*?)public static LecturePresenter For\b/s or die "LecturePresenters.For 를 찾지 못했어요\n";
no_exotic('LecturePresenters.cs', $pres_data);
my $pres_calls = () = $pres_data =~ /new LecturePresenter\(/g;
while ($pres_data =~ /public static readonly LecturePresenter (\w+)\s*=\s*new LecturePresenter\(\s*"(\w+)"\s*,\s*"([^"]*)"\s*,\s*($CAT)\s*,\s*($CAT)\s*,\s*new Color32\((\d+),\s*(\d+),\s*(\d+),\s*\d+\)\)\s*;/g) {
  $presenter{$1} = { key => $2, role => $3, nextLine => lits($4), demoLine => lits($5), accent => sprintf('#%02x%02x%02x', $6, $7, $8) };
}
bad("담당자 정의 $pres_calls 개 중 " . scalar(keys %presenter) . "개만 읽었어요") unless $pres_calls == keys %presenter;
my ($by_cat_block) = $pres =~ /ByCategory\s*=\s*\{(.*?)\};/s or die "ByCategory 를 찾지 못했어요\n";
my @by_cat = $by_cat_block =~ /^\s*(\w+)\s*,/mg;
my ($job_block) = $pres =~ /CategoryJobs\s*=\s*\{(.*?)\n\s*\};/s or die "CategoryJobs 를 찾지 못했어요\n";
my @jobs = map { lits($_) } $job_block =~ /($CAT)\s*,/g;
die "ByCategory 가 11개가 아니에요 (" . scalar(@by_cat) . ")\n" unless @by_cat == 11;
die "CategoryJobs 가 11개가 아니에요 (" . scalar(@jobs) . ")\n" unless @jobs == 11;
for my $name (@by_cat) { die "담당자 $name 을 찾지 못했어요\n" unless $presenter{$name} }
my %pres_by_key = map { $_->{key} => $_ } values %presenter;

# ---------- 씬 빌더 : 조작법 (+ 2D/3D) ----------
my %controls;
my %dim;
{
  my $dir = File::Spec->catdir($PROJECT, 'Assets', '01_Scripts', 'Editor', 'Scenes');
  my @files;
  my @stack = ($dir);
  while (my $d = shift @stack) {
    opendir(my $dh, $d) or next;
    for my $f (sort readdir $dh) {
      next if $f =~ /^\./;
      my $p = File::Spec->catfile($d, $f);
      if (-d $p) { push @stack, $p } elsif ($f =~ /^Build(\d{4})_.*\.cs$/) { push @files, [$1, $p] }
    }
    closedir $dh;
  }
  for my $f (@files) {
    my ($id, $path) = @$f;
    my $txt = slurp($path);
    unless ($txt =~ /\b(Stage|NewScene3D|NewScene2D|NewSceneUI)\(\s*(?:"(\d{4})"|([A-Za-z_][A-Za-z0-9_.]*))\s*,\s*(.*)/s) { bad("$id 씬 빌더에서 Stage/NewScene* 호출을 찾지 못했어요 ($path)"); next }
    my ($helper, $lit_id, $id_name, $rest) = ($1, $2, $3, $4);
    # 첫 인자(씬 id)가 파일 이름의 id와 같은지. 상수면 const string 으로 풀어서 본다
    my $call_id = $lit_id;
    if (!defined $call_id) {
      if ($txt =~ /\bconst\s+string\s+\Q$id_name\E\s*=\s*"(\d{4})"\s*;/s) { $call_id = $1 } else { bad("$id 씬 빌더의 첫 인자 $id_name 가 어떤 id 인지 읽지 못했어요"); next }
    }
    bad("$id 씬 빌더 호출의 id($call_id)가 파일 이름의 id와 달라요") if $call_id ne $id;
    # 조작법 인자: 문자열 이어 붙임(" … " + " … ") 하나, 또는 const string 상수 하나. 그 밖의 식이 이어지면 읽을 수 없다
    if ($rest =~ /^[\@\$]"/) { bad("$id 씬 빌더의 조작법에 지원하지 않는 C# 문자열 문법(\@\"…\" · \$\"…\")이 있어요"); next }
    my $literal;
    if ($rest =~ /^($CAT)\s*[,)]/s) {
      $literal = $1;
    } elsif ($rest =~ /^([A-Za-z_]\w*)\s*[,)]/s) {
      my $const = $1;
      unless ($txt =~ /\bconst\s+string\s+\Q$const\E\s*=\s*($CAT)\s*;/s) { bad("$id 조작법 상수 $const 를 찾지 못했어요"); next }
      $literal = $1;
    } else {
      bad("$id 씬 빌더의 조작법 인자를 읽지 못했어요 (문자열 또는 const string 만 지원)");
      next;
    }
    $controls{$id} = lits($literal);
    $dim{$id} = $helper =~ /2D|UI/ ? '2D' : '3D';
  }
}
sub strip_rich {
  my ($s) = @_;
  # TMP 서식 태그만 지운다. ObjectPool<T> 같은 제네릭은 남긴다
  $s =~ s{</?(?:b|i|u|s|sub|sup|mark|nobr|color|size|font|material|align|alpha|cspace|indent|line-height|lowercase|uppercase|smallcaps|margin|mspace|pos|rotate|space|sprite|style|voffset|width|link|gradient|noparse|page)\b[^>]*>}{}gi;
  $s =~ s/\t/ /g;
  $s =~ s/\s+$//;
  return $s;
}

# ---------- 위키 : 씬에서 보여 주는 것 · 핵심 스크립트 · 구현 방식 · 씬 경로 ----------
my %wiki_page;
my %wiki;
{
  my $dir = File::Spec->catdir($ROOT, 'docs', 'wiki');
  opendir(my $dh, $dir) or die "위키 폴더를 열 수 없어요: $dir\n";
  for my $f (sort readdir $dh) {
    next unless $f =~ /^(\d\d)-.*\.md$/;
    my $n = $1 + 0;
    (my $page = decode('UTF-8', $f)) =~ s/\.md$//;
    $wiki_page{$n} = $page;
    my $txt = slurp(File::Spec->catfile($dir, $f));
    while ($txt =~ /^### (\d{4})\s*\n(.*?)(?=^### |\z)/gms) {
      my ($id, $b) = ($1, $2);
      my %w = (page => $n);
      ($w{scenePath}) = $b =~ /^- 씬: `(.+?)`/m;
      ($w{script})    = $b =~ /^- 핵심 스크립트: `(.+?)`/m;
      ($w{mode})      = $b =~ /^- 구현 방식: (.+?)\s*$/m;
      ($w{shows})     = $b =~ /^- 씬에서 보여 주는 것: (.+?)\s*$/m;
      $wiki{$id} = \%w;
    }
  }
}

# ---------- Notion 행 주소 ----------
my %notion;
my $notion_file = File::Spec->catfile($HERE, 'notion-urls.tsv');
if (-f $notion_file) {
  for my $line (split /\r?\n/, slurp($notion_file)) {
    next if $line =~ /^\s*(#|$)/;
    my ($id, $url) = split /\t/, $line;
    unless (defined $url && $id =~ /^\d{4}$/ && $url =~ m{^https://(?:app\.)?notion\.(?:com|so)/\S+$}) { bad("notion-urls.tsv 줄이 이상해요: $line"); next }
    bad("notion-urls.tsv 에 같은 id 가 두 번: $id") if exists $notion{$id};
    $notion{$id} = $url;
  }
} elsif (!$no_notion) {
  bad("notion-urls.tsv 가 없어요 (Notion 주소 없이 만들려면 --no-notion)");
}

# ---------- 합치기 · 검사 ----------
for my $e (@entries) {
  my $id  = $e->{id};
  my $cat = $e->{cat};
  if ($cat < 1 || $cat > 11) { bad("$id 분야 번호가 이상해요: $cat"); next }
  bad("$id 분야 번호와 id 앞 두 자리가 달라요") unless sprintf('%02d', $cat) eq substr($id, 0, 2);
  $e->{controls} = strip_rich($controls{$id} // '');
  $e->{dim}      = $dim{$id} // '';
  my $w = $wiki{$id};
  if ($w) {
    $e->{shows}  = $w->{shows}  // '';
    $e->{script} = $w->{script} // '';
    $e->{mode}   = $w->{mode}   // '';
  } else {
    bad("$id 위키 항목이 없어요");
  }
  my $dir = $cat_dirs[$cat - 1];
  my $scene_path = "Assets/00_Scenes/$dir/$e->{scene}.unity";
  bad("$id 위키의 씬 경로가 달라요: " . ($w->{scenePath} // '(없음)') . " ≠ $scene_path") if $w && ($w->{scenePath} // '') ne $scene_path;
  bad("$id 씬 파일이 없어요: $scene_path") unless -f File::Spec->catfile($PROJECT, split('/', $scene_path));
  $e->{scenePath} = $scene_path;
  $e->{github}    = "$GH/blob/$BRANCH/" . pct("$PROJECT_DIR/$scene_path");
  my $page = $wiki_page{$cat};
  if ($page) { $e->{wiki} = "$GH/wiki/" . pct($page) . "#$id" } else { bad("$id 분야 $cat 의 위키 파일이 없어요") }
  $e->{notion} = $notion{$id} // '';

  # 빈 칸 검사 (소분류는 없는 용어가 많아서 비어 있어도 된다)
  for my $k (qw(id term easy concept check controls prompt vague scene scenePath dim shows script mode wiki github)) {
    bad("$id 의 '$k' 칸이 비었어요") unless defined $e->{$k} && $e->{$k} =~ /\S/;
  }
  bad("$id 의 notion 칸이 비었어요") if !$no_notion && $e->{notion} eq '';
}
bad("용어가 " . scalar(@entries) . "개예요 (${EXPECT}개여야 해요)") unless @entries == $EXPECT;
for my $id (keys %notion)    { bad("notion-urls.tsv 에만 있는 id: $id") unless $by_id{$id} }
for my $id (keys %controls)  { bad("씬 빌더에만 있는 조작법 id: $id") unless $by_id{$id} }
for my $id (keys %wiki)      { bad("위키에만 있는 id: $id")  unless $by_id{$id} }

if (@problems) {
  print STDERR "glossary.json 을 만들지 못했어요 (" . scalar(@problems) . "건):\n";
  print STDERR "  - $_\n" for @problems[0 .. ($#problems > 39 ? 39 : $#problems)];
  print STDERR "  … 외 " . (@problems - 40) . "건\n" if @problems > 40;
  exit 1;
}

# ---------- 분야 · 담당자 ----------
my @categories;
for my $n (1 .. 11) {
  my $p = $presenter{ $by_cat[$n - 1] };
  my @in = grep { $_->{cat} == $n } @entries;
  push @categories, {
    n        => $n,
    name     => $cat_names[$n - 1],
    folder   => $cat_dirs[$n - 1],
    count    => scalar(@in),
    presenter => $p->{key},
    intro    => $jobs[$n - 1],
    targetExample => $tgt[$n - 1],
    numberExample => $num[$n - 1],
    wiki     => "$GH/wiki/" . pct($wiki_page{$n}),
  };
}
my %presenters = map { $_->{key} => { role => $_->{role}, nextLine => $_->{nextLine}, demoLine => $_->{demoLine}, accent => $_->{accent} } } values %pres_by_key;

# ---------- 쓰기 (용어 하나가 한 줄이라 바뀐 곳이 diff 로 보인다) ----------
my $js = JSON::PP->new->canonical(1);   # 문자열을 그대로(문자 → UTF-8 은 아래에서 한 번에)
my @rows = map { $js->encode($_) } @entries;
my $body =
  "{\n" .
  '"version":1,' . "\n" .
  '"repo":' . $js->encode({ url => $GH, branch => $BRANCH, project => $PROJECT_DIR, wiki => "$GH/wiki" }) . ",\n" .
  '"presenters":' . $js->encode(\%presenters) . ",\n" .
  '"categories":' . $js->encode(\@categories) . ",\n" .
  '"entries":[' . "\n" . join(",\n", @rows) . "\n]\n}\n";
my $dir = dirname($out);
die "출력 폴더가 없어요: $dir\n" unless -d $dir;
open(my $fh, '>:raw', "$out.tmp") or die "쓸 수 없어요: $out.tmp ($!)\n";
print $fh encode('UTF-8', $body);
close $fh;
rename("$out.tmp", $out) or die "이름을 바꿀 수 없어요: $out ($!)\n";
printf "glossary.json: 용어 %d개 · 분야 %d개 · Notion 주소 %d개 → %s (%d바이트)\n", scalar(@entries), scalar(@categories), scalar(grep { $_->{notion} } @entries), $out, length(encode('UTF-8', $body));
