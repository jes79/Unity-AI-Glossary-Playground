using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0411_RimLight
{
    [GlossaryScene("0411")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0411_RimLight")]
    public static void Build()
    {
        var ctx = NewScene3D("0411",
            "R : 오른쪽 캐릭터 림 라이트 ON/OFF   슬라이더 : Rim Power, Intensity\n" +
            "관찰: 캐릭터가 돌아도 카메라에서 본 외곽선 부분만 하늘색으로 빛남");
        FrameCamera(ctx, new Vector3(0f, 2f, -6f), new Vector3(0f, 1.2f, 0f));
        DarkBackground(ctx, new Color(0.05f, 0.05f, 0.08f));
        ctx.Light.intensity = 0.8f;
        AddBloomVolume(ctx, "0411", 0.6f, 1.1f);

        var noRim = ShaderMat("Shader_0411_NoRim", "RimLight", m =>
        {
            m.SetColor("_BaseColor", new Color(0.4f, 0.32f, 0.5f));
            m.SetFloat("_RimIntensity", 0f);
        });
        var rim = ShaderMat("Shader_0411_Rim", "RimLight", m =>
        {
            m.SetColor("_BaseColor", new Color(0.4f, 0.32f, 0.5f));
            m.SetColor("_RimColor", new Color(0.45f, 0.85f, 1.6f)); // 하늘색 (살짝 HDR)
            m.SetFloat("_RimPower", 3f);
            m.SetFloat("_RimIntensity", 1.5f);
        });

        var a = Character("Character_NoRim", new Vector3(-1.6f, 0f, 0f), noRim);
        var b = Character("Character_Rim", new Vector3(1.6f, 0f, 0f), rim);
        Spin(a, new Vector3(0f, 40f, 0f));
        Spin(b, new Vector3(0f, 40f, 0f));
        WorldLabel("림 없음", new Vector3(-1.6f, 2.8f, 0f), ctx.Camera, 2.6f);
        WorldLabel("Fresnel 림 라이트", new Vector3(1.6f, 2.8f, 0f), ctx.Camera, 2.6f);

        var demo = new GameObject("RimLightDemo").AddComponent<RimLightDemo>();
        SetArray(demo, "rimRenderers", b.GetComponentsInChildren<Renderer>());

        var panel = SidePanel(ctx, "림 라이트 (Fresnel)");
        AddButton(panel, "R : 림 ON/OFF", demo.ToggleRim);
        AddSlider(panel, "Rim Power (두께, 클수록 얇음)", 0.5f, 10f, 3f, demo.SetPower);
        AddSlider(panel, "Rim Intensity (밝기)", 0f, 4f, 1.5f, demo.SetIntensity);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
