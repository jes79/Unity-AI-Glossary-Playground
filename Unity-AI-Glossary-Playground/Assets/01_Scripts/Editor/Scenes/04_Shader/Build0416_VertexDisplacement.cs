using System.Collections.Generic;
using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0416_VertexDisplacement
{
    [GlossaryScene("0416")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0416_VertexDisplacement")]
    public static void Build()
    {
        var ctx = NewScene3D("0416",
            "Space : 바람 ON/OFF   슬라이더 : 세기, 속도, 물결 간격, 바람 방향\n" +
            "관찰: 뿌리는 고정되고 끝으로 갈수록 크게 휘며, 바람 방향으로 물결처럼 퍼짐 (그림자도 같이 흔들림)");
        FrameCamera(ctx, new Vector3(0f, 3.2f, -8f), new Vector3(0f, 0.4f, 0f));
        ctx.Ground.GetComponent<Renderer>().sharedMaterial = UrpLit("Shader_0416_Soil", new Color(0.3f, 0.24f, 0.16f), 0.1f);

        var mesh = BuildGrassMesh(5000, new Vector2(12f, 8f));
        SaveAsset(mesh, GeneratedSub, "0416_GrassPatch.asset");

        var mat = ShaderMat("Shader_0416_GrassWind", "GrassWind", m =>
        {
            m.SetColor("_BottomColor", new Color(0.08f, 0.25f, 0.08f));
            m.SetColor("_TopColor", new Color(0.55f, 0.85f, 0.3f));
            m.SetFloat("_WindStrength", 0.35f);
            m.SetFloat("_WindSpeed", 2f);
            m.SetFloat("_WindFrequency", 0.6f);
        });

        var grass = new GameObject("GrassPatch");
        grass.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = grass.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        mr.shadowCastingMode = ShadowCastingMode.On;

        // 바람 방향 화살표
        var arrow = new GameObject("WindArrow");
        arrow.transform.position = new Vector3(0f, 2.6f, 2f);
        var arrowMat = UrpUnlit("Shader_0416_Arrow", new Color(0.7f, 0.9f, 1f));
        Part(PrimitiveType.Cylinder, "Shaft", arrow.transform, Vector3.zero, new Vector3(0.08f, 0.6f, 0.08f), arrowMat).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        var head = Part(PrimitiveType.Cube, "Head", arrow.transform, new Vector3(0f, 0f, 0.65f), new Vector3(0.25f, 0.25f, 0.25f), arrowMat);
        head.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        foreach (var c in arrow.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
        WorldLabel("바람", new Vector3(0f, 3.2f, 2f), ctx.Camera, 2.4f);

        var demo = new GameObject("GrassWindDemo").AddComponent<GrassWindDemo>();
        Set(demo, "grass", mr);
        Set(demo, "windArrow", arrow.transform);

        var panel = SidePanel(ctx, "버텍스 애니메이션 (풀)");
        AddButton(panel, "Space : 바람 ON/OFF", demo.ToggleWind);
        AddSlider(panel, "바람 세기", 0f, 1f, 0.35f, demo.SetStrength);
        AddSlider(panel, "바람 속도", 0f, 8f, 2f, demo.SetSpeed);
        AddSlider(panel, "물결 간격 (Frequency)", 0f, 2f, 0.6f, demo.SetFrequency);
        AddSlider(panel, "바람 방향 (°)", 0f, 360f, 15f, demo.SetAngle);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }

    // 풀잎 = 정점 5개(아래 2, 중간 2, 끝 1)짜리 얇은 잎. uv.y = 높이 비율(0 뿌리 ~ 1 끝)
    private static Mesh BuildGrassMesh(int blades, Vector2 area)
    {
        var rng = new System.Random(16);
        float R() => (float)rng.NextDouble();
        var verts = new List<Vector3>(blades * 5);
        var normals = new List<Vector3>(blades * 5);
        var uvs = new List<Vector2>(blades * 5);
        var tris = new List<int>(blades * 9);

        for (int b = 0; b < blades; b++)
        {
            var root = new Vector3((R() - 0.5f) * area.x, 0f, (R() - 0.5f) * area.y);
            float yaw = R() * Mathf.PI * 2f;
            var side = new Vector3(Mathf.Cos(yaw), 0f, Mathf.Sin(yaw));
            var facing = Vector3.Cross(side, Vector3.up);
            float height = 0.35f + R() * 0.55f;
            float width = 0.05f + R() * 0.04f;
            var lean = facing * ((R() - 0.5f) * 0.25f); // 잎마다 살짝 기울기
            var normal = (facing + Vector3.up * 0.6f).normalized;

            int i0 = verts.Count;
            verts.Add(root - side * width);
            verts.Add(root + side * width);
            verts.Add(root - side * width * 0.6f + Vector3.up * height * 0.55f + lean * 0.4f);
            verts.Add(root + side * width * 0.6f + Vector3.up * height * 0.55f + lean * 0.4f);
            verts.Add(root + Vector3.up * height + lean);
            for (int k = 0; k < 5; k++) normals.Add(normal);
            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));
            uvs.Add(new Vector2(0f, 0.55f));
            uvs.Add(new Vector2(1f, 0.55f));
            uvs.Add(new Vector2(0.5f, 1f));
            tris.AddRange(new[] { i0, i0 + 2, i0 + 1, i0 + 1, i0 + 2, i0 + 3, i0 + 2, i0 + 4, i0 + 3 });
        }

        var mesh = new Mesh { name = "GrassPatch" };
        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        // 셰이더가 정점을 옆으로 밀어도 잘려 보이지 않게 경계 상자를 넉넉히
        var bounds = mesh.bounds;
        bounds.Expand(new Vector3(1.5f, 0.5f, 1.5f));
        mesh.bounds = bounds;
        return mesh;
    }
}
