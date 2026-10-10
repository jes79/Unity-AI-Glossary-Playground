using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1011_ObjectPooling
{
    [GlossaryScene("1011")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1011_ObjectPooling")]
    public static void Build()
    {
        var ctx = NewScene3D("1011", "Space : 발사 ON/OFF   ↑ / ↓ : 연사 속도 ±10\n" +
                                     "왼쪽 = Instantiate/Destroy, 오른쪽 = ObjectPool<noparse><T></noparse>\n" +
                                     "풀은 '실제 생성' 수가 금방 멈추고 Get/Release 만 늘어나는 것을 관찰", true, 40f);
        ctx.Camera.transform.position = new Vector3(0f, 14f, -10f);
        ctx.Camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        var canvas = EnsureCanvas(ctx);

        // 총알 프리팹 (콜라이더 없음 → 순수하게 생성/파괴 비용만 비교)
        var bulletGo = Prim(PrimitiveType.Sphere, "Bullet", Vector3.zero, Vector3.one * 0.25f, new Color(1f, 0.85f, 0.3f));
        Object.DestroyImmediate(bulletGo.GetComponent<Collider>());
        bulletGo.AddComponent<PoolBullet>();
        var prefab = SavePrefab(bulletGo, Sub, "Bullet1011").GetComponent<PoolBullet>();

        Transform Turret(string name, float x, Color color)
        {
            var baseGo = Prim(PrimitiveType.Cylinder, name, new Vector3(x, 0.5f, -4f), new Vector3(1.4f, 0.5f, 1.4f), color);
            var barrel = Prim(PrimitiveType.Cube, "Barrel", new Vector3(x, 1.1f, -3.2f), new Vector3(0.35f, 0.35f, 1.4f), color * 0.7f);
            barrel.transform.SetParent(baseGo.transform, true);
            var muzzle = new GameObject("Muzzle").transform;
            muzzle.position = new Vector3(x, 1.1f, -2.4f);
            muzzle.SetParent(baseGo.transform, true);
            return muzzle;
        }
        var left = Turret("Turret_Instantiate", -5f, new Color(0.9f, 0.4f, 0.4f));
        var right = Turret("Turret_ObjectPool", 5f, new Color(0.4f, 0.85f, 0.5f));
        var l1 = WorldLabel("Instantiate / Destroy", new Vector3(-5f, 0.05f, -6.2f), 6f);
        l1.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        var l2 = WorldLabel("ObjectPool<noparse><T></noparse>", new Vector3(5f, 0.05f, -6.2f), 6f);
        l2.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Prim(PrimitiveType.Cube, "Divider", new Vector3(0f, 0.3f, 5f), new Vector3(0.2f, 0.6f, 20f), new Color(0.3f, 0.3f, 0.35f));

        var demo = new GameObject("BulletPoolingDemo").AddComponent<BulletPoolingDemo>();
        Set(demo, "bulletPrefab", prefab);
        Set(demo, "leftMuzzle", left);
        Set(demo, "rightMuzzle", right);

        var b = CreateButtonRow(canvas.transform, 60f, "발사 ON/OFF", "연사 -10", "연사 +10", "카운터 리셋");
        OnClick(b[0], demo.ToggleFire);
        OnClick(b[1], demo.ChangeRate, -10f);
        OnClick(b[2], demo.ChangeRate, 10f);
        OnClick(b[3], demo.ResetCounters);

        HowToPanel(canvas.transform, "ObjectPool<noparse><T></noparse> 로 바꾸는 법",
            "using UnityEngine.Pool;\n\n" +
            "pool = new ObjectPool<noparse><Bullet></noparse>(\n" +
            "  () => Instantiate(prefab),        // 부족할 때 생성\n" +
            "  b => b.gameObject.SetActive(true),  // Get\n" +
            "  b => b.gameObject.SetActive(false), // Release\n" +
            "  b => Destroy(b.gameObject),        // 넘치면 파괴\n" +
            "  true, 20, 100);                     // 검사, 기본, 최대\n\n" +
            "발사:  var b = pool.Get(); b.Launch(...);\n" +
            "끝:    pool.Release(b);   // Destroy 대신\n\n" +
            "<color=#FC6>주의</color> 꺼낼 때 위치·속도·타이머 등 상태를\n반드시 초기화 (이전 사용 흔적이 남아 있음)",
            560f, 480f);

        Save(ctx);
    }
}
