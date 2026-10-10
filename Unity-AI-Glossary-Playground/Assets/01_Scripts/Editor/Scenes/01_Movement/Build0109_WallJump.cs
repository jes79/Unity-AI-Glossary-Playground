using GlossaryDemo.Movement;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0109_WallJump
{
    [GlossaryScene("0109")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0109_WallJump")]
    public static void Build()
    {
        var ctx = NewScene2D("0109",
            "A/D : 이동   Space : 점프 / 벽에 붙어서 점프 = 벽 점프\nW : 벽 타기·벽 점프 ON/OFF\n가운데 좁은 통로를 벽 점프로 번갈아 차고 올라가 맨 위 발판에 도착해 보세요 (주황색 = 미끄러지는 중)");

        Platform2D("Floor", new Vector2(0f, -4.8f), new Vector2(19.4f, 0.6f));
        Platform2D("Wall_L", new Vector2(-9.7f, 0f), new Vector2(0.4f, 11f));
        Platform2D("Wall_R", new Vector2(9.7f, 0f), new Vector2(0.4f, 11f));

        // 좁은 수직 통로 (벽 사이 2.6m)
        Platform2D("Shaft_L", new Vector2(1.2f, -0.6f), new Vector2(0.6f, 7.6f), new Color32(95, 105, 130, 255));
        Platform2D("Shaft_R", new Vector2(4.4f, 0.65f), new Vector2(0.6f, 7.1f), new Color32(95, 105, 130, 255)); // 아래 1.6m 틈으로 통로 진입
        Platform2D("Goal", new Vector2(7.2f, 4.1f), new Vector2(4.2f, 0.4f), new Color32(90, 160, 110, 255));
        Label3D("도착!", new Vector3(7.2f, 4.8f, 0f), 4f, new Color(0.6f, 1f, 0.6f), billboard: false);
        Label3D("← 아래 틈으로 들어가서\n벽 점프로 올라가기", new Vector3(6.8f, -1.5f, 0f), 3f, SubTextColor, billboard: false);

        var player = Player2D(new Vector2(7f, -4f));
        var p = player.gameObject.AddComponent<WallJumpPlayer>();
        Set(p, "body", player);
        SetLayerMask(p, "groundMask", LayerMask.GetMask("Ground"));
        Save(ctx);
    }
}
