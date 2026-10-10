using GlossaryDemo.EnemyAI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static GlossaryKit;
using static EnemyAIBuildUtil;

public static class Build0704_NavMeshObstacleLink
{
    [GlossaryScene("0704")]
    [MenuItem("Tools/Glossary/Build Scene/07 EnemyAI/0704_NavMeshObstacleLink")]
    public static void Build()
    {
        var ctx = NewScene3D("0704", "마우스 클릭 : 목적지   1 / 2 / 3 : 남쪽·북쪽·단상 위로 이동\nC : 상자 Carve ON/OFF   M : 상자 움직임 정지   J : 링크 점프 방식\n가운데 통로의 움직이는 상자를 Carve 로 피해 가는지 비교\n높은 단상은 NavMesh Link(파란 호)로만 올라갈 수 있음");
        TopCamera(ctx, 22f, -16f, 55f, 0f);

        // 가운데에 통로가 있는 벽
        var env = new GameObject("Environment").transform;
        Wall("Wall_Left", new Vector3(-9.5f, 0.75f, 0f), new Vector3(11f, 1.5f, 1f), env, "Ground");
        Wall("Wall_Right", new Vector3(9.5f, 0.75f, 0f), new Vector3(11f, 1.5f, 1f), env, "Ground");

        // 걸어서 못 올라가는 높은 단상 (높이 2m > Step Height)
        var platform = Wall("Platform", new Vector3(9f, 1f, 9f), new Vector3(5f, 2f, 5f), env, "Ground");
        platform.GetComponent<Renderer>().sharedMaterial = ColorMat(new Color32(90, 120, 170, 255));

        // 움직이는 상자 = NavMesh Obstacle. Obstacle 레이어라 굽기에는 포함되지 않음
        var crate = Prim(PrimitiveType.Cube, "MovingCrate (NavMeshObstacle)", new Vector3(0f, 0.75f, 0f), new Vector3(2.2f, 1.5f, 1.5f), new Color32(200, 140, 60, 255));
        crate.layer = LayerMask.NameToLayer("Obstacle");
        var obstacle = crate.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Box;
        obstacle.size = Vector3.one;
        obstacle.carving = true;              // Carve: NavMesh 에 실시간으로 구멍을 냄
        obstacle.carveOnlyStationary = false; // 움직이는 중에도 구멍 갱신
        Label(crate.transform, "Obstacle (Carve)", 1.4f, 3f);

        BakeNavMesh(ctx.Ground, "0704", "Obstacle");

        // NavMesh Link: 바닥 → 단상 위 (양방향)
        var linkGo = new GameObject("NavMeshLink (Jump)");
        var link = linkGo.AddComponent<NavMeshLink>();
        Vector3 start = new Vector3(4.6f, 0f, 9f), end = new Vector3(7.4f, 2f, 9f);
        link.startPoint = start;
        link.endPoint = end;
        link.width = 2f;
        link.bidirectional = true;
        link.UpdateLink();
        // 링크를 눈에 보이게 호로 그림
        var arc = Line("LinkArc", new Color(0.3f, 0.6f, 1f), 0.12f, linkGo.transform);
        arc.positionCount = 16;
        for (int i = 0; i < 16; i++)
        {
            float t = i / 15f;
            arc.SetPosition(i, Vector3.Lerp(start, end, t) + Vector3.up * (0.1f + 2f * 4f * t * (1f - t)));
        }

        var goals = new Transform[3];
        Vector3[] goalPos = { new(-8f, 0f, -9f), new(-8f, 0f, 9f), new(9f, 2f, 9f) };
        for (int i = 0; i < 3; i++)
        {
            var g = Prim(PrimitiveType.Cylinder, $"Goal_{i + 1}", goalPos[i] + Vector3.up * 0.03f, new Vector3(1f, 0.03f, 1f), new Color32(255, 220, 80, 255));
            Object.DestroyImmediate(g.GetComponent<Collider>());
            Label(g.transform, (i + 1).ToString(), 1.2f, 5f, new Color(1f, 0.9f, 0.4f));
            goals[i] = g.transform;
        }

        var marker = Prim(PrimitiveType.Cylinder, "ClickMarker", new Vector3(0f, -5f, 0f), new Vector3(0.6f, 0.02f, 0.6f), Color.white);
        Object.DestroyImmediate(marker.GetComponent<Collider>());

        var agentGo = Prim(PrimitiveType.Capsule, "Agent", new Vector3(0f, 1f, -9f), Vector3.one, EnemyColor);
        Object.DestroyImmediate(agentGo.GetComponent<Collider>());
        agentGo.layer = LayerMask.NameToLayer("Enemy");
        var agent = agentGo.AddComponent<NavMeshAgent>();
        agent.speed = 4.5f;
        agent.angularSpeed = 540f;
        agent.acceleration = 15f;
        agent.radius = 0.5f;
        agent.height = 2f;
        agent.baseOffset = 1f;

        var demo = agentGo.AddComponent<NavObstacleLinkDemo>();
        Set(demo, "movingCrate", obstacle);
        SetVector3(demo, "crateA", new Vector3(-2f, 0.75f, 0f));
        SetVector3(demo, "crateB", new Vector3(2f, 0.75f, 0f));
        SetArray(demo, "goals", goals);
        Set(demo, "clickMarker", marker.transform);
        Set(demo, "pathLine", Line("PathLine", new Color(1f, 0.85f, 0.2f), 0.12f));

        Save(ctx);
    }
}
