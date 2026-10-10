using GlossaryDemo.Rendering;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1010_Profiler
{
    [GlossaryScene("1010")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1010_Profiler")]
    public static void Build()
    {
        var ctx = NewSceneUI("1010", "Space / [스파이크] : 일부러 30MB 쓰레기 + 무거운 계산 (렉 1프레임)\n" +
                                     "G : GC.Collect   A : 3초마다 자동 스파이크\n" +
                                     "Profiler 창을 켜고 튀는 막대를 클릭해 원인을 찾아보세요 (오른쪽 순서)");
        var canvas = ctx.Canvas.transform;

        // 프레임 시간 그래프 (화면 가운데 아래)
        var frame = CreateImage("GraphFrame", canvas, new Vector2(0f, 290f), new Vector2(920f, 330f), PanelColor, RoundedSprite, true);
        AnchorBottom(frame.rectTransform);
        var raw = CreateRect("Graph", frame.transform, new Vector2(0f, -10f), new Vector2(880f, 280f)).gameObject.AddComponent<RawImage>();
        raw.raycastTarget = false;
        CreateText("GraphTitle", frame.transform, new Vector2(0f, 148f), new Vector2(880f, 30f),
            "프레임 시간 (ms)   <color=#50C878>— 16.7ms (60fps)</color>   <color=#E6B43C>— 33.3ms (30fps)</color>", 20f, TextColor, TextAlignmentOptions.Left);

        var demo = new GameObject("ProfilerStatsDemo").AddComponent<ProfilerStatsDemo>();
        Set(demo, "graph", raw);

        var b = CreateButtonRow(canvas, 60f, "스파이크!", "GC.Collect", "자동 스파이크");
        OnClick(b[0], demo.Spike);
        OnClick(b[1], demo.ForceGC);
        OnClick(b[2], demo.ToggleAuto);

        HowToPanel(canvas, "Profiler 로 스파이크 원인 찾기",
            "1. Window > Analysis > <b>Profiler</b> (Ctrl+7)\n" +
            "2. 위쪽 ● Record 켠 채 Play → [스파이크!] 누르기\n" +
            "3. CPU Usage 그래프에서 <b>솟은 막대 클릭</b> (그 프레임 정지)\n" +
            "4. 아래 창을 <b>Hierarchy</b> 보기로 바꾸고\n   <b>Time ms</b> 열 내림차순 정렬\n" +
            "5. 펼쳐 보면 <b>GlossaryDemo.Spike</b> (ProfilerMarker) 발견\n" +
            "6. <b>GC Alloc</b> 열 정렬 → 30MB 할당 위치 확인\n" +
            "7. 더 깊이: Deep Profile 또는 Call Stacks 버튼\n" +
            "8. GC.Collect 스파이크는 GC.Collect 항목으로 보임\n\n" +
            "<color=#9BE89B>팁</color>\n· 에디터 오버헤드가 섞이므로 정확한 측정은\n  Development Build + Autoconnect Profiler\n" +
            "· 코드에서는 ProfilerRecorder 로 수치를 읽음 (이 씬)\n· 내 코드 구간은 ProfilerMarker 로 이름 붙이기",
            600f, 600f);

        Save(ctx);
    }
}
