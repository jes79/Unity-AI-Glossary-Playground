using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0417_WaterShader
{
    [GlossaryScene("0417")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0417_WaterShader")]
    public static void Build()
    {
        var ctx = NewScene3D("0417",
            "D : Depth 사용 ON/OFF 비교   슬라이더 : 최대 깊이, 거품 거리\n" +
            "관찰: 경사진 모래 쪽은 밝은 청록, 가운데 깊은 곳은 짙은 남색. 물체가 수면과 닿는 곳에 거품", ground: false);
        FrameCamera(ctx, new Vector3(0f, 6f, -12f), new Vector3(0f, -0.8f, 2f));
        ProceduralSky();
        RequireDepthTexture(ctx.Camera); // 이 셰이더는 카메라 Depth Texture 가 꼭 필요

        // ── 물밑 지형: 깊은 바닥(-3m) + 뒤쪽으로 올라오는 모래 경사 + 바위·기둥 ──
        var sand = UrpLit("Shader_0417_Sand", new Color(0.85f, 0.75f, 0.55f), 0.1f);
        var rock = UrpLit("Shader_0417_Rock", new Color(0.45f, 0.43f, 0.42f), 0.2f);
        Part(PrimitiveType.Cube, "SeaFloor", null, new Vector3(0f, -3.5f, 0f), new Vector3(30f, 1f, 30f), sand);
        float slope = Mathf.Atan2(3.8f, 9f) * Mathf.Rad2Deg;
        Part(PrimitiveType.Cube, "Beach", null, new Vector3(0f, -1.2f, 5f), new Vector3(30f, 0.5f, 10f), sand).transform.rotation = Quaternion.Euler(-slope, 0f, 0f);
        Part(PrimitiveType.Cube, "BeachTop", null, new Vector3(0f, 0.35f, 13.5f), new Vector3(30f, 0.5f, 8f), sand);
        Part(PrimitiveType.Sphere, "Island", null, new Vector3(-3.5f, -1.3f, -0.5f), Vector3.one * 3.2f, rock);
        Part(PrimitiveType.Sphere, "ShallowRock", null, new Vector3(2f, -2.6f, 0.5f), Vector3.one * 1.8f, rock);
        Part(PrimitiveType.Cylinder, "Pillar", null, new Vector3(3.5f, -1.2f, -2.5f), new Vector3(0.6f, 2.5f, 0.6f), rock);
        var box = Part(PrimitiveType.Cube, "FloatingBox", null, new Vector3(0f, 0f, -3f), Vector3.one * 0.9f, UrpLit("Shader_0417_Box", new Color(0.7f, 0.45f, 0.25f), 0.2f));

        // ── 물: 반투명 Plane (Unity Plane = 10x10m, 정점 11x11 → 버텍스 물결 가능) ──
        var waterMat = ShaderMat("Shader_0417_Water", "Water", m =>
        {
            m.SetTexture("_NoiseTex", NoiseTexture());
            m.SetTextureScale("_NoiseTex", new Vector2(0.5f, 0.5f));
            m.SetFloat("_DepthMaxDistance", 2.5f);
            m.SetFloat("_FoamDistance", 0.35f);
        });
        var water = Part(PrimitiveType.Plane, "Water", null, new Vector3(0f, 0f, 2f), new Vector3(3f, 1f, 3f), waterMat);
        Object.DestroyImmediate(water.GetComponent<Collider>());
        water.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

        var demo = new GameObject("WaterDepthDemo").AddComponent<WaterDepthDemo>();
        Set(demo, "water", water.GetComponent<Renderer>());
        Set(demo, "floatingBox", box.transform);

        var panel = SidePanel(ctx, "물 셰이더 (Depth)");
        AddButton(panel, "D : Depth 사용 ON/OFF", demo.ToggleDepth);
        AddSlider(panel, "최대 깊이 (m)", 0.2f, 6f, 2.5f, demo.SetDepthMax);
        AddSlider(panel, "거품 거리 (m)", 0f, 1.5f, 0.35f, demo.SetFoam);
        AddText(panel, "<size=16>카메라의 Rendering > Depth Texture 를 On 으로 둠\n(파이프라인 에셋 설정은 그대로)</size>", 16f, 46f);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
