# 06 UI·UX

uGUI(Canvas)와 UI Toolkit 중 어느 쪽인지 요청 첫 줄에 적으세요. 이 프로젝트는 uGUI + TextMeshPro입니다.

씬 18개 · 폴더 `Assets/00_Scenes/06_UI`

| 번호 | 용어 | 쉽게 말하면 | 씬에서 보여 주는 것 | 구현 방식 |
| --- | --- | --- | --- | --- |
| 0601 | [Canvas / uGUI](#0601) | 기본 UI 시스템 | 체력바·점수·일시정지 버튼 HUD, Render Mode Overlay↔Camera 비교 | 런타임 데모 |
| 0602 | [Canvas Scaler](#0602) | 해상도별 UI 크기 맞춤 | 1920x1080 기준 Scale With Screen Size, 기기 해상도 미리보기 | 런타임 데모 |
| 0603 | [Anchor / Pivot](#0603) | UI 고정 기준점 | 오른쪽 위 Anchor 고정 점수 vs 가운데 Anchor, Pivot 회전 중심 | 런타임 데모 |
| 0604 | [TextMeshPro (TMP)](#0604) | 선명한 글자 컴포넌트 | 한글 동적 폰트 아틀라스가 채워지는 모습, Font Asset Creator 단계 | 설명 + 대체 구현 |
| 0605 | [Layout Group](#0605) | 자동 정렬 (가로·세로·격자) | Grid Layout Group 4x5 인벤토리 자동 배치 | 런타임 데모 |
| 0606 | [Scroll View](#0606) | 스크롤 목록 | Scroll View에 상점 아이템 동적 생성·구매 | 런타임 데모 |
| 0607 | [월드 스페이스 UI](#0607) | 캐릭터 머리 위에 떠 있는 UI | 적 머리 위 World Space 체력바, 빌보드 비교 | 런타임 데모 |
| 0608 | [체력바 (딜레이 감소)](#0608) | 깎인 부분이 늦게 따라 줄어듦 | 체력바 즉시 감소 + 흰 바 0.5초 뒤 따라옴 | 런타임 데모 |
| 0609 | [쿨타임 UI (Radial Fill)](#0609) | 원형으로 차오르는 쿨다운 | Image Fill Radial 360 스킬 쿨타임 UI | 런타임 데모 |
| 0610 | [대화 시스템 / 타자 효과](#0610) | 대사창, 한 글자씩 출력 | 한 글자씩 출력, 클릭하면 전부 표시 | 런타임 데모 |
| 0611 | [팝업 / 모달](#0611) | 확인·취소 창 | 재사용 확인/취소 팝업, 모달 차단막 | 런타임 데모 |
| 0612 | [일시정지 메뉴](#0612) | Time.timeScale 0 + 메뉴 | ESC 일시정지 메뉴(계속·설정·메인으로), timeScale 0 | 런타임 데모 |
| 0613 | [드래그 앤 드롭](#0613) | UI를 끌어다 놓기 | IDragHandler로 인벤토리 아이템 슬롯 교환 | 런타임 데모 |
| 0614 | [툴팁 (Tooltip)](#0614) | 마우스 올리면 설명 | 마우스를 올리면 이름·설명 툴팁 | 런타임 데모 |
| 0615 | [미니맵](#0615) | 위에서 본 작은 지도 | 탑뷰 카메라 + Render Texture 미니맵, 아이콘·줌 | 런타임 데모 |
| 0616 | [로딩 화면 (비동기 로딩)](#0616) | 진행률 표시하며 씬 로드 | LoadSceneAsync 진행률 바 로딩 화면 | 런타임 데모 |
| 0617 | [조이스틱 (모바일)](#0617) | 화면 가상 조이스틱 | 모바일 가상 조이스틱 + 점프 버튼 | 런타임 데모 |
| 0618 | [세이프 에어리어 (Safe Area)](#0618) | 노치 영역 피하기 | 노치 흉내 + Screen.safeArea 앵커 적용 | 런타임 데모 |

## 항목별 상세

### 0601

**Canvas / uGUI** — 기본 UI 시스템

> 요청 예문: "uGUI로 체력바, 점수, 일시정지 버튼이 있는 HUD를 만들어줘."

- 씬: `Assets/00_Scenes/06_UI/0601_CanvasUGUI.unity`
- 핵심 스크립트: `HudController`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0601_CanvasUGUI.cs`
- 구현 방식: 런타임 데모
- 개념: 모든 UI 요소가 올라가는 판이 Canvas, 그 위의 버튼·이미지·텍스트 묶음이 uGUI 다. Render Mode 가 Overlay 면 화면에 바로 덮이고, Screen Space Camera 면 카메라를 거쳐 후처리·3D 가 끼어들 수 있다.
- 씬에서 보여 주는 것: 체력바·점수·일시정지 버튼 HUD, Render Mode Overlay↔Camera 비교
- 확인 포인트: 체력바·점수·일시정지 버튼이 창 크기를 바꿔도 제 위치에 있는지 보세요. Render Mode 를 Overlay ↔ Camera 로 바꾸면 UI 가 후처리 영향을 받는지가 달라집니다.

### 0602

**Canvas Scaler** — 해상도별 UI 크기 맞춤

> 요청 예문: "1920x1080 기준으로 모바일에서도 UI가 깨지지 않게 Canvas Scaler를 설정해줘."

- 씬: `Assets/00_Scenes/06_UI/0602_CanvasScaler.unity`
- 핵심 스크립트: `CanvasScalerDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0602_CanvasScaler.cs`
- 구현 방식: 런타임 데모
- 개념: 해상도가 달라도 UI 크기를 일정하게 유지해 주는 컴포넌트. Scale With Screen Size + 기준 해상도(보통 1920x1080) 설정이 사실상 표준이고, 이걸 빼먹으면 기기마다 UI 크기가 제각각이 된다.
- 씬에서 보여 주는 것: 1920x1080 기준 Scale With Screen Size, 기기 해상도 미리보기
- 확인 포인트: 기기 해상도 미리보기를 바꿔 보세요. Scale With Screen Size 면 UI 비율이 유지되는데, Constant Pixel Size 로 바꾸면 작은 화면에서 UI 가 화면을 뒤덮습니다.

### 0603

**Anchor / Pivot** — UI 고정 기준점

> 요청 예문: "점수는 화면 오른쪽 위에 Anchor로 고정해줘."

- 씬: `Assets/00_Scenes/06_UI/0603_AnchorPivot.unity`
- 핵심 스크립트: `AnchorPivotDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0603_AnchorPivot.cs`
- 구현 방식: 런타임 데모
- 개념: Anchor 는 부모 중 어디를 기준으로 붙을지, Pivot 은 자기 자신의 어느 점을 기준으로 위치·회전·크기를 적용할지를 정한다. 해상도 대응이 깨지는 대부분의 원인이 Anchor 설정이다.
- 씬에서 보여 주는 것: 오른쪽 위 Anchor 고정 점수 vs 가운데 Anchor, Pivot 회전 중심
- 확인 포인트: 창 크기를 바꿔 보세요. 오른쪽 위 Anchor 점수는 모서리에 붙어 있고, 가운데 Anchor 요소는 중앙을 유지합니다. Pivot 을 옮기면 회전 중심이 바뀌는 것도 보세요.

### 0604

**TextMeshPro (TMP)** — 선명한 글자 컴포넌트

> 요청 예문: "TextMeshPro로 한글 폰트 에셋을 만드는 법 알려줘."

- 씬: `Assets/00_Scenes/06_UI/0604_TextMeshPro.unity`
- 핵심 스크립트: `TmpFontDemo, GuidePages`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0604_TextMeshPro.cs`
- 구현 방식: 설명 + 대체 구현
- 개념: SDF 방식으로 글자를 그려 확대해도 선명하고, 외곽선·그림자·그라데이션을 지원하는 텍스트 컴포넌트. 한글은 글자 수가 많아 Dynamic 아틀라스로 필요한 글자만 그때그때 굽는 설정이 거의 필수다.
- 씬에서 보여 주는 것: 한글 동적 폰트 아틀라스가 채워지는 모습, Font Asset Creator 단계
- 확인 포인트: 글자를 입력해 아틀라스가 실시간으로 채워지는 모습을 보세요. 처음 쓰는 글자가 추가되는 순간이 보입니다. 패널의 Font Asset Creator 단계도 함께 읽어 보세요.

### 0605

**Layout Group** — 자동 정렬 (가로·세로·격자)

> 요청 예문: "Grid Layout Group으로 4x5 인벤토리 칸을 자동 배치해줘."

- 씬: `Assets/00_Scenes/06_UI/0605_LayoutGroup.unity`
- 핵심 스크립트: `InventoryGridDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0605_LayoutGroup.cs`
- 구현 방식: 런타임 데모
- 개념: 자식 UI 를 가로·세로·격자로 자동 배치해 주는 컴포넌트. 항목 수가 변하는 목록·인벤토리에서 좌표를 직접 계산하지 않게 해 준다.
- 씬에서 보여 주는 것: Grid Layout Group 4x5 인벤토리 자동 배치
- 확인 포인트: 인벤토리 칸이 4x5 격자로 자동 정렬되는지 보세요. 아이템을 추가·제거하면 좌표를 건드리지 않아도 재배치됩니다.

### 0606

**Scroll View** — 스크롤 목록

> 요청 예문: "Scroll View에 상점 아이템 목록을 동적으로 생성해줘."

- 씬: `Assets/00_Scenes/06_UI/0606_ScrollView.unity`
- 핵심 스크립트: `ShopScrollDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0606_ScrollView.cs`
- 구현 방식: 런타임 데모
- 개념: 보이는 영역(Viewport)보다 큰 내용(Content)을 잘라 보여 주고 스크롤하게 하는 묶음. Mask + Content 크기 + Layout 조합이고, Content 크기가 자동으로 안 늘면 스크롤이 안 되는 흔한 함정이 있다.
- 씬에서 보여 주는 것: Scroll View에 상점 아이템 동적 생성·구매
- 확인 포인트: 상점 목록을 스크롤해 보세요. 아이템을 동적으로 추가해도 스크롤 범위가 함께 늘어납니다. 범위를 벗어난 항목이 잘려 보이는지도 확인해 보세요.

### 0607

**월드 스페이스 UI** — 캐릭터 머리 위에 떠 있는 UI

> 요청 예문: "적 머리 위에 World Space 체력바를 띄우고 항상 카메라를 보게 해줘."

- 씬: `Assets/00_Scenes/06_UI/0607_WorldSpaceUI.unity`
- 핵심 스크립트: `WorldHealthBar, WorldSpaceUIDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0607_WorldSpaceUI.cs`
- 구현 방식: 런타임 데모
- 개념: Canvas 를 World Space 로 두어 3D 공간에 UI 를 세우는 방식. 적 머리 위 체력바·이름표에 쓰며, 카메라를 보도록 매 프레임 돌려 주는 처리(빌보드)가 보통 함께 필요하다.
- 씬에서 보여 주는 것: 적 머리 위 World Space 체력바, 빌보드 비교
- 확인 포인트: 카메라를 돌려 보세요. 빌보드를 켠 체력바는 항상 정면을 보고, 끄면 옆에서 납작하게 보입니다. 거리가 멀어지면 작게 보이는 것도 Overlay UI 와 다른 점입니다.

### 0608

**체력바 (딜레이 감소)** — 깎인 부분이 늦게 따라 줄어듦

> 요청 예문: "체력바가 즉시 줄고 뒤의 흰 바가 0.5초 뒤 따라오게 해줘."

- 씬: `Assets/00_Scenes/06_UI/0608_HealthBarDelay.unity`
- 핵심 스크립트: `DelayedHealthBar, HealthBarDelayDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0608_HealthBarDelay.cs`
- 구현 방식: 런타임 데모
- 개념: 체력이 깎일 때 본 바를 즉시 줄이고, 흰 바를 조금 늦게 따라 줄이는 연출. 얼마나 깎였는지를 눈으로 보여 줘서 같은 수치 변화를 더 극적으로 만든다.
- 씬에서 보여 주는 것: 체력바 즉시 감소 + 흰 바 0.5초 뒤 따라옴
- 확인 포인트: 피해를 입혀 보세요. 색 바가 즉시 줄고, 흰 바가 0.5초 뒤 따라 줄어듭니다. 연속으로 맞으면 흰 바가 누적되어 더 크게 보입니다.

### 0609

**쿨타임 UI (Radial Fill)** — 원형으로 차오르는 쿨다운

> 요청 예문: "Image Fill Method Radial 360으로 스킬 쿨타임 UI를 만들어줘."

- 씬: `Assets/00_Scenes/06_UI/0609_CooldownRadial.unity`
- 핵심 스크립트: `SkillCooldownSlot, CooldownDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0609_CooldownRadial.cs`
- 구현 방식: 런타임 데모
- 개념: Image 의 Fill 기능으로 쿨다운 남은 시간을 원형으로 보여 주는 UI. 숫자보다 직관적이어서 액션 게임 스킬 표시의 표준이다.
- 씬에서 보여 주는 것: Image Fill Radial 360 스킬 쿨타임 UI
- 확인 포인트: 스킬을 쓰고 원형 덮개가 시계 방향으로 줄어드는지, 다 줄어드는 순간 다시 쓸 수 있는지 보세요.

### 0610

**대화 시스템 / 타자 효과** — 대사창, 한 글자씩 출력

> 요청 예문: "NPC 대화창에서 글자가 한 글자씩 나오고 클릭하면 전부 표시되게 해줘."

- 씬: `Assets/00_Scenes/06_UI/0610_DialogueTypewriter.unity`
- 핵심 스크립트: `DialogueTypewriter`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0610_DialogueTypewriter.cs`
- 구현 방식: 런타임 데모
- 개념: 대사를 한 글자씩 출력해 읽는 속도를 만들어 주는 연출. 클릭하면 전부 즉시 표시하는 건너뛰기를 반드시 넣어야 답답하지 않다.
- 씬에서 보여 주는 것: 한 글자씩 출력, 클릭하면 전부 표시
- 확인 포인트: 대사가 한 글자씩 나오는지 보고, 출력 중에 클릭해 전부 즉시 표시되는지 확인해 보세요. 다 나온 뒤 클릭하면 다음 대사로 넘어갑니다.

### 0611

**팝업 / 모달** — 확인·취소 창

> 요청 예문: "'정말 나가시겠습니까?' 확인 팝업을 재사용 가능하게 만들어줘."

- 씬: `Assets/00_Scenes/06_UI/0611_PopupModal.unity`
- 핵심 스크립트: `ConfirmPopup, PopupDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0611_PopupModal.cs`
- 구현 방식: 런타임 데모
- 개념: 확인·취소를 묻고 그 전까지 뒤쪽 조작을 막는 창. 뒤를 막는 투명 차단막(모달)이 없으면 창 뒤의 버튼이 눌리는 버그가 생긴다.
- 씬에서 보여 주는 것: 재사용 확인/취소 팝업, 모달 차단막
- 확인 포인트: 팝업을 띄워 뒤쪽 버튼이 눌리지 않는지 확인해 보세요. 확인/취소 콜백을 다르게 넘겨 같은 팝업을 재사용하는 구조도 함께 보세요.

### 0612

**일시정지 메뉴** — Time.timeScale 0 + 메뉴

> 요청 예문: "ESC로 일시정지 메뉴(계속, 설정, 메인으로)를 열어줘."

- 씬: `Assets/00_Scenes/06_UI/0612_PauseMenu.unity`
- 핵심 스크립트: `PauseMenuController`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0612_PauseMenu.cs`
- 구현 방식: 런타임 데모
- 개념: Time.timeScale 을 0 으로 만들어 게임을 멈추고 메뉴를 띄우는 처리. timeScale 0 에서는 Update 의 시간 기반 처리가 멈추므로, UI 애니메이션은 unscaledDeltaTime 을 써야 한다.
- 씬에서 보여 주는 것: ESC 일시정지 메뉴(계속·설정·메인으로), timeScale 0
- 확인 포인트: ESC 로 일시정지해 캐릭터와 이펙트가 멈추는지, 그 상태에서 메뉴 버튼은 정상 동작하는지 보세요. 계속하기를 누르면 timeScale 이 1 로 돌아옵니다.

### 0613

**드래그 앤 드롭** — UI를 끌어다 놓기

> 요청 예문: "IDragHandler로 인벤토리 아이템을 드래그해서 슬롯끼리 교환해줘."

- 씬: `Assets/00_Scenes/06_UI/0613_DragAndDrop.unity`
- 핵심 스크립트: `DragItem, InventorySlot`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0613_DragAndDrop.cs`
- 구현 방식: 런타임 데모
- 개념: IBeginDragHandler · IDragHandler · IDropHandler 인터페이스로 UI 를 끌어다 놓는 처리. 인벤토리 정렬·장비 장착·카드 게임에 쓴다.
- 씬에서 보여 주는 것: IDragHandler로 인벤토리 아이템 슬롯 교환
- 확인 포인트: 아이템을 다른 칸으로 끌어다 놓아 서로 교환되는지 보세요. 빈 칸에 놓으면 이동하고, 칸 밖에 놓으면 원래 자리로 돌아옵니다.

### 0614

**툴팁 (Tooltip)** — 마우스 올리면 설명

> 요청 예문: "아이템에 마우스를 올리면 이름과 설명 툴팁을 띄워줘."

- 씬: `Assets/00_Scenes/06_UI/0614_Tooltip.unity`
- 핵심 스크립트: `TooltipTrigger, TooltipView`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0614_Tooltip.cs`
- 구현 방식: 런타임 데모
- 개념: 마우스를 올렸을 때 설명을 띄우는 UI. 화면 밖으로 넘치지 않게 위치를 보정하고, 살짝 지연을 두어 지나갈 때마다 뜨지 않게 하는 처리가 중요하다.
- 씬에서 보여 주는 것: 마우스를 올리면 이름·설명 툴팁
- 확인 포인트: 아이템에 마우스를 올려 이름·설명 툴팁이 뜨는지, 화면 가장자리에서도 툴팁이 잘리지 않는지 보세요.

### 0615

**미니맵** — 위에서 본 작은 지도

> 요청 예문: "Render Texture와 탑뷰 카메라로 미니맵을 만들어줘."

- 씬: `Assets/00_Scenes/06_UI/0615_Minimap.unity`
- 핵심 스크립트: `MinimapController`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0615_Minimap.cs`
- 구현 방식: 런타임 데모
- 개념: 위에서 내려다보는 카메라의 화면을 Render Texture 로 받아 UI 에 띄우는 방식. 아이콘만 그리려면 전용 레이어를 쓰고, 줌·회전·플레이어 중심 고정은 UI 쪽에서 처리한다.
- 씬에서 보여 주는 것: 탑뷰 카메라 + Render Texture 미니맵, 아이콘·줌
- 확인 포인트: 이동하며 미니맵 안의 아이콘이 함께 움직이는지, 줌을 바꾸면 보이는 범위가 달라지는지 보세요.

### 0616

**로딩 화면 (비동기 로딩)** — 진행률 표시하며 씬 로드

> 요청 예문: "LoadSceneAsync로 진행률 바가 있는 로딩 화면을 만들어줘."

- 씬: `Assets/00_Scenes/06_UI/0616_AsyncLoading.unity`
- 핵심 스크립트: `AsyncSceneLoader`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0616_AsyncLoading.cs`
- 구현 방식: 런타임 데모
- 개념: LoadSceneAsync 로 씬을 백그라운드에서 읽으며 진행률을 보여 주는 화면. progress 가 0.9 에서 멈추는 동작(활성화 대기)을 알고 있어야 진행률 바가 어색하지 않다.
- 씬에서 보여 주는 것: LoadSceneAsync 진행률 바 로딩 화면
- 확인 포인트: 로딩을 시작해 진행률 바가 차오르다 90% 에서 잠깐 머물고 완료되는지 보세요. 로딩 중에도 화면이 멈추지 않고 애니메이션이 도는지 함께 보세요.

### 0617

**조이스틱 (모바일)** — 화면 가상 조이스틱

> 요청 예문: "모바일용 가상 조이스틱과 점프 버튼을 만들어줘."

- 씬: `Assets/00_Scenes/06_UI/0617_VirtualJoystick.unity`
- 핵심 스크립트: `VirtualJoystick, JumpButton`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0617_VirtualJoystick.cs`
- 구현 방식: 런타임 데모
- 개념: 터치 위치를 기준으로 방향 입력을 만들어 주는 화면 UI. 손가락이 닿은 곳을 중심으로 잡는 방식(동적 조이스틱)이 고정식보다 조작이 편하다.
- 씬에서 보여 주는 것: 모바일 가상 조이스틱 + 점프 버튼
- 확인 포인트: 조이스틱을 끌어 캐릭터가 그 방향으로 움직이는지, 중앙에서 벗어난 거리만큼 속도가 달라지는지 보세요. 손을 떼면 중앙으로 돌아옵니다.

### 0618

**세이프 에어리어 (Safe Area)** — 노치 영역 피하기

> 요청 예문: "아이폰 노치에 UI가 가리지 않게 Safe Area를 적용해줘."

- 씬: `Assets/00_Scenes/06_UI/0618_SafeArea.unity`
- 핵심 스크립트: `SafeAreaFitter, SafeAreaDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0618_SafeArea.cs`
- 구현 방식: 런타임 데모
- 개념: 노치·펀치홀·둥근 모서리에 UI 가 가리지 않도록 Screen.safeArea 영역 안에 배치하는 처리. 모바일 출시에서 빼먹기 쉬운 항목이다.
- 씬에서 보여 주는 것: 노치 흉내 + Screen.safeArea 앵커 적용
- 확인 포인트: 노치 흉내 영역을 켜 보세요. Safe Area 를 적용한 UI 는 노치를 피해 내려오고, 끄면 점수나 버튼이 노치에 가립니다.

