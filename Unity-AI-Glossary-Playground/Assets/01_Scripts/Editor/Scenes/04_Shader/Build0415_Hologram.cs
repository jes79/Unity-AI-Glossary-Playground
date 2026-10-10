using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0415_Hologram
{
    [GlossaryScene("0415")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0415_Hologram")]
    public static void Build()
    {
        var ctx = NewScene3D("0415",
            "F : 깜빡임 ON/OFF   G : 글리치 ON/OFF\n슬라이더 : 스캔라인 속도(음수면 아래로), 밀도\n" +
            "관찰: 가는 줄무늬와 큰 밝은 띠가 위로 흐르고, 외곽이 더 진하며, 가끔 옆으로 지직거림");
        FrameCamera(ctx, new Vector3(0f, 2.2f, -5.5f), new Vector3(0f, 1.3f, 0f));
        DarkBackground(ctx, new Color(0.02f, 0.03f, 0.05f));
        ctx.Light.intensity = 0.4f;
        AddBloomVolume(ctx, "0415", 0.9f, 0.9f);

        var holo = ShaderMat("Shader_0415_Hologram", "Hologram", m =>
        {
            m.SetColor("_HoloColor", new Color(0.3f, 1.4f, 2.2f));
            m.SetFloat("_ScanSpeed", 1.2f);
            m.SetFloat("_ScanDensity", 18f);
        });

        // 프로젝터 받침대 (발광 링)
        Prim(PrimitiveType.Cylinder, "Projector", new Vector3(0f, 0.05f, 0f), new Vector3(1.6f, 0.05f, 1.6f), new Color(0.2f, 0.22f, 0.26f));
        var ringMat = UrpLit("Shader_0415_ProjectorGlow", Color.black, 0.5f, 0f, m =>
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", new Color(0.3f, 1.4f, 2.2f) * 1.5f);
        });
        var ring = Part(PrimitiveType.Cylinder, "ProjectorRing", null, new Vector3(0f, 0.11f, 0f), new Vector3(1.3f, 0.01f, 1.3f), ringMat);
        Object.DestroyImmediate(ring.GetComponent<Collider>());

        var character = Character("HologramCharacter", new Vector3(0f, 0.12f, 0f), holo);
        foreach (var r in character.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
        Spin(character, new Vector3(0f, 30f, 0f));

        // 비교용 홀로그램 구 (곡면에서 Fresnel 이 잘 보임)
        var orb = Part(PrimitiveType.Sphere, "HologramOrb", null, new Vector3(2.2f, 1.3f, 0.5f), Vector3.one * 1.1f, holo);
        orb.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

        var demo = new GameObject("HologramDemo").AddComponent<HologramDemo>();
        var renderers = new System.Collections.Generic.List<Renderer>(character.GetComponentsInChildren<Renderer>()) { orb.GetComponent<Renderer>() };
        SetArray(demo, "holoRenderers", renderers.ToArray());

        var panel = SidePanel(ctx, "홀로그램");
        AddSlider(panel, "스캔라인 속도", -3f, 3f, 1.2f, demo.SetScanSpeed);
        AddSlider(panel, "스캔라인 밀도 (줄/m)", 4f, 40f, 18f, demo.SetScanDensity);
        AddButton(panel, "F : 깜빡임 ON/OFF", demo.ToggleFlicker);
        AddButton(panel, "G : 글리치 ON/OFF", demo.ToggleGlitch);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
