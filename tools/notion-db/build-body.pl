use strict; use warnings;
# DB 각 페이지의 본문 마크다운을 만든다.
#   입력: guide.tsv(개념/확인 포인트), controls.tsv(조작법), shows.tsv(씬에서 보여 주는 것), meta.tsv(용어/경로/스크립트 등)
#   출력: bodies/<id>.md


sub load {
  my ($path) = @_;
  my %h;
  open(my $fh, "<", $path) or die "$path: $!";
  while (my $l = <$fh>) { chomp $l; my @c = split(/\t/, $l, -1); next unless @c >= 2; $h{$c[0]} = [@c[1..$#c]]; }
  close $fh;
  return \%h;
}

my $dir    = $ARGV[0];
my $guide  = load("$dir/guide.tsv");
my $ctl    = load("$dir/controls.tsv");
my $shows  = load("$dir/shows.tsv");
my $meta   = load("$dir/meta.tsv");

mkdir "$dir/bodies";
for my $id (sort keys %$meta) {
  my ($term, $summary, $prompt, $scene, $scripts, $builder, $mode) = @{$meta->{$id}};
  my $concept = $guide->{$id}[0] // "";
  my $check   = $guide->{$id}[1] // "";
  my $show    = $shows->{$id}[0] // "";
  my $raw     = $ctl->{$id}[0] // "";

  # controls 를 줄 단위로 나눈 뒤, "키 : 동작" 조각은 표로, 나머지는 설명 문장으로 분류
  # TMP 리치 텍스트 태그(<b> <color=..> 등)는 Notion 에서 의미가 없으니 제거.
  # ObjectPool<T> 같은 제네릭은 남겨야 하므로 알려진 태그 이름만 지운다.
  $raw =~ s{</?(?:b|i|u|s|color|size|align|sprite|font)(?:=[^>]*)?>}{}gi;

  my @rows; my @notes;
  for my $line (split(/\\n/, $raw)) {
    $line =~ s/^\s+|\s+$//g;
    next unless length $line;
    if ($line =~ / : /) {
      for my $chunk (split(/\s{2,}/, $line)) {
        $chunk =~ s/^\s+|\s+$//g;
        next unless length $chunk;
        if ($chunk =~ /^(.{1,80}?) : (.+)$/) { push @rows, [$1, $2]; }
        else { push @notes, $chunk; }
      }
    } else { push @notes, $line; }
  }

  my @out;
  push @out, "## 개념", "", $concept, "";
  push @out, "## 테스트 방법", "";
  push @out, "허브 씬에서 **$id $term** 을 고르거나 `$scene` 을 열고 Play 하세요.", "";
  if (@rows) {
    push @out, "| 조작 | 하는 일 |", "| --- | --- |";
    push @out, "| $_->[0] | $_->[1] |" for @rows;
    push @out, "";
  }
  if (@notes) {
    push @out, "이렇게 해 보세요", "";
    push @out, "- $_" for @notes;
    push @out, "";
  }
  push @out, "## 확인 포인트", "", $check, "";
  push @out, "씬에서 보여 주는 것: $show", "" if length $show;
  push @out, "## 이렇게 요청하세요", "", "> $prompt", "";
  push @out, "## 코드 보기", "";
  push @out, "- 씬: `$scene`";
  push @out, "- 핵심 스크립트: `$scripts`" if length $scripts;
  push @out, "- 빌더: `$builder`" if length $builder;
  push @out, "- 구현 방식: $mode" if length $mode;

  open(my $out, ">", "$dir/bodies/$id.md") or die $!;
  print $out join("\n", @out), "\n";
  close $out;
}
print "generated ", scalar(keys %$meta), " bodies\n";
