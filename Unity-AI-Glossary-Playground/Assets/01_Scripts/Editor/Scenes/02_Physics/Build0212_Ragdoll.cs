using System.Collections.Generic;
using GlossaryDemo.PhysicsDemo;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0212_Ragdoll
{
    [GlossaryScene("0212")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0212_Ragdoll")]
    public static void Build()
    {
        var ctx = NewScene3D("0212",
            "왼쪽 클릭 : 적의 몸(부위)을 맞혀 죽이기 → Animator 끄고 래그돌로 전환\nR : 모두 부활   A : 'Animator 끄기' ON/OFF (OFF = 나쁜 예)\n맞은 부위와 방향에 따라 쓰러지는 모습이 매번 다릅니다\n(실무: GameObject > 3D Object > Ragdoll... 마법사로 휴머노이드 뼈에 자동 구성)",
            true, 30f);
        ctx.Camera.transform.position = new Vector3(0f, 6f, -10f);
        ctx.Camera.transform.rotation = Quaternion.Euler(22f, 0f, 0f);

        var controller = CreateWalkController();
        Vector3[] spots = { new(-4f, 0f, 2f), new(0f, 0f, 4f), new(4f, 0f, 1f) };
        for (int i = 0; i < spots.Length; i++)
            CreateRagdoll($"Enemy_{i + 1}", spots[i], Quaternion.Euler(0f, i * 120f, 0f), controller);

        // 쓰러질 때 걸리는 장애물
        Prim(PrimitiveType.Cube, "Step", new Vector3(-2f, 0.25f, -1f), new Vector3(3f, 0.5f, 1.5f), new Color32(110, 116, 132, 255));

        var demo = new GameObject("RagdollDemo").AddComponent<RagdollDemo>();
        SetLayerMask(demo, "ragdollMask", LayerMask.GetMask("Ragdoll"));
        Save(ctx);
    }

    // ── 래그돌 인형: 뼈(빈 Transform) + Collider + Rigidbody + CharacterJoint, 보이는 모양은 자식 프리미티브 ──
    private static void CreateRagdoll(string name, Vector3 pos, Quaternion rot, AnimatorController controller)
    {
        int layer = LayerMask.NameToLayer("Ragdoll");
        var root = new GameObject(name);
        root.transform.SetPositionAndRotation(pos, rot);
        var animator = root.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        var bones = new List<Rigidbody>();
        var skin = new Color32(220, 110, 100, 255);
        var dark = new Color32(150, 70, 70, 255);

        Rigidbody Bone(string boneName, Transform parent, Vector3 localPos, float mass, Rigidbody connect)
        {
            var go = new GameObject(boneName) { layer = layer };
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            if (connect != null)
            {
                // 관절: 부모 뼈와 연결. 뼈 방향(아래/위 = Y)을 비틀림 축으로
                var j = go.AddComponent<CharacterJoint>();
                j.connectedBody = connect;
                j.axis = Vector3.up;
                j.swingAxis = Vector3.right;
                var lo = j.lowTwistLimit; lo.limit = -30f; j.lowTwistLimit = lo;
                var hi = j.highTwistLimit; hi.limit = 30f; j.highTwistLimit = hi;
                var s1 = j.swing1Limit; s1.limit = 50f; j.swing1Limit = s1;
                var s2 = j.swing2Limit; s2.limit = 30f; j.swing2Limit = s2;
                j.enablePreprocessing = false;
            }
            bones.Add(rb);
            return rb;
        }

        void BoxPart(Rigidbody bone, Vector3 center, Vector3 size, Color color)
        {
            var col = bone.gameObject.AddComponent<BoxCollider>();
            col.center = center;
            col.size = size;
            var v = Prim(PrimitiveType.Cube, "Visual", center, size, color, bone.transform);
            Object.DestroyImmediate(v.GetComponent<Collider>());
            v.layer = layer;
        }

        void CapsulePart(Rigidbody bone, Vector3 center, float radius, float height, Color color)
        {
            var col = bone.gameObject.AddComponent<CapsuleCollider>();
            col.center = center;
            col.radius = radius;
            col.height = height;
            col.direction = 1;
            var v = Prim(PrimitiveType.Capsule, "Visual", center, new Vector3(radius * 2f, height * 0.5f, radius * 2f), color, bone.transform);
            Object.DestroyImmediate(v.GetComponent<Collider>());
            v.layer = layer;
        }

        var pelvis = Bone("Pelvis", root.transform, new Vector3(0f, 1f, 0f), 3f, null);
        BoxPart(pelvis, Vector3.zero, new Vector3(0.42f, 0.25f, 0.25f), dark);
        var chest = Bone("Chest", pelvis.transform, new Vector3(0f, 0.35f, 0f), 3f, pelvis);
        BoxPart(chest, new Vector3(0f, 0.05f, 0f), new Vector3(0.46f, 0.4f, 0.26f), skin);
        var head = Bone("Head", chest.transform, new Vector3(0f, 0.42f, 0f), 1f, chest);
        var headCol = head.gameObject.AddComponent<SphereCollider>();
        headCol.center = new Vector3(0f, 0.08f, 0f);
        headCol.radius = 0.15f;
        var hv = Prim(PrimitiveType.Sphere, "Visual", headCol.center, Vector3.one * 0.3f, new Color32(240, 200, 170, 255), head.transform);
        Object.DestroyImmediate(hv.GetComponent<Collider>());
        hv.layer = layer;

        foreach (int side in new[] { -1, 1 })
        {
            string s = side < 0 ? "L" : "R";
            var upperArm = Bone($"UpperArm_{s}", chest.transform, new Vector3(0.34f * side, 0.18f, 0f), 1f, chest);
            CapsulePart(upperArm, new Vector3(0f, -0.16f, 0f), 0.07f, 0.32f, skin);
            var lowerArm = Bone($"LowerArm_{s}", upperArm.transform, new Vector3(0f, -0.34f, 0f), 0.8f, upperArm);
            CapsulePart(lowerArm, new Vector3(0f, -0.15f, 0f), 0.06f, 0.3f, skin);

            var upperLeg = Bone($"UpperLeg_{s}", pelvis.transform, new Vector3(0.12f * side, -0.16f, 0f), 2f, pelvis);
            CapsulePart(upperLeg, new Vector3(0f, -0.21f, 0f), 0.08f, 0.42f, dark);
            var lowerLeg = Bone($"LowerLeg_{s}", upperLeg.transform, new Vector3(0f, -0.44f, 0f), 1.5f, upperLeg);
            CapsulePart(lowerLeg, new Vector3(0f, -0.2f, 0f), 0.07f, 0.4f, dark);
        }

        var enemy = root.AddComponent<RagdollEnemy>();
        Set(enemy, "animator", animator);
        SetArray(enemy, "bones", bones.ToArray());
        Set(enemy, "pelvis", pelvis.transform);
    }

    // ── 걷기 애니메이션 클립 + 컨트롤러 (Animator 가 뼈를 움직이는 "살아 있는" 상태용) ──
    private static AnimatorController CreateWalkController()
    {
        var clip = new AnimationClip { name = "0212_Walk", frameRate = 30f };
        void Swing(string path, float a, float b)
        {
            // 0초 a → 0.5초 b → 1초 a 로 X 축 회전 반복. Y/Z 는 0 으로 고정 키
            var x = new AnimationCurve(new Keyframe(0f, a), new Keyframe(0.5f, b), new Keyframe(1f, a));
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.x"), x);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.y"), AnimationCurve.Constant(0f, 1f, 0f));
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), "localEulerAnglesRaw.z"), AnimationCurve.Constant(0f, 1f, 0f));
        }
        Swing("Pelvis/UpperLeg_L", 25f, -25f);
        Swing("Pelvis/UpperLeg_R", -25f, 25f);
        Swing("Pelvis/UpperLeg_L/LowerLeg_L", 5f, 35f);
        Swing("Pelvis/UpperLeg_R/LowerLeg_R", 35f, 5f);
        Swing("Pelvis/Chest/UpperArm_L", -20f, 20f);
        Swing("Pelvis/Chest/UpperArm_R", 20f, -20f);
        Swing("Pelvis/Chest/UpperArm_L/LowerArm_L", -15f, -30f);
        Swing("Pelvis/Chest/UpperArm_R/LowerArm_R", -30f, -15f);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        SaveAsset(clip, Sub, "0212_Walk.anim");

        string path = $"{GeneratedFolder}/{Sub}/0212_Ragdoll.controller";
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null) AssetDatabase.DeleteAsset(path);
        return AnimatorController.CreateAnimatorControllerAtPathWithClip(path, clip);
    }
}
