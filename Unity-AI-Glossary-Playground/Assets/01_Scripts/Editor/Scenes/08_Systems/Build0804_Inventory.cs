using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0804_Inventory
{
    [GlossaryScene("0804")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0804_Inventory")]
    public static void Build()
    {
        var items = EnsureItems();
        var ctx = NewSceneUI("0804", "위 버튼 : 아이템 넣기   슬롯 클릭 : 1개 사용/버림\n같은 아이템은 한 칸에 최대 99개까지 겹침 → 넘치면 다음 칸\n장비(검)는 겹치지 않아 1칸에 1개\n20칸이 꽉 차면 넣지 못한 개수를 알려 줌");
        var root = ctx.Canvas.transform;
        var demo = new GameObject("InventoryDemo").AddComponent<InventoryDemo>();

        // 추가 버튼
        var addItems = new[] { items["potion_hp"], items["slime_jelly"], items["sword_steel"], items["gem"] };
        int[] amounts = { 10, 37, 1, 99 };
        string[] labels = { "체력 포션 +10", "슬라임 젤리 +37", "강철 검 +1", "보석 +99" };
        for (int i = 0; i < addItems.Length; i++)
        {
            var b = SmallButton(root, $"Add_{i}", new Vector2(CenterX - 390f + i * 210f, 400f), new Vector2(200f, 60f), labels[i]);
            OnClick(b, demo.AddByButton, i);
        }
        OnClick(SmallButton(root, "AddRandom", new Vector2(CenterX + 450f, 400f), new Vector2(160f, 60f), "랜덤", new Color32(150, 100, 220, 255)), demo.AddRandom);

        // 슬롯 5 x 4
        const int cols = 5, rows = 4;
        const float size = 120f, gap = 12f;
        var bg = Panel(root, "InventoryPanel", new Vector2(CenterX, -20f), new Vector2(cols * (size + gap) + 40f, rows * (size + gap) + 90f), new Color32(30, 33, 42, 255));
        Label(bg.transform, "Title", new Vector2(0f, rows * (size + gap) * 0.5f + 20f), new Vector2(600f, 40f), "인벤토리 (20칸)", 26f, TextAlignmentOptions.Center);

        var icons = new Image[cols * rows];
        var counts = new TextMeshProUGUI[cols * rows];
        var frames = new Image[cols * rows];
        for (int i = 0; i < cols * rows; i++)
        {
            int cx = i % cols, cy = i / cols;
            var pos = new Vector2((cx - (cols - 1) * 0.5f) * (size + gap), ((rows - 1) * 0.5f - cy) * (size + gap) - 20f);
            var frame = CreateImage($"Slot_{i}", bg.transform, pos, new Vector2(size, size), new Color32(38, 41, 52, 255), RoundedSprite, true);
            var btn = frame.gameObject.AddComponent<Button>();
            btn.targetGraphic = frame;
            OnClick(btn, demo.ClickSlot, i);
            var icon = CreateImage("Icon", frame.transform, new Vector2(0f, 4f), new Vector2(84f, 84f), Color.white);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.enabled = false;
            Label(frame.transform, "Index", new Vector2(-38f, 44f), new Vector2(40f, 24f), (i + 1).ToString(), 15f, TextAlignmentOptions.Left, SubTextColor);
            var count = Label(frame.transform, "Count", new Vector2(10f, -40f), new Vector2(90f, 32f), "", 24f, TextAlignmentOptions.Right);
            count.fontStyle = FontStyles.Bold;
            icons[i] = icon;
            counts[i] = count;
            frames[i] = frame;
        }

        OnClick(SmallButton(root, "Sort", new Vector2(CenterX - 130f, -440f), new Vector2(240f, 60f), "정렬"), demo.Sort);
        OnClick(SmallButton(root, "Clear", new Vector2(CenterX + 130f, -440f), new Vector2(240f, 60f), "비우기", EnemyColor), demo.Clear);
        var detail = Label(root, "Detail", new Vector2(CenterX, -500f), new Vector2(1100f, 40f), "", 22f, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.4f));

        SetArray(demo, "addableItems", addItems);
        var so = new SerializedObject(demo);
        var p = so.FindProperty("addAmounts");
        p.arraySize = amounts.Length;
        for (int i = 0; i < amounts.Length; i++) p.GetArrayElementAtIndex(i).intValue = amounts[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        SetArray(demo, "slotIcons", icons);
        SetArray(demo, "slotCounts", counts);
        SetArray(demo, "slotFrames", frames);
        Set(demo, "detailText", detail);

        Save(ctx);
    }
}
