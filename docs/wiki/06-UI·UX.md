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
- 씬에서 보여 주는 것: 체력바·점수·일시정지 버튼 HUD, Render Mode Overlay↔Camera 비교

### 0602

**Canvas Scaler** — 해상도별 UI 크기 맞춤

> 요청 예문: "1920x1080 기준으로 모바일에서도 UI가 깨지지 않게 Canvas Scaler를 설정해줘."

- 씬: `Assets/00_Scenes/06_UI/0602_CanvasScaler.unity`
- 핵심 스크립트: `CanvasScalerDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0602_CanvasScaler.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: 1920x1080 기준 Scale With Screen Size, 기기 해상도 미리보기

### 0603

**Anchor / Pivot** — UI 고정 기준점

> 요청 예문: "점수는 화면 오른쪽 위에 Anchor로 고정해줘."

- 씬: `Assets/00_Scenes/06_UI/0603_AnchorPivot.unity`
- 핵심 스크립트: `AnchorPivotDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0603_AnchorPivot.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: 오른쪽 위 Anchor 고정 점수 vs 가운데 Anchor, Pivot 회전 중심

### 0604

**TextMeshPro (TMP)** — 선명한 글자 컴포넌트

> 요청 예문: "TextMeshPro로 한글 폰트 에셋을 만드는 법 알려줘."

- 씬: `Assets/00_Scenes/06_UI/0604_TextMeshPro.unity`
- 핵심 스크립트: `TmpFontDemo, GuidePages`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0604_TextMeshPro.cs`
- 구현 방식: 설명 + 대체 구현
- 씬에서 보여 주는 것: 한글 동적 폰트 아틀라스가 채워지는 모습, Font Asset Creator 단계

### 0605

**Layout Group** — 자동 정렬 (가로·세로·격자)

> 요청 예문: "Grid Layout Group으로 4x5 인벤토리 칸을 자동 배치해줘."

- 씬: `Assets/00_Scenes/06_UI/0605_LayoutGroup.unity`
- 핵심 스크립트: `InventoryGridDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0605_LayoutGroup.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: Grid Layout Group 4x5 인벤토리 자동 배치

### 0606

**Scroll View** — 스크롤 목록

> 요청 예문: "Scroll View에 상점 아이템 목록을 동적으로 생성해줘."

- 씬: `Assets/00_Scenes/06_UI/0606_ScrollView.unity`
- 핵심 스크립트: `ShopScrollDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0606_ScrollView.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: Scroll View에 상점 아이템 동적 생성·구매

### 0607

**월드 스페이스 UI** — 캐릭터 머리 위에 떠 있는 UI

> 요청 예문: "적 머리 위에 World Space 체력바를 띄우고 항상 카메라를 보게 해줘."

- 씬: `Assets/00_Scenes/06_UI/0607_WorldSpaceUI.unity`
- 핵심 스크립트: `WorldHealthBar, WorldSpaceUIDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0607_WorldSpaceUI.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: 적 머리 위 World Space 체력바, 빌보드 비교

### 0608

**체력바 (딜레이 감소)** — 깎인 부분이 늦게 따라 줄어듦

> 요청 예문: "체력바가 즉시 줄고 뒤의 흰 바가 0.5초 뒤 따라오게 해줘."

- 씬: `Assets/00_Scenes/06_UI/0608_HealthBarDelay.unity`
- 핵심 스크립트: `DelayedHealthBar, HealthBarDelayDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0608_HealthBarDelay.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: 체력바 즉시 감소 + 흰 바 0.5초 뒤 따라옴

### 0609

**쿨타임 UI (Radial Fill)** — 원형으로 차오르는 쿨다운

> 요청 예문: "Image Fill Method Radial 360으로 스킬 쿨타임 UI를 만들어줘."

- 씬: `Assets/00_Scenes/06_UI/0609_CooldownRadial.unity`
- 핵심 스크립트: `SkillCooldownSlot, CooldownDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0609_CooldownRadial.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: Image Fill Radial 360 스킬 쿨타임 UI

### 0610

**대화 시스템 / 타자 효과** — 대사창, 한 글자씩 출력

> 요청 예문: "NPC 대화창에서 글자가 한 글자씩 나오고 클릭하면 전부 표시되게 해줘."

- 씬: `Assets/00_Scenes/06_UI/0610_DialogueTypewriter.unity`
- 핵심 스크립트: `DialogueTypewriter`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0610_DialogueTypewriter.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: 한 글자씩 출력, 클릭하면 전부 표시

### 0611

**팝업 / 모달** — 확인·취소 창

> 요청 예문: "'정말 나가시겠습니까?' 확인 팝업을 재사용 가능하게 만들어줘."

- 씬: `Assets/00_Scenes/06_UI/0611_PopupModal.unity`
- 핵심 스크립트: `ConfirmPopup, PopupDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0611_PopupModal.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: 재사용 확인/취소 팝업, 모달 차단막

### 0612

**일시정지 메뉴** — Time.timeScale 0 + 메뉴

> 요청 예문: "ESC로 일시정지 메뉴(계속, 설정, 메인으로)를 열어줘."

- 씬: `Assets/00_Scenes/06_UI/0612_PauseMenu.unity`
- 핵심 스크립트: `PauseMenuController`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0612_PauseMenu.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: ESC 일시정지 메뉴(계속·설정·메인으로), timeScale 0

### 0613

**드래그 앤 드롭** — UI를 끌어다 놓기

> 요청 예문: "IDragHandler로 인벤토리 아이템을 드래그해서 슬롯끼리 교환해줘."

- 씬: `Assets/00_Scenes/06_UI/0613_DragAndDrop.unity`
- 핵심 스크립트: `DragItem, InventorySlot`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0613_DragAndDrop.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: IDragHandler로 인벤토리 아이템 슬롯 교환

### 0614

**툴팁 (Tooltip)** — 마우스 올리면 설명

> 요청 예문: "아이템에 마우스를 올리면 이름과 설명 툴팁을 띄워줘."

- 씬: `Assets/00_Scenes/06_UI/0614_Tooltip.unity`
- 핵심 스크립트: `TooltipTrigger, TooltipView`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0614_Tooltip.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: 마우스를 올리면 이름·설명 툴팁

### 0615

**미니맵** — 위에서 본 작은 지도

> 요청 예문: "Render Texture와 탑뷰 카메라로 미니맵을 만들어줘."

- 씬: `Assets/00_Scenes/06_UI/0615_Minimap.unity`
- 핵심 스크립트: `MinimapController`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0615_Minimap.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: 탑뷰 카메라 + Render Texture 미니맵, 아이콘·줌

### 0616

**로딩 화면 (비동기 로딩)** — 진행률 표시하며 씬 로드

> 요청 예문: "LoadSceneAsync로 진행률 바가 있는 로딩 화면을 만들어줘."

- 씬: `Assets/00_Scenes/06_UI/0616_AsyncLoading.unity`
- 핵심 스크립트: `AsyncSceneLoader`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0616_AsyncLoading.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: LoadSceneAsync 진행률 바 로딩 화면

### 0617

**조이스틱 (모바일)** — 화면 가상 조이스틱

> 요청 예문: "모바일용 가상 조이스틱과 점프 버튼을 만들어줘."

- 씬: `Assets/00_Scenes/06_UI/0617_VirtualJoystick.unity`
- 핵심 스크립트: `VirtualJoystick, JumpButton`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0617_VirtualJoystick.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: 모바일 가상 조이스틱 + 점프 버튼

### 0618

**세이프 에어리어 (Safe Area)** — 노치 영역 피하기

> 요청 예문: "아이폰 노치에 UI가 가리지 않게 Safe Area를 적용해줘."

- 씬: `Assets/00_Scenes/06_UI/0618_SafeArea.unity`
- 핵심 스크립트: `SafeAreaFitter, SafeAreaDemo`
- 빌더: `Assets/01_Scripts/Editor/Scenes/06_UI/Build0618_SafeArea.cs`
- 구현 방식: 런타임 데모
- 씬에서 보여 주는 것: 노치 흉내 + Screen.safeArea 앵커 적용

