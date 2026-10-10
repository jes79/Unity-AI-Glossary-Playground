using System.Collections.Generic;
using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1014_LOD
{
    [GlossaryScene("1014")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1014_LOD")]
    public static void Build()
    {
        var ctx = NewScene3D("1014", "W/S 또는 휠 : 카메라 멀리·가까이   C : LOD 단계별 색칠\n" +
                                     "0 / 1 / 2 : LOD 강제 고정   F : 자동으로 되돌리기\n" +
                                     "멀어질수록 창문·지붕이 사라지고 결국 상자 → 안 그림(Culled)", true, 300f);
        ctx.Camera.farClipPlane = 400f;
        var canvas = EnsureCanvas(ctx);

        var wall = new Color(0.85f, 0.8f, 0.7f);
        var roofC = new Color(0.7f, 0.3f, 0.25f);
        var glass = new Color(0.4f, 0.65f, 0.9f);
        var groups = new List<LODGroup>();
        for (int i = 0; i < 3; i++)
        {
            var root = new GameObject($"Building_{i} (LOD Group)");
            root.transform.position = new Vector3(-12f + i * 12f, 0f, 0f);

            // LOD0: 몸체 + 창문 16개 + 문 + 지붕 + 굴뚝 (자세히)
            var lod0 = new List<Renderer>();
            var l0 = new GameObject("LOD0").transform; l0.SetParent(root.transform, false);
            lod0.Add(Part(PrimitiveType.Cube, "Body", l0, new Vector3(0f, 3f, 0f), new Vector3(6f, 6f, 5f), wall));
            for (int f = 0; f < 2; f++)
                for (int w = 0; w < 4; w++)
                    for (int side = 0; side < 2; side++)
                        lod0.Add(Part(PrimitiveType.Cube, "Window", l0, new Vector3(-2.1f + w * 1.4f, 2f + f * 2.4f, side == 0 ? -2.53f : 2.53f), new Vector3(0.8f, 1f, 0.08f), glass));
            lod0.Add(Part(PrimitiveType.Cube, "Door", l0, new Vector3(0f, 0.8f, -2.55f), new Vector3(1.1f, 1.6f, 0.1f), new Color(0.4f, 0.25f, 0.15f)));
            var roof0 = Part(PrimitiveType.Cube, "Roof", l0, new Vector3(0f, 6.9f, 0f), new Vector3(4.6f, 4.6f, 5.4f), roofC);
            roof0.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            lod0.Add(roof0);
            lod0.Add(Part(PrimitiveType.Cylinder, "Chimney", l0, new Vector3(1.8f, 8.2f, 1f), new Vector3(0.6f, 1f, 0.6f), new Color(0.5f, 0.45f, 0.45f)));

            // LOD1: 몸체 + 지붕만
            var lod1 = new List<Renderer>();
            var l1 = new GameObject("LOD1").transform; l1.SetParent(root.transform, false);
            lod1.Add(Part(PrimitiveType.Cube, "Body", l1, new Vector3(0f, 3f, 0f), new Vector3(6f, 6f, 5f), wall));
            var roof1 = Part(PrimitiveType.Cube, "Roof", l1, new Vector3(0f, 6.9f, 0f), new Vector3(4.6f, 4.6f, 5.4f), roofC);
            roof1.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            lod1.Add(roof1);

            // LOD2: 상자 하나
            var l2 = new GameObject("LOD2").transform; l2.SetParent(root.transform, false);
            var lod2 = new List<Renderer> { Part(PrimitiveType.Cube, "Box", l2, new Vector3(0f, 4f, 0f), new Vector3(6f, 8f, 5f), wall) };

            // LOD Group 3단계: 화면 높이의 60% 이상 LOD0, 25% 이상 LOD1, 8% 이상 LOD2, 그 아래 Culled
            var group = root.AddComponent<LODGroup>();
            group.SetLODs(new[]
            {
                new LOD(0.60f, lod0.ToArray()),
                new LOD(0.25f, lod1.ToArray()),
                new LOD(0.08f, lod2.ToArray()),
            });
            group.fadeMode = LODFadeMode.None;
            group.RecalculateBounds();
            groups.Add(group);
        }

        var demo = new GameObject("LodDemo").AddComponent<LodDemo>();
        Set(demo, "cam", ctx.Camera);
        SetArray(demo, "buildings", groups.ToArray());
        SetArray(demo, "debugMaterials",
            UnlitMat("Lod1014_Debug0", new Color(0.3f, 0.9f, 0.4f)),
            UnlitMat("Lod1014_Debug1", new Color(1f, 0.85f, 0.3f)),
            UnlitMat("Lod1014_Debug2", new Color(1f, 0.35f, 0.3f)));

        var b = CreateButtonRow(canvas.transform, 60f, "가까이", "멀리", "C 색칠", "자동 LOD");
        OnClick(b[0], demo.Near);
        OnClick(b[1], demo.Far);
        OnClick(b[2], demo.ToggleColorize);
        OnClick(b[3], demo.ForceLod, -1);

        HowToPanel(canvas.transform, "LOD Group 3단계 설정",
            "1. 빈 오브젝트(건물 루트)에 <b>LOD Group</b> 추가\n" +
            "2. 자식으로 LOD0(고품질) / LOD1 / LOD2(저품질) 모델 배치\n" +
            "3. LOD Group 막대의 각 칸을 클릭 → <b>Add</b> 로 렌더러 지정\n" +
            "4. 칸 경계를 드래그해 전환 비율 조절 (60% / 25% / 8%)\n" +
            "5. Fade Mode = Cross Fade 면 부드럽게 전환\n\n" +
            "<color=#9BE89B>코드</color> (이 씬)\n" +
            "group.SetLODs(new[] {\n  new LOD(0.6f, lod0Renderers),\n  new LOD(0.25f, lod1Renderers),\n  new LOD(0.08f, lod2Renderers) });\n\n" +
            "Project Settings > Quality > <b>LOD Bias</b> 로 전체 조절",
            560f, 500f);

        Save(ctx);
    }

    private static Renderer Part(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Color color)
    {
        var go = Prim(type, name, pos, scale, color, parent);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        return go.GetComponent<Renderer>();
    }
}
