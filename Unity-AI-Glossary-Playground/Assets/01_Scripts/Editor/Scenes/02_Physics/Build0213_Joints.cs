using GlossaryDemo.PhysicsDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0213_Joints
{
    [GlossaryScene("0213")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0213_Joints")]
    public static void Build()
    {
        var ctx = NewScene3D("0213",
            "WASD : 이동 (몸으로 문을 밀어 열기)   마우스 클릭 : 클릭한 물체 밀기\nL : 문 각도 제한 ON/OFF   C : 문 자동 닫힘(Spring) ON/OFF\n가운데 = HingeJoint 문, 오른쪽 = SpringJoint 추, 왼쪽 = HingeJoint 로 이은 밧줄",
            true, 30f);
        ctx.Camera.transform.position = new Vector3(0f, 10f, -12f);
        ctx.Camera.transform.rotation = Quaternion.Euler(38f, 0f, 0f);

        var frameColor = new Color32(110, 116, 132, 255);
        // ── 1) HingeJoint 문 ──
        // 벽 + 문틀 구멍 (x: -1 ~ 1 이 문 자리)
        Prim(PrimitiveType.Cube, "Wall_L", new Vector3(-4f, 1.5f, 2f), new Vector3(6f, 3f, 0.3f), frameColor);
        Prim(PrimitiveType.Cube, "Wall_R", new Vector3(4f, 1.5f, 2f), new Vector3(6f, 3f, 0.3f), frameColor);
        var doorGo = Prim(PrimitiveType.Cube, "Door (HingeJoint)", new Vector3(0f, 1.25f, 2f), new Vector3(1.9f, 2.4f, 0.12f), new Color32(170, 120, 70, 255));
        var doorRb = doorGo.AddComponent<Rigidbody>();
        doorRb.mass = 5f;
        doorRb.angularDamping = 1f;
        var hinge = doorGo.AddComponent<HingeJoint>();
        hinge.anchor = new Vector3(-0.5f, 0f, 0f);  // 문 왼쪽 끝 (로컬 좌표, 스케일 기준 -0.5 = 가장자리)
        hinge.axis = Vector3.up;                     // 세로 축으로 회전 = 경첩
        hinge.useLimits = true;
        hinge.limits = new JointLimits { min = -100f, max = 100f, bounciness = 0.1f };
        hinge.useSpring = true;
        hinge.spring = new JointSpring { spring = 8f, damper = 2f, targetPosition = 0f }; // 놓으면 0° 로 돌아옴
        Label("HingeJoint 문", new Vector3(0f, 3.2f, 2f), 5f, Color.white);

        // ── 2) SpringJoint 추 ──
        var ceiling = Prim(PrimitiveType.Cube, "SpringAnchor", new Vector3(7f, 4.5f, -1f), new Vector3(1f, 0.2f, 1f), frameColor);
        var anchorRb = ceiling.AddComponent<Rigidbody>();
        anchorRb.isKinematic = true;
        var weight = Prim(PrimitiveType.Cube, "Weight (SpringJoint)", new Vector3(7f, 2.2f, -1f), Vector3.one * 0.8f, new Color32(90, 160, 220, 255));
        var wrb = weight.AddComponent<Rigidbody>();
        wrb.mass = 1f;
        var spring = weight.AddComponent<SpringJoint>();
        spring.connectedBody = anchorRb;
        spring.autoConfigureConnectedAnchor = false;
        spring.anchor = new Vector3(0f, 0.5f, 0f);
        spring.connectedAnchor = new Vector3(0f, -0.5f, 0f);
        spring.spring = 40f;
        spring.damper = 0.5f;
        spring.minDistance = 0f;
        spring.maxDistance = 0.3f;
        var springLine = Line("SpringLine", 0.04f, new Color(0.8f, 0.9f, 1f));
        springLine.gameObject.AddComponent<JointLineFollower>().Init(ceiling.transform, weight.transform);
        Label("SpringJoint", new Vector3(7f, 5.2f, -1f), 5f, Color.white);

        // ── 3) 밧줄: 마디를 HingeJoint 로 연결 ──
        var top = Prim(PrimitiveType.Cube, "RopeAnchor", new Vector3(-7f, 5f, -1f), new Vector3(1f, 0.2f, 1f), frameColor);
        var prev = top.AddComponent<Rigidbody>();
        prev.isKinematic = true;
        for (int i = 0; i < 8; i++)
        {
            var link = Prim(PrimitiveType.Capsule, $"RopeLink_{i}", new Vector3(-7f, 4.6f - i * 0.5f, -1f), new Vector3(0.18f, 0.25f, 0.18f), new Color32(200, 180, 130, 255));
            var lrb = link.AddComponent<Rigidbody>();
            lrb.mass = 0.3f;
            lrb.linearDamping = 0.2f;
            var hj = link.AddComponent<HingeJoint>();
            hj.connectedBody = prev;
            hj.anchor = new Vector3(0f, 1f, 0f); // 마디 윗끝 (캡슐 로컬 높이 2 의 절반)
            hj.axis = Vector3.forward;
            prev = lrb;
        }
        var ball = Prim(PrimitiveType.Sphere, "RopeBall", new Vector3(-7f, 0.6f, -1f), Vector3.one * 0.7f, EnemyColor);
        var brb = ball.AddComponent<Rigidbody>();
        brb.mass = 1f;
        var bj = ball.AddComponent<HingeJoint>();
        bj.connectedBody = prev;
        bj.anchor = new Vector3(0f, 0.5f, 0f);
        bj.axis = Vector3.forward;
        Label("HingeJoint 밧줄", new Vector3(-7f, 5.7f, -1f), 5f, Color.white);

        var player = Player(new Vector3(0f, 1f, -3f));
        _ = player;

        var demo = new GameObject("JointDemo").AddComponent<JointDemo>();
        Set(demo, "door", hinge);
        Save(ctx);
    }
}
