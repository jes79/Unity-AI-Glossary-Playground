using GlossaryDemo.PhysicsDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0210_PhysicsMaterial
{
    [GlossaryScene("0210")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0210_PhysicsMaterial")]
    public static void Build()
    {
        var ctx = NewScene3D("0210",
            "R : 다시 떨어뜨리기 (6초마다 자동)\nC : 공의 Bounce Combine Maximum ↔ Average\n왼쪽 공 3개는 Bounciness 0 / 0.5 / 0.8, 오른쪽 상자 3개는 Friction 0 / 0.3 / 0.8 입니다\n생성된 재질: Assets/09_Generated/Physics/*.asset (PhysicsMaterial)",
            true, 30f);
        ctx.Camera.transform.position = new Vector3(0f, 7f, -13f);
        ctx.Camera.transform.rotation = Quaternion.Euler(22f, 0f, 0f);

        // 공: Bounciness 비교 (요청값 0.8). 바닥 재질(탄성 0)과 합칠 때 공 값이 그대로 쓰이도록 Maximum
        float[] bounce = { 0f, 0.5f, 0.8f };
        var balls = new Rigidbody[bounce.Length];
        for (int i = 0; i < bounce.Length; i++)
        {
            var mat = PhysicsMat($"Ball_Bounce{bounce[i] * 10:0}", bounce[i], 0.6f, PhysicsMaterialCombine.Maximum);
            var ball = Prim(PrimitiveType.Sphere, $"Ball (Bounciness {bounce[i]})", new Vector3(-7f + i * 2.5f, 6f, 2f), Vector3.one,
                Color.HSVToRGB(0.55f + i * 0.08f, 0.6f, 0.95f));
            ball.GetComponent<Collider>().sharedMaterial = mat;
            balls[i] = ball.AddComponent<Rigidbody>();
            Label($"Bounciness {bounce[i]}", new Vector3(-7f + i * 2.5f, 0.25f, 0.6f), 4f, Color.white);
        }

        // 경사로 (마찰 1, Multiply → 상자 마찰값이 그대로 결과가 됨)
        var rampRot = Quaternion.Euler(-25f, 0f, 0f);
        var rampCenter = new Vector3(4.5f, 1.6f, 3f);
        var ramp = Prim(PrimitiveType.Cube, "Ramp (25°)", rampCenter, new Vector3(7.5f, 0.3f, 8f), new Color32(110, 120, 150, 255));
        ramp.transform.rotation = rampRot;
        ramp.GetComponent<Collider>().sharedMaterial = PhysicsMat("Ramp_Friction1_Multiply", 0f, 1f, PhysicsMaterialCombine.Average, PhysicsMaterialCombine.Multiply);

        float[] friction = { 0f, 0.3f, 0.8f };
        var boxes = new Rigidbody[friction.Length];
        for (int i = 0; i < friction.Length; i++)
        {
            var mat = PhysicsMat($"Box_Friction{friction[i] * 10:0}", 0f, friction[i]);
            Vector3 local = new Vector3(-2.5f + i * 2.5f, 0.15f + 0.4f, 2.8f); // 경사로 윗부분 표면 위
            var box = Prim(PrimitiveType.Cube, $"Box (Friction {friction[i]})", rampCenter + rampRot * local, Vector3.one * 0.8f,
                Color.HSVToRGB(0.08f + i * 0.04f, 0.6f, 0.9f));
            box.transform.rotation = rampRot;
            box.GetComponent<Collider>().sharedMaterial = mat;
            boxes[i] = box.AddComponent<Rigidbody>();
            Label($"Friction {friction[i]}", rampCenter + rampRot * (local + new Vector3(0f, 1f, 0f)), 4f, Color.white);
        }

        var demo = new GameObject("PhysicsMaterialDemo").AddComponent<PhysicsMaterialDemo>();
        SetArray(demo, "balls", balls);
        SetArray(demo, "boxes", boxes);
        Save(ctx);
    }
}
