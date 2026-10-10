using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0605_LayoutGroup
{
    [GlossaryScene("0605")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0605_LayoutGroup")]
    public static void Build()
    {
        var ctx = NewSceneUI("0605",
            "+ / - : 칸 추가·제거 (자동으로 다음 자리에 배치)\nC : 4열 ↔ 5열   S : 간격 변경\n" +
            "L : Grid Layout Group 끄기 → 칸을 추가하면 한 자리에 겹침 (비교)");
        var root = ctx.Canvas.transform;

        // ── Grid Layout Group 인벤토리 ──
        var title = Body(root, "GridTitle", new Vector2(-20f, 345f), new Vector2(620f, 40f), "Grid Layout Group : ON", 26f, TextAlignmentOptions.Center, TitleColor);
        var gridPanel = Panel(root, "InventoryGrid", new Vector2(-20f, -10f), new Vector2(620f, 640f), new Color32(36, 40, 52, 255));
        var grid = gridPanel.gameObject.AddComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(16, 16, 16, 16);
        grid.cellSize = new Vector2(100f, 100f);
        grid.spacing = new Vector2(6f, 6f);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        // 칸 템플릿 (비활성 자식은 Layout Group 계산에서 빠짐)
        var template = Panel(gridPanel.transform, "SlotTemplate", Vector2.zero, new Vector2(100f, 100f), new Color32(58, 64, 82, 255));
        var icon = CreateImage("Icon", template.transform, Vector2.zero, Vector2.zero, Color.white, RoundedSprite, true);
        Stretch(icon.rectTransform, 18f);
        var count = CreateText("Count", template.transform, Vector2.zero, Vector2.zero, "", 20f, Color.white, TextAlignmentOptions.BottomRight);
        Stretch(count.rectTransform, 6f);
        count.fontStyle = FontStyles.Bold;

        // ── Horizontal Layout Group 단축바 ──
        Body(root, "HotbarTitle", new Vector2(600f, -225f), new Vector2(480f, 34f), "Horizontal Layout Group (단축바)", 22f, TextAlignmentOptions.Center, TitleColor);
        var hotbar = Panel(root, "Hotbar", new Vector2(600f, -300f), new Vector2(480f, 110f), new Color32(36, 40, 52, 255));
        var h = hotbar.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(12, 12, 12, 12);
        h.spacing = 10f;
        h.childControlWidth = h.childControlHeight = true;
        h.childForceExpandWidth = h.childForceExpandHeight = true;
        for (int i = 0; i < 5; i++)
        {
            var s = Panel(hotbar.transform, $"Hot_{i + 1}", Vector2.zero, Vector2.zero, new Color32(58, 64, 82, 255));
            CreateText("Key", s.transform, Vector2.zero, new Vector2(60f, 60f), (i + 1).ToString(), 30f, SubTextColor);
        }

        // ── Vertical Layout Group 목록 ──
        Body(root, "ListTitle", new Vector2(600f, 375f), new Vector2(480f, 34f), "Vertical Layout Group (퀘스트 목록)", 22f, TextAlignmentOptions.Center, TitleColor);
        var list = Panel(root, "QuestList", new Vector2(600f, 140f), new Vector2(420f, 420f), new Color32(36, 40, 52, 255));
        var v = list.gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(14, 14, 14, 14);
        v.spacing = 10f;
        v.childControlWidth = v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = true;
        string[] quests = { "슬라임 10마리 처치", "약초 5개 모으기", "대장장이와 대화", "동굴 입구 찾기", "보스 처치" };
        foreach (var q in quests)
        {
            var row = Panel(list.transform, $"Quest_{q}", Vector2.zero, Vector2.zero, new Color32(58, 64, 82, 255));
            var t = CreateText("Text", row.transform, Vector2.zero, Vector2.zero, "  • " + q, 22f, TextColor, TextAlignmentOptions.Left);
            Stretch(t.rectTransform, 8f);
        }

        var buttons = ButtonRow(root, new Vector2(0f, -490f), 220f, 64f, 22f, "칸 추가 (+)", "칸 제거 (-)", "4열↔5열 (C)", "간격 (S)", "Layout ON/OFF (L)");

        var demo = new GameObject("InventoryGridDemo").AddComponent<InventoryGridDemo>();
        Set(demo, "grid", grid);
        Set(demo, "slotTemplate", template.gameObject);
        Set(demo, "layoutStateLabel", title);
        OnClick(buttons[0], demo.AddSlot);
        OnClick(buttons[1], demo.RemoveSlot);
        OnClick(buttons[2], demo.ToggleColumns);
        OnClick(buttons[3], demo.ToggleSpacing);
        OnClick(buttons[4], demo.ToggleLayout);

        Save(ctx);
    }
}
