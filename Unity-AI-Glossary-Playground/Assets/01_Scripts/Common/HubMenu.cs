using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GlossaryDemo
{
    // 허브 씬: 분야별 탭 + 용어 목록. 항목을 누르면 해당 데모 씬으로 이동
    // UI 는 런타임에 카탈로그를 읽어 코드로 생성
    // 오른쪽 위 [강의 모드로 시작] = 고른 분야의 첫 용어부터 담당자가 설명 (강의 모드가 켜져 있으면 용어를 눌러도 그 용어부터 강의)
    // 강의 플레이어 빌드에서는 허브에서 Esc 로 프로그램을 끔 (데모 씬은 Esc 를 일시정지 등에 쓰므로 허브에서만)
    public class HubMenu : MonoBehaviour
    {
        private static int lastCategory = 1;

        // 허브 아래쪽에 한 번 띄울 알림 (예: 명령줄로 없는 용어 번호를 줬을 때, LectureLaunch)
        public static string Notice;

        private Transform canvasRoot;
        private RectTransform listContent;
        private ScrollRect scroll;
        private Image[] tabImages;
        private TextMeshProUGUI subtitle;
        private TextMeshProUGUI lectureToggleLabel;
        private Image lectureToggleImage;
        private TextMeshProUGUI presenterText;

        private static readonly Color Bg = new Color32(20, 22, 28, 255);
        private static readonly Color Card = new Color32(40, 44, 56, 255);
        private static readonly Color Accent = new Color32(88, 140, 255, 255);

        private void Start()
        {
            Time.timeScale = 1f;
            Build();
            ShowNotice();
            ShowCategory(lastCategory);
        }

        private void Update()
        {
            if (Keyboard.current == null || LectureMode.TypingInInputField) return;
            if (Keyboard.current[LectureMode.ToggleKey].wasPressedThisFrame) ToggleLecture();
            if (LectureLaunch.IsLecturePlayer && Keyboard.current[Key.Escape].wasPressedThisFrame) Application.Quit();
        }

        // 화면 아래쪽에 알림 띠를 한 줄 띄우고 비움 (다시 허브로 돌아오면 사라짐)
        private void ShowNotice()
        {
            if (string.IsNullOrEmpty(Notice)) return;
            var bar = Rect("Notice", canvasRoot);
            Place(bar, new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(1400f, 56f));
            bar.gameObject.AddComponent<Image>().color = new Color32(150, 60, 40, 235);
            var text = Text("Label", bar, Notice, 24f, Color.white);
            Stretch(text.rectTransform);
            text.alignment = TextAlignmentOptions.Center;
            Notice = null;
        }

        private void ToggleLecture()
        {
            LectureMode.SetEnabled(!LectureMode.Enabled);
            RefreshLecture();
        }

        // 고른 분야의 첫 용어부터 강의 시작
        private void StartLecture()
        {
            if (LectureMode.TryFirstOf(lastCategory, out var first)) LectureMode.StartAt(first);
        }

        private void OpenEntry(GlossaryEntry entry)
        {
            if (LectureMode.Enabled) LectureMode.StartAt(entry); // 강의 모드면 고른 용어부터 강의
            else SceneManager.LoadScene(entry.Scene);
        }

        private void RefreshLecture()
        {
            bool on = LectureMode.Enabled;
            lectureToggleLabel.text = on ? "강의 모드 켜짐 (F4)" : "강의 모드 꺼짐 (F4)";
            lectureToggleImage.color = on ? Accent : Card;
            subtitle.text = on
                ? "강의 모드: 디렉터 인사 뒤 분야 담당자가 설명합니다. [강의 모드로 시작] = 분야 첫 용어부터, 용어를 누르면 그 용어부터."
                : $"{GlossaryCatalog.Entries.Length}개 용어를 각각 씬으로 구현했습니다. 왼쪽에서 분야를 고르고 용어를 누르세요.";

            var p = LecturePresenters.For(lastCategory);
            presenterText.text = $"이 분야 담당  <color=#FFFFFF><b>{p.Role}</b></color>  ({LectureMode.CountInCategory(lastCategory)}개 용어)";
        }

        private void Build()
        {
            var canvasGo = new GameObject("HubCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasRoot = canvasGo.transform;
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var bg = Rect("Background", canvasGo.transform);
            Stretch(bg);
            bg.gameObject.AddComponent<Image>().color = Bg;

            var title = Text("Title", canvasGo.transform, "유니티 AI 요청 용어집 · 예제 씬", 48f, Color.white);
            title.fontStyle = FontStyles.Bold;
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(60f, -40f), new Vector2(1100f, 70f));
            subtitle = Text("Subtitle", canvasGo.transform, "", 24f, new Color(0.65f, 0.7f, 0.8f));
            Place(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(60f, -110f), new Vector2(1200f, 40f));
            subtitle.enableAutoSizing = true;
            subtitle.fontSizeMin = 16f;
            subtitle.fontSizeMax = 24f;

            // 오른쪽 위: 강의 모드 켜기/끄기 + 시작 버튼 + 이 분야 담당자
            var toggle = Button(canvasGo.transform, "", 22f, ToggleLecture);
            Place((RectTransform)toggle.transform, new Vector2(1f, 1f), new Vector2(-460f, -40f), new Vector2(260f, 60f));
            lectureToggleImage = toggle.GetComponent<Image>();
            lectureToggleLabel = toggle.GetComponentInChildren<TextMeshProUGUI>();
            lectureToggleLabel.alignment = TextAlignmentOptions.Center;

            var start = Button(canvasGo.transform, "<b>▶ 강의 모드로 시작</b>", 24f, StartLecture);
            Place((RectTransform)start.transform, new Vector2(1f, 1f), new Vector2(-60f, -40f), new Vector2(380f, 60f));
            start.GetComponent<Image>().color = new Color32(64, 170, 110, 255);
            start.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            presenterText = Text("Presenter", canvasGo.transform, "", 20f, new Color(0.65f, 0.7f, 0.8f));
            presenterText.alignment = TextAlignmentOptions.Right;
            presenterText.textWrappingMode = TextWrappingModes.NoWrap;
            Place(presenterText.rectTransform, new Vector2(1f, 1f), new Vector2(-60f, -110f), new Vector2(600f, 40f));

            // 왼쪽 분야 탭
            var tabs = Rect("Tabs", canvasGo.transform);
            tabs.anchorMin = new Vector2(0f, 0f);
            tabs.anchorMax = new Vector2(0f, 1f);
            tabs.pivot = new Vector2(0f, 1f);
            tabs.anchoredPosition = new Vector2(60f, -170f);
            tabs.sizeDelta = new Vector2(420f, -200f);
            var tl = tabs.gameObject.AddComponent<VerticalLayoutGroup>();
            tl.spacing = 8f;
            tl.childControlWidth = tl.childControlHeight = true;
            tl.childForceExpandHeight = false;

            tabImages = new Image[GlossaryCatalog.Categories.Length];
            for (int i = 0; i < GlossaryCatalog.Categories.Length; i++)
            {
                int cat = i + 1;
                int count = 0;
                foreach (var e in GlossaryCatalog.Entries) if (e.Category == cat) count++;
                var btn = Button(tabs, $"{cat:D2}  {GlossaryCatalog.Categories[i]}  <color=#8890A0>({count})</color>", 24f, () => ShowCategory(cat));
                btn.GetComponent<LayoutElement>().preferredHeight = 62f;
                btn.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
                btn.GetComponentInChildren<TextMeshProUGUI>().margin = new Vector4(20f, 0f, 0f, 0f);
                tabImages[i] = btn.GetComponent<Image>();
            }

            // 오른쪽 목록 (Scroll View)
            var scrollRt = Rect("List", canvasGo.transform);
            scrollRt.anchorMin = new Vector2(0f, 0f);
            scrollRt.anchorMax = new Vector2(1f, 1f);
            scrollRt.offsetMin = new Vector2(510f, 40f);
            scrollRt.offsetMax = new Vector2(-60f, -170f);
            scroll = scrollRt.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.scrollSensitivity = 40f;
            scrollRt.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.15f);
            scrollRt.gameObject.AddComponent<RectMask2D>();

            listContent = Rect("Content", scrollRt);
            listContent.anchorMin = new Vector2(0f, 1f);
            listContent.anchorMax = new Vector2(1f, 1f);
            listContent.pivot = new Vector2(0.5f, 1f);
            listContent.sizeDelta = Vector2.zero;
            var vl = listContent.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(16, 16, 16, 16);
            vl.spacing = 10f;
            vl.childControlWidth = vl.childControlHeight = true;
            vl.childForceExpandHeight = false;
            listContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = listContent;
            scroll.viewport = scrollRt;
        }

        private void ShowCategory(int cat)
        {
            lastCategory = cat;
            RefreshLecture();
            for (int i = 0; i < tabImages.Length; i++) tabImages[i].color = i + 1 == cat ? Accent : Card;
            for (int i = listContent.childCount - 1; i >= 0; i--) Destroy(listContent.GetChild(i).gameObject);

            string currentSub = null;
            foreach (var e in GlossaryCatalog.Entries)
            {
                if (e.Category != cat) continue;
                if (!string.IsNullOrEmpty(e.Sub) && e.Sub != currentSub)
                {
                    currentSub = e.Sub;
                    var header = Text("Sub", listContent, e.Sub, 24f, new Color(1f, 0.85f, 0.45f));
                    header.fontStyle = FontStyles.Bold;
                    header.gameObject.AddComponent<LayoutElement>().preferredHeight = 40f;
                }

                var entry = e;
                bool available = Application.CanStreamedLevelBeLoaded(entry.Scene);
                string label = $"<b>{entry.Id}  {entry.Term}</b>\n<size=20><color=#A8B0C0>{entry.Summary}</color></size>";
                var btn = Button(listContent, label, 26f, () => OpenEntry(entry));
                btn.GetComponent<LayoutElement>().preferredHeight = 86f;
                var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                txt.alignment = TextAlignmentOptions.MidlineLeft;
                txt.margin = new Vector4(24f, 0f, 24f, 0f);
                btn.interactable = available;
                if (!available) txt.text += "  <size=18><color=#E07070>(씬 없음)</color></size>";
            }
            scroll.verticalNormalizedPosition = 1f;
        }

        // ───── UI 헬퍼 ─────

        private static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color)
        {
            var rt = Rect(name, parent);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.font = GlossaryInfoPanel.Font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button Button(Transform parent, string label, float size, UnityEngine.Events.UnityAction onClick)
        {
            var rt = Rect("Item", parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = Card;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            rt.gameObject.AddComponent<LayoutElement>();
            var txt = Text("Label", rt, label, size, Color.white);
            Stretch(txt.rectTransform);
            return btn;
        }
    }
}
