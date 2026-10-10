using GlossaryDemo.Movement;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0110_GroundCheck
{
    [GlossaryScene("0110")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0110_GroundCheck")]
    public static void Build()
    {
        var ctx = NewScene3D("0110",
            "WASD : 이동   Space : 점프 (땅 위일 때만)\nR : SphereCast ↔ Raycast   G : 마스크 Ground 만 ↔ 모든 레이어\n주황 상자(Obstacle 레이어) 위에서는 Ground 만 인식하므로 점프가 안 됩니다. 발판 가장자리에 걸쳐 서서 R 로 비교해 보세요",
            true, 30f);

        int ground = LayerMask.NameToLayer("Ground");
        int obstacle = LayerMask.NameToLayer("Obstacle");

        // Ground 레이어 발판 (초록)
        var groundColor = new Color32(90, 160, 110, 255);
        foreach (var (pos, size) in new[] { (new Vector3(-5f, 0.5f, 2f), new Vector3(3f, 1f, 3f)), (new Vector3(-1f, 1f, 4f), new Vector3(2.5f, 2f, 2.5f)), (new Vector3(5f, 0.75f, 3f), new Vector3(3f, 1.5f, 3f)) })
        {
            var p = Prim(PrimitiveType.Cube, "GroundPlatform", pos, size, groundColor);
            p.layer = ground;
        }
        Label3D("Ground 레이어", new Vector3(-5f, 2f, 2f), 5f, new Color(0.6f, 1f, 0.6f));

        // Obstacle 레이어 상자 (주황): 밟을 수는 있지만 Ground Check 마스크에 없음
        foreach (var pos in new[] { new Vector3(1.5f, 0.5f, -1f), new Vector3(-3f, 0.5f, -3f) })
        {
            var box = Prim(PrimitiveType.Cube, "ObstacleBox", pos, new Vector3(2f, 1f, 2f), new Color32(230, 150, 70, 255));
            box.layer = obstacle;
        }
        Label3D("Obstacle 레이어", new Vector3(1.5f, 1.6f, -1f), 5f, new Color(1f, 0.8f, 0.5f));

        var player = PlayerCapsule(new Vector3(-2f, 1.1f, 0f));
        var rb = player.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        // 벽·발판 옆면에 달라붙지 않도록 마찰 0
        player.GetComponent<CapsuleCollider>().sharedMaterial = NoFriction3D;

        var visual = Prim(PrimitiveType.Sphere, "CastVisual", Vector3.zero, Vector3.one * 0.8f, Color.white);
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        visual.GetComponent<Renderer>().sharedMaterial = TransparentMat("GroundCheckCast", new Color(0.3f, 1f, 0.4f, 0.45f));

        var lineGo = new GameObject("CastLine");
        var line = lineGo.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.widthMultiplier = 0.05f;
        line.sharedMaterial = ParticleMat("MoveLine", Color.white, false, WhiteSprite.texture);

        var check = player.AddComponent<GroundCheckPlayer>();
        SetLayerMask(check, "groundMask", LayerMask.GetMask("Ground"));
        Set(check, "castVisual", visual.transform);
        Set(check, "castLine", line);
        Set(check, "body", player.GetComponent<Renderer>());

        var follow = ctx.Camera.gameObject.AddComponent<SimpleFollowCamera>();
        Set(follow, "target", player.transform);
        SetVector3(follow, "offset", new Vector3(0f, 6f, -9f));
        ctx.Camera.transform.position = player.transform.position + new Vector3(0f, 6f, -9f);
        Save(ctx);
    }

    private static PhysicsMaterial NoFriction3D
    {
        get
        {
            string path = $"{GeneratedFolder}/{Sub}/NoFriction3D.asset";
            var m = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
            if (m != null) return m;
            var mat = new PhysicsMaterial("NoFriction3D")
            {
                dynamicFriction = 0f, staticFriction = 0f, frictionCombine = PhysicsMaterialCombine.Minimum
            };
            return SaveAsset(mat, Sub, "NoFriction3D.asset");
        }
    }
}
