using GlossaryDemo.EnemyAI;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static EnemyAIBuildUtil;

public static class Build0708_WaveSpawner
{
    [GlossaryScene("0708")]
    [MenuItem("Tools/Glossary/Build Scene/07 EnemyAI/0708_WaveSpawner")]
    public static void Build()
    {
        var ctx = NewScene3D("0708", "자동 진행: 네 모서리 스폰 지점에서 적이 나와 가운데 기지로 이동\n기지 포탑이 자동 사격. 모두 처치하면 3초 뒤 다음 웨이브\nK : 현재 적 모두 처치 (빠르게 다음 웨이브 보기)   R : 처음부터\n웨이브마다 적 수가 5 → 10 → 15 ... 로 늘어나는지 관찰");
        TopCamera(ctx, 24f, -17f, 55f, 0f);

        var baseGo = Prim(PrimitiveType.Cylinder, "Base_Tower", new Vector3(0f, 1f, 0f), new Vector3(2f, 1f, 2f), PlayerColor);
        Label(baseGo.transform, "기지", 2.2f, 5f);
        var ring = Ring(baseGo.transform, 10f, new Color(0.4f, 0.7f, 1f), "TowerRange");

        Vector3[] sp = { new(-13f, 0.5f, -13f), new(13f, 0.5f, -13f), new(13f, 0.5f, 13f), new(-13f, 0.5f, 13f) };
        var points = new Transform[sp.Length];
        for (int i = 0; i < sp.Length; i++)
        {
            var p = Prim(PrimitiveType.Cylinder, $"SpawnPoint_{i}", new Vector3(sp[i].x, 0.03f, sp[i].z), new Vector3(2f, 0.03f, 2f), new Color32(170, 60, 200, 255));
            Object.DestroyImmediate(p.GetComponent<Collider>());
            points[i] = p.transform;
        }

        // 적 프리팹 (빨간 큐브)
        var enemyGo = Prim(PrimitiveType.Cube, "WaveEnemy", Vector3.zero, new Vector3(0.8f, 0.8f, 0.8f), EnemyColor);
        Object.DestroyImmediate(enemyGo.GetComponent<Collider>());
        enemyGo.layer = LayerMask.NameToLayer("Enemy");
        enemyGo.AddComponent<WaveEnemy>();
        var enemyPrefab = SavePrefab(enemyGo, Sub, "WaveEnemy_0708").GetComponent<WaveEnemy>();

        var banner = BigText(ctx, new Vector2(200f, -60f), "", 44f);

        var spawnerGo = new GameObject("WaveSpawner");
        var spawner = spawnerGo.AddComponent<WaveSpawner>();
        SetInt(spawner, "baseCount", 5);
        SetInt(spawner, "increasePerWave", 5);
        Set(spawner, "enemyPrefab", enemyPrefab);
        SetArray(spawner, "spawnPoints", points);
        Set(spawner, "baseTarget", baseGo.transform);
        Set(spawner, "bannerText", banner);
        Set(spawner, "shotLine", Line("ShotLine", new Color(0.6f, 0.9f, 1f), 0.1f));

        Save(ctx);
    }
}
