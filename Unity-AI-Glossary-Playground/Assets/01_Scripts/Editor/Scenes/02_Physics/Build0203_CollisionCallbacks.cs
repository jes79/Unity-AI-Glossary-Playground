using GlossaryDemo.PhysicsDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0203_CollisionCallbacks
{
    [GlossaryScene("0203")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0203_CollisionCallbacks")]
    public static void Build()
    {
        var ctx = NewScene3D("0203",
            "WASD : 이동\n빨간 적과 부딪히면 OnCollisionEnter → 체력 -10\n초록 독 늪(트리거)에 들어가면 OnTriggerEnter, 안에 있으면 OnTriggerStay(초당 -5), 나오면 OnTriggerExit\n오른쪽 로그에서 어떤 콜백이 언제 불리는지 보세요",
            true, 24f);

        var player = Player(new Vector3(0f, 1f, -3f));
        var health = AddHealth(player.gameObject, 100f, 1.5f, false);

        // 적: Rigidbody + Enemy 태그. 플레이어를 쫓아옴
        Vector3[] spots = { new(-6f, 0.6f, 5f), new(6f, 0.6f, 5f), new(0f, 0.6f, 8f) };
        for (int i = 0; i < spots.Length; i++)
        {
            var e = Prim(PrimitiveType.Cube, $"Enemy_{i + 1}", spots[i], Vector3.one * 1.2f, EnemyColor);
            e.tag = "Enemy";
            e.layer = LayerMask.NameToLayer("Enemy");
            var rb = e.AddComponent<Rigidbody>();
            rb.mass = 2f;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            var chaser = e.AddComponent<ChaserEnemy>();
            Set(chaser, "target", player.transform);
            SetFloat(chaser, "speed", 1.8f + i * 0.5f);
        }

        // 독 늪: Is Trigger 박스 (보이는 판은 콜라이더 없음)
        var swamp = new GameObject("PoisonSwamp (Trigger)");
        swamp.transform.position = new Vector3(-5f, 0.5f, -4f);
        var box = swamp.AddComponent<BoxCollider>();
        box.size = new Vector3(5f, 1f, 4f);
        box.isTrigger = true;
        var plate = Prim(PrimitiveType.Cube, "SwampVisual", new Vector3(0f, -0.48f, 0f), new Vector3(5f, 0.04f, 4f), Color.white, swamp.transform);
        Object.DestroyImmediate(plate.GetComponent<Collider>());
        plate.GetComponent<Renderer>().sharedMaterial = TransparentMat("PoisonSwamp", new Color(0.4f, 1f, 0.3f, 0.45f));
        Label("독 늪 (Trigger)", new Vector3(-5f, 1.5f, -4f), 5f, new Color(0.7f, 1f, 0.6f));

        // 그냥 부딪히기만 하는 벽
        Prim(PrimitiveType.Cube, "Rock", new Vector3(5f, 0.75f, -3f), new Vector3(2f, 1.5f, 2f), new Color32(110, 116, 132, 255));

        var log = RightPanelText(ctx, "CallbackLog", "<b>콜백 로그</b>", new Vector2(620f, 420f));
        var events = player.gameObject.AddComponent<CollisionEventPlayer>();
        Set(events, "health", health);
        Set(events, "logText", log);
        Save(ctx);
    }
}
