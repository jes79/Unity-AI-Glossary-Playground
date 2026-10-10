using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0810_Shop
{
    [GlossaryScene("0810")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0810_Shop")]
    public static void Build()
    {
        var items = EnsureItems();
        var ctx = NewSceneUI("0810", "[구매] : 가격만큼 골드 차감 → 가방에 추가\n[판매] : 구매가의 50% 를 받고 가방에서 제거\n골드가 모자라면 구매 실패 메시지\n[골드 +500] 으로 재화 추가");
        var root = ctx.Canvas.transform;
        var demo = new GameObject("ShopDemo").AddComponent<ShopDemo>();

        var stock = new[] { items["potion_hp"], items["potion_mp"], items["sword_wood"], items["sword_steel"], items["armor_leather"], items["gem"] };

        var header = Panel(root, "GoldPanel", new Vector2(CenterX, 420f), new Vector2(1100f, 90f), new Color32(30, 33, 42, 255));
        Label(header.transform, "ShopTitle", new Vector2(-330f, 0f), new Vector2(400f, 60f), "<b>잡화점</b>", 34f);
        CreateImage("Coin", header.transform, new Vector2(140f, 0f), new Vector2(50f, 50f), new Color(1f, 0.8f, 0.25f), KnobSprite);
        var gold = Label(header.transform, "Gold", new Vector2(290f, 0f), new Vector2(240f, 60f), "500 G", 36f, TextAlignmentOptions.Left, new Color(1f, 0.85f, 0.35f));
        gold.fontStyle = FontStyles.Bold;
        OnClick(SmallButton(header.transform, "AddGold", new Vector2(470f, 0f), new Vector2(130f, 56f), "골드 +500", new Color32(200, 150, 40, 255), 20f), demo.AddGold, 500);

        var owned = new TextMeshProUGUI[stock.Length];
        for (int i = 0; i < stock.Length; i++)
        {
            var it = stock[i];
            var row = Panel(root, $"Row_{it.id}", new Vector2(CenterX, 290f - i * 110f), new Vector2(1100f, 96f));
            var iconBg = Panel(row.transform, "IconBg", new Vector2(-490f, 0f), new Vector2(80f, 80f), new Color32(30, 33, 42, 255));
            CreateImage("Icon", iconBg.transform, Vector2.zero, new Vector2(66f, 66f), Color.white, it.icon).preserveAspect = true;
            var n = Label(row.transform, "Name", new Vector2(-280f, 14f), new Vector2(300f, 36f), it.itemName, 26f, TextAlignmentOptions.Left, it.RarityColor);
            n.fontStyle = FontStyles.Bold;
            Label(row.transform, "Effect", new Vector2(-280f, -20f), new Vector2(300f, 30f), it.EffectText, 18f, TextAlignmentOptions.Left, SubTextColor);
            Label(row.transform, "Price", new Vector2(-30f, 0f), new Vector2(200f, 40f), $"<color=#FFD54A>{it.price} G</color>", 26f);
            owned[i] = Label(row.transform, "Owned", new Vector2(130f, 0f), new Vector2(140f, 40f), "보유 0", 24f);
            OnClick(SmallButton(row.transform, "Buy", new Vector2(320f, 0f), new Vector2(150f, 64f), "구매"), demo.Buy, i);
            OnClick(SmallButton(row.transform, "Sell", new Vector2(480f, 0f), new Vector2(150f, 64f), $"판매 {it.SellPrice}G", new Color32(70, 160, 110, 255), 20f), demo.Sell, i);
        }

        var log = Label(root, "Log", new Vector2(CenterX, -420f), new Vector2(1100f, 50f), "", 26f, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.4f));

        SetArray(demo, "stock", stock);
        SetArray(demo, "ownedTexts", owned);
        Set(demo, "goldText", gold);
        Set(demo, "logText", log);

        Save(ctx);
    }
}
