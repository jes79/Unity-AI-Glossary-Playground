using GlossaryDemo.Movement;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;

public static class Build0105_CoyoteTime
{
    [GlossaryScene("0105")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0105_CoyoteTime")]
    public static void Build()
    {
        var ctx = NewScene2D("0105", "A/D : 이동   Space : 점프\nC : 코요테 타임 ON/OFF\n발판 끝에서 살짝 떨어진 뒤 점프해 보세요");
        ctx.Camera.transform.position = new Vector3(2f, 0f, -10f);

        // 발판: 사이 간격을 일부러 넓게 → 코요테 타임 없으면 끝에서 점프가 자주 실패
        int ground = LayerMask.NameToLayer("Ground");
        Vector3[] platforms = { new(-5f, -3f), new(1f, -3f), new(7f, -2f), new(12f, -1f) };
        float[] widths = { 6f, 4f, 3.5f, 4f };
        for (int i = 0; i < platforms.Length; i++)
        {
            var p = CreateSpriteObject($"Platform_{i}", SquareSprite, platforms[i], new Vector2(widths[i], 0.6f), GroundColor);
            p.gameObject.layer = ground;
            p.gameObject.AddComponent<BoxCollider2D>();
        }

        var player = CreateSpriteObject("Player", SquareSprite, new Vector3(-6f, -1.5f), new Vector2(0.8f, 1f), PlayerColor, order: 1);
        player.gameObject.layer = LayerMask.NameToLayer("Player");
        var rb = player.gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        player.gameObject.AddComponent<BoxCollider2D>();

        var controller = player.gameObject.AddComponent<CoyoteTimePlayer>();
        Set(controller, "body", player);
        SetLayerMask(controller, "groundMask", LayerMask.GetMask("Ground"));

        Save(ctx);
    }
}
