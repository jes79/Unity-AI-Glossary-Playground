using GlossaryDemo.Systems;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0803_IFrame
{
    [GlossaryScene("0803")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0803_IFrame")]
    public static void Build()
    {
        var ctx = NewScene2D("0803", "WASD : 이동 (위에서 본 2D)\nI : 무적 시간(i-frame) ON/OFF\n빨간 가시에 닿으면 HP -1 → 1초 동안 깜빡이며 무적\nOFF 로 바꾸고 가시에 닿아 보세요 (매 프레임 맞아서 HP 가 순식간에 사라짐)");
        ctx.Camera.transform.position = new Vector3(3f, 0f, -10f);

        // 가시 스프라이트 (톱니 모양)
        var spike = CreateSprite("IFrame_Spike", 64, 64, (u, v) =>
        {
            float a = Mathf.Atan2(v - 0.5f, u - 0.5f);
            float r = new Vector2(u - 0.5f, v - 0.5f).magnitude;
            float edge = 0.32f + 0.14f * Mathf.Abs(Mathf.Sin(a * 4f));
            return r < edge ? new Color(1f, 1f, 1f, 1f) : Color.clear;
        }, 64f);

        // 가시 함정들 (Enemy 태그 + 트리거)
        Vector3[] pos = { new(0f, 2f), new(3f, -2f), new(6f, 1f), new(9f, -1f), new(2f, 0f) };
        Vector3[] move = { new(0f, -3f), new(3f, 0f), new(0f, -3.5f), new(-2f, 3f), Vector3.zero };
        for (int i = 0; i < pos.Length; i++)
        {
            var s = CreateSpriteObject($"Spike_{i}", spike, pos[i], new Vector2(1f, 1f), new Color(0.95f, 0.25f, 0.3f), null, 1);
            s.gameObject.tag = "Enemy";
            var col = s.gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
            var rb = s.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            var m = s.gameObject.AddComponent<PingPongMover>();
            SetVector3(m, "offset", move[i]);
            SetFloat(m, "speed", 0.35f + i * 0.05f);
            SetFloat(m, "spin", 180f);
        }

        // 바깥 벽
        Platform2D("Wall_Top", new Vector3(4f, 4.6f), new Vector2(16f, 0.4f));
        Platform2D("Wall_Bottom", new Vector3(4f, -4.6f), new Vector2(16f, 0.4f));
        Platform2D("Wall_Left", new Vector3(-4f, 0f), new Vector2(0.4f, 9.6f));
        Platform2D("Wall_Right", new Vector3(12f, 0f), new Vector2(0.4f, 9.6f));

        var player = CreateSpriteObject("Player", CircleSprite, new Vector3(-2.5f, -3f), new Vector2(0.8f, 0.8f), PlayerColor, null, 2);
        player.gameObject.layer = LayerMask.NameToLayer("Player");
        var prb = player.gameObject.AddComponent<Rigidbody2D>();
        prb.gravityScale = 0f;
        prb.constraints = RigidbodyConstraints2D.FreezeRotation;
        prb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        player.gameObject.AddComponent<CircleCollider2D>().radius = 0.45f;

        ctx.Canvas = CreateCanvas("Canvas");
        var hpFill = Bar(ctx.Canvas.transform, "HpBar", new Vector2(250f, -60f), new Vector2(600f, 30f), new Color(0.4f, 0.9f, 0.4f));
        AnchorTop(hpFill.transform.parent.GetComponent<RectTransform>());

        var ctrl = player.gameObject.AddComponent<IFramePlayer>();
        Set(ctrl, "sprite", player);
        Set(ctrl, "hpFill", hpFill);
        SetFloat(ctrl, "invincibleTime", 1f);

        Save(ctx);
    }
}
