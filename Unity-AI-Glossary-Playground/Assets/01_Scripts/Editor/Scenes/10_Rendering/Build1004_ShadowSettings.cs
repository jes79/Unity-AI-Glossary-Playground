using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1004_ShadowSettings
{
    [GlossaryScene("1004")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1004_ShadowSettings")]
    public static void Build()
    {
        var ctx = NewScene3D("1004", "D : 그림자 거리 바꾸기 (10/30/50/100/150m)   S : Soft ↔ Hard\n" +
                                     "M : 모바일 프리셋 (30m, Soft 끔)   P : PC 프리셋 (150m, Soft)\n" +
                                     "멀리 있는 기둥의 그림자가 거리 밖에서 사라지는 지점을 관찰", false);
        var ground = Prim(PrimitiveType.Cube, "Ground", new Vector3(0f, -0.5f, 70f), new Vector3(30f, 1f, 170f), GroundColor);
        ground.layer = LayerMask.NameToLayer("Ground");
        ctx.Camera.transform.position = new Vector3(0f, 6f, -10f);
        ctx.Camera.transform.rotation = Quaternion.Euler(14f, 0f, 0f);
        ctx.Camera.farClipPlane = 300f;
        ctx.Light.transform.rotation = Quaternion.Euler(35f, -60f, 0f);
        var canvas = EnsureCanvas(ctx);

        // 5m 간격 기둥 + 10m 마다 거리 표지
        for (int i = 0; i <= 30; i++)
        {
            float z = i * 5f;
            Prim(PrimitiveType.Cube, $"Pillar_L_{i}", new Vector3(-4f, 1.5f, z), new Vector3(0.6f, 3f, 0.6f), new Color(0.85f, 0.85f, 0.88f));
            Prim(PrimitiveType.Cube, $"Pillar_R_{i}", new Vector3(4f, 1.5f, z), new Vector3(0.6f, 3f, 0.6f), new Color(0.85f, 0.85f, 0.88f));
            if (i % 2 == 0)
            {
                var label = WorldLabel($"{z + 10f:0}m", new Vector3(0f, 0.05f, z), 6f, null, new Color(1f, 0.85f, 0.4f));
                label.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }
        // 가까운 곳 디테일 (Soft/Hard 차이 관찰용)
        Prim(PrimitiveType.Sphere, "Sphere", new Vector3(-1.5f, 1f, 2f), Vector3.one * 2f, new Color(0.9f, 0.5f, 0.4f));
        Prim(PrimitiveType.Cylinder, "Cylinder", new Vector3(1.5f, 1f, 3f), new Vector3(0.8f, 1f, 0.8f), new Color(0.4f, 0.7f, 0.9f));

        var demo = new GameObject("ShadowSettingsDemo").AddComponent<ShadowSettingsDemo>();
        Set(demo, "sun", ctx.Light);

        var b = CreateButtonRow(canvas.transform, 60f, "D 거리 변경", "S Soft/Hard", "M 모바일", "P PC");
        OnClick(b[0], demo.NextDistance);
        OnClick(b[1], demo.ToggleSoft);
        OnClick(b[2], demo.MobilePreset);
        OnClick(b[3], demo.PcPreset);

        HowToPanel(canvas.transform, "모바일 그림자 설정 (에디터)",
            "1. Project 창에서 사용 중인 <b>URP Asset</b> 선택\n   (Project Settings > Quality 의 Render Pipeline Asset)\n" +
            "2. Shadows > <b>Max Distance = 30</b>\n" +
            "3. Shadows > Cascade Count = 1 ~ 2\n" +
            "4. Shadows > <b>Soft Shadows 체크 해제</b>\n   (또는 라이트 Shadow Type = Hard Shadows)\n" +
            "5. Main Light Shadow Resolution 1024 이하\n\n" +
            "<color=#9BE89B>코드로</color>\n" +
            "var urp = (UniversalRenderPipelineAsset)\n  GraphicsSettings.currentRenderPipeline;\n" +
            "urp.shadowDistance = 30f;\n" +
            "light.shadows = LightShadows.Hard;\n\n" +
            "<color=#FC6>주의</color>: Play 중 에셋을 바꾸면 파일에 남음 →\n이 데모는 씬을 나갈 때 원래 값으로 되돌림",
            520f, 560f);

        Save(ctx);
    }
}
