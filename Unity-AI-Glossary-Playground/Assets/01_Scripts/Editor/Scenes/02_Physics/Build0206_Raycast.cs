using GlossaryDemo.PhysicsDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0206_Raycast
{
    [GlossaryScene("0206")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0206_Raycast")]
    public static void Build()
    {
        var ctx = NewScene3D("0206",
            "WASD / 방향키 / 마우스 오른쪽 드래그 : 둘러보기\n왼쪽 클릭 또는 Space : 화면 중앙 Raycast → 맞은 이름 Debug.Log 출력\nL : LayerMask 로 유리창(Obstacle 레이어) 무시\n화면 가운데 아래 글자는 매 프레임 쏘는 Raycast 결과입니다",
            true, 40f);
        ctx.Camera.transform.position = new Vector3(0f, 1.7f, -9f);
        ctx.Camera.transform.rotation = Quaternion.identity;

        // 표적들 (이름이 출력되므로 알아보기 쉬운 이름)
        Prim(PrimitiveType.Cube, "빨간 상자", new Vector3(-4f, 0.75f, 2f), Vector3.one * 1.5f, EnemyColor);
        Prim(PrimitiveType.Sphere, "파란 공", new Vector3(0f, 1f, 4f), Vector3.one * 2f, PlayerColor);
        Prim(PrimitiveType.Capsule, "초록 캡슐", new Vector3(4f, 1f, 2f), Vector3.one, new Color32(110, 200, 120, 255));
        Prim(PrimitiveType.Cylinder, "노란 기둥", new Vector3(-7f, 2f, 8f), new Vector3(1f, 2f, 1f), new Color32(230, 200, 80, 255));
        Prim(PrimitiveType.Cube, "뒤쪽 벽", new Vector3(0f, 3f, 14f), new Vector3(30f, 6f, 0.5f), new Color32(100, 105, 120, 255));
        Prim(PrimitiveType.Cube, "숨은 상자", new Vector3(7f, 0.75f, 8f), Vector3.one * 1.5f, new Color32(200, 120, 220, 255));

        // 유리창 (Obstacle 레이어, 반투명). L 로 무시하면 뒤의 "숨은 상자" 가 맞음
        var glass = Prim(PrimitiveType.Cube, "유리창 (Obstacle)", new Vector3(7f, 1.5f, 5f), new Vector3(3f, 3f, 0.1f), Color.white);
        glass.layer = LayerMask.NameToLayer("Obstacle");
        glass.GetComponent<Renderer>().sharedMaterial = TransparentMat("Glass", new Color(0.6f, 0.85f, 1f, 0.3f));

        // 맞은 점 표시
        var marker = Prim(PrimitiveType.Sphere, "HitMarker", Vector3.zero, Vector3.one * 0.15f, new Color32(255, 230, 90, 255));
        Object.DestroyImmediate(marker.GetComponent<Collider>());
        marker.layer = LayerMask.NameToLayer("Ignore Raycast");

        // 카메라 자식 총구 (레이 선을 그릴 시작점)
        var gunTip = new GameObject("GunTip").transform;
        gunTip.SetParent(ctx.Camera.transform, false);
        gunTip.localPosition = new Vector3(0.4f, -0.3f, 0.6f);
        var line = Line("RayLine", 0.03f, new Color(1f, 0.9f, 0.4f));
        line.enabled = false;

        ctx.Canvas = CreateCanvas("DemoCanvas");
        var cross = CreateText("Crosshair", ctx.Canvas.transform, Vector2.zero, new Vector2(60f, 60f), "+", 48f, Color.white);
        cross.rectTransform.anchorMin = cross.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        var aim = CreateText("AimText", ctx.Canvas.transform, new Vector2(0f, -60f), new Vector2(600f, 40f), "", 26f, new Color(1f, 0.9f, 0.5f));
        aim.rectTransform.anchorMin = aim.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        var log = RightPanelText(ctx, "DebugLog", "<b>Debug.Log 출력</b>", new Vector2(520f, 300f));

        var caster = ctx.Camera.gameObject.AddComponent<CenterRaycaster>();
        Set(caster, "cam", ctx.Camera);
        Set(caster, "hitMarker", marker.transform);
        Set(caster, "rayLine", line);
        Set(caster, "gunTip", gunTip);
        Set(caster, "aimText", aim);
        Set(caster, "logText", log);
        Save(ctx);
    }
}
