using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0809_DropTable
{
    [GlossaryScene("0809")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0809_DropTable")]
    public static void Build()
    {
        var ctx = NewSceneUI("0809", "버튼 : 1회 / 10회 / 100회 / 1000회 뽑기\n일반 70 · 희귀 25 · 전설 5 가중치로 랜덤 드롭\n막대 그래프(히스토그램) = 실제로 나온 비율. 흰 가로선 = 기대 비율\n적게 뽑으면 들쭉날쭉, 많이 뽑을수록 기대값에 수렴");
        var root = ctx.Canvas.transform;
        var demo = new GameObject("DropTableDemo").AddComponent<DropTableDemo>();

        string[] names = { "일반", "희귀", "전설" };
        float[] weights = { 70f, 25f, 5f };
        Color[] colors = { new Color(0.8f, 0.82f, 0.86f), new Color(0.35f, 0.65f, 1f), new Color(1f, 0.72f, 0.2f) };
        const float barMax = 380f;

        var chart = Panel(root, "Histogram", new Vector2(CenterX, 30f), new Vector2(900f, 560f), new Color32(30, 33, 42, 255));
        var bars = new Image[3];
        var labels = new TextMeshProUGUI[3];
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 260f;
            var bar = CreateImage($"Bar_{names[i]}", chart.transform, new Vector2(x, -230f), new Vector2(150f, 2f), colors[i], WhiteSprite);
            bar.rectTransform.pivot = new Vector2(0.5f, 0f);
            bars[i] = bar;
            // 기대 비율 표시선
            var expected = CreateImage($"Expected_{names[i]}", chart.transform, new Vector2(x, -230f + barMax * weights[i] / 100f), new Vector2(190f, 4f), new Color(1f, 1f, 1f, 0.6f), WhiteSprite);
            expected.raycastTarget = false;
            labels[i] = Label(chart.transform, $"Label_{names[i]}", new Vector2(x + 30f, 190f), new Vector2(220f, 130f), names[i], 20f, TextAlignmentOptions.TopLeft);
        }

        var result = Label(root, "Result", new Vector2(CenterX, -290f), new Vector2(900f, 50f), "", 32f, TextAlignmentOptions.Center);
        var recent = new Image[10];
        for (int i = 0; i < 10; i++)
            recent[i] = CreateImage($"Recent_{i}", root, new Vector2(CenterX - 270f + i * 60f, -350f), new Vector2(46f, 46f), new Color(1f, 1f, 1f, 0.08f), KnobSprite);

        int[] times = { 1, 10, 100, 1000 };
        for (int i = 0; i < times.Length; i++)
        {
            var b = SmallButton(root, $"Roll_{times[i]}", new Vector2(CenterX - 450f + i * 220f, -440f), new Vector2(200f, 64f), $"{times[i]}회 뽑기");
            OnClick(b, demo.RollTimes, times[i]);
        }
        OnClick(SmallButton(root, "Reset", new Vector2(CenterX + 450f, -440f), new Vector2(160f, 64f), "초기화", EnemyColor), demo.ResetCounts);

        var so = new SerializedObject(demo);
        var arr = so.FindProperty("table");
        arr.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            var e = arr.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("name").stringValue = names[i];
            e.FindPropertyRelative("weight").floatValue = weights[i];
            e.FindPropertyRelative("color").colorValue = colors[i];
            e.FindPropertyRelative("bar").objectReferenceValue = bars[i];
            e.FindPropertyRelative("label").objectReferenceValue = labels[i];
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        SetFloat(demo, "barMaxHeight", barMax);
        Set(demo, "resultText", result);
        SetArray(demo, "recentIcons", recent);

        Save(ctx);
    }
}
