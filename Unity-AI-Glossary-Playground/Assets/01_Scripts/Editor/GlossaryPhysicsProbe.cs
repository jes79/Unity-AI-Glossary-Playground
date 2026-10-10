using System.Linq;
using GlossaryDemo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 2D 물리 점검: 씬을 열고 Physics2D 를 수동으로 2초 시뮬레이션해서 Rigidbody2D 들이 어디에 멈추는지 로그로 남김
// 명령줄: Unity -batchmode -executeMethod GlossaryPhysicsProbe.Run -quit
public static class GlossaryPhysicsProbe
{
    [MenuItem("Tools/Glossary/Debug/Probe 2D Physics")]
    public static void Run()
    {
        foreach (var e in GlossaryCatalog.Entries)
        {
            var scene = EditorSceneManager.OpenScene(e.ScenePath, OpenSceneMode.Single);
            var bodies = Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None)
                .Where(b => b.bodyType == RigidbodyType2D.Dynamic && b.simulated).ToArray();
            if (bodies.Length == 0) continue;

            var start = bodies.Select(b => b.position).ToArray();
            var prev = Physics2D.simulationMode;
            Physics2D.simulationMode = SimulationMode2D.Script;
            for (int i = 0; i < 100; i++) Physics2D.Simulate(0.02f);
            Physics2D.simulationMode = prev;

            for (int i = 0; i < bodies.Length; i++)
            {
                var b = bodies[i];
                bool fellThrough = b.position.y < start[i].y - 5f;
                Debug.Log($"[Probe2D] {e.Id} {b.name}: {start[i]} -> {b.position} contacts={b.GetContacts(new Collider2D[8])}{(fellThrough ? "  FELL_THROUGH" : "")}");
            }
        }
    }
}
