using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GlossaryDemo
{
    // 모든 데모 씬 왼쪽 위에 뜨는 정보 패널
    // - 용어 / 쉽게 말하면 / 요청 예문 / 조작법 / 실시간 상태(Status)
    // - [허브] 버튼으로 목록 씬으로 돌아가고, F1 또는 [접기]로 패널을 접음
    // UI 는 런타임에 코드로 생성하므로 씬 빌더는 이 컴포넌트에 id 와 조작법만 넣으면 됨
    // 강의 모드(LectureMode)가 켜져 있으면 LectureOverlay 를 붙이고 이 패널은 숨기거나 '조작 카드'로 줄임
    [DefaultExecutionOrder(-100)]
    public class GlossaryInfoPanel : MonoBehaviour
    {
        public const float PanelWidth = 560f; // 데모 UI 는 왼쪽 위 이 영역을 피해서 배치

        [SerializeField] private string entryId;
        [SerializeField, TextArea(2, 8)] private string controls;

        private static GlossaryInfoPanel instance;
        private static TMP_FontAsset font;

        // 강의 모드에서 패널을 어떻게 보일지
        public enum LectureView
        {
            Off,    // 강의 모드 아님 (원래 패널)
            Hidden, // 숨김 (담당자가 말하는 단계)
            Card,   // 조작법 + 확인 포인트 + 실시간 상태만 (③ 직접 보기)
        }

        private RectTransform body;
        private RectTransform header;
        private TextMeshProUGUI categoryText;
        private TextMeshProUGUI summaryText;
        private TextMeshProUGUI lectureTitle;
        private LectureView lectureView = LectureView.Off;
        private LectureOverlay lecture;
        private RectTransform panelRect;    // 패널 전체 (높이가 화면을 넘는지 재는 용도)
        private RectTransform canvasRect;
        private RectTransform details;      // 개념 · 요청 예문 (F2 로 접기)
        private TextMeshProUGUI statusText;
        private TextMeshProUGUI foldLabel;
        private TextMeshProUGUI hintText;
        private bool folded;
        private bool detailsOpen = true;
        private bool autoCollapsed;         // 화면을 넘쳐서 자동으로 접은 적이 있는가

        private const string HintText = "F1 : 패널 접기   F2 : 개념·요청 예문 접기";
        private const string HintClosed = "F1 : 패널 접기   F2 : 개념·요청 예문 보기";
        private const string HintAuto = "F1 : 패널 접기   F2 : 개념·요청 예문 보기 (화면을 넘쳐 접음)";

        // 데모 스크립트에서 현재 값(속도, 상태 이름 등)을 패널 아래쪽에 표시할 때 사용
        public static string Status
        {
            set
            {
                if (instance == null || instance.statusText == null) return;
                instance.statusText.text = value;
                instance.statusText.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }

        public static TMP_FontAsset Font
        {
            get
            {
                if (font != null) return font;

                var korean = Resources.Load<TMP_FontAsset>("Fonts/GlossaryFont");

                // GlossaryFont 는 동적(Dynamic) 폰트라 원본 ttf 가 있어야 글리프를 만든다.
                // 원본(malgun.ttf)은 재배포 라이선스 때문에 git 에 없으므로, 새로 받은 프로젝트에서는
                // 이 참조가 끊겨 있고 그대로 쓰면 모든 글자가 '보이지 않는' 상태가 된다.
                // → 에디터에서는 Tools > Glossary > Setup Korean Font 가 자동으로 고치고,
                //   그래도 비어 있으면 기본 폰트로 떨어뜨려 최소한 글자는 보이게 한다.
                if (korean != null && korean.sourceFontFile == null)
                {
                    Debug.LogWarning("[Glossary] 한글 폰트 원본(Assets/06_Fonts/malgun.ttf)이 없어 글자가 안 보입니다. " +
                                     "Tools > Glossary > Setup Korean Font 를 실행하세요. 지금은 기본 폰트로 대체합니다.");
                    korean = null;
                }

                font = korean != null ? korean : TMP_Settings.defaultFontAsset;
                return font;
            }
        }

        // 씬 빌더가 넣은 조작법 (강의 모드 대사에서도 사용)
        public string Controls => controls;

        private void Awake()
        {
            instance = this;
            EnsureEventSystem();
            Build();
            if (LectureMode.Enabled) StartLecture();
        }

        private void StartLecture()
        {
            if (lecture != null || !GlossaryCatalog.TryFind(entryId, out var entry)) return;
            lecture = LectureOverlay.Create(this, entry);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            // 강의 모드 켜기/끄기 (켜면 지금 씬의 용어부터 담당자 인사로 시작). 입력창에 글자를 치는 중에는 무시
            if (kb[LectureMode.ToggleKey].wasPressedThisFrame && !LectureMode.TypingInInputField)
            {
                if (lecture != null)
                {
                    LectureMode.SetEnabled(false);
                    lecture.Close();
                    lecture = null;
                }
                else
                {
                    LectureMode.Prepare(null);
                    StartLecture();
                }
            }

            if (lecture != null) return; // 강의 중에는 패널 접기 키를 쓰지 않음
            if (kb.f1Key.wasPressedThisFrame) ToggleFold();
            if (kb.f2Key.wasPressedThisFrame) ToggleDetails();
        }

        // 강의 모드 표시 전환. Off 로 돌리면 접기 상태까지 원래와 똑같이 복원
        public void SetLectureView(LectureView view)
        {
            lectureView = view;
            bool on = view != LectureView.Off;
            panelRect.gameObject.SetActive(view != LectureView.Hidden);
            header.gameObject.SetActive(!on);
            if (categoryText != null) categoryText.gameObject.SetActive(!on);
            if (summaryText != null) summaryText.gameObject.SetActive(!on);
            details.gameObject.SetActive(!on && detailsOpen);
            lectureTitle.gameObject.SetActive(view == LectureView.Card);
            hintText.gameObject.SetActive(!on);
            body.gameObject.SetActive(on || !folded);
            // 카드는 강의 진행 막대 바로 아래에 붙임 (둘 다 원래 패널 자리 = 왼쪽 위 안에 들어감)
            panelRect.anchoredPosition = on ? new Vector2(16f, -16f - LectureOverlay.ProgressHeight - 8f) : new Vector2(16f, -16f);
        }

        public void ToggleFold()
        {
            folded = !folded;
            body.gameObject.SetActive(!folded);
            foldLabel.text = folded ? "펼치기" : "접기";
        }

        public void ToggleDetails()
        {
            detailsOpen = !detailsOpen;
            autoCollapsed = true; // 직접 조작한 뒤에는 자동으로 다시 접지 않음
            if (details != null) details.gameObject.SetActive(detailsOpen);
            if (hintText != null) hintText.text = detailsOpen ? HintText : HintClosed;
        }

        // 실시간 상태(Status)가 긴 씬에서는 패널이 화면 아래로 넘쳐 개념·요청 예문이 잘린다.
        // 넘치는 순간 한 번만 자동으로 접고, 다시 보고 싶으면 F2 로 펼치게 둔다 (껐다 켰다 반복 방지)
        private void LateUpdate()
        {
            if (lectureView != LectureView.Off) return;
            if (autoCollapsed || folded || !detailsOpen) return;
            if (panelRect == null || canvasRect == null || details == null) return;
            if (panelRect.rect.height <= canvasRect.rect.height - 24f) return;

            detailsOpen = false;
            autoCollapsed = true;
            details.gameObject.SetActive(false);
            if (hintText != null) hintText.text = HintAuto;
        }

        public void GoHub()
        {
            Time.timeScale = 1f;
            if (Application.CanStreamedLevelBeLoaded(GlossaryCatalog.HubScene))
                SceneManager.LoadScene(GlossaryCatalog.HubScene);
            else
                Debug.LogWarning("허브 씬이 Build Settings 에 없음 (Tools > Glossary > Build All Scenes 실행)");
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private void Build()
        {
            GlossaryCatalog.TryFind(entryId, out var entry);

            var canvasGo = new GameObject("GlossaryInfoCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = NewRect("Panel", canvasGo.transform);
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0f, 1f);
            panel.anchoredPosition = new Vector2(16f, -16f);
            panel.sizeDelta = new Vector2(PanelWidth, 0f);
            var panelImg = panel.gameObject.AddComponent<Image>();
            panelImg.color = new Color(0.07f, 0.08f, 0.1f, 0.86f);
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 14, 14);
            layout.spacing = 8f;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            panelRect = panel;
            canvasRect = (RectTransform)canvasGo.transform;

            // 헤더: 제목 + 버튼
            header = NewRect("Header", panel);
            var hl = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 8f;
            hl.childControlWidth = hl.childControlHeight = true;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = true;
            header.gameObject.AddComponent<LayoutElement>().preferredHeight = 44f;

            string title = string.IsNullOrEmpty(entry.Id) ? entryId : $"{entry.Id}  {entry.Term}";
            var titleText = NewText("Title", header, title, 26f, new Color(1f, 0.86f, 0.4f));
            titleText.fontStyle = FontStyles.Bold;
            titleText.textWrappingMode = TextWrappingModes.NoWrap;
            titleText.overflowMode = TextOverflowModes.Truncate; // 동적 한글 폰트에 '…' 글리프가 없어 Ellipsis 는 경고가 남
            // 긴 용어("시야 판정 (Field of View)")도 잘리지 않게 칸에 맞춰 글자 크기를 줄임
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 15f;
            titleText.fontSizeMax = 26f;
            titleText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            NewButton(header, "허브", GoHub);
            foldLabel = NewButton(header, "접기", ToggleFold);

            body = NewRect("Body", panel);
            var bl = body.gameObject.AddComponent<VerticalLayoutGroup>();
            bl.spacing = 6f;
            bl.childControlWidth = bl.childControlHeight = true;
            bl.childForceExpandWidth = true;
            bl.childForceExpandHeight = false;

            // 강의 모드 ③ 단계에서만 보이는 카드 제목
            lectureTitle = NewText("LectureTitle", body, $"<color=#FFD479>③ 직접 보기</color>   {entry.Term}", 22f, Color.white);
            lectureTitle.fontStyle = FontStyles.Bold;
            lectureTitle.gameObject.SetActive(false);

            if (!string.IsNullOrEmpty(entry.Id))
            {
                string sub = string.IsNullOrEmpty(entry.Sub) ? "" : $" › {entry.Sub}";
                categoryText = NewText("Category", body, $"{entry.CategoryName}{sub}", 18f, new Color(0.6f, 0.65f, 0.75f));
                if (!string.IsNullOrEmpty(entry.Summary))
                    summaryText = NewText("Summary", body, entry.Summary, 22f, Color.white);
            }

            // 조작법(지금 뭘 누르나) → 확인 포인트(그래서 뭘 보면 되나) 순서로 둠
            if (!string.IsNullOrEmpty(controls))
                NewText("Controls", body, $"<color=#9BE89B>조작</color>\n{controls}", 19f, new Color(0.85f, 0.95f, 0.85f));

            if (!string.IsNullOrEmpty(entry.Check))
                NewText("Check", body, $"<color=#FFD479>확인 포인트</color>\n{entry.Check}", 18f, new Color(0.95f, 0.9f, 0.78f));

            statusText = NewText("Status", body, "", 19f, new Color(1f, 0.8f, 0.55f));
            statusText.gameObject.SetActive(false);

            // 개념 설명과 요청 예문은 길어서 F2 로 접고 펴는 '자세히' 블록에 둔다
            details = NewRect("Details", body);
            var dl = details.gameObject.AddComponent<VerticalLayoutGroup>();
            dl.spacing = 6f;
            dl.childControlWidth = dl.childControlHeight = true;
            dl.childForceExpandWidth = true;
            dl.childForceExpandHeight = false;

            if (!string.IsNullOrEmpty(entry.Id))
            {
                if (!string.IsNullOrEmpty(entry.Concept))
                    NewText("Concept", details, $"<color=#C8A8FF>개념</color>\n{entry.Concept}", 18f, new Color(0.88f, 0.85f, 0.95f));
                var prompt = NewText("Prompt", details, $"<color=#7FB2FF>요청 예문</color>  {entry.Prompt}", 18f, new Color(0.82f, 0.86f, 0.95f));
                prompt.fontStyle = FontStyles.Italic;
            }
            details.gameObject.SetActive(detailsOpen);

            hintText = NewText("Hint", body, HintText, 15f, new Color(0.5f, 0.53f, 0.6f));
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Color color)
        {
            var rt = NewRect(name, parent);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.font = Font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.richText = true;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static TextMeshProUGUI NewButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            var rt = NewRect(label + "Button", parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0.25f, 0.3f, 0.42f, 1f);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            rt.gameObject.AddComponent<LayoutElement>().preferredWidth = 76f;

            var txt = NewText("Label", rt, label, 20f, Color.white);
            txt.alignment = TextAlignmentOptions.Center;
            var trt = txt.rectTransform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            return txt;
        }
    }
}
