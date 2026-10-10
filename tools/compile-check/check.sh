#!/usr/bin/env bash
# 컴파일 검사. 여러 작업자가 동시에 돌려도 충돌하지 않도록 이름별 폴더에서 빌드함
# 사용: bash tools/compile-check/check.sh <이름> [경로필터]
#   경로필터를 주면 그 문자열이 들어간 파일의 오류만 출력 (예: 01_Movement)
here="$(cd "$(dirname "$0")" && pwd)"
name="${1:-default}"
filter="${2:-}"
run="$here/.runs/$name"
mkdir -p "$run"
proj_win="$(cd "$here/../../Unity-AI-Glossary-Playground" && pwd -W)"
sed "s|\$(MSBuildThisFileDirectory)..\\..\\Unity-AI-Glossary-Playground|$proj_win|; s|Project=\"Refs.props\"|Project=\"$(cd "$here" && pwd -W)/Refs.props\"|" "$here/CompileCheck.csproj" > "$run/CompileCheck.csproj"
out=$(dotnet build "$run/CompileCheck.csproj" -nologo -v q "-p:UnityProject=$proj_win" 2>&1 | grep -E " error " | sed 's/ \[.*CompileCheck.csproj\]//' | sort -u)
if [ -n "$filter" ]; then out=$(echo "$out" | grep -F "$filter"); fi
if [ -z "$out" ]; then echo "OK (no errors${filter:+ in $filter})"; else echo "$out"; echo "---"; echo "$(echo "$out" | wc -l) error line(s)"; fi
