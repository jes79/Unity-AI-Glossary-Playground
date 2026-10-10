using GlossaryDemo.Movement;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0106_JumpBuffering
{
    [GlossaryScene("0106")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0106_JumpBuffering")]
    public static void Build()
    {
        var ctx = NewScene2D("0106",
            "A/D : 이동   Space : 점프\nB : 점프 버퍼링 ON/OFF\n높은 곳에서 떨어지다 땅에 닿기 직전에 Space 를 눌러 보세요. ON 이면 착지 순간 바로 점프(노란색 = 예약 중)");

        Platform2D("Floor", new Vector2(0f, -4.6f), new Vector2(19f, 0.6f));
        Platform2D("Step_1", new Vector2(-1f, -2.2f), new Vector2(2.5f, 0.4f));
        Platform2D("Step_2", new Vector2(3f, -0.2f), new Vector2(2.5f, 0.4f));
        Platform2D("HighLedge", new Vector2(7.5f, 1.8f), new Vector2(3f, 0.4f));
        Platform2D("Wall_L", new Vector2(-9.7f, 0f), new Vector2(0.4f, 11f));
        Platform2D("Wall_R", new Vector2(9.7f, 0f), new Vector2(0.4f, 11f));

        var player = Player2D(new Vector2(-6f, -3.5f));
        var p = player.gameObject.AddComponent<JumpBufferPlayer>();
        Set(p, "body", player);
        SetLayerMask(p, "groundMask", LayerMask.GetMask("Ground"));
        Save(ctx);
    }
}
