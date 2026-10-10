using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0406_Emission
{
    [GlossaryScene("0406")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0406_Emission")]
    public static void Build()
    {
        var ctx = NewScene3D("0406",
            "슬라이더 : HDR 강도 (기본 3 = 색 × 8)   E : Emission ON/OFF\nB : Bloom ON/OFF   P : 맥동(Pulse)\n" +
            "관찰: 강도 1 이하에선 그냥 파란 선, 1 을 넘기면 Bloom 으로 테두리가 번져 빛남");
        FrameCamera(ctx, new Vector3(0f, 2f, -4.6f), new Vector3(0f, 1.75f, 0f));
        DarkBackground(ctx);
        ctx.Light.intensity = 0.6f;
        var volume = AddBloomVolume(ctx, "0406", 1.2f, 1f);

        var steel = UrpLit("Shader_0406_Blade", new Color(0.6f, 0.63f, 0.68f), 0.75f, 0.85f);
        var leather = UrpLit("Shader_0406_Handle", new Color(0.35f, 0.2f, 0.12f), 0.2f);
        var gold = UrpLit("Shader_0406_Guard", new Color(0.85f, 0.65f, 0.25f), 0.6f, 1f);
        // 테두리: Base Color 는 어두운 파랑, Emission 은 파랑 × 2^3 (HDR 강도 3)
        var edge = UrpLit("Shader_0406_GlowEdge", new Color(0.05f, 0.1f, 0.2f), 0.5f, 0f, m =>
        {
            m.EnableKeyword("_EMISSION"); // URP Lit 의 Emission 체크박스와 같음
            m.SetColor("_EmissionColor", new Color(0.2f, 0.55f, 1f) * Mathf.Pow(2f, 3f));
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        });

        var sword = new GameObject("Sword");
        sword.transform.position = new Vector3(0f, 0.25f, 0f);
        Part(PrimitiveType.Sphere, "Pommel", sword.transform, new Vector3(0f, 0f, 0f), Vector3.one * 0.16f, gold);
        Part(PrimitiveType.Cylinder, "Handle", sword.transform, new Vector3(0f, 0.32f, 0f), new Vector3(0.09f, 0.3f, 0.09f), leather);
        Part(PrimitiveType.Cube, "Guard", sword.transform, new Vector3(0f, 0.66f, 0f), new Vector3(0.9f, 0.1f, 0.18f), gold);
        Part(PrimitiveType.Cube, "Blade", sword.transform, new Vector3(0f, 1.95f, 0f), new Vector3(0.22f, 2.5f, 0.05f), steel);
        var tip = Part(PrimitiveType.Cube, "BladeTip", sword.transform, new Vector3(0f, 3.2f, 0f), new Vector3(0.156f, 0.156f, 0.05f), steel);
        tip.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        var edgeL = Part(PrimitiveType.Cube, "GlowEdge_L", sword.transform, new Vector3(-0.12f, 1.95f, 0f), new Vector3(0.03f, 2.5f, 0.065f), edge);
        var edgeR = Part(PrimitiveType.Cube, "GlowEdge_R", sword.transform, new Vector3(0.12f, 1.95f, 0f), new Vector3(0.03f, 2.5f, 0.065f), edge);
        var tipL = Part(PrimitiveType.Cube, "GlowTip_L", sword.transform, new Vector3(-0.06f, 3.26f, 0f), new Vector3(0.03f, 0.18f, 0.065f), edge);
        tipL.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);
        var tipR = Part(PrimitiveType.Cube, "GlowTip_R", sword.transform, new Vector3(0.06f, 3.26f, 0f), new Vector3(0.03f, 0.18f, 0.065f), edge);
        tipR.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Spin(sword, new Vector3(0f, 35f, 0f));

        var demo = new GameObject("EmissionDemo").AddComponent<EmissionDemo>();
        SetArray(demo, "edgeRenderers", edgeL.GetComponent<Renderer>(), edgeR.GetComponent<Renderer>(), tipL.GetComponent<Renderer>(), tipR.GetComponent<Renderer>());
        Set(demo, "volume", volume);

        var panel = SidePanel(ctx, "Emission (HDR)");
        AddSlider(panel, "HDR 강도 (2^n 배)", -2f, 6f, 3f, demo.SetIntensity);
        AddButton(panel, "E : Emission ON/OFF", demo.ToggleEmission);
        AddButton(panel, "B : Bloom ON/OFF", demo.ToggleBloom);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
