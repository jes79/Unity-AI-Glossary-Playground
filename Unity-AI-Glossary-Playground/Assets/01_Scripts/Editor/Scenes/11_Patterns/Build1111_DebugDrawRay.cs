using GlossaryDemo.Patterns;
using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build1111_DebugDrawRay
{
    [GlossaryScene("1111")]
    [MenuItem("Tools/Glossary/Build Scene/11 Patterns/1111_DebugDrawRay")]
    public static void Build()
    {
        var ctx = NewScene3D("1111",
            "마우스 : 포탑 조준 → Raycast 선 (맞으면 빨강 + 법선 하늘색, 안 맞으면 초록)\n클릭 : Debug.Log 로 명중 정보 출력 (Console 창 + 화면 아래 미니 콘솔)\n" +
            "A : 자동 회전   L : LineRenderer 끄기 → Debug.DrawRay 는 Scene 뷰 / Game 뷰 Gizmos ON 에서만 보임", true, 30f);
        PatternsBuildUtil.LookAt(ctx.Camera, new Vector3(0f, 15f, -13f), new Vector3(0f, 0f, 1.5f));

        int obstacle = LayerMask.NameToLayer("Obstacle");
        var obstacleMat = ColorMat(new Color(0.55f, 0.57f, 0.65f));
        Vector3[] boxes = { new(-6f, 1f, 5f), new(-2f, 0.75f, 8f), new(3f, 1.25f, 6f), new(7f, 1f, 2f), new(-8f, 1f, 0f), new(5f, 0.75f, 9f) };
        for (int i = 0; i < boxes.Length; i++)
        {
            var b = Prim(PrimitiveType.Cube, $"Box_{i + 1}", boxes[i], new Vector3(1.6f, boxes[i].y * 2f, 1.6f), Color.white);
            b.GetComponent<Renderer>().sharedMaterial = obstacleMat;
            b.layer = obstacle;
        }
        for (int i = 0; i < 2; i++)
        {
            var t = Prim(PrimitiveType.Sphere, $"MovingTarget_{i + 1}", new Vector3(-3f + i * 6f, 0.7f, 4f), Vector3.one * 1.4f, EnemyColor);
            t.layer = LayerMask.NameToLayer("Enemy");
            var w = t.AddComponent<MinimapWanderer>(); // 0615 의 배회 스크립트 재사용
            SetFloat(w, "range", 9f);
            SetFloat(w, "speed", 2f);
        }

        // 포탑
        var turret = new GameObject("Turret");
        turret.transform.position = new Vector3(0f, 0f, -3f);
        var tbase = Prim(PrimitiveType.Cylinder, "Base", new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.4f, 1.4f), new Color(0.3f, 0.35f, 0.45f), turret.transform);
        Object.DestroyImmediate(tbase.GetComponent<Collider>());
        var head = new GameObject("Head").transform;
        head.SetParent(turret.transform, false);
        head.localPosition = new Vector3(0f, 1.1f, 0f);
        var dome = Prim(PrimitiveType.Sphere, "Dome", Vector3.zero, Vector3.one * 0.9f, PlayerColor, head);
        Object.DestroyImmediate(dome.GetComponent<Collider>());
        var barrel = Prim(PrimitiveType.Cube, "Barrel", new Vector3(0f, 0f, 0.7f), new Vector3(0.25f, 0.25f, 1f), new Color(0.2f, 0.25f, 0.35f), head);
        Object.DestroyImmediate(barrel.GetComponent<Collider>());
        var muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(head, false);
        muzzle.localPosition = new Vector3(0f, 0f, 1.25f);

        var marker = Prim(PrimitiveType.Sphere, "HitMarker", Vector3.zero, Vector3.one * 0.3f, Color.yellow);
        Object.DestroyImmediate(marker.GetComponent<Collider>());
        marker.GetComponent<Renderer>().sharedMaterial = UnlitMat("Unlit_1111_Hit", new Color(1f, 0.9f, 0.2f));

        var lineGo = new GameObject("RayLine (LineRenderer)");
        var lr = lineGo.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.widthMultiplier = 0.08f;
        lr.useWorldSpace = true;
        lr.sharedMaterial = ParticleMat("Particle_1111_Line", Color.white, false, WhiteSprite.texture); // 정점 색(빨강/초록)을 쓰는 머티리얼
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var t2 = turret.AddComponent<RaycastDebugTurret>();
        Set(t2, "head", head);
        Set(t2, "muzzle", muzzle);
        SetLayerMask(t2, "hitMask", LayerMask.GetMask("Obstacle", "Enemy"));
        Set(t2, "line", lr);
        Set(t2, "hitMarker", marker.transform);

        // ── UI: 미니 콘솔 + 버튼 ──
        var canvas = CreateCanvas("UI Canvas");
        var root = canvas.transform;
        var consolePanel = Panel(root, "MiniConsole", Vector2.zero, new Vector2(900f, 290f), new Color(0.04f, 0.05f, 0.07f, 0.92f));
        Anchor(consolePanel.rectTransform, Vector2.zero, Vector2.zero, new Vector2(20f, 20f));
        CreateText("Title", consolePanel.transform, new Vector2(0f, 122f), new Vector2(860f, 30f), "미니 콘솔 (Application.logMessageReceived)", 18f, SubTextColor, TextAlignmentOptions.Left);
        var output = CreateText("Output", consolePanel.transform, Vector2.zero, Vector2.zero, "", 18f, TextColor, TextAlignmentOptions.BottomLeft);
        Stretch(output.rectTransform, 14f);
        output.rectTransform.offsetMax = new Vector2(-14f, -40f);
        output.richText = true;
        var console = consolePanel.gameObject.AddComponent<OnScreenConsole>();
        Set(console, "output", output);

        var col = ButtonColumn(root, new Vector2(760f, 120f), 300f, 58f, 21f, "Debug.Log", "Debug.LogWarning", "Debug.LogError", "자동 회전 (A)", "LineRenderer (L)", "콘솔 지우기");
        OnClick(col[0], t2.LogInfo);
        OnClick(col[1], t2.LogWarning);
        OnClick(col[2], t2.LogError);
        OnClick(col[3], t2.ToggleSweep);
        OnClick(col[4], t2.ToggleLine);
        OnClick(col[5], console.Clear);

        Save(ctx);
    }
}
