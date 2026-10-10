using GlossaryDemo.Movement;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0102_RigidbodyMovement
{
    [GlossaryScene("0102")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0102_RigidbodyMovement")]
    public static void Build()
    {
        var ctx = NewScene3D("0102",
            "WASD : 이동   Space : 점프\nM : 이동 방식 전환 (velocity ↔ transform.position)\n상자를 밀고 벽에 몸을 비벼 보세요. transform 방식은 벽에서 떨리고 상자를 밀어내지 못하고 파고듭니다",
            true, 22f);
        ctx.Camera.transform.position = new Vector3(0f, 13f, -14f);
        ctx.Camera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

        // 둘레 벽
        var wallColor = new Color32(110, 116, 132, 255);
        Prim(PrimitiveType.Cube, "Wall_N", new Vector3(0f, 1f, 8f), new Vector3(16f, 2f, 0.5f), wallColor);
        Prim(PrimitiveType.Cube, "Wall_E", new Vector3(8f, 1f, 0f), new Vector3(0.5f, 2f, 16f), wallColor);
        Prim(PrimitiveType.Cube, "Wall_W", new Vector3(-8f, 1f, 0f), new Vector3(0.5f, 2f, 16f), wallColor);

        // 밀 수 있는 상자 (가벼운 것 / 무거운 것)
        for (int i = 0; i < 4; i++)
        {
            var box = Prim(PrimitiveType.Cube, $"LightBox_{i}", new Vector3(-3f + i * 1.5f, 0.5f, 3f), Vector3.one, new Color32(230, 180, 80, 255));
            box.AddComponent<Rigidbody>().mass = 1f;
        }
        var heavy = Prim(PrimitiveType.Cube, "HeavyBox (mass 20)", new Vector3(4f, 0.75f, -2f), Vector3.one * 1.5f, new Color32(170, 90, 70, 255));
        heavy.AddComponent<Rigidbody>().mass = 20f;
        Label3D("가벼운 상자 mass 1", new Vector3(-0.75f, 1.6f, 3f), 5f, Color.white);
        Label3D("무거운 상자 mass 20", new Vector3(4f, 2.2f, -2f), 5f, Color.white);

        var player = PlayerCapsule(new Vector3(-3f, 1f, -3f));
        var rb = player.AddComponent<Rigidbody>();
        rb.mass = 2f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // 넘어지지 않게 X/Z 회전 고정
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        var mover = player.AddComponent<RigidbodyMover>();
        SetLayerMask(mover, "groundMask", ~LayerMask.GetMask("Player"));

        Save(ctx);
    }
}
