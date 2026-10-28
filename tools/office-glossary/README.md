# 사무실 용어집 자료(glossary.json) 만들기

OMP Studio 사무실 플러그인(`sidebar-reorder`)의 🎓 강의 방 → 📚 용어집(책장)이 쓰는 자료
`glossary.json` 을 이 저장소에서 다시 만드는 스크립트다. 용어집 창은 여기서 만든 JSON만 읽는다.
Unity 쪽 데이터나 위키를 고치면 아래 명령으로 다시 뽑아 플러그인 폴더에 덮어 쓴다.

## 실행

```bash
cd tools/office-glossary
perl build-glossary.pl                  # 기본 출력: %APPDATA%\omp-studio\plugins\sidebar-reorder\glossary.json
perl build-glossary.pl --out <경로>     # 다른 곳에 만들 때
perl build-glossary.pl --no-notion      # notion-urls.tsv 없이 (Notion 칸을 비운 채) 만들 때
```

perl 은 Git for Windows 에 들어 있는 것을 쓴다(`tools/notion-db/` 스크립트와 같다). 만든 뒤 플러그인을 다시 불러오면
(`studio_plugins reload sidebar-reorder`) 반영된다. `glossary.json` 자체는 저장소에 올리지 않는다(PC마다 이 스크립트로 만든다).

## 어디서 무엇을 읽나

| 칸 | 읽는 곳 |
| --- | --- |
| 번호 · 분야 · 소분류 · 용어 · 쉽게 말하면 · 요청 예문 · 씬 이름 | `Assets/01_Scripts/Common/GlossaryCatalog.Data.cs` (요청 예문은 바깥 큰따옴표를 뗀다) |
| 개념 · 확인 포인트 | `Assets/01_Scripts/Common/GlossaryCatalog.Guide.cs` |
| 막연한 요청 · 분야별 '채울 거리'(대상·수치 예) | `Assets/01_Scripts/Common/LectureRequestExamples.cs` |
| 분야 → 담당자(dev·art·plan·sound) · 담당자 말투 · 분야 소개 문구 | `Assets/01_Scripts/Common/LecturePresenters.cs` |
| 조작법(키 → 하는 일) · 2D/3D | 씬 빌더 `Assets/01_Scripts/Editor/Scenes/**/Build{id}_*.cs` 의 `NewScene2D/3D/UI·Stage(id, controls, …)` 두 번째 인자 (`const string` 으로 둔 것도 읽는다). TMP 서식 태그는 지운다 |
| 씬에서 보여 주는 것 · 핵심 스크립트 · 구현 방식 · 씬 경로 | `docs/wiki/{분야}.md` 의 항목별 상세 (씬 경로는 Data.cs 의 씬 이름과 맞는지 검사) |
| 위키 주소 | `https://github.com/{저장소}/wiki/{분야 페이지}#{id}` (저장소 주소는 `git remote origin`) |
| GitHub 씬 파일 주소 | `https://github.com/{저장소}/blob/{브랜치}/{Unity 폴더}/Assets/00_Scenes/{분야 폴더}/{씬}.unity` |
| Notion 행 주소 | `notion-urls.tsv` (아래) |

## Notion 행 주소 갱신 (`notion-urls.tsv`)

`id<TAB>주소` 한 줄씩 182줄. Notion 용어 DB(`collection://577e1e08-ef43-8367-8ca6-07f36c45f4d1`)의
`번호` · `url` 칸을 그대로 옮긴 것이다. 행이 바뀌었으면 Notion MCP 로 다시 읽어 이 파일을 고친다.

```
notion-query-data-sources  (SQL)
SELECT "번호", url FROM "collection://577e1e08-ef43-8367-8ca6-07f36c45f4d1" ORDER BY "번호"
-- 한 번에 100줄까지라 두 번째는 WHERE "번호" > '0607' 로 이어서 읽는다
```

## 검사 (하나라도 걸리면 파일을 쓰지 않고 종료 코드 1)

- 용어가 182개가 아니거나 id 가 겹침, 분야 번호와 id 앞 두 자리가 다름
- 개념 · 확인 포인트 · 막연한 요청 · 조작법 · 위키 항목이 없는 용어, 위키의 씬 경로와 Data.cs 의 씬 이름이 다름
- 씬 파일(`.unity`)이 디스크에 없음
- Notion 주소가 없거나 182개가 아님 (`--no-notion` 이 아닐 때)
- 위 칸 중 하나라도 빈 문자열 (소분류는 없는 용어가 많아 비어 있어도 된다)

## 만들어지는 모양

```
{ "version":1, "repo":{url, branch, project, wiki},
  "presenters": { dev|art|plan|sound|director: {role, nextLine, demoLine, accent} },
  "categories": [ {n, name, folder, count, presenter, intro, targetExample, numberExample, wiki} ×11 ],
  "entries":    [ {id, cat, sub, term, easy, concept, check, controls, prompt, vague, scene, scenePath, dim,
                   shows, script, mode, wiki, github, notion} ×182 ]   // 용어 하나가 한 줄 → 바뀐 곳이 diff 로 보인다
}
```

### 읽는 방식에 대한 검사

- `new GlossaryEntry(…)` 는 인자를 닫는 괄호 앞까지 모두 읽어야 하고(남는 글자 · 7칸이 아닌 줄은 실패), 호출 수와 읽은 수가 같아야 한다. Guide · VagueById · 담당자 정의도 항목 수와 읽은 수를 맞춰 본다.
- 씬 빌더 첫 호출(`Stage/NewScene*`)의 첫 인자 id 가 파일 이름 `Build{id}_` 의 id 와 같아야 한다(상수면 `const string` 으로 풀어 본다).
- 조작법 인자는 문자열 이어 붙임 또는 `const string` 상수만 읽는다. `@"…"`(축자) · `$"…"`(보간) 문자열이나 변수가 이어 붙은 식이 보이면 틀린 글자를 만들지 않고 실패한다.
