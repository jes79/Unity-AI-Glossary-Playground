using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1012_DrawCallBatching
{
    [GlossaryScene("1012")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1012_DrawCallBatching")]
    public static void Build()
    {
        var ctx = NewScene3D("1012", "B : 일반 ↔ Static Batching 그리드   S : SRP Batcher ON/OFF\n" +
                                     "M : 일반 그리드에 MaterialPropertyBlock 색 (SRP Batcher 깨뜨리기)\n" +
                                     "상태창의 Batches / SetPass 숫자 변화를 관찰 (Game 창 Stats 와 같은 값)", true, 40f);
        ctx.Camera.transform.position = new Vector3(0f, 18f, -16f);
        ctx.Camera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        var canvas = EnsureCanvas(ctx);

        Color[] colors = { new Color(0.9f, 0.4f, 0.35f), new Color(0.35f, 0.75f, 0.45f), new Color(0.4f, 0.55f, 0.95f), new Color(0.95f, 0.8f, 0.3f) };
        GameObject MakeGrid(string name)
        {
            var root = new GameObject(name);
            for (int x = 0; x < 20; x++)
                for (int z = 0; z < 20; z++)
                {
                    var type = (x + z) % 3 == 0 ? PrimitiveType.Sphere : PrimitiveType.Cube;
                    var go = Prim(type, $"Box_{x}_{z}", new Vector3(-14.25f + x * 1.5f, 0.5f, -10f + z * 1.5f), Vector3.one * 0.9f, colors[(x * 7 + z * 3) % 4], root.transform);
                    Object.DestroyImmediate(go.GetComponent<Collider>());
                }
            return root;
        }
        var dynamicGrid = MakeGrid("Grid_Dynamic (일반)");
        var staticGrid = MakeGrid("Grid_StaticBatched (Start 에서 Combine)");
        staticGrid.SetActive(false);

        var demo = new GameObject("BatchingDemo").AddComponent<BatchingDemo>();
        Set(demo, "dynamicGrid", dynamicGrid);
        Set(demo, "staticGrid", staticGrid);

        var b = CreateButtonRow(canvas.transform, 60f, "B Static Batching", "S SRP Batcher", "M PropertyBlock");
        OnClick(b[0], demo.ToggleStatic);
        OnClick(b[1], demo.ToggleSrpBatcher);
        OnClick(b[2], demo.ToggleMpb);

        HowToPanel(canvas.transform, "Draw Call 줄이는 설정",
            "<b>Static Batching</b> (움직이지 않는 물체)\n" +
            "· 오브젝트 Inspector 오른쪽 위 Static ▼ > <b>Batching Static</b>\n" +
            "· Project Settings > Player > Other > Static Batching 체크\n" +
            "· 코드: StaticBatchingUtility.Combine(root) (이 씬)\n\n" +
            "<b>SRP Batcher</b> (URP 기본, 가장 효과 큼)\n" +
            "· URP Asset > Rendering > <b>SRP Batcher</b> 체크\n" +
            "· 같은 셰이더면 머티리얼이 달라도 묶임\n" +
            "· MaterialPropertyBlock 사용 시 빠짐 → 머티리얼로 구분\n" +
            "· 커스텀 셰이더는 CBUFFER(UnityPerMaterial) 필요\n\n" +
            "<b>확인</b>: Game 창 Stats, Window > Analysis > <b>Frame Debugger</b>\n(어떤 이유로 배치가 끊겼는지 표시)",
            580f, 520f);

        Save(ctx);
    }
}
