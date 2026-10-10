using GlossaryDemo.Movement;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0114_TargetLock
{
    [GlossaryScene("0114")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0114_TargetLock")]
    public static void Build()
    {
        var ctx = NewScene3D("0114",
            "WASD : 이동   Tab : 가장 가까운 적 록온 / 해제\nQ / E : 다른 적으로 전환\n록온하면 몸이 적을 향하고 카메라가 플레이어 뒤에서 적을 바라봅니다. A/D 로 적 주위를 돌아 보세요",
            true, 50f);

        Vector3[] enemyPos = { new(6f, 0f, 6f), new(-7f, 0f, 8f), new(10f, 0f, -4f), new(-9f, 0f, -6f), new(0f, 0f, 14f) };
        for (int i = 0; i < enemyPos.Length; i++)
        {
            var e = Prim(PrimitiveType.Capsule, $"Enemy_{i + 1}", enemyPos[i] + Vector3.up, Vector3.one, EnemyColor);
            e.tag = "Enemy";
            e.layer = LayerMask.NameToLayer("Enemy");
            var eye = Prim(PrimitiveType.Cube, "Eye", new Vector3(0f, 0.45f, 0.45f), new Vector3(0.5f, 0.12f, 0.2f), Color.white, e.transform);
            Object.DestroyImmediate(eye.GetComponent<Collider>());
            e.AddComponent<LockOnWanderer>();
            Label3D($"Enemy_{i + 1}", new Vector3(0f, 1.4f, 0f), 4f, new Color(1f, 0.7f, 0.7f), e.transform);
        }
        for (int i = 0; i < 6; i++)
            Prim(PrimitiveType.Cube, $"Rock_{i}", new Vector3(-12f + i * 5f, 0.6f, 3f - (i % 3) * 5f), new Vector3(1.2f, 1.2f, 1.2f), new Color32(110, 116, 132, 255));

        var player = PlayerCapsule(new Vector3(0f, 1.1f, 0f));
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        player.AddComponent<CharacterController>().center = Vector3.zero;
        var lockOn = player.AddComponent<TargetLockPlayer>();

        var reticle = CreateSpriteObject("LockOnReticle", RingSprite, Vector3.zero, Vector2.one * 0.9f, new Color(1f, 0.85f, 0.3f));
        reticle.gameObject.AddComponent<WorldLabelBillboard>();
        reticle.gameObject.SetActive(false);
        Set(lockOn, "reticle", reticle.transform);
        Set(lockOn, "cameraTransform", ctx.Camera.transform);

        var cam = ctx.Camera.gameObject.AddComponent<LockOnCamera>();
        Set(cam, "player", lockOn);
        ctx.Camera.transform.position = new Vector3(0f, 5f, -7f);
        Save(ctx);
    }
}
