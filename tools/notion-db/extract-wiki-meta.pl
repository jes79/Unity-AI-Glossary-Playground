use strict; use warnings;
# 위키 항목별 상세에서 id, 용어, 쉽게 말하면, 요청 예문, 씬, 핵심 스크립트, 빌더, 구현 방식을 뽑는다.

local $/ = undef;
my $txt = <>;
while ($txt =~ /^### ([0-9]{4})\s*\n(.*?)(?=^### |\z)/gms) {
  my ($id, $b) = ($1, $2);
  my ($term, $summary) = ("", "");
  if ($b =~ /^\*\*(.+?)\*\* — (.+?)\s*$/m) { ($term, $summary) = ($1, $2); }
  my ($prompt) = $b =~ /^> 요청 예문: (.+?)\s*$/m;
  my ($scene)  = $b =~ /^- 씬: `(.+?)`/m;
  my ($script) = $b =~ /^- 핵심 스크립트: `(.+?)`/m;
  my ($build)  = $b =~ /^- 빌더: `(.+?)`/m;
  my ($mode)   = $b =~ /^- 구현 방식: (.+?)\s*$/m;
  for ($term, $summary, $prompt, $scene, $script, $build, $mode) { $_ = "" unless defined $_; s/\t/ /g; }
  print join("\t", $id, $term, $summary, $prompt, $scene, $script, $build, $mode), "\n";
}
