# 예제 씬 작성 규칙

용어집 항목 하나 = 데모 씬 하나. 씬은 손으로 만들지 않고 **에디터 빌더 스크립트**로 생성한다
(`Tools > Glossary > Build All Scenes`). 이 문서는 새 씬을 추가하거나 고칠 때 지켜야 할 규칙이다.

## 환경

- Unity 6000.6.4f1, URP 17.6 (3D 렌더러 `Assets/Settings/PC_Renderer`), C# / .NET Standard 2.1
- **New Input System 전용** (`activeInputHandler: 1`) → `UnityEngine.Input.GetKey` 등 구 Input API 사용 금지.
  `GlossaryDemo.DemoInput`(키보드·마우스 직접 읽기) 또는 `UnityEngine.InputSystem.Keyboard.current` 사용.
- 설치된 패키지: URP, Shader Graph, VFX Graph, Cinemachine 6.6 (네임스페이스 `Unity.Cinemachine`), AI Navigation 2.0
  (`Unity.AI.Navigation`), Timeline, Input System, uGUI + TextMeshPro, 2D Sprite/Tilemap 모듈.
- **없는 패키지**: DOTween, Addressables, Localization, 2D Animation(본), Animation Rigging. 이런 용어는
  직접 구현한 대체 데모 + 정보 패널 설명으로 보여 준다(예: Tween → `Ease` 기반 미니 트윈).
- 한글 폰트: `GlossaryKit.EnsureKoreanFont()`가 맑은 고딕으로 동적 TMP 폰트(`Resources/Fonts/GlossaryFont`)를 만들고
  TMP 기본 폰트로 지정한다. 런타임 코드에서 TMP 텍스트를 만들면 `GlossaryInfoPanel.Font`를 넣는다.

## 폴더와 이름

| 대상 | 위치 |
| --- | --- |
| 씬 | `Assets/00_Scenes/{NN_분야}/{id}_{Name}.unity` (경로는 카탈로그가 결정, `Save(ctx)`가 저장) |
| 런타임 스크립트 | `Assets/01_Scripts/{NN_분야}/` |
| 씬 빌더 | `Assets/01_Scripts/Editor/Scenes/{NN_분야}/Build{id}_{Name}.cs` |
| 셰이더 | `Assets/02_Shaders/{NN_분야}/` (`Shader "Glossary/..."`) |
| 생성 에셋 | 머티리얼 `04_Materials/Generated`, 스프라이트 `05_Sprites/Generated`, 사운드 `08_Audio/Generated`, 그 밖 `09_Generated/{sub}` |

분야 폴더: `01_Movement 02_Physics 03_Animation 04_Shader 05_Effects 06_UI 07_EnemyAI 08_Systems 09_Audio 10_Rendering 11_Patterns`

### 네임스페이스

런타임 스크립트는 분야별 네임스페이스를 쓴다. 유니티 클래스 이름과 겹치는 네임스페이스(`Physics`, `Animation`, `Audio`…)는
`Physics.Raycast` 같은 호출을 깨뜨리므로 금지.

| 분야 | 네임스페이스 |
| --- | --- |
| 공통 | `GlossaryDemo` |
| 01 | `GlossaryDemo.Movement` |
| 02 | `GlossaryDemo.PhysicsDemo` |
| 03 | `GlossaryDemo.AnimationDemo` |
| 04 | `GlossaryDemo.ShaderDemo` |
| 05 | `GlossaryDemo.Effects` |
| 06 | `GlossaryDemo.UIDemo` |
| 07 | `GlossaryDemo.EnemyAI` |
| 08 | `GlossaryDemo.Systems` |
| 09 | `GlossaryDemo.AudioDemo` |
| 10 | `GlossaryDemo.Rendering` |
| 11 | `GlossaryDemo.Patterns` |

빌더 클래스는 전역 네임스페이스의 `public static class Build{id}_{Name}` (이름이 id 로 유일함).
런타임 클래스 이름도 분야 안에서 유일하게, 가능하면 `{Name}Demo` / 역할 이름으로 짓는다.

## 빌더 스크립트 형태

```csharp
using GlossaryDemo.Movement;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;

public static class Build0105_CoyoteTime
{
    [GlossaryScene("0105")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0105_CoyoteTime")]
    public static void Build()
    {
        var ctx = NewScene2D("0105", "A/D : 이동   Space : 점프\nC : 코요테 타임 ON/OFF");
        // ... 오브젝트 구성, Set(component, "field", value) 로 직렬화 필드 연결
        Save(ctx);
    }
}
```

- 씬 시작: `NewScene3D(id, controls, ground)` / `NewScene2D(id, controls)` / `NewSceneUI(id, controls)`.
  모두 EventSystem 과 **정보 패널**(`GlossaryInfoPanel`: 용어·쉽게 말하면·조작법·확인 포인트·개념·요청 예문)을 자동으로 넣는다.
- `controls` 문자열에는 조작법과 "무엇을 관찰할지"를 2~4줄로 쓴다.
- 새 용어를 추가하면 `GlossaryCatalog.Data.cs` 의 항목과 함께 **`GlossaryCatalog.Guide.cs` 에도 같은 id 로
  `개념`(무엇이고 언제 쓰나)과 `확인 포인트`(씬에서 무엇을 보면 되나)를 넣는다.** 둘이 비면 패널에서 해당 줄이 빠진다.
  같은 문장을 위키(`docs/wiki/{분야}.md`)와 Notion DB 의 `개념`·`확인 포인트` 칸에도 맞춰 둔다.
- 데모 UI 는 화면 **왼쪽 위 600×700 영역**(1920×1080 기준, 정보 패널 자리)을 피해서 배치한다.
  패널 높이는 글자 길이에 따라 513~1012px 로 변하므로 여유를 두는 편이 안전하다.
  실제로 가리는지는 빌드한 플레이어에 `-glossaryOverlapCheck -glossaryNoCapture` 를 주고 돌리면
  `[PanelOverlap] {씬} {UI 경로} {몇 %}` 로 Player.log 에 나온다.
  (패널이 화면 아래로 넘칠 만큼 길어지면 `개념`·`요청 예문` 블록은 자동으로 접히고 F2 로 다시 펼 수 있다.)
- 실시간 값은 `GlossaryInfoPanel.Status = "..."` 로 정보 패널 아래에 표시한다.
- 직렬화 필드는 `[SerializeField] private` + `[Header]`/`[Range]`, 빌더에서 `Set/SetFloat/SetLayerMask...`로 연결.
- 버튼은 `CreateButtonRow(ctx.Canvas.transform, 60f, "A", "B")` + `OnClick(btn, component.Method)` (영구 리스너).
- 이미지·사운드·텍스처는 파일을 들여오지 않고 `CreateSprite / CreateTexture / CreateToneClip / CreateMelodyClip`로 생성.
- 레이어: `Ground(6) Player(7) Enemy(8) PlayerBullet(9) EnemyBullet(10) Interactable(11) Obstacle(12) Minimap(13) Ragdoll(14) Hitbox(15)`
  태그: `Player`(기본), `Enemy Coin Checkpoint Interactable Bullet`. 정렬 레이어: `Background Default Characters Foreground`.
- 빌더는 여러 번 실행해도 같은 결과가 나와야 한다(생성 에셋은 덮어쓰기).
- 씬마다 독립적이어야 한다. 다른 씬의 런타임 스크립트를 재사용하는 것은 괜찮지만 씬 오브젝트를 공유하지 않는다.

## 코드 스타일

- 주석은 한국어, "왜/무엇을 보여 주는지" 위주로 간결하게. 학습용이므로 핵심 줄에는 설명 주석을 단다.
- 한 데모의 런타임 로직은 1~3개 스크립트로. 용어 이름이 코드에 그대로 보이도록 짓는다(예: `coyoteTime`, `jumpBufferTime`).
- Rigidbody 속도는 Unity 6 API `linearVelocity` / `angularVelocity` 사용 (`velocity`는 obsolete).
- `FindObjectOfType` 대신 `FindAnyObjectByType` / `FindObjectsByType`.
