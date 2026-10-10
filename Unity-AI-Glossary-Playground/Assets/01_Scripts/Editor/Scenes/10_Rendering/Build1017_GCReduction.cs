using GlossaryDemo.Rendering;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1017_GCReduction
{
    [GlossaryScene("1017")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1017_GCReduction")]
    public static void Build()
    {
        var ctx = NewSceneUI("1017", "1 : 나쁜 코드만   2 : 좋은 코드만   3 : 둘 다\n" +
                                     "상태창의 KB/프레임 과 'GC 실행 횟수'가 나쁜 코드에서만 계속 오르는 것을 관찰\n" +
                                     "Profiler 에서는 GlossaryDemo.BadUpdate / GoodUpdate 마커의 GC Alloc 열 확인");
        var canvas = ctx.Canvas.transform;

        TextMeshProUGUI Panel(string name, float x, string title, Color titleColor, string code)
        {
            var bg = CreateImage(name, canvas, new Vector2(x, -20f), new Vector2(620f, 520f), PanelColor, RoundedSprite, true);
            AnchorBottom(bg.rectTransform);
            bg.rectTransform.anchoredPosition = new Vector2(x, 360f);
            var t = CreateText("Title", bg.transform, new Vector2(0f, 228f), new Vector2(580f, 40f), title, 28f, titleColor);
            t.fontStyle = FontStyles.Bold;
            var c = CreateText("Code", bg.transform, new Vector2(0f, 10f), new Vector2(580f, 380f), code, 18f, TextColor, TextAlignmentOptions.TopLeft);
            c.richText = true;
            var live = CreateText("Live", bg.transform, new Vector2(0f, -228f), new Vector2(580f, 40f), "", 22f, new Color(1f, 0.85f, 0.5f));
            return live;
        }

        var bad = Panel("BadPanel", -330f, "나쁜 코드 (매 프레임 쓰레기)", new Color(1f, 0.55f, 0.55f),
            "<noparse>void Update() {\n" +
            "  var alive = enemies.Where(e => e.Hp > 0)\n" +
            "                     .ToList();      // LINQ + 새 List\n" +
            "  var scores = new List<int>();     // 매번 new\n" +
            "  foreach (var e in alive) scores.Add(e.Score);\n" +
            "  int total = scores.Sum();\n" +
            "  text.text = \"적: \" + alive.Count\n" +
            "            + \" 점수: \" + total;   // 문자열 + 박싱\n" +
            "}</noparse>\n\n" +
            "<color=#F99>문제</color>\n· LINQ / 람다 → 할당\n· new List 매 프레임\n· 문자열 이어붙이기 → 새 string");

        var good = Panel("GoodPanel", 330f, "좋은 코드 (할당 0)", new Color(0.55f, 1f, 0.6f),
            "<noparse>List<int> cachedAlive = new List<int>(256); // 한 번만\n\n" +
            "void Update() {\n" +
            "  cachedAlive.Clear();             // 재사용\n" +
            "  int total = 0;\n" +
            "  for (int i = 0; i < enemies.Length; i++)\n" +
            "    if (enemies[i].Hp > 0) {\n" +
            "      cachedAlive.Add(i); total += enemies[i].Score; }\n" +
            "  text.SetText(\"적: {0} 점수: {1}\",\n" +
            "               cachedAlive.Count, total); // TMP 무할당\n" +
            "}</noparse>\n\n" +
            "<color=#9F9>요령</color>\n· 컬렉션은 필드에 캐시 후 Clear\n· LINQ 대신 for\n· TMP SetText / StringBuilder 재사용");

        var demo = new GameObject("GcAllocDemo").AddComponent<GcAllocDemo>();
        Set(demo, "badText", bad);
        Set(demo, "goodText", good);

        var b = CreateButtonRow(canvas, 60f, "1 나쁜 코드만", "2 좋은 코드만", "3 둘 다");
        OnClick(b[0], demo.SetMode, 0);
        OnClick(b[1], demo.SetMode, 1);
        OnClick(b[2], demo.SetMode, 2);

        Save(ctx);
    }
}
