using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0419_Triplanar
{
    [GlossaryScene("0419")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0419_Triplanar")]
    public static void Build()
    {
        var ctx = NewScene3D("0419",
            "T : 트라이플래너 ↔ 메쉬 UV   M : 이끼 ON/OFF   Space : 바위 회전 ON/OFF\n슬라이더 : 이끼 양, Tiling, 경계 날카로움\n" +
            "관찰: 바위가 돌아가도 이끼는 항상 위쪽에만. UV 모드로 바꾸면 늘어난 절벽 텍스처가 찌그러짐");
        FrameCamera(ctx, new Vector3(0f, 4.5f, -9f), new Vector3(0f, 1.2f, 0f));
        ProceduralSky();

        // 텍스처: 회색 바위(노이즈 + 갈라진 선), 초록 이끼(얼룩 노이즈)
        var rockTex = CreateTexture("Shader_RockTex", 256, 256, (u, v) =>
        {
            float n = TileNoise(u, v, 4, 5, 41);
            float crack = Mathf.Pow(1f - Mathf.Abs(TileNoise(u, v, 3, 3, 43) * 2f - 1f), 14f);
            float g = Mathf.Lerp(0.32f, 0.62f, n) * (1f - crack * 0.6f);
            return new Color(g, g * 0.97f, g * 0.93f, 1f);
        });
        var mossTex = CreateTexture("Shader_MossTex", 256, 256, (u, v) =>
        {
            float n = TileNoise(u, v, 8, 4, 51);
            return Color.Lerp(new Color(0.12f, 0.3f, 0.06f), new Color(0.4f, 0.65f, 0.15f), n);
        });

        var mat = ShaderMat("Shader_0419_TriplanarMoss", "TriplanarMoss", m =>
        {
            m.SetTexture("_RockTex", rockTex);
            m.SetTexture("_MossTex", mossTex);
            m.SetFloat("_Tiling", 0.6f);
            m.SetFloat("_MossAmount", 0.5f);
        });

        var rockMesh = BuildRockMesh();
        SaveAsset(rockMesh, GeneratedSub, "0419_Rock.asset");

        var targets = new System.Collections.Generic.List<Renderer>();
        var spinners = new System.Collections.Generic.List<ShaderDemoSpinner>();
        (Vector3 pos, Vector3 scale, Vector3 spin)[] rocks =
        {
            (new Vector3(-3f, 1.1f, 0f), new Vector3(2.2f, 1.6f, 2f), new Vector3(15f, 25f, 0f)),
            (new Vector3(0f, 1.3f, -0.5f), new Vector3(2.6f, 2.2f, 2.4f), new Vector3(0f, 20f, 18f)),
            (new Vector3(3f, 0.9f, 0.2f), new Vector3(1.8f, 1.3f, 1.6f), new Vector3(-20f, -15f, 0f)),
        };
        for (int i = 0; i < rocks.Length; i++)
        {
            var go = new GameObject($"Rock_{i}");
            go.transform.position = rocks[i].pos;
            go.transform.localScale = rocks[i].scale;
            go.AddComponent<MeshFilter>().sharedMesh = rockMesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            targets.Add(mr);
            spinners.Add(Spin(go, rocks[i].spin));
        }

        // 길게 늘린 절벽(큐브): 메쉬 UV 로는 텍스처가 늘어나지만 트라이플래너는 월드 크기 그대로
        var cliff = Part(PrimitiveType.Cube, "StretchedCliff", null, new Vector3(0f, 1.5f, 4f), new Vector3(11f, 3f, 1.5f), mat);
        targets.Add(cliff.GetComponent<Renderer>());

        var demo = new GameObject("TriplanarDemo").AddComponent<TriplanarDemo>();
        SetArray(demo, "targets", targets.ToArray());
        SetArray(demo, "rollingRocks", spinners.ToArray());

        var panel = SidePanel(ctx, "트라이플래너 + 이끼");
        AddButton(panel, "T : 트라이플래너 ↔ UV", demo.ToggleTriplanar);
        AddButton(panel, "M : 이끼 ON/OFF", demo.ToggleMoss);
        AddSlider(panel, "이끼 양", 0f, 1f, 0.5f, demo.SetMossAmount);
        AddSlider(panel, "Tiling (월드 1m 당 반복)", 0.1f, 2f, 0.6f, demo.SetTiling);
        AddSlider(panel, "투영 경계 날카로움", 1f, 16f, 4f, demo.SetSharpness);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }

    // 구 메쉬를 노이즈로 울퉁불퉁하게 → 바위. 같은 위치의 정점은 같은 양만큼 밀어서 이음새가 벌어지지 않게
    private static Mesh BuildRockMesh()
    {
        var temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var src = temp.GetComponent<MeshFilter>().sharedMesh;
        var mesh = Object.Instantiate(src);
        mesh.name = "Rock";
        Object.DestroyImmediate(temp);

        var verts = mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            var p = verts[i];
            var d = p.normalized;
            float n = Mathf.PerlinNoise(d.x * 1.7f + 3.1f, d.y * 1.7f + 7.3f) * 0.6f
                      + Mathf.PerlinNoise(d.z * 2.3f + 1.7f, d.x * 2.3f + 5.9f) * 0.4f;
            float flat = d.y < -0.5f ? 0.85f : 1f; // 바닥 쪽은 조금 납작하게
            verts[i] = d * 0.5f * (0.78f + n * 0.45f) * flat;
        }
        mesh.vertices = verts;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
