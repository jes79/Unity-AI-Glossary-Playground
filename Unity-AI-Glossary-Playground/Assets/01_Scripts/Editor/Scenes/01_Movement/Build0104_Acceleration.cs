using GlossaryDemo.Movement;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0104_Acceleration
{
    [GlossaryScene("0104")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0104_Acceleration")]
    public static void Build()
    {
        var ctx = NewScene2D("0104",
            "A/D : 두 러너가 동시에 이동\n1 : 가속 0.2 / 감속 0.1 (요청값)   2 : 빙판   3 : 아주 빠름\n위(가속/감속)는 서서히 출발·미끄러지듯 멈추고, 아래(즉시)는 딱 끊깁니다. 속도 그래프 모양을 비교하세요");

        var smooth = CreateLane("Smooth", 0.8f, true, new Color32(120, 200, 255, 255), "가속/감속 적용");
        var instant = CreateLane("Instant", -2.8f, false, new Color32(255, 150, 110, 255), "즉시 반응 (가속 없음)");

        var demo = new GameObject("AccelerationDemo").AddComponent<AccelerationDemo>();
        Set(demo, "smooth", smooth);
        Set(demo, "instant", instant);
        Save(ctx);
    }

    private static AccelerationRunner CreateLane(string name, float y, bool useAccel, Color color, string label)
    {
        Platform2D($"Floor_{name}", new Vector2(0f, y - 0.8f), new Vector2(18.5f, 0.2f));
        var runner = CreateSpriteObject($"Runner_{name}", SquareSprite, new Vector3(-6f, y - 0.2f), new Vector2(0.8f, 1f), color, order: 1);
        var r = runner.gameObject.AddComponent<AccelerationRunner>();
        SetBool(r, "useAcceleration", useAccel);
        SetFloat(r, "accelerationTime", 0.2f);
        SetFloat(r, "decelerationTime", 0.1f);

        // 속도 그래프 (0 ~ 최고 속도)
        var graphGo = new GameObject($"SpeedGraph_{name}");
        var lr = graphGo.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.widthMultiplier = 0.06f;
        lr.positionCount = 0;
        lr.sharedMaterial = UnlitMat($"MoveGraph_{name}", color);
        Set(r, "graph", lr);
        var origin = new Vector2(-8.5f, y - 2.3f);
        SetVector2(r, "graphOrigin", origin);
        CreateSpriteObject($"GraphAxis_{name}", SquareSprite, new Vector3(0f, origin.y - 0.03f), new Vector2(17f, 0.03f), SubTextColor);
        CreateSpriteObject($"GraphMax_{name}", SquareSprite, new Vector3(0f, origin.y + 1.2f), new Vector2(17f, 0.02f), new Color(1f, 1f, 1f, 0.2f));

        Label3D(label, new Vector3(5.5f, y + 1f, 0f), 4f, color, billboard: false);
        Label3D("속도", new Vector3(-9.1f, origin.y + 0.6f, 0f), 3f, SubTextColor, billboard: false);
        return r;
    }
}
