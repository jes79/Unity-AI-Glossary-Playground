using GlossaryDemo.EnemyAI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static GlossaryKit;
using static EnemyAIBuildUtil;

public static class Build0703_NavMeshAgent
{
    [GlossaryScene("0703")]
    [MenuItem("Tools/Glossary/Build Scene/07 EnemyAI/0703_NavMeshAgent")]
    public static void Build()
    {
        var ctx = NewScene3D("0703", "WASD : 플레이어 이동   Space : 추적 ON/OFF   P : 경로선 표시\n벽 뒤로 숨어 보세요 → 적이 벽을 돌아서 쫓아옴\n노란 선 = NavMeshAgent 가 계산한 경로(corners)\nNavMesh 는 바닥의 NavMeshSurface 에 구워져 있음 (Window > AI > Navigation)");
        TopCamera(ctx);

        // 미로처럼 배치한 벽 (Ground 레이어로 두어 NavMesh 에 구멍을 냄)
        var walls = new GameObject("Walls").transform;
        Wall("Wall_A", new Vector3(-4f, 1f, 2f), new Vector3(10f, 2f, 1f), walls, "Ground");
        Wall("Wall_B", new Vector3(5f, 1f, -3f), new Vector3(1f, 2f, 12f), walls, "Ground");
        Wall("Wall_C", new Vector3(-8f, 1f, -6f), new Vector3(1f, 2f, 8f), walls, "Ground");
        Wall("Wall_D", new Vector3(2f, 1f, 8f), new Vector3(12f, 2f, 1f), walls, "Ground");
        Wall("Wall_E", new Vector3(10f, 1f, 4f), new Vector3(6f, 2f, 1f), walls, "Ground");

        // 플레이어/적을 만들기 전에 굽기 (Player/Enemy 레이어는 굽기에서 제외)
        BakeNavMesh(ctx.Ground, "0703");

        var player = Player(new Vector3(-10f, 0f, 8f));

        var enemy = Prim(PrimitiveType.Capsule, "Enemy_NavAgent", new Vector3(10f, 1f, -10f), Vector3.one, EnemyColor);
        Object.DestroyImmediate(enemy.GetComponent<Collider>());
        enemy.layer = LayerMask.NameToLayer("Enemy");
        var agent = enemy.AddComponent<NavMeshAgent>();
        agent.speed = 4f;
        agent.angularSpeed = 540f;
        agent.acceleration = 12f;
        agent.stoppingDistance = 1.5f;
        agent.radius = 0.5f;
        agent.height = 2f;
        agent.baseOffset = 1f;
        Label(enemy.transform, "NavMesh Agent", 1.6f, 3f);

        var chaser = enemy.AddComponent<NavChaser>();
        Set(chaser, "target", player.transform);
        Set(chaser, "pathLine", Line("PathLine", new Color(1f, 0.85f, 0.2f), 0.12f));

        Save(ctx);
    }
}
