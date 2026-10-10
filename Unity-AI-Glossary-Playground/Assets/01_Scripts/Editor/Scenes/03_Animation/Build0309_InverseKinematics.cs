using GlossaryDemo.AnimationDemo;
using UnityEditor;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

// 설명 + 대체 구현: Humanoid 모델이 없으므로 2본 IK 를 직접 풀어 계단 위 발 맞추기를 보여 줌
public static class Build0309_InverseKinematics
{
    [GlossaryScene("0309")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0309_InverseKinematics")]
    public static void Build()
    {
        var ctx = NewScene3D("0309", "A/D : 계단 오르내리기\nI : Foot IK ON/OFF\n계단 위에 멈춰서 두 발이 서로 다른 칸에 놓이게 해 보세요.\nOFF 면 한쪽 발이 공중에 뜨거나 계단에 박힙니다");
        FrameCamera(ctx.Camera, new Vector3(1f, 2.6f, -6.5f), new Vector3(1f, 1.2f, 0f));
        var canvas = EnsureCanvas(ctx);
        int groundLayer = LayerMask.NameToLayer("Ground");

        // 계단: 폭 0.7, 단 높이 0.25 × 6칸 + 위 평지
        const float stepW = 0.7f, stepH = 0.25f;
        for (int i = 0; i < 6; i++)
        {
            float h = (i + 1) * stepH;
            var s = Prim(PrimitiveType.Cube, $"Step_{i}", new Vector3(i * stepW + stepW * 0.5f, h * 0.5f, 0f), new Vector3(stepW, h, 2f),
                i % 2 == 0 ? new Color(0.45f, 0.5f, 0.6f) : new Color(0.4f, 0.44f, 0.54f));
            s.layer = groundLayer;
        }
        var top = Prim(PrimitiveType.Cube, "TopFloor", new Vector3(6 * stepW + 1.5f, 6 * stepH * 0.5f, 0f), new Vector3(3f, 6 * stepH, 2f), new Color(0.45f, 0.5f, 0.6f));
        top.layer = groundLayer;

        // 두 마디 다리를 가진 캐릭터 (+X 를 바라봄)
        var root = new GameObject("IK_Character");
        root.transform.position = new Vector3(-2f, 0f, 0f);
        root.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        Color body = PlayerColor, dark = PlayerColor * 0.7f;
        dark.a = 1f;
        var hips = Bone("Hips", root.transform, new Vector3(0f, 1f, 0f));
        Visual(PrimitiveType.Cube, hips, new Vector3(0f, 0.02f, 0f), new Vector3(0.5f, 0.22f, 0.3f), dark);
        var chest = Bone("Chest", hips, new Vector3(0f, 0.12f, 0f));
        Visual(PrimitiveType.Cube, chest, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.6f, 0.32f), body);
        var head = Bone("Head", chest, new Vector3(0f, 0.62f, 0f));
        Visual(PrimitiveType.Sphere, head, new Vector3(0f, 0.2f, 0f), Vector3.one * 0.4f, SkinColor);
        Visual(PrimitiveType.Cube, head, new Vector3(0f, 0.24f, 0.17f), new Vector3(0.28f, 0.08f, 0.1f), new Color(0.1f, 0.1f, 0.15f));
        foreach (float sx in new[] { -0.39f, 0.39f })
        {
            var arm = Bone(sx < 0 ? "ArmL" : "ArmR", chest, new Vector3(sx, 0.56f, 0f));
            Visual(PrimitiveType.Cube, arm, new Vector3(0f, -0.3f, 0f), new Vector3(0.16f, 0.62f, 0.16f), SkinColor);
        }

        Transform[] Leg(string side, float x)
        {
            var thigh = Bone($"Thigh{side}", hips, new Vector3(x, -0.05f, 0f));
            Visual(PrimitiveType.Cube, thigh, new Vector3(0f, -0.24f, 0f), new Vector3(0.2f, 0.48f, 0.2f), dark);
            var shin = Bone($"Shin{side}", thigh, new Vector3(0f, -0.48f, 0f));
            Visual(PrimitiveType.Sphere, shin, Vector3.zero, Vector3.one * 0.2f, dark, "Knee");
            Visual(PrimitiveType.Cube, shin, new Vector3(0f, -0.235f, 0f), new Vector3(0.17f, 0.47f, 0.17f), dark);
            var foot = Bone($"Foot{side}", shin, new Vector3(0f, -0.47f, 0f));
            Visual(PrimitiveType.Cube, foot, new Vector3(0f, -0.01f, 0.08f), new Vector3(0.18f, 0.07f, 0.3f), new Color(0.2f, 0.22f, 0.3f));
            return new[] { thigh, shin, foot };
        }
        var l = Leg("L", -0.14f);
        var r = Leg("R", 0.14f);

        // IK 목표 표시 (바닥의 작은 원판)
        var markL = Visual(PrimitiveType.Cylinder, null, Vector3.zero, new Vector3(0.22f, 0.005f, 0.22f), new Color(1f, 0.8f, 0.2f), "IKTarget_L");
        var markR = Visual(PrimitiveType.Cylinder, null, Vector3.zero, new Vector3(0.22f, 0.005f, 0.22f), new Color(1f, 0.5f, 0.3f), "IKTarget_R");

        var demo = root.AddComponent<FootIKDemo>();
        Set(demo, "hips", hips);
        Set(demo, "thighL", l[0]); Set(demo, "shinL", l[1]); Set(demo, "footL", l[2]);
        Set(demo, "thighR", r[0]); Set(demo, "shinR", r[1]); Set(demo, "footR", r[2]);
        Set(demo, "targetMarkerL", markL.transform);
        Set(demo, "targetMarkerR", markR.transform);
        SetLayerMask(demo, "groundMask", LayerMask.GetMask("Ground"));

        ExplainPanel(canvas, "실제 Foot IK (Humanoid)",
            "1. 모델 Rig = Humanoid, Animator 레이어 톱니 → <b>IK Pass</b> 체크\n" +
            "2. 스크립트에서\n" +
            "<color=#9FD0FF>void OnAnimatorIK(int layer) {\n" +
            "  // 발 아래로 Raycast 해서 hit 을 얻은 뒤\n" +
            "  animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);\n" +
            "  animator.SetIKPosition(AvatarIKGoal.LeftFoot, hit.point + up*발높이);\n" +
            "  animator.SetIKRotation(AvatarIKGoal.LeftFoot,\n" +
            "    Quaternion.FromToRotation(up, hit.normal) * transform.rotation);\n" +
            "}</color>\n" +
            "3. 엉덩이(bodyPosition)를 낮은 발 쪽으로 내려 줌\n" +
            "Generic 리그: Animation Rigging 패키지 Two Bone IK Constraint\n\n" +
            "<color=#9BE89B>이 씬</color>: 같은 계산(코사인 법칙 2본 IK)을 직접 구현", 600f, 430f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }
}
