# Notion 용어 DB 본문 만들기

[유니티 AI 요청 용어 DB](https://app.notion.com/p/9339668fce8b498da70b713964e24be6)의
각 페이지 본문(개념 / 테스트 방법 / 확인 포인트 / 이렇게 요청하세요 / 코드 보기)을
저장소 안의 데이터만으로 다시 만들어 내는 스크립트다.

본문은 전부 **이미 저장소에 있는 것들을 조합한 결과**라서, 씬이나 설명을 고치면 여기서 다시 뽑으면 된다.

| 들어가는 것 | 어디서 | 쓰이는 곳 |
| --- | --- | --- |
| 개념, 확인 포인트 | `Assets/01_Scripts/Common/GlossaryCatalog.Guide.cs` | `## 개념`, `## 확인 포인트` |
| 조작법(키 → 하는 일) | 씬 빌더의 `NewScene3D/2D/UI(id, controls)` 두 번째 인자 | `## 테스트 방법` 의 표 |
| 용어·씬 경로·스크립트·요청 예문 | `docs/wiki/{분야}.md` 의 항목별 상세 | 제목 줄, `## 코드 보기`, `## 이렇게 요청하세요` |
| 씬에서 보여 주는 것 | `docs/wiki/{분야}.md` | `## 확인 포인트` 아래 한 줄 |

## 실행

```bash
cd tools/notion-db
out=.out && mkdir -p $out

# 1) 씬 빌더에서 조작법 뽑기 (id<TAB>controls)
find ../../Unity-AI-Glossary-Playground/Assets/01_Scripts/Editor/Scenes -name 'Build*.cs' | sort \
  | while read f; do perl extract-controls.pl "$f"; done | sort -u > $out/controls.tsv

# 2) 위키에서 용어·경로·스크립트·요청 예문 뽑기
cat ../../docs/wiki/[0-1][0-9]-*.md | perl extract-wiki-meta.pl | sort -u > $out/meta.tsv

# 3) 위키에서 '씬에서 보여 주는 것' 뽑기
awk '/^### [0-9][0-9][0-9][0-9]$/ { id=$2 }
     /^- 씬에서 보여 주는 것: / { sub(/^- 씬에서 보여 주는 것: /,""); print id "\t" $0 }' \
  ../../docs/wiki/[0-1][0-9]-*.md > $out/shows.tsv

# 4) GlossaryCatalog.Guide.cs 에서 개념·확인 포인트 뽑기
awk '/^[ \t]*\["[0-9][0-9][0-9][0-9]"\] = \(/ {
       id=$0; sub(/^[ \t]*\["/,"",id); sub(/".*/,"",id)
       c=$0; sub(/^[^(]*\("/,"",c); sub(/",$/,"",c)
       getline nxt; k=nxt; sub(/^[ \t]*"/,"",k); sub(/"\),$/,"",k)
       print id "\t" c "\t" k }' \
  ../../Unity-AI-Glossary-Playground/Assets/01_Scripts/Common/GlossaryCatalog.Guide.cs > $out/guide.tsv

# 5) 페이지 본문 마크다운 182개 생성 → $out/bodies/{id}.md
perl build-body.pl $out
```

`.out/bodies/{id}.md` 를 Notion 각 페이지 본문에 그대로 붙이면 된다
(`번호` 칼럼이 파일 이름과 같다). 네 개 입력 파일 모두 182줄이어야 정상이다.

## 주의

- perl 스크립트는 인코딩 레이어를 쓰지 않고 **UTF-8 바이트를 그대로** 통과시킨다.
  `binmode(..., ":utf8")` 이나 `use utf8` 을 넣으면 한글이 이중 인코딩되어 깨진다.
- `controls` 안의 TMP 리치 텍스트 태그(`<b>`, `<color=...>`)는 `build-body.pl` 이 지운다.
  `ObjectPool<T>` 같은 제네릭은 남긴다.
- 조작법에 `"` 가 들어간 씬(0902)은 추출이 어긋나므로 생성 후 눈으로 확인한다.
