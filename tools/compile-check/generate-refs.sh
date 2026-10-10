#!/usr/bin/env bash
# Unity 가 실제로 쓴 컴파일 옵션(Library/Bee/.../Assembly-CSharp-Editor.rsp)에서 참조 DLL 과 define 을 뽑아 Refs.props 생성
set -e
here="$(cd "$(dirname "$0")" && pwd)"
proj="$(cd "$here/../../Unity-AI-Glossary-Playground" && pwd -W)"
rsp=$(ls -t "$(cd "$here/../../Unity-AI-Glossary-Playground" && pwd)"/Library/Bee/artifacts/*/Assembly-CSharp-Editor.rsp | head -1)
{
  echo '<Project>'
  echo '  <PropertyGroup>'
  printf '    <DefineConstants>'
  grep '^-define:' "$rsp" | sed 's/^-define://' | tr '\n' ';'
  echo '</DefineConstants>'
  echo '  </PropertyGroup>'
  echo '  <ItemGroup>'
  grep '^-r:' "$rsp" | sed 's/^-r:"\(.*\)"$/\1/; s/^-r://' | grep -v 'Assembly-CSharp' | while read -r ref; do
    [[ "$ref" != /* && "$ref" != [A-Za-z]:* ]] && ref="$proj/$ref"
    echo "    <Reference Include=\"$ref\" Private=\"false\" />"
  done
  echo '  </ItemGroup>'
  echo '</Project>'
} > "$here/Refs.props"
echo "Refs.props generated from $rsp"
