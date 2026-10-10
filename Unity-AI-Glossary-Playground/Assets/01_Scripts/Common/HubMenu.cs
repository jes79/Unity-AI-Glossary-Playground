using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GlossaryDemo
{
    // 허브 씬: 분야별 탭 + 용어 목록. 항목을 누르면 해당 데모 씬으로 이동
    // UI 는 런타임에 카탈로그를 읽어 코드로 생성
    public class HubMenu : MonoBehaviour
    {
        private static int lastCategory = 1;

        private RectTransform listContent;
        private ScrollRect scroll;
        private Image[] tabImages;

        private static readonly Color Bg = new Color32(20, 22, 28, 255);
        private static readonly Color Card = new Color32(40, 44, 56, 255);
        private static readonly Color Accent = new Color32(88, 140, 255, 255);

        private void Start()
        {
            Time.timeScale = 1f;
            Build();
            ShowCategory(lastCategory);
        }

        private void Build()
        {
            var canvasGo = new GameObject("HubCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(60f, -40f), new Vector2(1400f, 70f));
            var sub = Text("Subtitle", canvasGo.transform, $"{GlossaryCatalog.Entries.Length}개 용어를 각각 씬으로 구현했습니다. 왼쪽에서 분야를 고르고 용어를 누르세요.", 24f, new Color(0.65f, 0.7f, 0.8f));
            Place(sub.rectTransform, new Vector2(0f, 1f), new Vector2(60f, -110f), new Vector2(1600f, 40f));

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
                var btn = Button(listContent, label, 26f, () => SceneManager.LoadScene(entry.Scene));
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
