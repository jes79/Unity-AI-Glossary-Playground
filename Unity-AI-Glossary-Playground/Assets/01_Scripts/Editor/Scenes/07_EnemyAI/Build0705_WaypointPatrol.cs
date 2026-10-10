using GlossaryDemo.EnemyAI;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static EnemyAIBuildUtil;

public static class Build0705_WaypointPatrol
{
    [GlossaryScene("0705")]
    [MenuItem("Tools/Glossary/Build Scene/07 EnemyAI/0705_WaypointPatrol")]
    public static void Build()
    {
        var ctx = NewScene3D("0705", "M : 순찰 모드 (Loop → PingPong → Random)\n+ / - : 대기 시간 0.5초씩 조절\n경비병이 웨이포인트 4개를 순서대로 돌며 각 지점에서 2초 대기\n노란 기둥 = 지금 향하는 웨이포인트");
        TopCamera(ctx);

        Vector3[] pts = { new(-4f, 0f, 6f), new(10f, 0f, 6f), new(10f, 0f, -6f), new(-4f, 0f, -6f) };
        var points = new Transform[pts.Length];
        var renderers = new Renderer[pts.Length];
        for (int i = 0; i < pts.Length; i++)
        {
            var wp = Prim(PrimitiveType.Cylinder, $"Waypoint_{i + 1}", pts[i] + Vector3.up * 0.5f, new Vector3(0.5f, 0.5f, 0.5f), new Color32(90, 180, 255, 255));
            Object.DestroyImmediate(wp.GetComponent<Collider>());
            Label(wp.transform, $"WP{i + 1}", 2.6f, 5f, new Color(0.6f, 0.85f, 1f));
            points[i] = wp.transform;
            renderers[i] = wp.GetComponent<Renderer>();
        }

        // 순찰 경로 표시
        var route = Line("PatrolRoute", new Color(0.35f, 0.7f, 1f, 1f), 0.08f);
        route.loop = true;
        route.positionCount = pts.Length;
        for (int i = 0; i < pts.Length; i++) route.SetPosition(i, pts[i] + Vector3.up * 0.05f);

        var guard = Prim(PrimitiveType.Capsule, "Guard", new Vector3(-4f, 1f, 6f), Vector3.one, EnemyColor);
        Object.DestroyImmediate(guard.GetComponent<Collider>());
        guard.layer = LayerMask.NameToLayer("Enemy");
        var eye = Prim(PrimitiveType.Cube, "Eye", new Vector3(0f, 0.5f, 0.45f), new Vector3(0.6f, 0.15f, 0.2f), Color.black, guard.transform);
        Object.DestroyImmediate(eye.GetComponent<Collider>());
        var label = Label(guard.transform, "", 1.6f, 4f);

        var patrol = guard.AddComponent<WaypointPatrol>();
        SetArray(patrol, "waypoints", points);
        SetArray(patrol, "waypointRenderers", renderers);
        SetFloat(patrol, "waitTime", 2f);
        Set(patrol, "label", label);

        Save(ctx);
    }
}
