using GlossaryDemo.PhysicsDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0214_TunnelingCCD
{
    [GlossaryScene("0214")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0214_TunnelingCCD")]
    public static void Build()
    {
        var ctx = NewScene3D("0214",
            "자동 발사 (0.25초마다 두 레인 동시에)\n1 Discrete  2 Continuous  3 ContinuousDynamic  4 ContinuousSpeculative : 오른쪽 레인 모드\n↑/↓ : 총알 속도\n왼쪽(Discrete)은 얇은 벽을 뚫고(빨간 총알), 오른쪽(Continuous)은 벽에 막히는지 보세요",
            true, 40f);
        ctx.Camera.transform.position = new Vector3(0f, 10f, -6f);
        ctx.Camera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

        const float wallZ = 8f;
        var wallColor = new Color32(170, 175, 190, 255);
        var wallL = Prim(PrimitiveType.Cube, "ThinWall_L (0.1m)", new Vector3(-3f, 1.5f, wallZ), new Vector3(4f, 3f, 0.1f), wallColor);
        var wallR = Prim(PrimitiveType.Cube, "ThinWall_R (0.1m)", new Vector3(3f, 1.5f, wallZ), new Vector3(4f, 3f, 0.1f), wallColor);
        wallL.layer = wallR.layer = LayerMask.NameToLayer("Obstacle");
        Label("Discrete (고정)", new Vector3(-3f, 3.6f, wallZ), 6f, new Color(1f, 0.7f, 0.6f));
        Label("선택 모드 (1~4)", new Vector3(3f, 3.6f, wallZ), 6f, new Color(0.6f, 1f, 0.7f));

        var gunL = Prim(PrimitiveType.Cube, "Gun_L", new Vector3(-3f, 1f, -0.8f), new Vector3(0.5f, 0.5f, 1f), new Color32(200, 120, 110, 255));
        var gunR = Prim(PrimitiveType.Cube, "Gun_R", new Vector3(3f, 1f, -0.8f), new Vector3(0.5f, 0.5f, 1f), new Color32(110, 200, 130, 255));
        Object.DestroyImmediate(gunL.GetComponent<Collider>());
        Object.DestroyImmediate(gunR.GetComponent<Collider>());
        var muzzleL = new GameObject("Muzzle_L").transform; muzzleL.position = new Vector3(-3f, 1f, 0f);
        var muzzleR = new GameObject("Muzzle_R").transform; muzzleR.position = new Vector3(3f, 1f, 0f);

        // 작은 총알 프리팹 (반지름 0.05)
        var bullet = Prim(PrimitiveType.Sphere, "FastBullet", Vector3.zero, Vector3.one * 0.1f, new Color32(255, 230, 120, 255));
        bullet.layer = LayerMask.NameToLayer("PlayerBullet");
        var rb = bullet.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.mass = 0.05f;
        bullet.AddComponent<TunnelingBullet>();
        var trail = bullet.AddComponent<TrailRenderer>();
        trail.time = 0.06f;
        trail.widthMultiplier = 0.08f;
        trail.sharedMaterial = LineMat;
        trail.startColor = new Color(1f, 0.9f, 0.5f, 1f);
        trail.endColor = new Color(1f, 0.9f, 0.5f, 0f);
        var prefab = SavePrefab(bullet, Sub, "0214_FastBullet");

        var demo = new GameObject("TunnelingDemo").AddComponent<TunnelingDemo>();
        Set(demo, "bulletPrefab", prefab.GetComponent<Rigidbody>());
        Set(demo, "leftMuzzle", muzzleL);
        Set(demo, "rightMuzzle", muzzleR);
        SetFloat(demo, "wallZ", wallZ);
        Save(ctx);
    }
}
