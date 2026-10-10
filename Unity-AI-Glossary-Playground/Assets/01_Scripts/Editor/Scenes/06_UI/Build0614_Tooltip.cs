using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0614_Tooltip
{
    [GlossaryScene("0614")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0614_Tooltip")]
    public static void Build()
    {
        var ctx = NewSceneUI("0614",
            "아이템에 마우스를 0.3초 올려 두면 이름·설명 툴팁 표시\nF : 마우스 따라가기 ↔ 아이템 옆 고정\n" +
            "오른쪽 아래 구석 아이템 : 툴팁이 화면 밖으로 안 나가게 뒤집히는지 확인");
        var root = ctx.Canvas.transform;

        Color common = new Color(0.8f, 0.8f, 0.8f), rare = new Color(0.4f, 0.7f, 1f), epic = new Color(0.75f, 0.45f, 1f), legend = new Color(1f, 0.6f, 0.2f);
        var data = new (string name, string rarity, Color rc, Color icon, string desc)[]
        {
            ("낡은 검", "일반", common, new Color(0.6f, 0.62f, 0.68f), "공격력 +5\n오래되어 날이 무뎌진 검."),
            ("화염의 검", "희귀", rare, new Color(0.95f, 0.4f, 0.25f), "공격력 +18\n화염 피해 +5\n<color=#FF8A65>적중 시 10% 확률로 화상</color>"),
            ("체력 물약", "일반", common, new Color(0.9f, 0.25f, 0.3f), "HP 50 회복\n전투 중에도 사용할 수 있다."),
            ("엘릭서", "영웅", epic, new Color(0.7f, 0.4f, 0.95f), "HP·MP 완전 회복\n<color=#999>재사용 대기 60초</color>"),
            ("가죽 갑옷", "일반", common, new Color(0.55f, 0.38f, 0.25f), "방어력 +8"),
            ("용비늘 갑옷", "전설", legend, new Color(0.3f, 0.7f, 0.5f), "방어력 +45\n화염 저항 +30%\n<i>용을 쓰러뜨린 자만이 입을 수 있다.</i>"),
            ("귀환 주문서", "일반", common, new Color(0.9f, 0.85f, 0.6f), "마을로 즉시 돌아간다.\n시전 시간 3초 (피격 시 취소)"),
        };

        for (int i = 0; i < data.Length; i++)
        {
            int col = i % 4, row = i / 4;
            CreateItem(root, data[i], new Vector2(-10f + col * 180f, 160f - row * 220f));
        }
        // 화면 구석 아이템 (툴팁 뒤집기 확인용)
        CreateItem(root, ("수상한 열쇠", "???", new Color(1f, 0.4f, 0.4f), new Color(0.95f, 0.75f, 0.2f),
            "어딘가의 문을 열 수 있을 것 같다...\n화면 오른쪽 아래 구석에 있어서 툴팁이 왼쪽 위로 뒤집혀 나옵니다."), new Vector2(860f, -400f));

        var modeBtn = CreateButtonRow(root, 60f, "툴팁: 마우스 따라감 (F)")[0];

        // ── 툴팁 창 (Vertical Layout + Content Size Fitter 로 내용 길이에 맞춰 높이 자동) ──
        var panel = Panel(root, "Tooltip", Vector2.zero, new Vector2(380f, 100f), new Color(0.06f, 0.07f, 0.09f, 0.96f));
        var prt = panel.rectTransform;
        prt.pivot = new Vector2(0f, 1f);
        panel.raycastTarget = false;
        var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 14, 16);
        layout.spacing = 6f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var group = panel.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        var title = CreateText("Title", prt, Vector2.zero, Vector2.zero, "이름", 28f, Color.white, TextAlignmentOptions.Left);
        title.fontStyle = FontStyles.Bold;
        var rarity = CreateText("Rarity", prt, Vector2.zero, Vector2.zero, "등급", 20f, Color.white, TextAlignmentOptions.Left);
        var line = CreateImage("Line", prt, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.15f));
        line.gameObject.AddComponent<LayoutElement>().preferredHeight = 2f;
        line.raycastTarget = false;
        var desc = CreateText("Description", prt, Vector2.zero, Vector2.zero, "설명", 21f, TextColor, TextAlignmentOptions.TopLeft);
        desc.richText = true;

        var view = panel.gameObject.AddComponent<TooltipView>();
        Set(view, "canvasRect", (RectTransform)ctx.Canvas.transform);
        Set(view, "panel", prt);
        Set(view, "group", group);
        Set(view, "titleText", title);
        Set(view, "rarityText", rarity);
        Set(view, "descText", desc);
        Set(view, "modeLabel", LabelOf(modeBtn));
        OnClick(modeBtn, view.ToggleFollowMouse);

        Save(ctx);
    }

    private static void CreateItem(Transform root, (string name, string rarity, Color rc, Color icon, string desc) d, Vector2 pos)
    {
        var slot = Panel(root, $"Item_{d.name}", pos, new Vector2(140f, 140f), new Color32(58, 64, 82, 255));
        var icon = CreateImage("Icon", slot.transform, Vector2.zero, new Vector2(100f, 100f), d.icon, RoundedSprite, true);
        icon.raycastTarget = false;
        CreateText("Glyph", slot.transform, Vector2.zero, new Vector2(120f, 80f), d.name.Substring(0, 1), 44f, new Color(1f, 1f, 1f, 0.9f)).fontStyle = FontStyles.Bold;
        CreateText("Name", slot.transform, new Vector2(0f, -90f), new Vector2(180f, 30f), d.name, 20f, d.rc);
        var trigger = slot.gameObject.AddComponent<TooltipTrigger>();
        SetString(trigger, "itemName", d.name);
        SetString(trigger, "rarity", d.rarity);
        SetColor(trigger, "rarityColor", d.rc);
        SetString(trigger, "description", d.desc);
    }
}
