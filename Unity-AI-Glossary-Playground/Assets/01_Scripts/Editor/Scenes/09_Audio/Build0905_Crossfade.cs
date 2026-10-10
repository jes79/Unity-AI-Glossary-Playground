using GlossaryDemo.AudioDemo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0905_Crossfade
{
    [GlossaryScene("0905")]
    [MenuItem("Tools/Glossary/Build Scene/09 Audio/0905_Crossfade")]
    public static void Build()
    {
        var ctx = NewSceneUI("0905", "버튼 / B : 전투 시작 ↔ 평화로\nC : 크로스페이드 ↔ 즉시 교체   E : 선형 ↔ 등전력 곡선\n두 볼륨 막대가 2초 동안 엇갈리며 바뀌는 것을 보고 들어 보세요");
        var canvas = ctx.Canvas;

        // 평화: 느린 장조 사인파 / 전투: 빠른 단조 사각파
        float C4 = 261.63f, E4 = 329.63f, G4 = 392f, A4 = 440f, C5 = 523.25f, D4 = 293.66f, F4 = 349.23f;
        var peace = CreateMelodyClip("0905_PeaceBGM", new[] { C4, E4, G4, C5, G4, E4, F4, A4, C5, A4, G4, E4, D4, F4, E4, 0f }, 0.35f, Wave.Sine, 0.35f);
        float A3 = 220f, C4b = 261.63f, E4b = 329.63f, Gs3 = 207.65f, B3 = 246.94f, D4b = 293.66f;
        var battle = CreateMelodyClip("0905_BattleBGM", new[] { A3, A3, E4b, A3, C4b, A3, E4b, D4b, Gs3, Gs3, E4b, Gs3, B3, Gs3, D4b, E4b }, 0.14f, Wave.Square, 0.18f);

        var peaceSrc = new GameObject("PeaceBGM").AddComponent<AudioSource>();
        peaceSrc.clip = peace;
        peaceSrc.loop = true;
        peaceSrc.playOnAwake = false;
        var battleSrc = new GameObject("BattleBGM").AddComponent<AudioSource>();
        battleSrc.clip = battle;
        battleSrc.loop = true;
        battleSrc.playOnAwake = false;

        var demo = new GameObject("CrossfadeDemo").AddComponent<CrossfadeDemo>();
        Set(demo, "peaceBgm", peaceSrc);
        Set(demo, "battleBgm", battleSrc);

        // 볼륨 막대 두 개 (세로)
        Image Meter(string label, float x, Color color)
        {
            var bg = CreateImage($"Meter_{label}", canvas.transform, new Vector2(x, -40f), new Vector2(120f, 380f), PanelColor, WhiteSprite);
            var fill = CreateImage("Fill", bg.transform, Vector2.zero, Vector2.zero, color, WhiteSprite);
            Stretch(fill.rectTransform, 6f);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Vertical;
            fill.fillOrigin = (int)Image.OriginVertical.Bottom;
            CreateText("Label", bg.transform, new Vector2(0f, -220f), new Vector2(240f, 40f), label, 28f, Color.white);
            return fill;
        }
        Set(demo, "peaceMeter", Meter("평화 BGM", -120f, new Color(0.4f, 0.85f, 0.55f)));
        Set(demo, "battleMeter", Meter("전투 BGM", 120f, EnemyColor));

        var buttons = CreateButtonRow(canvas.transform, 60f, "전투 시작", "평화로", "모드 전환");
        OnClick(buttons[0], demo.StartBattle);
        OnClick(buttons[1], demo.EndBattle);
        OnClick(buttons[2], demo.ToggleMode);

        ExplainPanel(canvas, "크로스페이드 구현",
            "<color=#9FD0FF>IEnumerator Crossfade(AudioSource a, AudioSource b, float time) {\n" +
            "  b.volume = 0; b.Play();\n" +
            "  for (float t = 0; t < time; t += Time.deltaTime) {\n" +
            "    a.volume = 1 - t / time;\n" +
            "    b.volume = t / time;\n" +
            "    yield return null;\n  }\n" +
            "  a.Stop();\n}</color>\n\n" +
            "• AudioSource 2개를 번갈아 사용 (A↔B)\n" +
            "• 일시정지 중에도 바꾸려면 unscaledDeltaTime\n" +
            "• Audio Mixer 를 쓰면 Snapshot.TransitionTo(2f) 로도 가능", 560f, 400f);

        Save(ctx);
    }
}
