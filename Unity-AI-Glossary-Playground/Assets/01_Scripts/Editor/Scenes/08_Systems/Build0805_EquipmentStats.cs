using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0805_EquipmentStats
{
    [GlossaryScene("0805")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0805_EquipmentStats")]
    public static void Build()
    {
        var items = EnsureItems();
        var ctx = NewSceneUI("0805", "장비 목록 클릭 : 장착 / 다시 클릭하면 해제\n무기는 한 번에 1개만 (새 무기를 끼면 이전 무기 보정이 교체됨)\n[허수아비 공격] 으로 공격력 변화가 데미지에 반영되는지 확인\n능력치 = 기본값 + 장비 보정치");
        var root = ctx.Canvas.transform;
        var demo = new GameObject("EquipmentDemo").AddComponent<EquipmentDemo>();

        // 장비 목록 (왼쪽)
        var list = new[] { items["sword_wood"], items["sword_steel"], items["sword_legend"], items["armor_leather"], items["armor_plate"] };
        var listPanel = Panel(root, "EquipList", new Vector2(CenterX - 360f, 30f), new Vector2(460f, 640f), new Color32(30, 33, 42, 255));
        Label(listPanel.transform, "Title", new Vector2(0f, 285f), new Vector2(420f, 40f), "장비 목록", 26f, TextAlignmentOptions.Center);
        var highlights = new Image[list.Length];
        for (int i = 0; i < list.Length; i++)
        {
            var row = CreateImage($"Row_{list[i].id}", listPanel.transform, new Vector2(0f, 200f - i * 108f), new Vector2(420f, 96f), PanelColor, RoundedSprite, true);
            var btn = row.gameObject.AddComponent<Button>();
            btn.targetGraphic = row;
            OnClick(btn, demo.ToggleEquip, i);
            var icon = CreateImage("Icon", row.transform, new Vector2(-160f, 0f), new Vector2(72f, 72f), Color.white, list[i].icon);
            icon.raycastTarget = false;
            var name = Label(row.transform, "Name", new Vector2(40f, 16f), new Vector2(300f, 34f), list[i].itemName, 24f, TextAlignmentOptions.Left, list[i].RarityColor);
            name.fontStyle = FontStyles.Bold;
            Label(row.transform, "Effect", new Vector2(40f, -18f), new Vector2(300f, 30f), list[i].EffectText, 20f, TextAlignmentOptions.Left, new Color(0.5f, 0.95f, 0.55f));
            highlights[i] = row;
        }

        // 캐릭터 + 장비 슬롯 (가운데)
        var charPanel = Panel(root, "Character", new Vector2(CenterX + 120f, 130f), new Vector2(400f, 440f), new Color32(30, 33, 42, 255));
        var body = CreateImage("Body", charPanel.transform, new Vector2(0f, 10f), new Vector2(120f, 220f), PlayerColor, RoundedSprite, true);
        CreateImage("Head", charPanel.transform, new Vector2(0f, 150f), new Vector2(90f, 90f), new Color32(255, 220, 180, 255), KnobSprite);
        var weaponSlot = Panel(charPanel.transform, "WeaponSlot", new Vector2(-130f, -120f), new Vector2(100f, 100f), new Color32(20, 22, 28, 255));
        var armorSlot = Panel(charPanel.transform, "ArmorSlot", new Vector2(130f, -120f), new Vector2(100f, 100f), new Color32(20, 22, 28, 255));
        Label(weaponSlot.transform, "Cap", new Vector2(0f, 64f), new Vector2(120f, 26f), "무기", 18f, TextAlignmentOptions.Center, SubTextColor);
        Label(armorSlot.transform, "Cap", new Vector2(0f, 64f), new Vector2(120f, 26f), "방어구", 18f, TextAlignmentOptions.Center, SubTextColor);
        var wIcon = CreateImage("Icon", weaponSlot.transform, Vector2.zero, new Vector2(80f, 80f), Color.white);
        var aIcon = CreateImage("Icon", armorSlot.transform, Vector2.zero, new Vector2(80f, 80f), Color.white);
        wIcon.enabled = aIcon.enabled = false;
        body.raycastTarget = false;

        // 능력치 (오른쪽)
        var statPanel = Panel(root, "Stats", new Vector2(CenterX + 490f, 130f), new Vector2(300f, 440f), new Color32(30, 33, 42, 255));
        var stats = Label(statPanel.transform, "StatsText", Vector2.zero, new Vector2(270f, 400f), "", 24f, TextAlignmentOptions.TopLeft);

        OnClick(SmallButton(root, "AttackDummy", new Vector2(CenterX + 120f, -170f), new Vector2(300f, 70f), "허수아비 공격", EnemyColor), demo.AttackDummy);
        OnClick(SmallButton(root, "UnequipAll", new Vector2(CenterX + 470f, -170f), new Vector2(300f, 70f), "모두 해제"), demo.UnequipAll);
        var log = Label(root, "Log", new Vector2(CenterX + 300f, -280f), new Vector2(800f, 60f), "", 22f, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.4f));

        SetArray(demo, "equipables", list);
        SetArray(demo, "listHighlights", highlights);
        Set(demo, "statsText", stats);
        Set(demo, "logText", log);
        Set(demo, "weaponSlotIcon", wIcon);
        Set(demo, "armorSlotIcon", aIcon);

        Save(ctx);
    }
}
