using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0530_ScreenTransition
{
    [GlossaryScene("0530")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0530_ScreenTransition")]
    public static void Build()
    {
        var ctx = Stage("0530", "1 : 검은색 페이드 아웃(0.5초) → 씬 로드 → 페이드 인(0.5초)\n2 : 와이프(왼→오 덮고, 다시 오른쪽으로 걷힘)\n로드할 때마다 구역 A(숲) ↔ B(동굴)가 바뀜 — 화면이 가려진 순간에 바뀌는지 관찰",
            new Vector3(0f, 4f, -9f), new Vector3(18f, 0f, 0f));
        ctx.Camera.clearFlags = CameraClearFlags.SolidColor;

        // 구역 A: 숲 (밝은 초록 바닥, 나무)
        var areaA = new GameObject("Area_A_Forest");
        Prim(PrimitiveType.Cube, "Grass", new Vector3(0f, 0.01f, 0f), new Vector3(30f, 0.02f, 30f), new Color32(90, 160, 80, 255), areaA.transform);
        for (int i = 0; i < 9; i++)
        {
            float x = -6f + i * 1.6f, z = 2f + (i % 3) * 1.5f;
            Prim(PrimitiveType.Cylinder, $"Trunk_{i}", new Vector3(x, 0.8f, z), new Vector3(0.25f, 0.8f, 0.25f), new Color32(110, 75, 50, 255), areaA.transform);
            Prim(PrimitiveType.Sphere, $"Leaves_{i}", new Vector3(x, 2f, z), Vector3.one * 1.4f, new Color32(60, 150, 70, 255), areaA.transform);
        }
        Label3D("구역 A — 숲", new Vector3(0f, 0.6f, -2f), 6f, areaA.transform).color = Color.white;

        // 구역 B: 동굴 (어두운 바닥, 수정 기둥, 보라 조명)
        var areaB = new GameObject("Area_B_Cave");
        Prim(PrimitiveType.Cube, "Rock", new Vector3(0f, 0.01f, 0f), new Vector3(30f, 0.02f, 30f), new Color32(45, 40, 55, 255), areaB.transform);
        for (int i = 0; i < 7; i++)
        {
            var c = Prim(PrimitiveType.Cube, $"Crystal_{i}", new Vector3(-5f + i * 1.7f, 0.9f, 2.5f + (i % 2)), new Vector3(0.5f, 1.8f, 0.5f), Color.white, areaB.transform);
            c.transform.rotation = Quaternion.Euler(0f, 45f, (i % 3 - 1) * 12f);
            c.GetComponent<Renderer>().sharedMaterial = EmissiveMat("Crystal", new Color(0.4f, 0.3f, 0.8f), new Color(0.6f, 0.4f, 1f), 1.5f);
        }
        var caveLight = new GameObject("CaveLight").AddComponent<Light>();
        caveLight.transform.SetParent(areaB.transform, false);
        caveLight.transform.position = new Vector3(0f, 3f, 1f);
        caveLight.type = LightType.Point; caveLight.color = new Color(0.6f, 0.4f, 1f); caveLight.range = 12f; caveLight.intensity = 4f;
        Label3D("구역 B — 동굴", new Vector3(0f, 0.6f, -2f), 6f, areaB.transform).color = new Color(0.85f, 0.75f, 1f);
        areaB.SetActive(false);

        var demo = new GameObject("ScreenTransitionDemo").AddComponent<ScreenTransitionDemo>();
        Set(demo, "areaA", areaA);
        Set(demo, "areaB", areaB);
        Set(demo, "cam", ctx.Camera);
        var b = Buttons(ctx, "페이드 전환", "와이프 전환");
        OnClick(b[0], demo.GoFade);
        OnClick(b[1], demo.GoWipe);
        Save(ctx);
    }
}
