using System.Collections.Generic;
using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0613_DragAndDrop
{
    [GlossaryScene("0613")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0613_DragAndDrop")]
    public static void Build()
    {
        var ctx = NewSceneUI("0613",
            "아이템을 마우스로 끌어서 다른 칸에 놓기 → 차 있는 칸이면 서로 교환\n칸 밖에 놓으면 원래 칸으로 돌아감\n" +
            "S : 교환 모드 ↔ 빈 칸에만 놓기");
        var root = ctx.Canvas.transform;

        Body(root, "BagTitle", new Vector2(80f, 330f), new Vector2(640f, 40f), "가방 (Grid Layout Group + IDropHandler)", 24f, TextAlignmentOptions.Center, TitleColor);
        var bag = Panel(root, "Bag", new Vector2(80f, 20f), new Vector2(640f, 540f), new Color32(36, 40, 52, 255));
        var grid = bag.gameObject.AddComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.cellSize = new Vector2(110f, 110f);
        grid.spacing = new Vector2(10f, 10f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;
        grid.childAlignment = TextAnchor.MiddleCenter;

        var slots = new List<InventorySlot>();
        for (int i = 0; i < 20; i++) slots.Add(CreateSlot(bag.transform, $"Bag_{i + 1:00}", Vector2.zero));

        // 장비 칸 (Layout Group 없이 직접 배치해도 같은 InventorySlot 으로 동작)
        Body(root, "EquipTitle", new Vector2(640f, 330f), new Vector2(360f, 40f), "장비 칸", 24f, TextAlignmentOptions.Center, TitleColor);
        string[] equipNames = { "Equip_무기", "Equip_방어구", "Equip_장신구" };
        for (int i = 0; i < 3; i++)
        {
            var s = CreateSlot(root, equipNames[i], new Vector2(640f, 220f - i * 140f));
            CreateText("Hint", s.transform, new Vector2(0f, -66f), new Vector2(160f, 24f), equipNames[i].Substring(6), 18f, SubTextColor);
            slots.Add(s);
        }

        // 아이템
        string[] names = { "검", "방패", "물약", "반지", "활", "투구", "열쇠", "보석" };
        Color[] colors =
        {
            new Color(0.75f, 0.8f, 0.9f), new Color(0.55f, 0.4f, 0.25f), new Color(0.9f, 0.3f, 0.35f), new Color(1f, 0.82f, 0.3f),
            new Color(0.5f, 0.75f, 0.4f), new Color(0.6f, 0.6f, 0.7f), new Color(0.95f, 0.7f, 0.2f), new Color(0.5f, 0.85f, 1f),
        };
        int[] startSlots = { 0, 1, 2, 6, 7, 12, 15, 21 };
        var items = new DragItem[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            var img = CreateImage($"Item_{names[i]}", slots[startSlots[i]].transform, Vector2.zero, new Vector2(90f, 90f), colors[i], RoundedSprite, true);
            img.gameObject.AddComponent<CanvasGroup>();
            CreateText("Name", img.transform, Vector2.zero, new Vector2(90f, 90f), names[i], 28f, new Color(0.1f, 0.1f, 0.15f)).fontStyle = FontStyles.Bold;
            items[i] = img.gameObject.AddComponent<DragItem>();
            SetString(items[i], "itemName", names[i]);
        }

        var modeBtn = CreateButtonRow(root, 60f, "모드: 슬롯끼리 교환 (S)")[0];

        var demo = new GameObject("DragDropDemo").AddComponent<DragDropDemo>();
        SetArray(demo, "slots", slots.ToArray());
        SetArray(demo, "items", items);
        var so = new SerializedObject(demo);
        var sp = so.FindProperty("startSlots");
        sp.arraySize = startSlots.Length;
        for (int i = 0; i < startSlots.Length; i++) sp.GetArrayElementAtIndex(i).intValue = startSlots[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        Set(demo, "modeLabel", LabelOf(modeBtn));
        OnClick(modeBtn, demo.ToggleSwap);

        Save(ctx);
    }

    private static InventorySlot CreateSlot(Transform parent, string name, Vector2 pos)
    {
        var bg = Panel(parent, name, pos, new Vector2(110f, 110f), new Color32(58, 64, 82, 255));
        var slot = bg.gameObject.AddComponent<InventorySlot>();
        Set(slot, "background", bg);
        return slot;
    }
}
