using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0420_GrayscaleInvert
{
    [GlossaryScene("0420")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0420_GrayscaleInvert")]
    public static void Build()
    {
        var ctx = NewScene3D("0420",
            "클릭 : 그 캐릭터 얼리기 (3초)   F : 전부 얼리기\nG : 흑백   I : 색상 반전   S : 화면 전체 흑백(포스트 프로세싱)\n" +
            "관찰: 얼면 파란색 계열 + 외곽 서리 빛으로 바뀌고 움직임이 멈춤");
        FrameCamera(ctx, new Vector3(0f, 2.6f, -7f), new Vector3(0f, 1.2f, 0f));
        ProceduralSky();

        // 색이 많은 줄무늬 텍스처 → 흑백/반전 차이가 잘 보이게
        var stripes = CreateTexture("Shader_RainbowStripes", 128, 128, (u, v) =>
        {
            float band = Mathf.Floor(v * 6f) / 6f;
            var c = Color.HSVToRGB(band, 0.75f, 1f);
            if (Mathf.Repeat(u * 8f, 1f) < 0.08f) c *= 0.75f;
            c.a = 1f;
            return c;
        });

        var mat = ShaderMat("Shader_0420_ColorEffects", "ColorEffects", m =>
        {
            m.SetTexture("_BaseMap", stripes);
            m.SetColor("_FreezeColor", new Color(0.45f, 0.75f, 1f));
            m.SetColor("_FreezeRimColor", new Color(0.8f, 1.2f, 1.6f));
        });

        var characters = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            var c = Character($"Character_{i}", new Vector3((i - 1) * 2.4f, 0f, 0f), mat);
            Spin(c, new Vector3(0f, 45f + i * 15f, 0f), 0.25f);
            characters[i] = c.transform;
        }

        // 화면 전체 흑백 비교용 볼륨 (시작할 때는 꺼 둠)
        var volume = AddBloomVolume(ctx, "0420", 0.3f, 1.1f, profile =>
        {
            var adjust = profile.Add<ColorAdjustments>(true);
            adjust.saturation.Override(-100f);
        });

        var demo = new GameObject("ColorEffectsDemo").AddComponent<ColorEffectsDemo>();
        SetArray(demo, "characters", characters);
        Set(demo, "screenVolume", volume);

        var panel = SidePanel(ctx, "흑백 / 반전 / 얼음");
        AddButton(panel, "F : 전부 얼리기", demo.FreezeAll);
        AddButton(panel, "G : 흑백 (셰이더)", demo.ToggleGrayscale);
        AddButton(panel, "I : 색상 반전 (셰이더)", demo.ToggleInvert);
        AddButton(panel, "S : 화면 전체 흑백 (Volume)", demo.ToggleScreenGrayscale);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
