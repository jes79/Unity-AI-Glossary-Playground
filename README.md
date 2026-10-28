# Unity-AI-Glossary-Playground

"유니티 AI 요청 용어집"의 182개 용어를 하나씩 **동작하는 예제 씬**으로 만든 학습용 프로젝트입니다.
각 씬은 용어집의 "이렇게 요청하세요" 예문을 그대로 구현한 결과물이고, 대부분 켜기/끄기 비교로 차이를 보여 줍니다.

- Unity 6000.6.4f1, URP 17.6, New Input System, Cinemachine 6.6, AI Navigation 2.0, Timeline
- 씬 182개 + 허브 씬 1개 (+ 0816 보조 씬 2개)
- 구현 방식: 런타임 데모 163 · 설명 + 대체 구현 14 · 에디터 도구 데모 5
- 용어별 설명·씬·스크립트 정리: [Wiki](https://github.com/jes79/Unity-AI-Glossary-Playground/wiki) (원본은 `docs/wiki/`)

![허브 씬: 분야를 고르고 용어를 누르면 해당 예제 씬으로 이동](docs/images/0000_Hub.jpg)

## 스크린샷

모든 씬 왼쪽 위에 용어 · 쉽게 말하면 · 조작법 · 확인 포인트 · 개념 · 요청 예문 · 실시간 상태를 보여 주는 정보 패널이 있습니다.

| | |
| --- | --- |
| ![0415 홀로그램](docs/images/0415_Hologram.jpg) | ![0501 Particle System](docs/images/0501_ParticleSystem.jpg) |
| **0415 홀로그램** · 위로 흐르는 스캔라인 셰이더 | **0501 Particle System** · 불꽃·불씨·연기 3레이어 모닥불 |
| ![0516 오라/버프](docs/images/0516_AuraBuff.jpg) | ![0522 VFX Graph](docs/images/0522_VFXGraph.jpg) |
| **0516 오라 / 버프** · 회전 마법진 + 상승 입자 | **0522 VFX Graph** · GPU 입자 5만 개 소용돌이 포탈 |
| ![0615 미니맵](docs/images/0615_Minimap.jpg) | ![0706 시야 판정](docs/images/0706_FieldOfView.jpg) |
| **0615 미니맵** · 탑뷰 카메라 + Render Texture | **0706 시야 판정** · 시야각 90°·10m, 벽에 가리면 못 봄 |
| ![0711 A*](docs/images/0711_AStar.jpg) | ![1013 GPU Instancing](docs/images/1013_GPUInstancing.jpg) |
| **0711 A\*** · 타일맵 최단 경로와 탐색 칸 | **1013 GPU Instancing** · GameObject 없이 나무 1000그루 |

## 처음 열 때

1. Unity Hub에서 `Unity-AI-Glossary-Playground` 폴더를 엽니다.
2. 메뉴 `Tools > Glossary > Build All Scenes`를 실행합니다. 씬과 생성 에셋을 코드로 다시 만들고, Build Settings에 등록합니다.
   (한글 폰트는 Windows의 맑은 고딕을 복사해 TMP 동적 폰트로 만듭니다. 재배포 문제로 `malgun.ttf`는 git에 올리지 않으므로, 저장소를 새로 받으면 폰트 원본이 없어 글자가 보이지 않습니다.
   프로젝트를 열면 `GlossaryFontGuard`가 이를 감지해 자동으로 복구합니다. 수동으로는 `Tools > Glossary > Setup Korean Font`.)
3. `Assets/00_Scenes/0000_Hub.unity`를 열고 Play → 왼쪽에서 분야, 오른쪽에서 용어를 고르면 해당 씬으로 이동합니다.

각 씬 왼쪽 위 정보 패널에 **용어 · 쉽게 말하면 · 조작법 · 확인 포인트(무엇을 보면 되는지) · 개념 · 요청 예문 · 실시간 상태**가 표시됩니다.
`F1`로 패널 전체를 접고, `F2`로 개념·요청 예문만 접습니다(길어서 화면을 넘치면 자동으로 접힙니다). `[허브]`로 목록으로 돌아갑니다.

## 강의 모드

수업 시간에 화면에 띄워 놓고 쓰는 진행 모드입니다. 예제 씬이 실제로 돌아가는 화면 위에 "개발실" 팀원 캐릭터(프로그래머·아트·기획·사운드·디렉터)가
나와 용어를 단계별로 설명합니다. 씬을 따로 고치지 않고 모든 씬의 정보 패널(`GlossaryInfoPanel`)이 강의 화면(`LectureOverlay`)을 띄웁니다.

| 담당 | 분야 |
| --- | --- |
| 프로그래머 | 01 이동·조작·카메라, 02 물리·충돌, 07 적 AI·길찾기, 10 조명·렌더링·최적화, 11 코드 구조·패턴·에디터 |
| 아트 | 03 애니메이션, 04 셰이더·머티리얼, 05 이펙트, 06 UI·UX |
| 기획 | 08 게임 시스템 |
| 사운드 | 09 오디오 |
| 디렉터 | 허브에서 강의를 시작할 때 강의 전체를 여는 인사 (좋은 요청의 4요소 소개) |

**시작하기**

- 허브 오른쪽 위 `[▶ 강의 모드로 시작]`: 디렉터 인사 뒤 지금 고른 분야의 첫 용어부터 시작합니다.
- `[강의 모드 꺼짐/켜짐]` 버튼(또는 `F4`)으로 켜 두면, 목록에서 누른 용어부터 강의가 시작됩니다(디렉터 인사 포함).
- 데모 씬 안에서도 `F4`로 켜고 끌 수 있습니다. 끄면 정보 패널이 원래대로 돌아옵니다. 강의 모드 상태는 씬이 바뀌어도 유지됩니다.

**용어 하나의 진행 순서**

| 단계 | 화면 |
| --- | --- |
| 시작 | 허브에서 강의를 시작했을 때 첫 용어 앞에 한 번. 디렉터가 좋은 요청의 4요소를 예시 문장으로 소개 |
| 분야 소개 | 분야가 바뀔 때마다. 담당자가 "이번엔 ○○ 분야예요"와 함께 회사에서 이 분야 담당이 하는 일, 배울 용어 수를 소개 |
| ① 소개 | "이번 용어는 ○○!" + 쉽게 말하면 |
| ② 개념 | 무엇이고 언제 쓰는지 |
| ③ 직접 보기 | 말풍선이 작아지고 왼쪽 위에 조작법·확인 포인트·실시간 상태 카드가 나옵니다. 데모 키는 그대로 동작 |
| ④ AI에게 요청 | 용어를 모르는 사람의 막연한 요청과 카탈로그의 구체적인 요청 예문을 나란히 보여 주고, 좋은 요청의 4요소(대상 · 기능 용어 · 조건·수치 · 내 환경) 중 예문에 들어 있는 칸은 초록, 빠진 칸은 주황 "빠짐 → 예: …"로 채울 거리를 보여 줌 |

마지막 단계에서 다음으로 넘기면 카탈로그 순서의 다음 용어 씬을 불러옵니다.

**강의 키** (데모가 쓰는 방향키·WASD·Space·숫자·글자 키와 겹치지 않게 골랐습니다. 프레젠터 리모컨의 PageDown/PageUp 으로 넘길 수 있습니다)

| 키 | 동작 |
| --- | --- |
| `PageDown` / `]` | 다음 (글자가 나오는 중이면 먼저 전부 표시) · 말풍선 클릭도 같음 |
| `PageUp` / `[` | 이전 단계 (첫 단계면 이전 용어의 마지막 단계) |
| `End` / `Home` | 다음 용어 / 이전 용어로 바로 이동 |
| `F3` | 말풍선 숨기기/보이기 |
| `Backspace` | 허브로 |
| `F4` | 강의 모드 켜기/끄기 |

- 캐릭터 그림은 `Assets/Resources/Lecture/{dev|art|plan|sound|director}_{rest|work|review|ask}.png`(투명 배경 픽셀아트)입니다.
  단계마다 상태가 바뀝니다: 분야 소개·③ = `review`, ① = `rest`, ② = `work`, ④ = `ask`. 그림은 원본 비율 그대로 정수 배(보통 2배, ③은 1배)로 그리고,
  `LectureSpriteImporter`(Editor)가 이 폴더를 점 필터·무압축·원본 크기·밉맵 없음으로 임포트해 흐려지지 않게 합니다. 그림이 없으면 직군 첫 글자를 원 안에 넣어 보여 줍니다.
- 담당 배정·분야 소개 문구는 `Assets/01_Scripts/Common/LecturePresenters.cs`, 막연한 요청 182개와 빠진 칸 예시는 `LectureRequestExamples.cs`, 단계별 대사 조합은 `LectureOverlay.cs`에 있습니다.
- 입력창(TMP_InputField)에 글자를 치는 중에는 강의 키(F4 포함)가 동작하지 않습니다.
- 데모와 겹치는 부분: 0616 로딩 화면의 "아무 키나 누르세요"는 강의 키를 받지 않고, 로딩이 활성화를 기다리는 동안에는 강의가 다른 씬으로 넘어가지 않습니다(말풍선으로 안내).
  0816 게임 씬의 10초 타이머는 강의 ③ 직접 보기에 도달한 뒤부터 흐릅니다.

## 분야

| 번호 | 분야 | 씬 수 | 폴더 |
| --- | --- | --- | --- |
| 01 | 캐릭터 이동·조작·카메라 | 15 | `01_Movement` |
| 02 | 물리·충돌 | 14 | `02_Physics` |
| 03 | 애니메이션 | 14 | `03_Animation` |
| 04 | 셰이더·머티리얼 | 20 | `04_Shader` |
| 05 | 이펙트 (파티클·대표 이펙트·포스트 프로세싱) | 30 | `05_Effects` |
| 06 | UI·UX | 18 | `06_UI` |
| 07 | 적 AI·길찾기 | 12 | `07_EnemyAI` |
| 08 | 게임 시스템 | 19 | `08_Systems` |
| 09 | 오디오 | 8 | `09_Audio` |
| 10 | 조명·렌더링·최적화 | 19 | `10_Rendering` |
| 11 | 코드 구조·패턴·에디터 | 13 | `11_Patterns` |

씬 이름 규칙: `{분야2자리}{순번2자리}_{Name}` (예: `0105_CoyoteTime`). 전체 목록은 `Assets/01_Scripts/Common/GlossaryCatalog.Data.cs`.

## 구조

```
Assets/
  00_Scenes/{분야}/            빌더가 생성한 씬
  01_Scripts/Common/           카탈로그, 정보 패널, 허브, 강의 모드(LectureMode·LectureOverlay·LecturePresenters), DemoInput, Ease
  Resources/Lecture/           강의 모드 캐릭터 그림 {담당}_{상태}.png
  01_Scripts/{분야}/           런타임 데모 스크립트 (네임스페이스 GlossaryDemo.*)
  01_Scripts/Editor/           GlossaryKit(씬 빌더 헬퍼), GlossaryValidator
  01_Scripts/Editor/Scenes/    씬 빌더 Build{id}_{Name}.cs
  02_Shaders/{분야}/           URP HLSL 셰이더
  04_Materials, 05_Sprites, 08_Audio, 09_Generated   빌더가 만드는 에셋 (텍스처·사운드도 코드로 생성)
docs/CONVENTIONS.md            새 씬을 추가할 때 지킬 규칙
tools/compile-check/           Unity 없이 C# 컴파일 검사 (dotnet)
```

## 도구 메뉴

- `Tools > Glossary > Build All Scenes` / `Build Scene > …` : 전체 또는 씬 하나 다시 만들기
- `Tools > Glossary > Validate All Scenes` : 셰이더 컴파일 오류, Missing Script, 분홍 머티리얼 점검
- `Tools > Glossary > Rendering/Patterns > …` : 라이트맵·오클루전 굽기, 적 스포너 창 등 데모용 에디터 도구
- `Tools > Glossary > Build Screenshot Player` : README 스크린샷용 Windows 플레이어 빌드
  (`Builds/Screenshots/Glossary.exe -glossaryShots <폴더> -glossaryShotList 0000_Hub,0415_Hologram` 로 실행하면 자동 캡처 후 종료)
  `-glossaryOverlapCheck -glossaryNoCapture` 를 주면 캡처 대신 정보 패널이 데모 UI 를 가리는 씬을 Player.log 에 적음
  강의 모드 화면은 목록 항목을 `씬@단계`로 씁니다(단계: `Opening SelfIntro Intro Concept Demo Prompt`, 예: `0105_CoyoteTime@Demo` → `0105_CoyoteTime_Demo.png`).
  `-glossaryLecture`를 주면 허브 등 단계를 안 적은 항목도 강의 모드로 찍고, 가림 점검은 강의 UI 를 `[LectureOverlap]`으로 적습니다.
- `Tools > Glossary > Build Lecture Player` : 수업용 강의 플레이어 빌드 → `Builds/LecturePlayer/Glossary.exe` (테두리 없는 전체화면, `F10` 끝내기)
  `Glossary.exe -glossaryStart 0105 -glossaryLecture` 로 실행하면 그 용어부터 강의 모드 (OMP Studio 앱의 ▶ 예제 실행이 이렇게 엽니다). `-glossaryLecture` 를 빼면 강의 없이 그 용어 씬만
  명령줄: `Unity.exe -batchmode -projectPath Unity-AI-Glossary-Playground -executeMethod GlossaryLecturePlayer.BuildFromCommandLine -quit`

명령줄 일괄 생성 + 점검:

```
Unity.exe -batchmode -projectPath Unity-AI-Glossary-Playground -executeMethod GlossaryBatch.BuildAndValidate -quit
```

## 대체 구현한 항목

설치하지 않은 패키지나 코드로 만들기 어려운 에셋은 같은 개념을 직접 구현하고, 실제 사용법을 씬 안 패널로 설명합니다.

| id | 용어 | 대체 방식 |
| --- | --- | --- |
| 0308 / 0309 | Humanoid 리타게팅 / IK | 본 이름 매핑 리타게터, 2본 IK 직접 구현 (사람형 모델 없음) |
| 0311 | 2D 본 애니메이션 | SpriteRenderer 계층 본 (2D Animation 패키지 없음) |
| 0313 | DOTween | 미니 트윈 (DOTween 없음) |
| 0402 | Shader Graph | 노드 레시피 + 같은 결과의 HLSL |
| 0520 | Decal | URP DecalProjector ↔ Quad 대체 전환 |
| 0522 | VFX Graph | 정점 셰이더로 그린 입자 5만 개 포탈 |
| 0604 | TMP 한글 폰트 | 동적 폰트 아틀라스 시연 + Font Asset Creator 단계 |
| 0819 | Localization | 문자열 테이블로 KO/EN 전환 |
| 0903 / 0907 | Audio Mixer / 스냅샷 | 스크립트 믹서 그룹, AudioLowPassFilter |
| 1009 | URP 2D Light | 라이트 맵 Render Texture + Multiply |
| 1018 | Addressables | Resources.LoadAsync |
| 1112 | Git | 미니 Git 시뮬레이터 + 설정 검사 |

## 검증 상태

- **에디터 점검**: 182개 씬 일괄 생성 + `Validate All Scenes`(셰이더 컴파일 오류·Missing Script·분홍 머티리얼) 통과
- **실행 점검**: Windows 플레이어로 185개 씬(보조 씬 포함)을 차례로 실행해 예외 로그와 화면을 확인했습니다.
  이때 찾은 문제(0417 물 셰이더 거품, 0819 문자열 테이블 연결 누락, 1011 씬 전환 멈춤, 0212 키네마틱 경고, 0604·0410 글자 겹침, 정보 패널 제목 잘림)는 모두 고쳤습니다.
- **자동 실행으로는 확인할 수 없는 부분** (키·마우스 조작이 필요한 연출):
  타격·폭발·디졸브처럼 눌러야 시작되는 연출, Cinemachine 카메라 조작감(0111~0113), 래그돌 쓰러짐(0212), Root Motion 구르기(0307)

## 주의할 점

- 0520: 씬에 있는 동안만 `QualitySettings.renderPipeline`을 Decal 복제 파이프라인으로 바꿉니다. 비정상 종료 시 Quality 설정의 파이프라인을 `PC_RPAsset`으로 되돌리세요.
- 1004 / 1012: Play 중 URP 에셋 값(그림자 거리, SRP Batcher)을 바꾸고 종료 시 되돌립니다.
- 0204: `Physics.IgnoreLayerCollision`을 실행 중에만 바꾸고 씬 종료 시 복원합니다.
- 0527 등 Depth of Field: 에디터에서는 동작하지만, 빌드에서는 URP가 DOF 셰이더를 제외해 효과가 빠질 수 있습니다.
