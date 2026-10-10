using System.Collections.Generic;
using GlossaryDemo;
using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1003_RealtimeBakedLighting
{
    private const string Id = "1003";

    [GlossaryScene(Id)]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1003_RealtimeBakedLighting")]
    public static void Build()
    {
        var ctx = NewScene3D(Id, "R / [Realtime 라이트] : 비교용 실시간 라이트 12개 ON/OFF (프레임 시간 비교)\n" +
                                 "굽기: 메뉴 <b>Tools > Glossary > Rendering > Bake Lighting for 1003</b>\n" +
                                 "구운 뒤 Play → 건물 그림자·주황 빛이 라이트맵으로 고정, 움직이는 공은 Light Probe 로 빛을 받음", true, 30f);
        ctx.Camera.transform.position = new Vector3(0f, 10f, -14f);
        ctx.Camera.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
        var canvas = EnsureCanvas(ctx);

        // 태양은 Mixed (그림자 정적 부분은 굽고, 동적 물체는 실시간)
        ctx.Light.intensity = 0.4f;
        ctx.Light.lightmapBakeType = LightmapBakeType.Mixed;
        SetStatic(ctx.Ground, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic);

        // 움직이지 않는 건물 = Static (Contribute GI) → 라이트맵을 받음
        var wall = new Color(0.8f, 0.78f, 0.72f);
        for (int i = 0; i < 4; i++)
        {
            var b = Prim(PrimitiveType.Cube, $"Building_{i}", new Vector3(-9f + i * 6f, 2f, 4f), new Vector3(3.5f, 4f, 3.5f), wall);
            SetStatic(b, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic);
            var roof = Prim(PrimitiveType.Cube, $"Roof_{i}", new Vector3(-9f + i * 6f, 4.3f, 4f), new Vector3(4f, 0.6f, 4f), new Color(0.6f, 0.3f, 0.25f));
            SetStatic(roof, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic);
        }

        // Baked 라이트: 건물 앞 주황 가로등 (굽고 나면 런타임 비용 0)
        var baked = new List<Light>();
        for (int i = 0; i < 4; i++)
        {
            var l = new GameObject($"Baked Lamp_{i}").AddComponent<Light>();
            l.type = LightType.Point;
            l.lightmapBakeType = LightmapBakeType.Baked;   // ← 핵심: Mode = Baked
            l.color = new Color(1f, 0.6f, 0.25f);
            l.range = 8f;
            l.intensity = 5f;
            l.shadows = LightShadows.Soft;
            l.transform.position = new Vector3(-9f + i * 6f, 2.5f, 1.2f);
            baked.Add(l);
        }

        // Realtime 라이트: 비교용 12개 (매 프레임 계산)
        var realtime = new List<Light>();
        for (int i = 0; i < 12; i++)
        {
            var l = new GameObject($"Realtime Light_{i}").AddComponent<Light>();
            l.type = LightType.Point;
            l.lightmapBakeType = LightmapBakeType.Realtime;
            l.color = Color.HSVToRGB(i / 12f, 0.6f, 1f);
            l.range = 5f;
            l.intensity = 3f;
            l.shadows = LightShadows.Hard;
            l.transform.position = new Vector3(-11f + i * 2f, 1.2f, -5f);
            l.enabled = false;
            realtime.Add(l);
        }

        // 움직이는 공 = 동적 → 라이트맵 대신 Light Probe 사용
        var ball = Prim(PrimitiveType.Sphere, "Moving Ball (Dynamic)", new Vector3(0f, 0.6f, -1.5f), Vector3.one * 1.2f, Color.white);

        // Light Probe Group: 바닥 위 격자
        var probes = new GameObject("Light Probe Group").AddComponent<LightProbeGroup>();
        var pts = new List<Vector3>();
        for (int x = -12; x <= 12; x += 3)
            for (int z = -6; z <= 6; z += 3)
            {
                pts.Add(new Vector3(x, 0.5f, z));
                pts.Add(new Vector3(x, 3f, z));
            }
        probes.probePositions = pts.ToArray();

        // 라이팅 설정 에셋: 빠르게 구워지도록 낮은 해상도/샘플
        var settings = new LightingSettings
        {
            name = "Lighting1003",
            bakedGI = true,
            realtimeGI = false,
            lightmapper = LightingSettings.Lightmapper.ProgressiveCPU,
            lightmapResolution = 8f,
            lightmapPadding = 2,
            directSampleCount = 16,
            indirectSampleCount = 64,
            environmentSampleCount = 64,
            mixedBakeMode = MixedLightingMode.Shadowmask,
            autoGenerate = false, // 빌더에서는 절대 굽지 않음
        };
        SaveAsset(settings, Sub, "Lighting1003.lighting");
        Lightmapping.lightingSettings = settings;

        var demo = new GameObject("BakedLightingDemo").AddComponent<BakedLightingDemo>();
        SetArray(demo, "bakedLights", baked.ToArray());
        SetArray(demo, "realtimeLights", realtime.ToArray());
        Set(demo, "movingBall", ball.transform);

        var b2 = CreateButtonRow(canvas.transform, 60f, "Realtime 라이트");
        OnClick(b2[0], demo.ToggleRealtime);

        HowToPanel(canvas.transform, "Baked 조명으로 굽는 법",
            "1. 움직이지 않는 건물 선택 → Inspector 오른쪽 위 <b>Static</b> 체크\n" +
            "   (최소 <b>Contribute GI</b>)\n" +
            "2. 라이트 선택 → Light > <b>Mode</b>\n" +
            "   · Realtime : 매 프레임 계산 (움직이는 빛)\n" +
            "   · Baked : 라이트맵에 미리 저장 (런타임 비용 ≈ 0)\n" +
            "   · Mixed : 정적은 굽고 동적 물체엔 실시간\n" +
            "3. 움직이는 물체용 <b>Light Probe Group</b> 배치\n" +
            "4. Window > Rendering > <b>Lighting</b> > Generate Lighting\n" +
            "   (이 씬은 메뉴 Tools > Glossary > Rendering > Bake Lighting for 1003)\n" +
            "5. 결과: 씬 폴더에 Lightmap-*.exr + LightingData 생성\n\n" +
            "<color=#9BE89B>관찰</color>: 굽기 전/후 Baked 라이트의 bakingOutput 이\n바뀌고 라이트맵 장수가 늘어남. 굽기 후 Baked 가로등은\n움직이는 공에 그림자를 만들지 않음 (구운 빛이라서)",
            600f, 560f);

        Save(ctx);
    }

    // 굽기는 무거운 작업이라 빌더가 아닌 별도 메뉴로 사용자가 직접 실행
    [MenuItem("Tools/Glossary/Rendering/Bake Lighting for 1003")]
    public static void Bake()
    {
        if (!OpenScene()) return;
        Lightmapping.BakeAsync(); // 비동기 굽기 (진행률은 에디터 오른쪽 아래에 표시)
        Debug.Log("[1003] 라이트맵 굽기 시작 (Lighting 창에서 진행 상황 확인)");
    }

    [MenuItem("Tools/Glossary/Rendering/Clear Baked Lighting for 1003")]
    public static void ClearBake()
    {
        if (!OpenScene()) return;
        Lightmapping.Clear();
        Lightmapping.ClearLightingDataAsset();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    private static bool OpenScene()
    {
        string path = GlossaryCatalog.Find(Id).ScenePath;
        if (SceneManager.GetActiveScene().path == path) return true;
        if (!System.IO.File.Exists(path))
        {
            EditorUtility.DisplayDialog("1003", "씬이 없습니다. 먼저 Build Scene/10 Rendering/1003 을 실행하세요.", "확인");
            return false;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
        EditorSceneManager.OpenScene(path);
        return true;
    }
}
