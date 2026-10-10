using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1005_SkyboxAmbient
{
    [GlossaryScene("1005")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1005_SkyboxAmbient")]
    public static void Build()
    {
        var ctx = NewScene3D("1005", "Space : 일시정지   ← / → : 시간 빨리 감기\n" +
                                     "버튼 : 아침 / 정오 / 저녁 / 자정 으로 이동\n" +
                                     "하늘색(Skybox)과 그늘 속 밝기(환경광)가 함께 변하는 것을 관찰", true, 60f);
        ctx.Camera.transform.position = new Vector3(0f, 3f, -12f);
        ctx.Camera.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
        var canvas = EnsureCanvas(ctx);

        // Procedural Skybox 머티리얼 에셋 (하늘을 셰이더가 계산해서 그림)
        var skyMat = CreateMaterial("Sky1005_Procedural", "Skybox/Procedural", m =>
        {
            m.SetFloat("_SunSize", 0.05f);
            m.SetFloat("_AtmosphereThickness", 1f);
            m.SetColor("_SkyTint", new Color(0.5f, 0.5f, 0.5f));
            m.SetColor("_GroundColor", new Color(0.37f, 0.35f, 0.34f));
            m.SetFloat("_Exposure", 1.3f);
        });
        RenderSettings.skybox = skyMat;
        RenderSettings.sun = ctx.Light;

        // 마을 느낌 소품
        for (int i = 0; i < 7; i++)
        {
            float x = -12f + i * 4f;
            float h = 2f + (i * 37 % 5);
            Prim(PrimitiveType.Cube, $"House_{i}", new Vector3(x, h * 0.5f, 6f + (i % 2) * 3f), new Vector3(3f, h, 3f), new Color(0.85f, 0.8f, 0.7f));
        }
        for (int i = 0; i < 8; i++)
        {
            Prim(PrimitiveType.Cylinder, $"Trunk_{i}", new Vector3(-14f + i * 4f, 0.8f, 0f), new Vector3(0.3f, 0.8f, 0.3f), new Color(0.45f, 0.3f, 0.2f));
            Prim(PrimitiveType.Sphere, $"Leaves_{i}", new Vector3(-14f + i * 4f, 2.2f, 0f), Vector3.one * 1.8f, new Color(0.3f, 0.6f, 0.3f));
        }
        Prim(PrimitiveType.Sphere, "Mirror Ball", new Vector3(0f, 1.2f, -4f), Vector3.one * 2f, Color.white).GetComponent<Renderer>().sharedMaterial =
            LitMat("Sky1005_Chrome", Color.white, 0.95f, 1f); // 하늘이 비치는 금속 공 (반사 변화 확인)

        var cycle = new GameObject("DayNightCycle").AddComponent<DayNightCycle>();
        Set(cycle, "sun", ctx.Light);
        Set(cycle, "skyboxTemplate", skyMat);
        SetGradient(cycle, "sunColor",
            (0f, new Color(0.3f, 0.35f, 0.6f)), (0.25f, new Color(1f, 0.55f, 0.3f)), (0.5f, new Color(1f, 0.97f, 0.9f)),
            (0.75f, new Color(1f, 0.5f, 0.25f)), (1f, new Color(0.3f, 0.35f, 0.6f)));
        SetGradient(cycle, "ambientSky",
            (0f, new Color(0.03f, 0.04f, 0.1f)), (0.25f, new Color(0.5f, 0.4f, 0.45f)), (0.5f, new Color(0.55f, 0.7f, 0.95f)),
            (0.75f, new Color(0.6f, 0.35f, 0.3f)), (1f, new Color(0.03f, 0.04f, 0.1f)));
        SetGradient(cycle, "ambientGround",
            (0f, new Color(0.02f, 0.02f, 0.04f)), (0.5f, new Color(0.3f, 0.28f, 0.25f)), (1f, new Color(0.02f, 0.02f, 0.04f)));
        SetGradient(cycle, "skyTint",
            (0f, new Color(0.1f, 0.1f, 0.3f)), (0.25f, new Color(0.7f, 0.45f, 0.4f)), (0.5f, new Color(0.5f, 0.5f, 0.55f)),
            (0.75f, new Color(0.8f, 0.4f, 0.3f)), (1f, new Color(0.1f, 0.1f, 0.3f)));

        var b = CreateButtonRow(canvas.transform, 60f, "아침 06시", "정오 12시", "저녁 18시", "자정 00시", "일시정지");
        OnClick(b[0], cycle.SetTime, 6f);
        OnClick(b[1], cycle.SetTime, 12f);
        OnClick(b[2], cycle.SetTime, 18f);
        OnClick(b[3], cycle.SetTime, 0f);
        OnClick(b[4], cycle.TogglePause);

        HowToPanel(canvas.transform, "에디터에서 설정하는 곳",
            "Window > Rendering > <b>Lighting</b> > Environment\n" +
            "· <b>Skybox Material</b> : 하늘 (Create > Material,\n  Shader = Skybox/Procedural 또는 6 Sided/Cubemap)\n" +
            "· <b>Sun Source</b> : 태양 Directional Light\n" +
            "· Environment Lighting <b>Source</b> : Skybox / Gradient / Color\n" +
            "· Intensity Multiplier : 환경광 세기\n\n" +
            "<color=#9BE89B>코드</color> (이 데모)\n" +
            "RenderSettings.skybox = 복제한 머티리얼;\n" +
            "RenderSettings.ambientSkyColor = gradient.Evaluate(t);\n" +
            "sun.transform.rotation = 시간 → 각도;\n" +
            "DynamicGI.UpdateEnvironment(); // 반사·환경광 갱신",
            540f, 430f);

        Save(ctx);
    }
}
