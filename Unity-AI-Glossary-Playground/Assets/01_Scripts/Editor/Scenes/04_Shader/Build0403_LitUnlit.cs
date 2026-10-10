using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0403_LitUnlit
{
    [GlossaryScene("0403")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0403_LitUnlit")]
    public static void Build()
    {
        var ctx = NewScene3D("0403",
            "U : 가운데 표지판 Lit ↔ Unlit   L : 조명 끄기/켜기\nSpace : 조명 자동 회전 ON/OFF   슬라이더 : 조명 각도·세기\n" +
            "관찰: 조명을 꺼도 Unlit 줄과 Unlit 표지판은 똑같이 밝음");
        FrameCamera(ctx, new Vector3(0f, 4.2f, -9f), new Vector3(0f, 0.8f, 0.5f));

        Color[] colors = { new Color(0.95f, 0.35f, 0.3f), new Color(0.35f, 0.8f, 0.4f), new Color(0.35f, 0.55f, 1f) };
        PrimitiveType[] shapes = { PrimitiveType.Sphere, PrimitiveType.Cube, PrimitiveType.Capsule };
        for (int i = 0; i < 3; i++)
        {
            var lit = UrpLit($"Shader_0403_Lit_{i}", colors[i], 0.4f);
            var unlit = UrpUnlit($"Shader_0403_Unlit_{i}", colors[i]);
            float z = (i - 1) * 1.8f;
            Part(shapes[i], $"Lit_{shapes[i]}", null, new Vector3(-3f, 0.6f, z), Vector3.one * 1.1f, lit);
            Part(shapes[i], $"Unlit_{shapes[i]}", null, new Vector3(3f, 0.6f, z), Vector3.one * 1.1f, unlit);
        }
        WorldLabel("Lit\n(조명 받음)", new Vector3(-3f, 2.6f, 2.5f), ctx.Camera, 3f);
        WorldLabel("Unlit\n(조명 무시)", new Vector3(3f, 2.6f, 2.5f), ctx.Camera, 3f);

        // 가운데 표지판 (UI 아이콘 같은 역할) - 기둥 + 판
        var signColor = new Color(1f, 0.8f, 0.2f);
        var signLit = UrpLit("Shader_0403_SignLit", signColor, 0.3f);
        var signUnlit = UrpUnlit("Shader_0403_SignUnlit", signColor);
        var pole = Prim(PrimitiveType.Cylinder, "SignPole", new Vector3(0f, 0.8f, 1.2f), new Vector3(0.12f, 0.8f, 0.12f), new Color(0.4f, 0.4f, 0.45f));
        var board = Part(PrimitiveType.Cube, "SignBoard", null, new Vector3(0f, 1.9f, 1.2f), new Vector3(1.6f, 1f, 0.1f), signLit);
        var icon = Part(PrimitiveType.Sphere, "SignIcon", null, new Vector3(0f, 1.9f, 1.13f), new Vector3(0.5f, 0.5f, 0.05f), signLit);
        pole.isStatic = false;

        var demo = new GameObject("LitUnlitDemo").AddComponent<LitUnlitDemo>();
        Set(demo, "mainLight", ctx.Light);
        SetArray(demo, "signRenderers", board.GetComponent<Renderer>(), icon.GetComponent<Renderer>());
        Set(demo, "signLit", signLit);
        Set(demo, "signUnlit", signUnlit);

        var panel = SidePanel(ctx, "Lit / Unlit");
        AddButton(panel, "U : 표지판 Lit ↔ Unlit", demo.ToggleSign);
        AddSlider(panel, "조명 각도 (Yaw)", 0f, 360f, 330f, demo.SetLightYaw);
        AddSlider(panel, "조명 세기", 0f, 2f, 1.2f, demo.SetLightIntensity);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
