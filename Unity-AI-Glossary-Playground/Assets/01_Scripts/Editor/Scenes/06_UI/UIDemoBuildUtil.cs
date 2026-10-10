using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;

// 06_UI / 11_Patterns 빌더에서 같이 쓰는 UI 헬퍼 (GlossaryKit 위에 얹은 작은 조립 함수들)
public static class UIDemoBuildUtil
{
    public static readonly Color DarkPanel = new Color(0.09f, 0.1f, 0.13f, 0.94f);
    public static readonly Color TitleColor = new Color(1f, 0.86f, 0.4f);
    public static readonly Color GoodColor = new Color32(90, 200, 120, 255);
    public static readonly Color WarnColor = new Color32(240, 170, 60, 255);

    // 둥근 모서리 패널
    public static Image Panel(Transform parent, string name, Vector2 pos, Vector2 size, Color? color = null)
        => CreateImage(name, parent, pos, size, color ?? PanelColor, RoundedSprite, true);

    // 왼쪽 위 정렬 본문 텍스트 (rich text)
    public static TextMeshProUGUI Body(Transform parent, string name, Vector2 pos, Vector2 size, string text, float fontSize = 22f,
        TextAlignmentOptions align = TextAlignmentOptions.TopLeft, Color? color = null)
    {
        var t = CreateText(name, parent, pos, size, text, fontSize, color ?? TextColor, align);
        t.richText = true;
        return t;
    }

    // 제목 + 본문으로 된 설명 상자. 본문 텍스트를 돌려줌 (런타임에서 바꿀 수 있게)
    public static TextMeshProUGUI InfoBox(Transform parent, string name, Vector2 pos, Vector2 size, string title, string body, float fontSize = 20f)
    {
        var panel = Panel(parent, name, pos, size, DarkPanel);
        var t = CreateText("Title", panel.transform, Vector2.zero, Vector2.zero, title, 25f, TitleColor, TextAlignmentOptions.TopLeft);
        Stretch(t.rectTransform);
        t.rectTransform.offsetMin = new Vector2(20f, 16f);
        t.rectTransform.offsetMax = new Vector2(-20f, -14f);
        t.fontStyle = FontStyles.Bold;
        var b = CreateText("Body", panel.transform, Vector2.zero, Vector2.zero, body, fontSize, TextColor, TextAlignmentOptions.TopLeft);
        Stretch(b.rectTransform);
        b.rectTransform.offsetMin = new Vector2(20f, 16f);
        b.rectTransform.offsetMax = new Vector2(-20f, -54f);
        b.richText = true;
        return b;
    }

    // Image.Type.Filled 이미지 (체력바, 쿨타임 등)
    public static Image Filled(Transform parent, string name, Vector2 pos, Vector2 size, Color color, Image.FillMethod method, int origin = 0)
    {
        var img = CreateImage(name, parent, pos, size, color, WhiteSprite);
        img.type = Image.Type.Filled;
        img.fillMethod = method;
        img.fillOrigin = origin;
        img.fillAmount = 1f;
        return img;
    }

    // 버튼의 라벨 TMP 를 찾아 줌 (런타임에서 "ON/OFF" 표시를 바꿀 때 연결용)
    public static TextMeshProUGUI LabelOf(Button button) => button.GetComponentInChildren<TextMeshProUGUI>();

    // 가로 버튼 줄을 원하는 위치(가운데 기준)에 배치. CreateButtonRow 와 달리 너비·앵커를 지정할 수 있음
    public static Button[] ButtonRow(Transform parent, Vector2 center, float width, float height, float fontSize, params string[] labels)
    {
        const float spacing = 14f;
        float total = labels.Length * width + (labels.Length - 1) * spacing;
        var buttons = new Button[labels.Length];
        for (int i = 0; i < labels.Length; i++)
        {
            float x = center.x - total * 0.5f + width * 0.5f + i * (width + spacing);
            buttons[i] = CreateButton($"Button_{labels[i]}", parent, new Vector2(x, center.y), new Vector2(width, height), labels[i], null, fontSize);
        }
        return buttons;
    }

    // 세로 버튼 열
    public static Button[] ButtonColumn(Transform parent, Vector2 top, float width, float height, float fontSize, params string[] labels)
    {
        const float spacing = 12f;
        var buttons = new Button[labels.Length];
        for (int i = 0; i < labels.Length; i++)
            buttons[i] = CreateButton($"Button_{labels[i]}", parent, new Vector2(top.x, top.y - i * (height + spacing)), new Vector2(width, height), labels[i], null, fontSize);
        return buttons;
    }

    // 사각형 테두리 (4개의 얇은 Image). 시뮬레이션 화면·세이프 에어리어 경계 표시용
    public static void Outline(RectTransform target, Color color, float thickness = 4f)
    {
        void Edge(string n, Vector2 aMin, Vector2 aMax, Vector2 size)
        {
            var img = CreateImage(n, target, Vector2.zero, size, color);
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
        }
        Edge("Edge_Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, thickness));
        Edge("Edge_Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, thickness));
        Edge("Edge_Left", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(thickness, 0f));
        Edge("Edge_Right", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(thickness, 0f));
    }

    // 앵커/피벗을 한 번에 지정
    public static void Anchor(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
    }
}
