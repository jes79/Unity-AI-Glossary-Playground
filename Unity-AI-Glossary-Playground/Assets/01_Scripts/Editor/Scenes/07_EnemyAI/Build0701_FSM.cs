using GlossaryDemo.EnemyAI;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static EnemyAIBuildUtil;

public static class Build0701_FSM
{
    [GlossaryScene("0701")]
    [MenuItem("Tools/Glossary/Build Scene/07 EnemyAI/0701_FSM")]
    public static void Build()
    {
        var ctx = NewScene3D("0701", "WASD : 플레이어 이동   Space : 근접 공격(3m 안)\nK : 적 즉사 (Dead 상태 확인)\n초록 원(10m) 안 → Chase, 빨간 원(2m) 안 → Attack\n적 색 = 상태 (회색 Idle · 초록 Patrol · 주황 Chase · 빨강 Attack)");
        TopCamera(ctx);

        var player = Player(new Vector3(-10f, 0f, -10f));

        // 순찰 지점 4개
        Vector3[] pts = { new(4f, 0f, 6f), new(10f, 0f, 6f), new(10f, 0f, 0f), new(4f, 0f, 0f) };
        var points = new Transform[pts.Length];
        for (int i = 0; i < pts.Length; i++)
        {
            var p = Prim(PrimitiveType.Cylinder, $"PatrolPoint_{i}", pts[i] + Vector3.up * 0.02f, new Vector3(0.6f, 0.02f, 0.6f), new Color32(80, 200, 120, 255));
            Object.DestroyImmediate(p.GetComponent<Collider>());
            points[i] = p.transform;
        }

        var enemy = Prim(PrimitiveType.Capsule, "Enemy_FSM", new Vector3(4f, 1f, 3f), Vector3.one, EnemyColor);
        enemy.layer = LayerMask.NameToLayer("Enemy");
        enemy.tag = "Enemy";
        var eye = Prim(PrimitiveType.Cube, "Eye", new Vector3(0f, 0.5f, 0.45f), new Vector3(0.6f, 0.15f, 0.2f), Color.black, enemy.transform);
        Object.DestroyImmediate(eye.GetComponent<Collider>());
        Ring(enemy.transform, 10f, new Color(0.3f, 0.9f, 0.4f), "ChaseRange");
        Ring(enemy.transform, 2f, new Color(1f, 0.3f, 0.3f), "AttackRange");
        var label = Label(enemy.transform, "Idle", 1.8f, 4f);

        var fsm = enemy.AddComponent<FsmEnemy>();
        SetArray(fsm, "patrolPoints", points);
        Set(fsm, "player", player);
        Set(fsm, "body", enemy.GetComponent<Renderer>());
        Set(fsm, "label", label);

        Save(ctx);
    }
}
