using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0808_LevelUp
{
    const int ChartCount = 15;

    [GlossaryScene("0808")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0808_LevelUp")]
    public static void Build()
    {
        var ctx = NewSceneUI("0808", "버튼 : 경험치 얻기 (+30 / +250 / +1000)\n필요 경험치가 레벨마다 1.2배씩 늘어남 (100 → 120 → 144 → 173 ...)\n아래 그래프 = 레벨별 필요 경험치. [곡선 전환] 으로 선형 증가와 비교\n한 번에 큰 경험치를 얻으면 여러 레벨이 한꺼번에 오름");
        var root = ctx.Canvas.transform;
        var demo = new GameObject("LevelUpDemo").AddComponent<LevelUpDemo>();

        var level = Label(root, "Level", new Vector2(CenterX - 420f, 380f), new Vector2(260f, 70f), "Lv. 1", 56f, TextAlignmentOptions.Left, new Color(1f, 0.85f, 0.4f));
        level.fontStyle = FontStyles.Bold;
        var fill = Bar(root, "XpBar", new Vector2(CenterX + 100f, 380f), new Vector2(760f, 40f), new Color(0.35f, 0.8f, 1f), 0f);
        var xpText = Label(fill.transform.parent, "XpText", Vector2.zero, new Vector2(760f, 40f), "", 24f, TextAlignmentOptions.Center);
        var popup = Label(root, "Popup", new Vector2(CenterX, 290f), new Vector2(900f, 80f), "", 54f, TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.3f));
        popup.fontStyle = FontStyles.Bold;
        popup.alpha = 0f;

        // 버튼
        var b1 = SmallButton(root, "Xp30", new Vector2(CenterX - 390f, 200f), new Vector2(240f, 64f), "슬라임 처치 +30");
        var b2 = SmallButton(root, "Xp250", new Vector2(CenterX - 130f, 200f), new Vector2(240f, 64f), "보스 처치 +250");
        var b3 = SmallButton(root, "Xp1000", new Vector2(CenterX + 130f, 200f), new Vector2(240f, 64f), "퀘스트 +1000", new Color32(150, 100, 220, 255));
        var reset = SmallButton(root, "Reset", new Vector2(CenterX + 390f, 200f), new Vector2(240f, 64f), "리셋", EnemyColor);
        OnClick(b1, demo.GainXp, 30);
        OnClick(b2, demo.GainXp, 250);
        OnClick(b3, demo.GainXp, 1000);
        OnClick(reset, demo.ResetLevel);

        // 그래프
        var chart = Panel(root, "Chart", new Vector2(CenterX, -230f), new Vector2(1180f, 500f), new Color32(30, 33, 42, 255));
        var curve = Label(chart.transform, "CurveText", new Vector2(-120f, 215f), new Vector2(900f, 40f), "", 24f);
        var toggle = SmallButton(chart.transform, "ToggleCurve", new Vector2(450f, 215f), new Vector2(220f, 50f), "곡선 전환");
        OnClick(toggle, demo.ToggleCurve);
        var bars = new Image[ChartCount];
        var labels = new TextMeshProUGUI[ChartCount];
        float w = 64f, gap = 10f;
        for (int i = 0; i < ChartCount; i++)
        {
            float x = (i - (ChartCount - 1) * 0.5f) * (w + gap);
            var bar = CreateImage($"Bar_{i + 1}", chart.transform, new Vector2(x, -190f), new Vector2(w, 10f), new Color(0.35f, 0.45f, 0.65f), WhiteSprite);
            bar.rectTransform.pivot = new Vector2(0.5f, 0f); // 아래에서 위로 자람
            bars[i] = bar;
            labels[i] = Label(chart.transform, $"Req_{i + 1}", new Vector2(x, 168f), new Vector2(w + 6f, 26f), "", 15f, TextAlignmentOptions.Center, SubTextColor);
            Label(chart.transform, $"Lv_{i + 1}", new Vector2(x, -210f), new Vector2(w, 26f), $"Lv{i + 1}", 16f, TextAlignmentOptions.Center, SubTextColor);
        }

        Set(demo, "xpFill", fill);
        Set(demo, "levelText", level);
        Set(demo, "xpText", xpText);
        Set(demo, "popupText", popup);
        Set(demo, "curveText", curve);
        SetArray(demo, "chartBars", bars);
        SetArray(demo, "chartLabels", labels);
        SetFloat(demo, "chartHeight", 330f);

        Save(ctx);
    }
}
