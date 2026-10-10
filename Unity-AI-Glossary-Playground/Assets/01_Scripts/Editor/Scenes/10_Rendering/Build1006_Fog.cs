using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1006_Fog
{
    [GlossaryScene("1006")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1006_Fog")]
    public static void Build()
    {
        var ctx = NewScene3D("1006", "W/S : 전진·후진   A/D : 회전\n" +
                                     "F : Fog ON/OFF   M : Linear → Exp → Exp2   + / - : 시야 거리 ±5m\n" +
                                     "기둥은 5m 간격 → 20m(4번째 기둥) 너머가 어둠에 묻히는지 관찰", false);
        ctx.Camera.transform.position = new Vector3(0f, 1.7f, -2f);
        ctx.Camera.transform.rotation = Quaternion.identity;
        ctx.Camera.clearFlags = CameraClearFlags.SolidColor;
        ctx.Camera.farClipPlane = 200f;
        var canvas = EnsureCanvas(ctx);

        // 던전 분위기: 약한 달빛 + 횃불 몇 개
        ctx.Light.intensity = 0.25f;
        ctx.Light.color = new Color(0.6f, 0.7f, 1f);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.25f, 0.24f, 0.28f);

        // 짙은 Fog: Linear, End = 20m (요청 예문 수치)
        var fogColor = new Color(0.06f, 0.06f, 0.08f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogStartDistance = 4f;
        RenderSettings.fogEndDistance = 20f;
        ctx.Camera.backgroundColor = fogColor;

        // 긴 복도 (길이 120m)
        var stone = new Color(0.45f, 0.43f, 0.4f);
        Prim(PrimitiveType.Cube, "Floor", new Vector3(0f, -0.5f, 58f), new Vector3(8f, 1f, 124f), new Color(0.3f, 0.28f, 0.26f));
        Prim(PrimitiveType.Cube, "Ceiling", new Vector3(0f, 5.5f, 58f), new Vector3(8f, 1f, 124f), new Color(0.25f, 0.24f, 0.23f));
        Prim(PrimitiveType.Cube, "Wall_L", new Vector3(-4.5f, 2.5f, 58f), new Vector3(1f, 6f, 124f), stone);
        Prim(PrimitiveType.Cube, "Wall_R", new Vector3(4.5f, 2.5f, 58f), new Vector3(1f, 6f, 124f), stone);
        for (int i = 1; i <= 24; i++)
        {
            float z = i * 5f - 2f; // 카메라(z=-2) 에서 정확히 5m 간격
            Prim(PrimitiveType.Cube, $"Pillar_L_{i}", new Vector3(-3.3f, 2.5f, z), new Vector3(0.8f, 5f, 0.8f), i % 4 == 0 ? new Color(0.8f, 0.65f, 0.2f) : stone); // 20m 마다 노란 기둥
            Prim(PrimitiveType.Cube, $"Pillar_R_{i}", new Vector3(3.3f, 2.5f, z), new Vector3(0.8f, 5f, 0.8f), i % 4 == 0 ? new Color(0.8f, 0.65f, 0.2f) : stone); // 20m 마다 노란 기둥
            if (i == 2 || i == 4) // TMP 글자는 Fog 영향을 받지 않아 가까운 표지만 둠
            {
                var label = WorldLabel($"{i * 5}m", new Vector3(0f, 0.02f, z), 8f, null, new Color(1f, 0.8f, 0.3f));
                label.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            if (i % 3 == 0)
            {
                var torch = new GameObject($"Torch_{i}").AddComponent<Light>();
                torch.type = LightType.Point;
                torch.color = new Color(1f, 0.55f, 0.2f);
                torch.range = 6f;
                torch.intensity = 3f;
                torch.transform.position = new Vector3(i % 2 == 0 ? -2.8f : 2.8f, 3f, z);
            }
        }

        var demo = new GameObject("FogDemo").AddComponent<FogDemo>();
        Set(demo, "cam", ctx.Camera);
        SetColor(demo, "fogColor", fogColor);

        var b = CreateButtonRow(canvas.transform, 60f, "F Fog ON/OFF", "M 모드", "시야 -5m", "시야 +5m");
        OnClick(b[0], demo.ToggleFog);
        OnClick(b[1], demo.NextMode);
        OnClick(b[2], demo.ChangeDistance, -5f);
        OnClick(b[3], demo.ChangeDistance, 5f);

        HowToPanel(canvas.transform, "Fog 설정하는 곳",
            "Window > Rendering > <b>Lighting</b> > Environment > <b>Other Settings</b>\n" +
            "· Fog 체크\n· Mode = <b>Linear</b>, Start = 4, <b>End = 20</b>\n· Color = 어두운 남색\n" +
            "· Camera > Background Type = Solid Color, 안개와 같은 색\n\n" +
            "<color=#9BE89B>코드</color>\nRenderSettings.fog = true;\nRenderSettings.fogMode = FogMode.Linear;\n" +
            "RenderSettings.fogEndDistance = 20f;\n\n" +
            "URP 커스텀 셰이더는 MixFog() 를 호출해야 안개가 적용됨\n(URP Lit/Unlit 은 자동)",
            520f, 440f);

        Save(ctx);
    }
}
