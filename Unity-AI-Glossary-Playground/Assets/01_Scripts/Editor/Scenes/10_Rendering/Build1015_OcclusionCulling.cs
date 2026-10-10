using System.Collections.Generic;
using GlossaryDemo;
using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1015_OcclusionCulling
{
    private const string Id = "1015";

    [GlossaryScene(Id)]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1015_OcclusionCulling")]
    public static void Build()
    {
        var ctx = NewScene3D(Id, "W/S : 전진·후진   A/D : 회전   O : 가려진 물체 실제로 끄기(시뮬레이션)\n" +
                                 "오른쪽 아래 미니맵: <color=#8F8>초록</color> 보임 / <color=#F77>빨강</color> 벽에 가려짐 / 회색 화면 밖\n" +
                                 "굽기: 메뉴 <b>Tools > Glossary > Rendering > Bake Occlusion Culling for 1015</b>", true, 34f);
        var canvas = EnsureCanvas(ctx);
        int obstacle = LayerMask.NameToLayer("Obstacle");
        int minimap = LayerMask.NameToLayer("Minimap");

        // 1인칭 카메라
        ctx.Camera.transform.position = new Vector3(-10f, 1.7f, -12f);
        ctx.Camera.transform.rotation = Quaternion.identity;
        ctx.Camera.cullingMask = ~(1 << minimap);
        ctx.Camera.useOcclusionCulling = true; // 기본값 true. 구운 데이터가 있을 때만 효과
        ctx.Ground.isStatic = true;

        // 3x3 방 (한 칸 10m, 가운데 문 2m) — 벽 = Occluder Static
        var wallColor = new Color(0.7f, 0.68f, 0.65f);
        var walls = new GameObject("Walls (Occluder Static)").transform;
        void Wall(Vector3 a, Vector3 b)
        {
            Vector3 c = (a + b) * 0.5f;
            Vector3 size = new Vector3(Mathf.Abs(b.x - a.x) + 0.4f, 4f, Mathf.Abs(b.z - a.z) + 0.4f);
            var w = Prim(PrimitiveType.Cube, "Wall", new Vector3(c.x, 2f, c.z), size, wallColor, walls);
            w.layer = obstacle;
            SetStatic(w, StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.BatchingStatic);
        }
        Wall(new Vector3(-15f, 0, -15f), new Vector3(15f, 0, -15f));
        Wall(new Vector3(-15f, 0, 15f), new Vector3(15f, 0, 15f));
        Wall(new Vector3(-15f, 0, -15f), new Vector3(-15f, 0, 15f));
        Wall(new Vector3(15f, 0, -15f), new Vector3(15f, 0, 15f));
        foreach (float line in new[] { -5f, 5f })
            for (int k = 0; k < 3; k++)
            {
                float s = -15f + k * 10f;
                Wall(new Vector3(line, 0, s), new Vector3(line, 0, s + 4f));      // 세로 벽 (문 양옆)
                Wall(new Vector3(line, 0, s + 6f), new Vector3(line, 0, s + 10f));
                Wall(new Vector3(s, 0, line), new Vector3(s + 4f, 0, line));      // 가로 벽
                Wall(new Vector3(s + 6f, 0, line), new Vector3(s + 10f, 0, line));
            }

        // 소품 = Occludee Static + 미니맵 표시
        var visibleMat = UnlitMat("Occ1015_Visible", new Color(0.3f, 0.95f, 0.4f));
        var occludedMat = UnlitMat("Occ1015_Occluded", new Color(1f, 0.3f, 0.3f));
        var outMat = UnlitMat("Occ1015_OutOfView", new Color(0.45f, 0.45f, 0.5f));
        var props = new List<Renderer>();
        var markers = new List<Renderer>();
        var propRoot = new GameObject("Props (Occludee Static)").transform;
        Color[] colors = { new Color(0.9f, 0.5f, 0.3f), new Color(0.4f, 0.6f, 0.95f), new Color(0.9f, 0.85f, 0.35f) };
        int idx = 0;
        for (int rx = 0; rx < 3; rx++)
            for (int rz = 0; rz < 3; rz++)
                for (int n = 0; n < 4; n++)
                {
                    float x = -15f + rx * 10f + 2.5f + (n % 2) * 5f;
                    float z = -15f + rz * 10f + 2.5f + (n / 2) * 5f;
                    var type = n % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere;
                    var p = Prim(type, $"Prop_{idx}", new Vector3(x, 0.6f, z), Vector3.one * 1.2f, colors[idx % 3], propRoot);
                    Object.DestroyImmediate(p.GetComponent<Collider>());
                    SetStatic(p, StaticEditorFlags.OccludeeStatic);
                    props.Add(p.GetComponent<Renderer>());

                    var mk = Prim(PrimitiveType.Cylinder, $"Marker_{idx}", new Vector3(x, 5f, z), new Vector3(1.4f, 0.05f, 1.4f), Color.white, propRoot);
                    Object.DestroyImmediate(mk.GetComponent<Collider>());
                    mk.layer = minimap;
                    mk.GetComponent<Renderer>().sharedMaterial = outMat;
                    markers.Add(mk.GetComponent<Renderer>());
                    idx++;
                }

        // 미니맵에서 플레이어 위치 표시 (카메라 자식)
        var me = Prim(PrimitiveType.Cube, "Player Marker", new Vector3(0f, 4f, 0f), new Vector3(0.8f, 0.1f, 1.6f), PlayerColor, ctx.Camera.transform);
        Object.DestroyImmediate(me.GetComponent<Collider>());
        me.layer = minimap;
        me.GetComponent<Renderer>().sharedMaterial = UnlitMat("Occ1015_Player", PlayerColor);

        // 미니맵 카메라 (위에서 내려다봄, 화면 오른쪽 아래)
        var mmGo = new GameObject("Minimap Camera");
        mmGo.transform.position = new Vector3(0f, 40f, 0f);
        mmGo.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        var mm = mmGo.AddComponent<Camera>();
        mmGo.AddComponent<UniversalAdditionalCameraData>();
        mm.orthographic = true;
        mm.orthographicSize = 16f;
        mm.rect = new Rect(0.73f, 0.1f, 0.25f, 0.44f);
        mm.depth = 5;
        mm.clearFlags = CameraClearFlags.SolidColor;
        mm.backgroundColor = BgColor;
        mm.useOcclusionCulling = false;

        var viz = new GameObject("OcclusionVisualizer").AddComponent<OcclusionVisualizer>();
        Set(viz, "playerCam", ctx.Camera);
        SetArray(viz, "props", props.ToArray());
        SetArray(viz, "markers", markers.ToArray());
        SetLayerMask(viz, "occluderMask", 1 << obstacle);
        Set(viz, "visibleMat", visibleMat);
        Set(viz, "occludedMat", occludedMat);
        Set(viz, "outOfViewMat", outMat);

        var b = CreateButtonRow(canvas.transform, 60f, "O 가려진 것 끄기");
        OnClick(b[0], viz.ToggleSimulate);

        HowToPanel(canvas.transform, "실내 맵 Occlusion Culling 굽기",
            "1. 벽·바닥 등 큰 정적 물체: Static ▼ > <b>Occluder Static</b>\n" +
            "2. 가려질 수 있는 정적 소품: <b>Occludee Static</b>\n" +
            "3. Window > Rendering > <b>Occlusion Culling</b>\n" +
            "4. Bake 탭 > Smallest Occluder (벽 두께 정도), Bake\n" +
            "5. Visualization 탭 + Scene 창 카메라 이동으로 확인\n" +
            "6. Camera 의 <b>Occlusion Culling</b> 체크 (기본 ON)\n\n" +
            "<color=#9BE89B>이 씬 메뉴</color>\nTools > Glossary > Rendering >\n · Bake Occlusion Culling for 1015\n · Open Occlusion Culling Window\n\n" +
            "움직이는 물체(캐릭터)는 Occludee 로만 판정되고\n화면 밖 판정(Frustum Culling)은 굽지 않아도 항상 자동",
            560f, 470f);

        Save(ctx);
    }

    // 굽기는 빌더에서 하지 않고 사용자가 메뉴로 실행
    [MenuItem("Tools/Glossary/Rendering/Bake Occlusion Culling for 1015")]
    public static void Bake()
    {
        if (!OpenScene()) return;
        StaticOcclusionCulling.smallestOccluder = 2f;
        StaticOcclusionCulling.smallestHole = 0.25f;
        StaticOcclusionCulling.backfaceThreshold = 100f;
        StaticOcclusionCulling.GenerateInBackground(); // 백그라운드 굽기 (진행률은 Occlusion 창/상태바)
        EditorApplication.ExecuteMenuItem("Window/Rendering/Occlusion Culling");
        Debug.Log("[1015] Occlusion Culling 굽기 시작");
    }

    [MenuItem("Tools/Glossary/Rendering/Open Occlusion Culling Window")]
    public static void OpenWindow() => EditorApplication.ExecuteMenuItem("Window/Rendering/Occlusion Culling");

    [MenuItem("Tools/Glossary/Rendering/Clear Occlusion Culling for 1015")]
    public static void Clear()
    {
        if (!OpenScene()) return;
        StaticOcclusionCulling.Clear();
    }

    private static bool OpenScene()
    {
        string path = GlossaryCatalog.Find(Id).ScenePath;
        if (SceneManager.GetActiveScene().path == path) return true;
        if (!System.IO.File.Exists(path))
        {
            EditorUtility.DisplayDialog(Id, "씬이 없습니다. 먼저 Build Scene/10 Rendering/1015 를 실행하세요.", "확인");
            return false;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
        EditorSceneManager.OpenScene(path);
        return true;
    }
}
