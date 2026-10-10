using GlossaryDemo.PhysicsDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0201_Collider
{
    [GlossaryScene("0201")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0201_Collider")]
    public static void Build()
    {
        var ctx = NewScene3D("0201",
            "마우스 왼쪽 클릭 : 클릭한 곳으로 총알 발사 (SphereCollider)\nV : Collider 모양(초록 반투명) 보이기/숨기기\n총알은 맞으면 사라집니다. 링(MeshCollider)의 구멍은 통과하고, 석상은 모양이 아니라 캡슐 기준으로 맞는 것을 보세요",
            true, 30f);
        ctx.Camera.transform.position = new Vector3(0f, 6f, -14f);
        ctx.Camera.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

        var viewMat = TransparentMat("ColliderView", new Color(0.3f, 1f, 0.4f, 0.28f));
        var views = new System.Collections.Generic.List<GameObject>();

        GameObject View(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Transform parent = null)
        {
            var v = Prim(type, name, pos, scale, Color.white, parent);
            Object.DestroyImmediate(v.GetComponent<Collider>());
            v.GetComponent<Renderer>().sharedMaterial = viewMat;
            views.Add(v);
            return v;
        }

        // 뒤쪽 벽: BoxCollider
        var wall = Prim(PrimitiveType.Cube, "Wall (BoxCollider)", new Vector3(0f, 2.5f, 10f), new Vector3(24f, 5f, 0.6f), new Color32(110, 116, 132, 255));
        Label("BoxCollider (벽)", new Vector3(-8f, 5.6f, 10f), 6f, Color.white);

        // 바위: SphereCollider
        Prim(PrimitiveType.Sphere, "Rock (SphereCollider)", new Vector3(-7f, 1.2f, 4f), Vector3.one * 2.4f, new Color32(140, 130, 120, 255));
        Label("SphereCollider", new Vector3(-7f, 3f, 4f), 5f, Color.white);

        // 기둥: Cylinder 프리미티브는 기본으로 CapsuleCollider 를 가짐 (윗면이 둥글게 판정됨)
        Prim(PrimitiveType.Cylinder, "Pillar (CapsuleCollider)", new Vector3(-2.5f, 1.5f, 5f), new Vector3(1.4f, 1.5f, 1.4f), new Color32(170, 150, 110, 255));
        View(PrimitiveType.Capsule, "PillarColliderView", new Vector3(-2.5f, 1.5f, 5f), new Vector3(1.42f, 1.52f, 1.42f));
        Label("CapsuleCollider", new Vector3(-2.5f, 3.6f, 5f), 5f, Color.white);

        // 링: MeshCollider (볼록하지 않은 메시 그대로 → 가운데 구멍으로 통과 가능, 정적 오브젝트 전용)
        var ringMesh = SaveAsset(CreateTorus(1.6f, 0.35f, 32, 12), Sub, "0201_Ring.asset");
        var ring = new GameObject("Ring (MeshCollider)");
        ring.transform.position = new Vector3(3f, 2.3f, 5f);
        ring.AddComponent<MeshFilter>().sharedMesh = ringMesh;
        ring.AddComponent<MeshRenderer>().sharedMaterial = ColorMat(new Color32(120, 170, 220, 255));
        ring.AddComponent<MeshCollider>().sharedMesh = ringMesh;
        Label("MeshCollider (구멍 통과)", new Vector3(3f, 4.6f, 5f), 5f, Color.white);

        // 석상: 보이는 모양은 복잡하지만 Collider 는 단순 캡슐 하나 (성능을 위해 흔히 쓰는 방식)
        var statue = new GameObject("Statue (CapsuleCollider 근사)");
        statue.transform.position = new Vector3(8f, 0f, 4f);
        var body = Prim(PrimitiveType.Cube, "Body", new Vector3(0f, 1.2f, 0f), new Vector3(0.8f, 1.6f, 0.5f), new Color32(200, 190, 170, 255), statue.transform);
        var head = Prim(PrimitiveType.Sphere, "Head", new Vector3(0f, 2.4f, 0f), Vector3.one * 0.6f, new Color32(200, 190, 170, 255), statue.transform);
        var arm = Prim(PrimitiveType.Cube, "Arm", new Vector3(0.9f, 1.7f, 0f), new Vector3(1.2f, 0.25f, 0.25f), new Color32(200, 190, 170, 255), statue.transform);
        foreach (var part in new[] { body, head, arm }) Object.DestroyImmediate(part.GetComponent<Collider>());
        var cap = statue.AddComponent<CapsuleCollider>();
        cap.center = new Vector3(0f, 1.35f, 0f);
        cap.height = 2.7f;
        cap.radius = 0.5f;
        View(PrimitiveType.Capsule, "StatueColliderView", new Vector3(0f, 1.35f, 0f), new Vector3(1f, 1.35f, 1f), statue.transform);
        Label("모양 ≠ Collider (팔은 안 맞음)", new Vector3(8f, 3.4f, 4f), 5f, Color.white);

        // 총알 프리팹: SphereCollider + Rigidbody + ColliderBullet
        var bullet = Prim(PrimitiveType.Sphere, "Bullet", Vector3.zero, Vector3.one * 0.25f, new Color32(255, 220, 90, 255));
        bullet.layer = LayerMask.NameToLayer("PlayerBullet");
        bullet.tag = "Bullet";
        var rb = bullet.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // 빠른 총알이 얇은 벽을 뚫지 않게 (0214 참고)
        bullet.AddComponent<ColliderBullet>();
        var prefab = SavePrefab(bullet, Sub, "0201_Bullet");

        var gun = Prim(PrimitiveType.Cube, "Gun", new Vector3(0f, 1f, -7f), new Vector3(0.6f, 0.6f, 1.2f), PlayerColor);
        var muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(gun.transform, false);
        muzzle.localPosition = new Vector3(0f, 0f, 0.7f);

        var shooter = gun.AddComponent<ColliderShooter>();
        Set(shooter, "bulletPrefab", prefab.GetComponent<Rigidbody>());
        Set(shooter, "muzzle", muzzle);
        SetArray(shooter, "colliderViews", views.ToArray());
        _ = wall;
        Save(ctx);
    }

    // 도넛(토러스) 메시: MeshCollider 가 "실제 모양 그대로" 판정하는 것을 보이기 위함
    private static Mesh CreateTorus(float radius, float tube, int segments, int sides)
    {
        var verts = new Vector3[(segments + 1) * (sides + 1)];
        var normals = new Vector3[verts.Length];
        var tris = new int[segments * sides * 6];
        for (int i = 0; i <= segments; i++)
        {
            float u = (float)i / segments * Mathf.PI * 2f;
            Vector3 center = new Vector3(Mathf.Cos(u), Mathf.Sin(u), 0f) * radius; // XY 평면의 링 (z 방향으로 구멍이 뚫림)
            for (int j = 0; j <= sides; j++)
            {
                float v = (float)j / sides * Mathf.PI * 2f;
                Vector3 n = new Vector3(Mathf.Cos(u) * Mathf.Cos(v), Mathf.Sin(u) * Mathf.Cos(v), Mathf.Sin(v));
                verts[i * (sides + 1) + j] = center + n * tube;
                normals[i * (sides + 1) + j] = n;
            }
        }
        int t = 0;
        for (int i = 0; i < segments; i++)
            for (int j = 0; j < sides; j++)
            {
                int a = i * (sides + 1) + j, b = a + sides + 1;
                tris[t++] = a; tris[t++] = b; tris[t++] = a + 1;      // 바깥쪽이 앞면이 되도록 감는 순서
                tris[t++] = b; tris[t++] = b + 1; tris[t++] = a + 1;
            }
        var mesh = new Mesh { name = "Ring", vertices = verts, normals = normals, triangles = tris };
        mesh.RecalculateBounds();
        return mesh;
    }
}
