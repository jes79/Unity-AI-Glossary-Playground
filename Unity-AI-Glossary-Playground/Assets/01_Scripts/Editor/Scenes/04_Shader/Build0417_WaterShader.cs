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
        // 조금 내려다보게 해 화면 위쪽 끝까지 모래(지형)로 채움 → 지평선 아래 스카이박스(베이지/어두운 띠)가 안 보이게
        FrameCamera(ctx, new Vector3(0f, 6f, -12f), new Vector3(0f, -1.5f, 2f));
        ProceduralSky();
        RequireDepthTexture(ctx.Camera); // 이 셰이더는 카메라 Depth Texture 가 꼭 필요

        // ── 물밑 지형: 깊은 바닥(-3m) + 뒤쪽으로 올라오는 모래 경사 + 바위·기둥 ──
        var sand = UrpLit("Shader_0417_Sand", new Color(0.85f, 0.75f, 0.55f), 0.1f);
        var rock = UrpLit("Shader_0417_Rock", new Color(0.45f, 0.43f, 0.42f), 0.2f);
        // 화면 좌우·위쪽 끝까지 지형이 덮도록 넓게 (예전 30m 폭은 화면 가장자리에서 끊겨 스카이박스 아래쪽이 베이지 띠로 보였음)
        Part(PrimitiveType.Cube, "SeaFloor", null, new Vector3(0f, -3.5f, 0f), new Vector3(120f, 1f, 40f), sand);
        float slope = Mathf.Atan2(3.8f, 9f) * Mathf.Rad2Deg;
        Part(PrimitiveType.Cube, "Beach", null, new Vector3(0f, -1.2f, 5f), new Vector3(120f, 0.5f, 10f), sand).transform.rotation = Quaternion.Euler(-slope, 0f, 0f);
        Part(PrimitiveType.Cube, "BeachTop", null, new Vector3(0f, 0.35f, 69.5f), new Vector3(240f, 0.5f, 120f), sand);
        Part(PrimitiveType.Sphere, "Island", null, new Vector3(-3.5f, -1.3f, -0.5f), Vector3.one * 3.2f, rock);
        Part(PrimitiveType.Sphere, "ShallowRock", null, new Vector3(2f, -2.6f, 0.5f), Vector3.one * 1.8f, rock);
        Part(PrimitiveType.Cylinder, "Pillar", null, new Vector3(3.5f, -1.2f, -2.5f), new Vector3(0.6f, 2.5f, 0.6f), rock);
        var box = Part(PrimitiveType.Cube, "FloatingBox", null, new Vector3(0f, 0f, -3f), Vector3.one * 0.9f, UrpLit("Shader_0417_Box", new Color(0.7f, 0.45f, 0.25f), 0.2f));

        // ── 물: 촘촘한 격자 메시 (0.5m 간격 → 버텍스 물결이 부드럽게). 화면 좌우 끝까지 덮도록 80x24m ──
        // 기본 Plane(10m, 정점 11x11)을 3배 키우면 정점 간격 3m 라 물결이 각지고, 30m 폭이라 화면 옆에서 끊겼음
        var waterMesh = SaveAsset(BuildWaterGrid(new Vector2(80f, 24f), 0.5f), GeneratedSub, "0417_WaterGrid.asset");
        var waterMat = ShaderMat("Shader_0417_Water", "Water", m =>
        {
            m.SetTexture("_NoiseTex", NoiseTexture());
            m.SetTextureScale("_NoiseTex", new Vector2(0.5f, 0.5f));
            m.SetFloat("_DepthMaxDistance", 2.5f);
            m.SetFloat("_FoamDistance", 0.35f);
        });
        var water = new GameObject("Water");
        water.transform.position = Vector3.zero; // 수면 높이 0, z -12 ~ 12 (뒤쪽 끝은 모래 아래에 묻힘)
        water.AddComponent<MeshFilter>().sharedMesh = waterMesh;
        var waterRenderer = water.AddComponent<MeshRenderer>();
        waterRenderer.sharedMaterial = waterMat;
        waterRenderer.shadowCastingMode = ShadowCastingMode.Off;
        waterRenderer.receiveShadows = false;

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

    // XZ 평면 격자 (가운데가 원점, 법선 위쪽). 셰이더가 정점을 위아래로 흔들어 물결을 만듦
    private static Mesh BuildWaterGrid(Vector2 size, float cell)
    {
        int nx = Mathf.Max(1, Mathf.RoundToInt(size.x / cell));
        int nz = Mathf.Max(1, Mathf.RoundToInt(size.y / cell));
        var vertices = new Vector3[(nx + 1) * (nz + 1)];
        var normals = new Vector3[vertices.Length];
        var uvs = new Vector2[vertices.Length];
        for (int z = 0; z <= nz; z++)
        for (int x = 0; x <= nx; x++)
        {
            int i = z * (nx + 1) + x;
            vertices[i] = new Vector3(-size.x * 0.5f + size.x * x / nx, 0f, -size.y * 0.5f + size.y * z / nz);
            normals[i] = Vector3.up;
            uvs[i] = new Vector2((float)x / nx, (float)z / nz);
        }

        var triangles = new int[nx * nz * 6];
        int t = 0;
        for (int z = 0; z < nz; z++)
        for (int x = 0; x < nx; x++)
        {
            int i = z * (nx + 1) + x;
            // 위에서 봤을 때 시계 방향 → 앞면이 위쪽 (셰이더가 Cull Back)
            triangles[t++] = i; triangles[t++] = i + nx + 1; triangles[t++] = i + 1;
            triangles[t++] = i + 1; triangles[t++] = i + nx + 1; triangles[t++] = i + nx + 2;
        }

        var mesh = new Mesh { name = "WaterGrid" };
        if (vertices.Length > 65000) mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        // 셰이더가 정점을 위아래로 움직이므로 컬링 경계를 조금 키움
        var b = mesh.bounds;
        b.Expand(new Vector3(0f, 2f, 0f));
        mesh.bounds = b;
        return mesh;
    }
}
