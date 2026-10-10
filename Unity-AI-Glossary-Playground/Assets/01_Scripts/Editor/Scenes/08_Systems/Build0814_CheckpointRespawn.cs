using GlossaryDemo.Systems;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0814_CheckpointRespawn
{
    [GlossaryScene("0814")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0814_CheckpointRespawn")]
    public static void Build()
    {
        var ctx = NewScene2D("0814", "A/D : 이동   Space : 점프   K : 스스로 죽기\nC : 체크포인트 사용 ON/OFF (OFF 면 항상 처음부터)\n깃발을 지나면 초록색으로 바뀌며 저장됨\n가시(빨강)에 닿거나 떨어지면 마지막 깃발에서 리스폰");

        var flagSprite = CreateSprite("Checkpoint_Flag", 64, 128, (u, v) =>
        {
            bool pole = u > 0.1f && u < 0.2f;
            bool cloth = u >= 0.2f && u < 0.2f + 0.7f * (1f - Mathf.Abs(v - 0.8f) / 0.18f) && v > 0.62f && v < 0.98f;
            return pole ? new Color(0.85f, 0.85f, 0.9f) : cloth ? Color.white : Color.clear;
        }, 64f);
        var spikeSprite = CreateSprite("Checkpoint_Spike", 64, 32, (u, v) =>
        {
            float tri = 1f - Mathf.Abs((u * 4f) % 1f - 0.5f) * 2f;
            return v < tri ? Color.white : Color.clear;
        }, 64f);

        // 발판 (구간 사이에 구멍)
        var level = new GameObject("Level").transform;
        float[] xs = { -4f, 5f, 13f, 21f, 30f, 39f };
        float[] ws = { 10f, 6f, 7f, 6f, 8f, 8f };
        float[] ys = { -3f, -2.5f, -2f, -2.6f, -2f, -2.4f };
        for (int i = 0; i < xs.Length; i++)
            Platform2D($"Platform_{i}", new Vector3(xs[i], ys[i]), new Vector2(ws[i], 0.6f), level);

        // 가시
        Vector3[] spikes = { new(3f, -2f), new(12f, -1.5f), new(14f, -1.5f), new(20f, -2.1f), new(28.5f, -1.5f), new(31f, -1.5f) };
        foreach (var s in spikes)
        {
            var sp = CreateSpriteObject("Spike", spikeSprite, s, new Vector2(1f, 1f), new Color(0.95f, 0.25f, 0.3f), level, 1);
            sp.gameObject.tag = "Enemy";
            var col = sp.gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.9f, 0.3f);
        }

        // 체크포인트 깃발
        Vector3[] flags = { new(7f, -1.2f), new(16f, -0.7f), new(23f, -1.3f), new(37f, -1.1f) };
        for (int i = 0; i < flags.Length; i++)
        {
            var f = CreateSpriteObject($"Flag_{i + 1}", flagSprite, flags[i], new Vector2(1f, 1f), new Color(0.65f, 0.65f, 0.7f), level, 1);
            f.gameObject.tag = "Checkpoint";
            var col = f.gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 3f);
        }

        var player = CreateSpriteObject("Player", SquareSprite, new Vector3(-6f, -2f), new Vector2(0.7f, 0.9f), new Color(0.3f, 0.65f, 1f), null, 3);
        player.gameObject.layer = LayerMask.NameToLayer("Player");
        var rb = player.gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        player.gameObject.AddComponent<BoxCollider2D>();
        var ctrl = player.gameObject.AddComponent<CheckpointPlayer>();
        SetLayerMask(ctrl, "groundMask", LayerMask.GetMask("Ground"));
        Set(ctrl, "sprite", player);
        Set(ctrl, "cam", ctx.Camera);

        ctx.Camera.transform.position = new Vector3(-3f, 0f, -10f);
        Save(ctx);
    }
}
