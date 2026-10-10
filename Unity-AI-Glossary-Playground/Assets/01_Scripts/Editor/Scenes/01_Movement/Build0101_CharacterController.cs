using GlossaryDemo.Movement;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0101_CharacterController
{
    [GlossaryScene("0101")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0101_CharacterController")]
    public static void Build()
    {
        var ctx = NewScene3D("0101",
            "WASD : 이동   Space : 점프\n1 / 2 / 3 : slopeLimit 45° / 30° / 60°\n30°·45°는 오르고 50°·60°는 막히는지, 계단(0.25)은 오르고 0.6 턱은 막히는지 보세요",
            true, 60f);

        // 경사로 4개: 각도별로 오르막 + 위쪽 발판
        float[] angles = { 30f, 45f, 50f, 60f };
        float[] xs = { -10.5f, -3.5f, 3.5f, 10.5f };
        const float height = 2.5f, z0 = 2f, thickness = 0.3f;
        for (int i = 0; i < angles.Length; i++)
        {
            float a = angles[i];
            float len = height / Mathf.Sin(a * Mathf.Deg2Rad);
            float run = len * Mathf.Cos(a * Mathf.Deg2Rad);
            var ramp = Prim(PrimitiveType.Cube, $"Ramp_{a}", new Vector3(xs[i], len * 0.5f * Mathf.Sin(a * Mathf.Deg2Rad) - thickness * 0.5f, z0 + run * 0.5f),
                new Vector3(4f, thickness, len), a <= 45f ? new Color32(90, 160, 110, 255) : new Color32(170, 100, 90, 255));
            ramp.transform.rotation = Quaternion.Euler(-a, 0f, 0f);
            ramp.layer = LayerMask.NameToLayer("Ground");

            var top = Prim(PrimitiveType.Cube, $"Top_{a}", new Vector3(xs[i], height * 0.5f - 0.05f, z0 + run + 2f), new Vector3(4f, height, 4f), GroundColor);
            top.layer = LayerMask.NameToLayer("Ground");
            Label3D($"{a}°", new Vector3(xs[i], 1.2f, z0 - 0.8f), 8f, Color.white);
        }

        // 계단: 0.25 높이 계단(stepOffset 0.3 이하 → 그냥 걸어 올라감) / 0.6 턱(막힘)
        for (int s = 0; s < 4; s++)
        {
            float h = 0.25f * (s + 1);
            Prim(PrimitiveType.Cube, $"Step_{s}", new Vector3(-6f, h * 0.5f, -4.6f - s * 0.8f), new Vector3(3f, h, 0.8f), new Color32(110, 120, 150, 255));
        }
        Label3D("계단 0.25", new Vector3(-6f, 1.8f, -4f), 6f, Color.white);
        Prim(PrimitiveType.Cube, "Ledge_0.6", new Vector3(0f, 0.3f, -6f), new Vector3(3f, 0.6f, 3f), new Color32(150, 110, 150, 255));
        Label3D("턱 0.6", new Vector3(0f, 1.6f, -4f), 6f, Color.white);

        // 벽: CharacterController 가 뚫지 않고 미끄러지듯 막히는 것 확인
        Prim(PrimitiveType.Cube, "Wall", new Vector3(7f, 1f, -6f), new Vector3(0.5f, 2f, 6f), new Color32(120, 125, 140, 255));

        var player = PlayerCapsule(new Vector3(0f, 1.1f, -1f));
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>()); // CharacterController 가 자체 캡슐 충돌을 가짐
        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = Vector3.zero;
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;
        var mover = player.AddComponent<CharacterControllerMover>();
        SetFloat(mover, "slopeLimit", 45f);

        var follow = ctx.Camera.gameObject.AddComponent<SimpleFollowCamera>();
        Set(follow, "target", player.transform);
        SetVector3(follow, "offset", new Vector3(0f, 7f, -10f));
        ctx.Camera.transform.position = player.transform.position + new Vector3(0f, 7f, -10f);

        Save(ctx);
    }
}
