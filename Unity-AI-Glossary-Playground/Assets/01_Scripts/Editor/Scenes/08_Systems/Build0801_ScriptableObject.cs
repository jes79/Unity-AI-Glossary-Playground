using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0801_ScriptableObject
{
    [GlossaryScene("0801")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0801_ScriptableObject")]
    public static void Build()
    {
        var items = EnsureItems(); // Assets/09_Generated/Systems/Items/*.asset 생성
        var ctx = NewSceneUI("0801", "카드의 [사용] : 아이템 효과 적용 (포션 회복 / 무기·갑옷 장착)\n카드 6장은 모두 같은 ItemCardView 스크립트 + 서로 다른 ItemData 에셋\nProject 창 Assets/09_Generated/Systems/Items 의 에셋 값을 바꾸면 카드도 바뀜\n(코드 수정 없이 데이터만 교체)");
        var root = ctx.Canvas.transform;

        var demo = new GameObject("ScriptableObjectDemo").AddComponent<ScriptableObjectDemo>();

        string[] ids = { "potion_hp", "potion_mp", "slime_jelly", "sword_steel", "sword_legend", "armor_plate" };
        for (int i = 0; i < ids.Length; i++)
        {
            var data = items[ids[i]];
            float x = CenterX - 420f + (i % 3) * 420f;
            float y = 260f - (i / 3) * 290f;
            var card = Panel(root, $"Card_{data.id}", new Vector2(x, y), new Vector2(400f, 270f));
            var iconBg = Panel(card.transform, "IconBg", new Vector2(-130f, 50f), new Vector2(110f, 110f), new Color32(30, 33, 42, 255));
            var icon = CreateImage("Icon", iconBg.transform, Vector2.zero, new Vector2(90f, 90f), Color.white, data.icon);
            icon.preserveAspect = true;
            var name = Label(card.transform, "Name", new Vector2(65f, 90f), new Vector2(250f, 40f), data.itemName, 28f);
            name.fontStyle = FontStyles.Bold;
            var info = Label(card.transform, "Info", new Vector2(65f, 0f), new Vector2(250f, 140f), "", 18f, TextAlignmentOptions.TopLeft);
            var use = SmallButton(card.transform, "UseButton", new Vector2(0f, -100f), new Vector2(370f, 46f), "사용");

            var view = card.gameObject.AddComponent<ItemCardView>();
            Set(view, "data", data);
            Set(view, "icon", icon);
            Set(view, "nameText", name);
            Set(view, "infoText", info);
            Set(view, "owner", demo);
            OnClick(use, view.Use);
        }

        var statsPanel = Panel(root, "StatsPanel", new Vector2(CenterX - 150f, -400f), new Vector2(900f, 200f), new Color32(35, 39, 50, 255));
        var stats = Label(statsPanel.transform, "Stats", new Vector2(-150f, 15f), new Vector2(560f, 150f), "", 24f, TextAlignmentOptions.TopLeft);
        var log = Label(statsPanel.transform, "Log", new Vector2(-150f, -75f), new Vector2(560f, 40f), "", 20f, TextAlignmentOptions.Left, new Color(1f, 0.85f, 0.4f));
        var hit = SmallButton(statsPanel.transform, "HitButton", new Vector2(290f, 0f), new Vector2(240f, 70f), "피격 (HP -15)", EnemyColor);
        OnClick(hit, demo.TakeHit);
        Set(demo, "statsText", stats);
        Set(demo, "logText", log);

        Save(ctx);
    }
}
