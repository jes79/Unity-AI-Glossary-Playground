using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0404_AlbedoNormalMap
{
    private const int Rows = 8, Cols = 4;
    private const float Mortar = 0.008f, Bevel = 0.014f;

    [GlossaryScene("0404")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0404_AlbedoNormalMap")]
    public static void Build()
    {
        var ctx = NewScene3D("0404",
            "N : 오른쪽 벽 Normal Map ON/OFF   A : Albedo ON/OFF\n슬라이더 : Bump Scale(요철 세기), 빛 회전 속도\n" +
            "관찰: 빛이 움직일 때 오른쪽 벽만 줄눈 요철에 그림자가 생김");
        FrameCamera(ctx, new Vector3(0f, 2.2f, -7.5f), new Vector3(0f, 1.5f, 0f));
        ctx.Light.intensity = 0.35f;
        ctx.Light.transform.rotation = Quaternion.Euler(20f, 60f, 0f);

        // ── 텍스처 생성: 벽돌 Albedo + 같은 높이 함수로 만든 Normal Map ──
        var albedo = CreateTexture("Shader_BrickAlbedo", 512, 512, BrickAlbedo);
        var normal = CreateTexture("Shader_BrickNormal", 512, 512, BrickNormal, normalMap: true);

        var plain = UrpLit("Shader_0404_AlbedoOnly", Color.white, 0.25f, 0f, m =>
        {
            m.SetTexture("_BaseMap", albedo);
            m.SetTextureScale("_BaseMap", new Vector2(2f, 1.5f));
            m.SetTexture("_BumpMap", null);
            m.DisableKeyword("_NORMALMAP");
        });
        var bumped = UrpLit("Shader_0404_AlbedoNormal", Color.white, 0.25f, 0f, m =>
        {
            m.SetTexture("_BaseMap", albedo);
            m.SetTextureScale("_BaseMap", new Vector2(2f, 1.5f)); // URP Lit 은 Base Map 의 Tiling 을 Normal Map 에도 같이 씀
            m.SetTexture("_BumpMap", normal);
            m.SetFloat("_BumpScale", 1f);
            m.EnableKeyword("_NORMALMAP"); // 이 키워드가 켜져야 URP Lit 이 노멀맵을 읽음
        });

        Part(PrimitiveType.Cube, "Wall_AlbedoOnly", null, new Vector3(-2.3f, 1.5f, 0f), new Vector3(4f, 3f, 0.3f), plain);
        var wall = Part(PrimitiveType.Cube, "Wall_AlbedoNormal", null, new Vector3(2.3f, 1.5f, 0f), new Vector3(4f, 3f, 0.3f), bumped);
        WorldLabel("Albedo 만", new Vector3(-2.3f, 3.4f, -0.2f), ctx.Camera, 3f);
        WorldLabel("Albedo + Normal Map", new Vector3(2.3f, 3.4f, -0.2f), ctx.Camera, 3f);

        // 벽 앞을 도는 점광원 (위치를 보이게 작은 구)
        var lightGo = new GameObject("OrbitPointLight");
        var point = lightGo.AddComponent<Light>();
        point.type = LightType.Point;
        point.range = 8f;
        point.intensity = 6f;
        point.color = new Color(1f, 0.92f, 0.8f);
        point.shadows = LightShadows.None;
        lightGo.transform.position = new Vector3(0f, 1.6f, -1.2f);
        var bulb = Part(PrimitiveType.Sphere, "Bulb", lightGo.transform, Vector3.zero, Vector3.one * 0.15f, UrpUnlit("Shader_0404_Bulb", new Color(1f, 0.95f, 0.7f)));
        Object.DestroyImmediate(bulb.GetComponent<Collider>());
        bulb.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

        var demo = new GameObject("NormalMapDemo").AddComponent<NormalMapDemo>();
        Set(demo, "normalWall", wall.GetComponent<Renderer>());
        Set(demo, "albedo", albedo);
        Set(demo, "orbitLight", point);

        var panel = SidePanel(ctx, "Albedo / Normal Map");
        AddButton(panel, "N : Normal Map ON/OFF", demo.ToggleNormalMap);
        AddButton(panel, "A : Albedo ON/OFF", demo.ToggleAlbedo);
        AddSlider(panel, "Bump Scale (요철 세기)", 0f, 2f, 1f, demo.SetBumpScale);
        AddSlider(panel, "빛 회전 속도", 0f, 3f, 1f, demo.SetOrbitSpeed);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }

    // 벽돌 높이: 줄눈(mortar) 0, 벽돌 면 1, 가장자리는 살짝 경사(bevel)
    private static float BrickHeight(float u, float v)
    {
        u = Mathf.Repeat(u, 1f);
        v = Mathf.Repeat(v, 1f);
        float row = Mathf.Floor(v * Rows);
        float fy = v * Rows - row;
        float x = u * Cols + (row % 2f) * 0.5f;
        float fx = x - Mathf.Floor(x);
        float dx = Mathf.Min(fx, 1f - fx) / Cols;  // uv 단위 거리
        float dy = Mathf.Min(fy, 1f - fy) / Rows;
        float d = Mathf.Min(dx, dy);
        float h = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(Mortar, Mortar + Bevel, d));
        // 벽돌 표면의 잔 울퉁불퉁함
        h *= 0.92f + 0.08f * TileNoise(u, v, 16, 2, 3);
        return h;
    }

    private static Color BrickAlbedo(float u, float v)
    {
        float row = Mathf.Floor(v * Rows);
        float x = u * Cols + (row % 2f) * 0.5f;
        float col = Mathf.Floor(x);
        float h = BrickHeight(u, v);
        // 벽돌마다 조금씩 다른 색
        float tone = Mathf.Repeat(Mathf.Sin(row * 12.9898f + col * 78.233f) * 43758.5453f, 1f);
        var brick = Color.Lerp(new Color(0.55f, 0.22f, 0.15f), new Color(0.75f, 0.38f, 0.25f), tone);
        brick *= 0.85f + 0.3f * TileNoise(u, v, 32, 2, 11);
        var mortar = new Color(0.62f, 0.6f, 0.56f) * (0.85f + 0.2f * TileNoise(u, v, 64, 1, 5));
        var c = Color.Lerp(mortar, brick, h);
        c.a = 1f;
        return c;
    }

    // 높이 차이(기울기)로 법선을 만들고 RGB = 법선 × 0.5 + 0.5 로 저장 (탄젠트 공간, 파란색이 "정면")
    private static Color BrickNormal(float u, float v)
    {
        const float e = 1f / 512f;
        const float strength = 0.025f;
        float hx = (BrickHeight(u + e, v) - BrickHeight(u - e, v)) / (2f * e);
        float hy = (BrickHeight(u, v + e) - BrickHeight(u, v - e)) / (2f * e);
        var n = new Vector3(-hx * strength, -hy * strength, 1f).normalized;
        return new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
    }
}
