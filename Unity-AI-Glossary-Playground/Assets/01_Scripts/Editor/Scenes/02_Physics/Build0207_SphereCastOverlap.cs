using GlossaryDemo.PhysicsDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0207_SphereCastOverlap
{
    [GlossaryScene("0207")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0207_SphereCastOverlap")]
    public static void Build()
    {
        var ctx = NewScene3D("0207",
            "왼쪽 클릭 : 그 위치에서 폭발 → OverlapSphere(반경 5m) 안의 적에게 데미지 (가까울수록 큼)\n오른쪽 클릭 : 포탑에서 그 방향으로 SphereCast (R : Raycast 와 비교)\n범위 안에 있어도 주황 상자(Obstacle 레이어)는 마스크에서 빠져 있어 무시됩니다",
            true, 34f);
        ctx.Camera.transform.position = new Vector3(0f, 16f, -13f);
        ctx.Camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);

        // 적 무리 (Enemy 레이어 + 체력)
        var rnd = new System.Random(207);
        for (int i = 0; i < 14; i++)
        {
            var pos = new Vector3((float)rnd.NextDouble() * 18f - 9f, 0.8f, (float)rnd.NextDouble() * 10f - 1f);
            var e = Prim(PrimitiveType.Capsule, $"Enemy_{i + 1}", pos, Vector3.one * 0.8f, EnemyColor);
            e.layer = LayerMask.NameToLayer("Enemy");
            e.tag = "Enemy";
            AddHealth(e, 100f, 1.6f);
        }
        // 상자 (Obstacle 레이어, 체력은 있지만 마스크에 없어서 안 맞음)
        for (int i = 0; i < 4; i++)
        {
            var c = Prim(PrimitiveType.Cube, $"Crate_{i}", new Vector3(-6f + i * 4f, 0.5f, 4.5f), Vector3.one, new Color32(230, 150, 70, 255));
            c.layer = LayerMask.NameToLayer("Obstacle");
            AddHealth(c, 100f, 1.2f);
        }

        var gun = Prim(PrimitiveType.Cube, "Turret", new Vector3(0f, 1f, -6f), new Vector3(1f, 1f, 1f), PlayerColor);
        Object.DestroyImmediate(gun.GetComponent<Collider>());

        var explosion = Prim(PrimitiveType.Sphere, "ExplosionVisual", Vector3.zero, Vector3.one, Color.white);
        Object.DestroyImmediate(explosion.GetComponent<Collider>());
        explosion.GetComponent<Renderer>().sharedMaterial = TransparentMat("ExplosionSphere", new Color(1f, 0.55f, 0.2f, 0.45f));
        explosion.SetActive(false);

        var castBall = Prim(PrimitiveType.Sphere, "CastVisual", Vector3.zero, Vector3.one, Color.white);
        Object.DestroyImmediate(castBall.GetComponent<Collider>());
        castBall.GetComponent<Renderer>().sharedMaterial = TransparentMat("SphereCastBall", new Color(0.4f, 0.8f, 1f, 0.45f));
        castBall.SetActive(false);

        var line = Line("CastLine", 1.2f, new Color(0.4f, 0.8f, 1f, 0.35f));
        line.enabled = false;

        var demo = new GameObject("AreaQueryDemo").AddComponent<AreaQueryDemo>();
        SetLayerMask(demo, "enemyMask", LayerMask.GetMask("Enemy"));
        SetLayerMask(demo, "groundMask", LayerMask.GetMask("Ground"));
        Set(demo, "explosionVisual", explosion.transform);
        Set(demo, "gun", gun.transform);
        Set(demo, "castVisual", castBall.transform);
        Set(demo, "castLine", line);
        Save(ctx);
    }
}
