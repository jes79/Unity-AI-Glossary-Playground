using GlossaryDemo.Systems;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;

public static class Build0802_DamageSystem
{
    [GlossaryScene("0802")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0802_DamageSystem")]
    public static void Build()
    {
        var ctx = NewScene3D("0802", "마우스 클릭 : 클릭한 물체에 데미지   1 / 2 / 3 : 공격력 5 / 10 / 25\nShift + 클릭 : 범위 폭발 (반경 3m, 2배 데미지)\n플레이어·적·상자는 모두 IDamageable → 같은 코드로 데미지를 받지만 반응은 제각각\n회색 벽·바닥은 IDamageable 이 없어서 무시됨", true, 24f);
        ctx.Camera.transform.position = new Vector3(2f, 9f, -11f);
        ctx.Camera.transform.rotation = Quaternion.Euler(38f, 0f, 0f);

        // ① 플레이어
        var player = Prim(PrimitiveType.Capsule, "Player (IDamageable)", new Vector3(-3f, 1f, 0f), Vector3.one, PlayerColor);
        player.layer = LayerMask.NameToLayer("Player");
        var pd = player.AddComponent<DamageablePlayer>();
        Set(pd, "body", player.GetComponent<Renderer>());
        Set(pd, "label", EnemyAIBuildUtil.Label(player.transform, "", 1.8f, 3.5f));

        // ② 적
        var enemy = Prim(PrimitiveType.Capsule, "Enemy (IDamageable)", new Vector3(2f, 1f, 1.5f), Vector3.one, EnemyColor);
        enemy.layer = LayerMask.NameToLayer("Enemy");
        var ed = enemy.AddComponent<DamageableEnemy>();
        Set(ed, "body", enemy.GetComponent<Renderer>());
        Set(ed, "label", EnemyAIBuildUtil.Label(enemy.transform, "", 1.8f, 3.5f));

        // ③ 상자 2개
        var crateColor = new Color32(185, 130, 70, 255);
        for (int i = 0; i < 2; i++)
        {
            var crate = Prim(PrimitiveType.Cube, $"Crate_{i} (IDamageable)", new Vector3(6f + i * 1.8f, 0.6f, -1f + i * 1.2f), Vector3.one * 1.2f, crateColor);
            var cd = crate.AddComponent<BreakableCrate>();
            Set(cd, "label", EnemyAIBuildUtil.Label(crate.transform, "", 1.3f, 3.5f));
            Set(cd, "debrisMaterial", ColorMat(crateColor));
        }

        // IDamageable 이 없는 벽
        Prim(PrimitiveType.Cube, "Wall (IDamageable 없음)", new Vector3(-0.5f, 1.25f, 5f), new Vector3(6f, 2.5f, 0.6f), new Color32(110, 116, 132, 255));

        var marker = new GameObject("ExplosionMarker").transform;
        EnemyAIBuildUtil.Ring(marker, 3f, new Color(1f, 0.5f, 0.2f), "Radius");
        marker.gameObject.SetActive(false);

        var dealer = new GameObject("DamageDealer").AddComponent<DamageDealer>();
        Set(dealer, "explosionMarker", marker);

        Save(ctx);
    }
}
