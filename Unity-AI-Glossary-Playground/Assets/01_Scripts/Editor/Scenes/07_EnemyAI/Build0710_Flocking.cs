using GlossaryDemo.EnemyAI;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static EnemyAIBuildUtil;

public static class Build0710_Flocking
{
    [GlossaryScene("0710")]
    [MenuItem("Tools/Glossary/Build Scene/07 EnemyAI/0710_Flocking")]
    public static void Build()
    {
        var ctx = NewScene3D("0710", "1 : 분리(Separation) ON/OFF   2 : 정렬(Alignment) ON/OFF\n3 : 응집(Cohesion) ON/OFF   R : 다시 흩뿌리기\n규칙을 하나씩 끄면서 무리 모양이 어떻게 무너지는지 관찰\n(분리 OFF → 한 점으로 뭉침, 정렬 OFF → 제각각, 응집 OFF → 흩어짐)", false);
        ctx.Camera.clearFlags = CameraClearFlags.SolidColor;
        ctx.Camera.backgroundColor = new Color32(16, 52, 82, 255);
        ctx.Camera.transform.position = new Vector3(2f, 3f, -20f);
        ctx.Camera.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
        ctx.Light.transform.rotation = Quaternion.Euler(70f, -20f, 0f);

        // 모래 바닥
        var floor = Prim(PrimitiveType.Cube, "SandFloor", new Vector3(2f, -5f, 0f), new Vector3(40f, 0.5f, 30f), new Color32(170, 150, 110, 255));
        Object.DestroyImmediate(floor.GetComponent<Collider>());

        // 물고기 프리팹: 몸통(타원) + 꼬리. +Z 가 앞
        var fishRoot = new GameObject("Fish");
        var bodyGo = Prim(PrimitiveType.Sphere, "Body", Vector3.zero, new Vector3(0.22f, 0.3f, 0.7f), new Color32(255, 160, 60, 255), fishRoot.transform);
        Object.DestroyImmediate(bodyGo.GetComponent<Collider>());
        var tail = Prim(PrimitiveType.Cube, "Tail", new Vector3(0f, 0f, -0.42f), new Vector3(0.04f, 0.3f, 0.22f), new Color32(255, 210, 90, 255), fishRoot.transform);
        Object.DestroyImmediate(tail.GetComponent<Collider>());
        var eye = Prim(PrimitiveType.Sphere, "Eye", new Vector3(0f, 0.05f, 0.25f), new Vector3(0.24f, 0.07f, 0.07f), Color.black, fishRoot.transform);
        Object.DestroyImmediate(eye.GetComponent<Collider>());
        var fishPrefab = SavePrefab(fishRoot, Sub, "Fish_0710");

        var flockGo = new GameObject("BoidsFlock");
        flockGo.transform.position = new Vector3(2f, 0f, 0f);
        var flock = flockGo.AddComponent<BoidsFlock>();
        Set(flock, "fishPrefab", fishPrefab.transform);
        SetInt(flock, "count", 80);

        Save(ctx);
    }
}
