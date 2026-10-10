using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0528_MotionBlur
{
    [GlossaryScene("0528")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0528_MotionBlur")]
    public static void Build()
    {
        var ctx = Stage("0528", "WASD : 이동   Shift / Space : 대시\nM : 블러 모드 (대시 때만 → 항상 → 끔)\n걷는 동안은 선명하고 대시하는 순간만 화면이 쭉 늘어지는지 관찰",
            new Vector3(0f, 7f, -9f), new Vector3(38f, 0f, 0f), true, 100f);
        EnablePostProcessing(ctx.Camera);

        var player = Prim(PrimitiveType.Cube, "Player", new Vector3(0f, 0.5f, 0f), Vector3.one, PlayerColor);

        // 블러가 잘 보이도록 촘촘한 줄무늬 기둥
        Color[] cols = { new Color32(230, 80, 80, 255), new Color32(240, 220, 90, 255), new Color32(90, 200, 120, 255), new Color32(90, 140, 240, 255) };
        int n = 0;
        for (int x = -40; x <= 40; x += 5)
            for (int z = -40; z <= 40; z += 5)
                if (Mathf.Abs(x) + Mathf.Abs(z) > 3)
                    Prim(PrimitiveType.Cube, $"Post_{n}", new Vector3(x, 0.75f, z), new Vector3(0.5f, 1.5f, 0.5f), cols[n++ % cols.Length]).isStatic = true;

        var profile = Profile("Profile_0528_MotionBlur");
        var blur = AddOverride<MotionBlur>(profile);
        blur.mode.Override(MotionBlurMode.CameraOnly);
        blur.quality.Override(MotionBlurQuality.High);
        blur.intensity.Override(0f);
        blur.clamp.Override(0.15f);
        SaveProfile(profile);
        var volume = GlobalVolume("Global Volume", profile);

        var demo = volume.gameObject.AddComponent<DashMotionBlurDemo>();
        Set(demo, "volume", volume);
        Set(demo, "player", player.transform);
        Set(demo, "cameraTransform", ctx.Camera.transform);
        var b = Buttons(ctx, "대시", "모드 변경");
        OnClick(b[0], demo.Dash);
        OnClick(b[1], demo.CycleMode);
        Save(ctx);
    }
}
