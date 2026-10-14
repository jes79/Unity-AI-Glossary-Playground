use strict; use warnings;
# 씬 빌더 파일에서 id(파일명 Build####_)와 controls(조작법) 문자열을 뽑는다.
# controls 는 Stage / NewScene3D / NewScene2D / NewSceneUI 의 두 번째 인자이며,
# "..." + "..." 로 이어 붙거나 첫 인자가 상수(Id)인 경우도 있다.
my $file = $ARGV[0];
exit 0 unless $file =~ /Build([0-9]{4})_/;
my $id = $1;
local $/ = undef;
my $txt = <>;
if ($txt =~ /(?:Stage|NewScene3D|NewScene2D|NewSceneUI)\(\s*(?:"[0-9]{4}"|[A-Za-z_][A-Za-z0-9_.]*)\s*,\s*((?:"[^"]*"\s*\+?\s*)+)/s) {
  my $raw = $1;
  my $s = "";
  while ($raw =~ /"([^"]*)"/gs) { $s .= $1; }
  $s =~ s/\s+$//;
  $s =~ s/\t/ /g;
  print "$id\t$s\n";
}
