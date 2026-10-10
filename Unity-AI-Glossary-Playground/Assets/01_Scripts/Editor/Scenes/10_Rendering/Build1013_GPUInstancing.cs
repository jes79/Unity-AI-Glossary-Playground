using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1013_GPUInstancing
{
    [GlossaryScene("1013")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1013_GPUInstancing")]
    public static void Build()
    {
        var ctx = NewScene3D("1013", "I : GPU Instancing ↔ GameObject 방식 전환\n" +
                                     "↑ / ↓ : 나무 수 ±1000 (1000 ~ 10000)\n" +
                                     "Hierarchy 에 나무 오브젝트가 하나도 없는데 1000그루가 보이는 것을 관찰", true, 120f);
        ctx.Ground.transform.position = new Vector3(0f, -0.5f, 30f);
        ctx.Ground.GetComponent<Renderer>().sharedMaterial = ColorMat(new Color(0.35f, 0.5f, 0.3f));
        ctx.Camera.transform.position = new Vector3(0f, 22f, -30f);
        ctx.Camera.transform.rotation = Quaternion.Euler(30f, 0f, 0f);
        ctx.Camera.farClipPlane = 300f;
        var canvas = EnsureCanvas(ctx);

        var forest = new GameObject("InstancedForest").AddComponent<InstancedForest>();
        Set(forest, "trunkMesh", BuiltinMesh(PrimitiveType.Cylinder));
        Set(forest, "leavesMesh", BuiltinMesh(PrimitiveType.Sphere));
        Set(forest, "trunkMaterial", InstancedLit("Inst1013_Trunk", new Color(0.45f, 0.3f, 0.18f)));   // Enable GPU Instancing ✓
        Set(forest, "leavesMaterial", InstancedLit("Inst1013_Leaves", new Color(0.25f, 0.6f, 0.3f)));

        var b = CreateButtonRow(canvas.transform, 60f, "I 방식 전환", "나무 -1000", "나무 +1000");
        OnClick(b[0], forest.ToggleMode);
        OnClick(b[1], forest.ChangeCount, -1000);
        OnClick(b[2], forest.ChangeCount, 1000);

        HowToPanel(canvas.transform, "GPU Instancing 쓰는 법",
            "<b>A. 머티리얼 체크만</b>\n" +
            "· 머티리얼 Inspector > Advanced > <b>Enable GPU Instancing</b>\n" +
            "· 같은 메시+머티리얼 오브젝트가 자동으로 묶임\n" +
            "  (URP 는 SRP Batcher 가 우선 → 끄거나 B 방식 사용)\n\n" +
            "<b>B. 코드로 직접 (이 씬)</b>\n" +
            "var rp = new RenderParams(material);\n" +
            "Graphics.RenderMeshInstanced(rp, mesh, 0, matrices);\n" +
            "· matrices = 나무마다 위치·회전·크기 (Matrix4x4.TRS)\n" +
            "· 매 프레임 Update 에서 호출\n" +
            "· GameObject 가 없어 충돌·클릭은 따로 처리\n\n" +
            "지형 나무는 Terrain 의 Tree/Detail 도 인스턴싱 사용",
            560f, 480f);

        Save(ctx);
    }
}
