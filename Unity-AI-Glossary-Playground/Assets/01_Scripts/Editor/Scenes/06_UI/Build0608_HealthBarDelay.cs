using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0608_HealthBarDelay
{
    [GlossaryScene("0608")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0608_HealthBarDelay")]
    public static void Build()
    {
        var ctx = NewSceneUI("0608",
            "1 : 피해 10   2 : 피해 30   3 : 5연타   H : 회복   R : 리셋\nD : 위쪽 바 딜레이 ON/OFF\n" +
            "빨간 바는 즉시, 흰 바는 0.5초 뒤 따라 줄어드는지 보세요 (아래 바는 비교용 즉시 감소)");
        var root = ctx.Canvas.transform;

        var group = CreateRect("Bars", root, new Vector2(230f, 40f), new Vector2(1000f, 420f));

        var top = CreateBar(group, "Bar_Delay", new Vector2(0f, 90f), "딜레이 감소 ON  (빨간 바 즉시 → 흰 바 0.5초 뒤)");
        SetBool(top, "useDelay", true);
        var bottom = CreateBar(group, "Bar_Instant", new Vector2(0f, -110f), "비교: 딜레이 없음 (즉시 감소)");
        SetBool(bottom, "useDelay", false);

        var buttons = ButtonRow(root, new Vector2(0f, -480f), 200f, 64f, 22f, "피해 10 (1)", "피해 30 (2)", "5연타 (3)", "회복 25 (H)", "딜레이 ON/OFF (D)", "리셋 (R)");

        var demo = new GameObject("HealthBarDelayDemo").AddComponent<HealthBarDelayDemo>();
        Set(demo, "withDelay", top);
        Set(demo, "withoutDelay", bottom);
        Set(demo, "shakeTarget", group);
        OnClick(buttons[0], demo.Damage10);
        OnClick(buttons[1], demo.Damage30);
        OnClick(buttons[2], demo.Combo);
        OnClick(buttons[3], demo.Heal25);
        OnClick(buttons[4], demo.ToggleDelay);
        OnClick(buttons[5], demo.ResetAll);

        Save(ctx);
    }

    // 배경 → 흰 딜레이 바 → 빨간 체력 바 순서로 겹침 (뒤에 있는 자식이 위에 그려짐)
    private static DelayedHealthBar CreateBar(RectTransform parent, string name, Vector2 pos, string title)
    {
        var holder = CreateRect(name, parent, pos, new Vector2(900f, 120f));
        Body(holder, "Title", new Vector2(0f, 45f), new Vector2(900f, 36f), title, 24f, TextAlignmentOptions.Left, TitleColor);

        var bg = Panel(holder, "Background", new Vector2(0f, -15f), new Vector2(900f, 62f), new Color(0.08f, 0.08f, 0.1f, 1f));
        var delay = Filled(bg.transform, "DelayFill (흰색)", Vector2.zero, Vector2.zero, new Color(1f, 0.95f, 0.85f), Image.FillMethod.Horizontal);
        Stretch(delay.rectTransform, 6f);
        var front = Filled(bg.transform, "FrontFill (빨강)", Vector2.zero, Vector2.zero, new Color(0.9f, 0.2f, 0.25f), Image.FillMethod.Horizontal);
        Stretch(front.rectTransform, 6f);
        var text = CreateText("HPText", bg.transform, Vector2.zero, Vector2.zero, "100 / 100", 26f, Color.white);
        Stretch(text.rectTransform);
        text.fontStyle = FontStyles.Bold;

        var bar = holder.gameObject.AddComponent<DelayedHealthBar>();
        Set(bar, "frontFill", front);
        Set(bar, "delayFill", delay);
        Set(bar, "hpText", text);
        return bar;
    }
}
