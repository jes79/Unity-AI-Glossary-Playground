using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0405_MetallicSmoothness
{
    [GlossaryScene("0405")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0405_MetallicSmoothness")]
    public static void Build()
    {
        var ctx = NewScene3D("0405",
            "슬라이더 : 갑옷 Metallic / Smoothness   R : 요청 값(0.9 / 0.7)\n" +
            "관찰: 왼쪽 표에서 오른쪽(Smoothness↑)일수록 반사가 또렷, 위(Metallic↑)일수록 주변 색을 비춤");
        FrameCamera(ctx, new Vector3(-0.8f, 3f, -10f), new Vector3(-0.8f, 2.3f, 0f));
        ProceduralSky();

        // 5x5 표: 머티리얼마다 값이 달라 Inspector 에서 숫자를 직접 확인할 수 있음
        var baseColor = new Color(0.85f, 0.65f, 0.3f);
        for (int m = 0; m < 5; m++)
        {
            for (int s = 0; s < 5; s++)
            {
                float metallic = m / 4f, smooth = s / 4f;
                var mat = UrpLit($"Shader_0405_M{m * 25:000}_S{s * 25:000}", baseColor, smooth, metallic);
                Part(PrimitiveType.Sphere, $"Sphere_M{metallic:0.00}_S{smooth:0.00}", null,
                    new Vector3(-6.6f + s * 1.1f, 0.6f + m * 1.05f, 0f), Vector3.one * 0.9f, mat);
            }
        }
        WorldLabel("Smoothness 0 → 1", new Vector3(-4.4f, -0.25f, -0.6f), ctx.Camera, 2.6f);
        WorldLabel("Metallic\n0 → 1 (↑)", new Vector3(-8.3f, 2.7f, 0f), ctx.Camera, 2.4f);

        // 갑옷 기사: 요청 예문 값 Metallic 0.9, Smoothness 0.7
        var armorMat = UrpLit("Shader_0405_Armor", new Color(0.78f, 0.8f, 0.85f), 0.7f, 0.9f);
        var knight = new GameObject("ArmorKnight");
        knight.transform.position = new Vector3(2.6f, 0f, 0f);
        var parts = new[]
        {
            Part(PrimitiveType.Capsule, "Body", knight.transform, new Vector3(0f, 1.3f, 0f), new Vector3(1.3f, 1.2f, 1f), armorMat),
            Part(PrimitiveType.Sphere, "Helmet", knight.transform, new Vector3(0f, 3f, 0f), Vector3.one * 0.95f, armorMat),
            Part(PrimitiveType.Sphere, "Shoulder_L", knight.transform, new Vector3(-0.85f, 2.2f, 0f), Vector3.one * 0.7f, armorMat),
            Part(PrimitiveType.Sphere, "Shoulder_R", knight.transform, new Vector3(0.85f, 2.2f, 0f), Vector3.one * 0.7f, armorMat),
            Part(PrimitiveType.Cube, "Visor", knight.transform, new Vector3(0f, 3f, -0.42f), new Vector3(0.6f, 0.12f, 0.15f), armorMat),
        };
        Spin(knight, new Vector3(0f, 25f, 0f));
        WorldLabel("갑옷 (0.9 / 0.7)", new Vector3(2.6f, 4.1f, 0f), ctx.Camera, 2.6f);

        // 반사될 거리: 색 기둥을 빙 둘러 세움 (금속일수록 이 색들이 비쳐 보임)
        Color[] pillarColors = { new Color(1f, 0.3f, 0.3f), new Color(0.3f, 1f, 0.4f), new Color(0.3f, 0.5f, 1f), new Color(1f, 0.85f, 0.2f), new Color(0.9f, 0.3f, 1f), Color.white };
        for (int i = 0; i < 12; i++)
        {
            float a = i / 12f * Mathf.PI * 2f;
            var pos = new Vector3(Mathf.Cos(a) * 9f - 1f, 2f, Mathf.Sin(a) * 9f);
            if (pos.z < -4f) continue; // 카메라 앞을 가리지 않게
            Part(PrimitiveType.Cube, $"Pillar_{i}", null, pos, new Vector3(1f, 4f, 1f), UrpUnlit($"Shader_0405_Pillar_{i % pillarColors.Length}", pillarColors[i % pillarColors.Length]));
        }

        // 실시간 반사 프로브: 시작할 때 주변을 한 번 찍어 금속 반사에 사용
        var probeGo = new GameObject("Reflection Probe");
        probeGo.transform.position = new Vector3(-1f, 2f, 0f);
        var probe = probeGo.AddComponent<ReflectionProbe>();
        probe.mode = ReflectionProbeMode.Realtime;
        probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
        probe.size = new Vector3(30f, 12f, 30f);
        probe.resolution = 128;

        var demo = new GameObject("MetallicSmoothnessDemo").AddComponent<MetallicSmoothnessDemo>();
        var renderers = new Renderer[parts.Length];
        for (int i = 0; i < parts.Length; i++) renderers[i] = parts[i].GetComponent<Renderer>();
        SetArray(demo, "armorRenderers", renderers);
        Set(demo, "probe", probe);

        var panel = SidePanel(ctx, "갑옷 Metallic / Smoothness");
        AddSlider(panel, "Metallic", 0f, 1f, 0.9f, demo.SetMetallic);
        AddSlider(panel, "Smoothness", 0f, 1f, 0.7f, demo.SetSmoothness);
        AddButton(panel, "R : 요청 값 (0.9 / 0.7)", demo.ResetToPrompt);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
