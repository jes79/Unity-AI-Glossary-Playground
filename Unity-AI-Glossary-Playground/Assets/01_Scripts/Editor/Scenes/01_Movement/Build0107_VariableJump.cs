using GlossaryDemo.Movement;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0107_VariableJump
{
    [GlossaryScene("0107")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0107_VariableJump")]
    public static void Build()
    {
        var ctx = NewScene2D("0107",
            "A/D : 이동   Space : 점프 (짧게 톡 / 길게 꾹)\nV : 가변 점프 ON/OFF\n노란 선이 마지막 점프의 최고점입니다. 누른 시간에 따라 높이가 달라지는지 보세요 (주황색 = 상승 속도 깎임)");

        const float floorTop = -4f;
        Platform2D("Floor", new Vector2(0f, floorTop - 0.3f), new Vector2(19f, 0.6f));
        Platform2D("Wall_L", new Vector2(-9.7f, 0f), new Vector2(0.4f, 11f));
        Platform2D("Wall_R", new Vector2(9.7f, 0f), new Vector2(0.4f, 11f));

        // 1m 눈금
        for (int m = 1; m <= 6; m++)
        {
            float y = floorTop + m;
            CreateSpriteObject($"Ruler_{m}m", SquareSprite, new Vector3(4f, y), new Vector2(10f, 0.02f), new Color(1f, 1f, 1f, 0.18f), order: -1);
            Label3D($"{m}m", new Vector3(9f, y + 0.2f, 0f), 3f, SubTextColor, billboard: false);
        }

        var marker = CreateSpriteObject("ApexMarker", SquareSprite, new Vector3(4f, floorTop), new Vector2(10f, 0.06f), new Color(1f, 0.85f, 0.3f), order: 0);

        var player = Player2D(new Vector2(2f, floorTop + 0.6f));
        var p = player.gameObject.AddComponent<VariableJumpPlayer>();
        Set(p, "body", player);
        Set(p, "apexMarker", marker.transform);
        SetLayerMask(p, "groundMask", LayerMask.GetMask("Ground"));
        Save(ctx);
    }
}
