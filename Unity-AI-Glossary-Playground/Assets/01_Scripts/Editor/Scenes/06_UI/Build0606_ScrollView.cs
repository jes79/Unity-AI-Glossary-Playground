using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0606_ScrollView
{
    [GlossaryScene("0606")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0606_ScrollView")]
    public static void Build()
    {
        var ctx = NewSceneUI("0606",
            "마우스 휠 / 드래그 / 스크롤바 : 목록 스크롤\nA : 아이템 10개 추가   C : 비우기   T / B : 맨 위 / 맨 아래\n" +
            "M : Movement Type (Elastic ↔ Clamped)   구매 버튼으로 골드 차감");
        var root = ctx.Canvas.transform;

        var gold = Body(root, "GoldText", new Vector2(180f, 420f), new Vector2(700f, 44f), "보유 골드", 30f, TextAlignmentOptions.Center, TitleColor);
        gold.fontStyle = FontStyles.Bold;

        // ── Scroll View: ScrollRect + Viewport(RectMask2D) + Content(Layout) + Scrollbar ──
        var view = Panel(root, "Scroll View", new Vector2(180f, -20f), new Vector2(700f, 800f), new Color32(32, 36, 47, 255));
        var scroll = view.gameObject.AddComponent<ScrollRect>();

        var viewport = CreateRect("Viewport", view.transform, Vector2.zero, Vector2.zero);
        Stretch(viewport);
        viewport.offsetMin = new Vector2(8f, 8f);
        viewport.offsetMax = new Vector2(-34f, -8f);
        viewport.gameObject.AddComponent<RectMask2D>();

        var content = CreateRect("Content", viewport, Vector2.zero, Vector2.zero);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.spacing = 8f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 항목 템플릿 (프리팹 역할, 비활성)
        var item = Panel(content, "ItemTemplate", Vector2.zero, new Vector2(600f, 100f), new Color32(52, 58, 76, 255));
        item.gameObject.AddComponent<LayoutElement>().preferredHeight = 100f;
        var icon = CreateImage("Icon", item.transform, Vector2.zero, new Vector2(72f, 72f), Color.white, RoundedSprite, true);
        Anchor(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f));
        var name = CreateText("Name", item.transform, Vector2.zero, new Vector2(330f, 40f), "아이템", 26f, Color.white, TextAlignmentOptions.Left);
        Anchor(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(104f, 16f));
        name.richText = true;
        var price = CreateText("Price", item.transform, Vector2.zero, new Vector2(330f, 34f), "0 G", 22f, new Color(1f, 0.85f, 0.4f), TextAlignmentOptions.Left);
        Anchor(price.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(104f, -20f));
        var buy = CreateButton("Buy", item.transform, Vector2.zero, new Vector2(150f, 60f), "구매", new Color32(60, 150, 90, 255), 22f);
        Anchor((RectTransform)buy.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16f, 0f));

        // 세로 스크롤바
        var barBg = CreateImage("Scrollbar Vertical", view.transform, Vector2.zero, Vector2.zero, new Color32(24, 27, 35, 255), RoundedSprite, true);
        var barRt = barBg.rectTransform;
        barRt.anchorMin = new Vector2(1f, 0f);
        barRt.anchorMax = new Vector2(1f, 1f);
        barRt.pivot = new Vector2(1f, 0.5f);
        barRt.sizeDelta = new Vector2(20f, -16f);
        barRt.anchoredPosition = new Vector2(-8f, 0f);
        var slideArea = CreateRect("Sliding Area", barRt, Vector2.zero, Vector2.zero);
        Stretch(slideArea);
        var handle = CreateImage("Handle", slideArea, Vector2.zero, Vector2.zero, new Color32(120, 130, 160, 255), RoundedSprite, true);
        Stretch(handle.rectTransform);
        var scrollbar = barBg.gameObject.AddComponent<Scrollbar>();
        scrollbar.handleRect = handle.rectTransform;
        scrollbar.targetGraphic = handle;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;

        scroll.content = content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.scrollSensitivity = 30f;
        scroll.verticalScrollbar = scrollbar;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

        InfoBox(root, "StructureInfo", new Vector2(760f, 40f), new Vector2(360f, 620f), "Scroll View 구조",
            "<b>Scroll View</b> (ScrollRect)\n" +
            " └ <b>Viewport</b> (RectMask2D: 밖은 잘라냄)\n" +
            "    └ <b>Content</b>\n" +
            "       · Vertical Layout Group\n" +
            "       · Content Size Fitter\n" +
            "         (Vertical = Preferred)\n" +
            "       └ 항목들 (Instantiate)\n" +
            " └ Scrollbar Vertical\n\n" +
            "<color=#9BE89B>동적 생성 순서</color>\n" +
            "1. 항목 프리팹(템플릿) 준비\n" +
            "2. 데이터 목록을 돌며\n   Instantiate(prefab, content)\n" +
            "3. 텍스트·아이콘 채우고\n   버튼에 AddListener\n\n" +
            "<color=#FFB74D>항목이 수백 개면</color> 화면에 보이는 것만 재사용하는 풀링(가상 스크롤)을 고려", 19f);

        var buttons = ButtonRow(root, new Vector2(0f, -490f), 200f, 60f, 21f, "10개 추가 (A)", "비우기 (C)", "맨 위 (T)", "맨 아래 (B)", "Elastic/Clamped (M)");

        var demo = new GameObject("ShopScrollDemo").AddComponent<ShopScrollDemo>();
        Set(demo, "scrollRect", scroll);
        Set(demo, "content", content);
        Set(demo, "itemTemplate", item.gameObject);
        Set(demo, "goldText", gold);
        OnClick(buttons[0], demo.AddTen);
        OnClick(buttons[1], demo.ClearAll);
        OnClick(buttons[2], demo.ScrollToTop);
        OnClick(buttons[3], demo.ScrollToBottom);
        OnClick(buttons[4], demo.ToggleMovementType);

        Save(ctx);
    }
}
