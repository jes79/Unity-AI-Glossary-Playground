using GlossaryDemo.AudioDemo;
using UnityEditor;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0906_PitchVariation
{
    [GlossaryScene("0906")]
    [MenuItem("Tools/Glossary/Build Scene/09 Audio/0906_PitchVariation")]
    public static void Build()
    {
        var ctx = NewScene2D("0906", "A/D : 걷기 (0.35초마다 발소리)   Space : 자동 걷기 ON/OFF\nP : 피치 랜덤 ON/OFF\n아래 막대 = 최근 16번 발소리의 pitch (가운데 선 = 1.0). OFF 면 모두 같은 높이");

        var step1 = CreateToneClip("0906_Footstep1", 150f, 0.09f, Wave.Square, 0.35f, 60f, 0.002f, 0.07f);
        var step2 = CreateToneClip("0906_Footstep2", 170f, 0.08f, Wave.Noise, 0.3f, attack: 0.002f, release: 0.07f);

        CreateSpriteObject("Ground", SquareSprite, new Vector3(0f, -0.5f), new Vector2(20f, 1f), GroundColor);
        var root = new GameObject("Walker");
        root.transform.position = new Vector3(0f, 0f, 0f);
        var body = CreateSpriteObject("Body", SquareSprite, new Vector3(0f, 0.5f), new Vector2(0.8f, 1f), PlayerColor, root.transform, 2);

        var src = root.AddComponent<AudioSource>();
        src.playOnAwake = false;

        // 피치 기록 막대 (기준선 = pitch 1.0 높이)
        var bars = new SpriteRenderer[16];
        var barRoot = new GameObject("PitchHistory");
        barRoot.transform.position = new Vector3(-3.75f, -4.4f, 0f);
        for (int i = 0; i < bars.Length; i++)
        {
            var bar = new GameObject($"Bar_{i}");
            bar.transform.SetParent(barRoot.transform, false);
            bar.transform.localPosition = new Vector3(i * 0.5f, 0f, 0f);
            // 피벗이 막대 아래쪽이 되도록 스프라이트를 위로 0.5 올린 자식
            var sr = CreateSpriteObject("Fill", SquareSprite, new Vector3(0f, 0.5f), Vector2.one, Color.white, bar.transform, 1);
            bar.transform.localScale = new Vector3(0.38f, 0.02f, 1f);
            bars[i] = sr;
        }
        // 막대 높이 = (pitch - 0.85) × 10 → pitch 1.0 은 1.5 유닛
        CreateSpriteObject("Line_1.0", SquareSprite, new Vector3(0f, -4.4f + 1.5f), new Vector2(8.5f, 0.03f), new Color(1f, 1f, 1f, 0.6f), order: 3);

        var demo = root.AddComponent<PitchVariationDemo>();
        Set(demo, "source", src);
        SetArray(demo, "footsteps", step1, step2);
        Set(demo, "body", body.transform);
        SetArray(demo, "historyBars", bars);

        var canvas = CreateCanvas("Canvas");
        ExplainPanel(canvas, "피치 랜덤 코드",
            "<color=#9FD0FF>void PlayFootstep() {\n" +
            "  source.pitch = Random.Range(0.9f, 1.1f);\n" +
            "  source.PlayOneShot(footstepClips[Random.Range(0, n)]);\n}</color>\n\n" +
            "• pitch 1.1 = 10% 높고 빠르게, 0.9 = 낮고 느리게\n" +
            "• 클립 2~4개 랜덤 + 피치 랜덤을 함께 쓰면 효과 큼\n" +
            "• 같은 AudioSource 로 BGM 도 틀면 BGM 피치도 바뀜\n  → 효과음 전용 AudioSource 사용\n" +
            "• 볼륨도 0.9~1.0 정도 랜덤을 주면 더 자연스러움", 540f, 340f);

        Save(ctx);
    }
}
