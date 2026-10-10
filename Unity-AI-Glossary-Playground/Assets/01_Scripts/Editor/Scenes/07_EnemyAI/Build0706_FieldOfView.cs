using GlossaryDemo.EnemyAI;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static EnemyAIBuildUtil;

public static class Build0706_FieldOfView
{
    [GlossaryScene("0706")]
    [MenuItem("Tools/Glossary/Build Scene/07 EnemyAI/0706_FieldOfView")]
    public static void Build()
    {
        var ctx = NewScene3D("0706", "WASD : 플레이어 이동\nB : 벽 가림 판정 ON/OFF   T : 감시 회전 ON/OFF\n노란 부채꼴(시야각 90°, 거리 10m) 안에 들어가면 빨갛게 발견\n벽 뒤에 숨으면 부채꼴이 벽에서 잘리고 발견되지 않음");
        TopCamera(ctx);

        var walls = new GameObject("Walls").transform;
        Wall("Wall_1", new Vector3(2f, 1f, 3f), new Vector3(3f, 2f, 1f), walls);
        Wall("Wall_2", new Vector3(-3f, 1f, 6f), new Vector3(1f, 2f, 3f), walls);
        Wall("Wall_3", new Vector3(7f, 1f, 7f), new Vector3(1f, 2f, 4f), walls);
        Wall("Pillar", new Vector3(-1f, 1f, 9f), new Vector3(1.5f, 2f, 1.5f), walls);

        var player = Player(new Vector3(-10f, 0f, 10f));

        var guard = Prim(PrimitiveType.Capsule, "Guard (FOV)", new Vector3(1f, 1f, -4f), Vector3.one, new Color(0.55f, 0.55f, 0.6f));
        Object.DestroyImmediate(guard.GetComponent<Collider>());
        guard.layer = LayerMask.NameToLayer("Enemy");
        var eye = Prim(PrimitiveType.Cube, "Eye", new Vector3(0f, 0.5f, 0.45f), new Vector3(0.6f, 0.15f, 0.2f), Color.black, guard.transform);
        Object.DestroyImmediate(eye.GetComponent<Collider>());
        Label(guard.transform, "경비병", 1.6f, 3.5f);

        // 시야 부채꼴 메시 (바닥 바로 위)
        var cone = new GameObject("ViewCone");
        cone.transform.SetParent(guard.transform, false);
        cone.transform.localPosition = new Vector3(0f, -0.93f, 0f);
        var mf = cone.AddComponent<MeshFilter>();
        var mr = cone.AddComponent<MeshRenderer>();
        var mat = TransparentUnlit("AI_FovCone", new Color(1f, 0.9f, 0.3f, 0.25f));
        mat.SetFloat("_Cull", 0f); // 양면
        mr.sharedMaterial = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var fov = guard.AddComponent<FieldOfView>();
        SetFloat(fov, "viewRadius", 10f);
        SetFloat(fov, "viewAngle", 90f);
        SetLayerMask(fov, "obstacleMask", LayerMask.GetMask("Obstacle"));
        Set(fov, "target", player.transform);
        Set(fov, "viewMeshFilter", mf);
        Set(fov, "viewRenderer", mr);
        Set(fov, "body", guard.GetComponent<Renderer>());

        Save(ctx);
    }
}
