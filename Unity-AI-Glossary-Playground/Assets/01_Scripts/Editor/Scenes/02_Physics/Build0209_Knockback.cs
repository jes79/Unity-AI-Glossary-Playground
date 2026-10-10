using GlossaryDemo.PhysicsDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0209_Knockback
{
    [GlossaryScene("0209")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0209_Knockback")]
    public static void Build()
    {
        var ctx = NewScene3D("0209",
            "WASD : 이동   Space : 앞쪽 공격 (맞은 적을 넉백)\nK : 넉백 ON/OFF\n적이나 회전 망치에 부딪히면 공격자 반대 방향으로 3m 밀려납니다. OFF 와 비교해 타격감 차이를 보세요",
            true, 30f);
        ctx.Camera.transform.position = new Vector3(0f, 14f, -13f);
        ctx.Camera.transform.rotation = Quaternion.Euler(50f, 0f, 0f);

        var player = Player(new Vector3(0f, 1f, -2f));
        var pkb = player.gameObject.AddComponent<KnockbackReceiver>();
        SetBool(pkb, "knockFromEnemyContact", true);
        Set(pkb, "body", player.GetComponent<Renderer>());

        // 쫓아오는 적 (넉백 받는 쪽도 됨)
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
            SetFloat(chaser, "speed", 2f);
            var kb = e.AddComponent<KnockbackReceiver>();
            Set(kb, "body", e.GetComponent<Renderer>());
        }

        // 회전 망치 (Kinematic Rigidbody, Enemy 태그)
        var hammerRoot = new GameObject("SpinningHammer");
        hammerRoot.transform.position = new Vector3(-5f, 0.6f, -5f);
        var hrb = hammerRoot.AddComponent<Rigidbody>();
        hrb.isKinematic = true;
        hammerRoot.AddComponent<KinematicSpinner>();
        var bar = Prim(PrimitiveType.Cube, "HammerBar", new Vector3(1.75f, 0f, 0f), new Vector3(3.5f, 0.6f, 0.4f), new Color32(200, 90, 80, 255), hammerRoot.transform);
        bar.tag = "Enemy";
        Prim(PrimitiveType.Cylinder, "HammerPivot", Vector3.zero, new Vector3(0.6f, 0.6f, 0.6f), new Color32(90, 90, 100, 255), hammerRoot.transform);

        var slash = Prim(PrimitiveType.Sphere, "SlashVisual", Vector3.zero, Vector3.one, Color.white);
        Object.DestroyImmediate(slash.GetComponent<Collider>());
        slash.GetComponent<Renderer>().sharedMaterial = TransparentMat("KnockbackSlash", new Color(1f, 1f, 0.6f, 0.3f));
        slash.SetActive(false);

        var attacker = player.gameObject.AddComponent<KnockbackAttacker>();
        SetLayerMask(attacker, "enemyMask", LayerMask.GetMask("Enemy"));
        Set(attacker, "slashVisual", slash.transform);
        Save(ctx);
    }
}
