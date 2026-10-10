using System.Collections.Generic;
using GlossaryDemo.AnimationDemo;
using UnityEditor;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

// 설명 + 대체 구현: 2D Animation 패키지 없이 SpriteRenderer 계층 + 본 회전 클립으로 2D 본 애니메이션
public static class Build0311_Bone2DAnimation
{
    private const string Pelvis = "Pelvis";
    private const string ChestP = "Pelvis/Chest";
    private const string HeadP = "Pelvis/Chest/Head";
    private const string UArmL = "Pelvis/Chest/UpperArmL";
    private const string FArmL = "Pelvis/Chest/UpperArmL/ForearmL";
    private const string UArmR = "Pelvis/Chest/UpperArmR";
    private const string FArmR = "Pelvis/Chest/UpperArmR/ForearmR";
    private const string ThighL = "Pelvis/ThighL";
    private const string ShinL = "Pelvis/ThighL/ShinL";
    private const string ThighR = "Pelvis/ThighR";
    private const string ShinR = "Pelvis/ThighR/ShinR";

    private static readonly List<GameObject> GizmoList = new List<GameObject>();
    private static readonly List<SpriteRenderer> Skin = new List<SpriteRenderer>();

    [GlossaryScene("0311")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0311_Bone2DAnimation")]
    public static void Build()
    {
        var ctx = NewScene2D("0311", "1 : 걷기   2 : 손 흔들기\nB : 본 표시 ON/OFF   H : 그림 숨기기 (본만 보기)   P : 일시정지\n본(노란 막대)이 돌면 그 아래 붙은 그림이 함께 움직이는 것을 보세요");
        GizmoList.Clear();
        Skin.Clear();

        Color body = new Color(0.35f, 0.75f, 0.55f), dark = new Color(0.22f, 0.5f, 0.38f);
        var root = new GameObject("Puppet");
        root.transform.position = new Vector3(0f, -1.2f, 0f);

        var pelvis = Bone2D("Pelvis", root.transform, Vector3.zero, null);
        var chest = Bone2D("Chest", pelvis, new Vector3(0f, 1.3f, 0f), pelvis);
        Part(pelvis, SquareSprite, new Vector3(0f, 0.65f, 0f), new Vector2(0.9f, 1.5f), body, 5);
        var head = Bone2D("Head", chest, new Vector3(0f, 0.25f, 0f), chest);
        Part(head, CircleSprite, new Vector3(0f, 0.5f, 0f), new Vector2(0.9f, 0.9f), SkinColor, 6);
        Part(head, CircleSprite, new Vector3(0.18f, 0.58f, 0f), new Vector2(0.12f, 0.12f), Color.black, 7);

        Transform Arm(string side, float x, int order, Color c)
        {
            var upper = Bone2D($"UpperArm{side}", chest, new Vector3(x, 0f, 0f), chest);
            Part(upper, SquareSprite, new Vector3(0f, -0.35f, 0f), new Vector2(0.24f, 0.75f), c, order);
            var fore = Bone2D($"Forearm{side}", upper, new Vector3(0f, -0.7f, 0f), upper);
            Part(fore, SquareSprite, new Vector3(0f, -0.3f, 0f), new Vector2(0.2f, 0.65f), SkinColor, order);
            BoneGizmo(fore, new Vector3(0f, -0.6f, 0f));
            return upper;
        }
        Transform Leg(string side, float x, int order, Color c)
        {
            var thigh = Bone2D($"Thigh{side}", pelvis, new Vector3(x, 0f, 0f), pelvis);
            Part(thigh, SquareSprite, new Vector3(0f, -0.4f, 0f), new Vector2(0.3f, 0.85f), c, order);
            var shin = Bone2D($"Shin{side}", thigh, new Vector3(0f, -0.8f, 0f), thigh);
            Part(shin, SquareSprite, new Vector3(0f, -0.4f, 0f), new Vector2(0.26f, 0.8f), c, order);
            BoneGizmo(shin, new Vector3(0f, -0.8f, 0f));
            return thigh;
        }
        // 뒤쪽(L) 팔다리는 정렬 순서를 낮게 해 몸 뒤로
        Arm("L", -0.05f, 2, dark);
        Arm("R", 0.05f, 9, body);
        Leg("L", -0.1f, 3, dark);
        Leg("R", 0.1f, 4, body);
        BoneGizmo(head, new Vector3(0f, 0.8f, 0f));

        CreateSpriteObject("Ground", SquareSprite, new Vector3(0f, -2.9f), new Vector2(20f, 0.3f), GroundColor);

        // 클립: 본의 Z 회전만 키프레임
        var walk = WalkClip();
        var wave = WaveClip();
        var ctrl = NewController("0311_Puppet");
        var sm = ctrl.layers[0].stateMachine;
        sm.defaultState = AddState(sm, "Walk", walk, new Vector3(300, 0));
        AddState(sm, "Wave", wave, new Vector3(300, 120));
        var animator = root.AddComponent<Animator>();
        animator.runtimeAnimatorController = ctrl;

        var demo = root.AddComponent<Bone2DDemo>();
        Set(demo, "animator", animator);
        SetArray(demo, "boneGizmos", GizmoList.ToArray());
        SetArray(demo, "skinParts", Skin.ToArray());
        SetArray(demo, "watchBones", root.transform.Find(UArmR), root.transform.Find(FArmR), root.transform.Find(ThighR), root.transform.Find(ShinR));

        var canvas = CreateCanvas("Canvas");
        ExplainPanel(canvas, "2D Animation 패키지로 하는 법",
            "1. Package Manager → <b>2D Animation</b> (+ 2D PSD Importer) 설치\n" +
            "2. 캐릭터 PSB/PNG 선택 → Sprite Editor → <b>Skinning Editor</b>\n" +
            "3. Create Bone 으로 골반→척추→머리, 어깨→팔꿈치→손 본 그리기\n" +
            "4. Auto Geometry → Auto Weights (메시와 본 가중치)\n" +
            "5. 씬에 놓고 <b>Sprite Skin</b> 컴포넌트 → Create Bones\n" +
            "6. Animation 창에서 본 회전 키프레임 (지금 이 씬과 동일)\n" +
            "7. IK Manager 2D + Limb Solver 로 손발 IK\n\n" +
            "<color=#9BE89B>이 씬</color>: 부위별 스프라이트를 본 Transform 아래 둔\n컷아웃 방식. 가중치 변형(휘어짐)만 없고 원리는 같음", 560f, 400f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }

    private static Transform Bone2D(string name, Transform parent, Vector3 localPos, Transform gizmoParent)
    {
        var t = Bone(name, parent, localPos);
        // 부모 본 → 이 본 방향으로 노란 막대 (본 시각화)
        if (gizmoParent != null) BoneGizmo(gizmoParent, localPos);
        var joint = CreateSpriteObject("Joint", CircleSprite, Vector3.zero, new Vector2(0.14f, 0.14f), new Color(1f, 0.85f, 0.2f), t, 20);
        GizmoList.Add(joint.gameObject);
        return t;
    }

    // bone 원점에서 localEnd 까지 이어지는 막대
    private static void BoneGizmo(Transform bone, Vector3 localEnd)
    {
        float len = localEnd.magnitude;
        if (len < 0.01f) return;
        var sr = CreateSpriteObject("BoneGizmo", SquareSprite, localEnd * 0.5f, new Vector2(0.06f, len), new Color(1f, 0.85f, 0.2f, 0.9f), bone, 19);
        sr.transform.localRotation = Quaternion.FromToRotation(Vector3.up, localEnd.normalized);
        GizmoList.Add(sr.gameObject);
    }

    private static void Part(Transform bone, Sprite sprite, Vector3 pos, Vector2 size, Color color, int order)
    {
        Skin.Add(CreateSpriteObject("Sprite", sprite, pos, size, color, bone, order));
    }

    private static AnimationClip WalkClip()
    {
        var t = T(0f, 0.25f, 0.5f, 0.75f, 1f);
        Vector3 Z(float z) => new Vector3(0f, 0f, z);
        return new ClipBuilder("0311_Walk")
            .Pos(Pelvis, t, V(0, 0, 0), V(0, -0.08f, 0), V(0, 0, 0), V(0, -0.08f, 0), V(0, 0, 0))
            .Rot(ChestP, t, Z(-4), Z(-6), Z(-4), Z(-6), Z(-4))
            .Rot(HeadP, t, Z(2), Z(4), Z(2), Z(4), Z(2))
            .Rot(ThighL, t, Z(30), Z(0), Z(-30), Z(0), Z(30))
            .Rot(ShinL, t, Z(0), Z(0), Z(-50), Z(-30), Z(0))
            .Rot(ThighR, t, Z(-30), Z(0), Z(30), Z(0), Z(-30))
            .Rot(ShinR, t, Z(-50), Z(-30), Z(0), Z(0), Z(-50))
            .Rot(UArmL, t, Z(-30), Z(0), Z(30), Z(0), Z(-30))
            .Rot(FArmL, t, Z(20), Z(25), Z(40), Z(25), Z(20))
            .Rot(UArmR, t, Z(30), Z(0), Z(-30), Z(0), Z(30))
            .Rot(FArmR, t, Z(40), Z(25), Z(20), Z(25), Z(40))
            .Loop().Save();
    }

    private static AnimationClip WaveClip()
    {
        var t = T(0f, 0.3f, 0.6f, 0.9f, 1.2f);
        Vector3 Z(float z) => new Vector3(0f, 0f, z);
        return new ClipBuilder("0311_Wave")
            .Pos(Pelvis, t, V(0, 0, 0), V(0, 0, 0), V(0, 0, 0), V(0, 0, 0), V(0, 0, 0))
            .Rot(ChestP, t, Z(0), Z(-3), Z(0), Z(-3), Z(0))
            .Rot(HeadP, t, Z(-5), Z(5), Z(-5), Z(5), Z(-5))
            .Rot(ThighL, t, Z(5), Z(5), Z(5), Z(5), Z(5))
            .Rot(ShinL, t, Z(0), Z(0), Z(0), Z(0), Z(0))
            .Rot(ThighR, t, Z(-5), Z(-5), Z(-5), Z(-5), Z(-5))
            .Rot(ShinR, t, Z(0), Z(0), Z(0), Z(0), Z(0))
            .Rot(UArmL, t, Z(-10), Z(-12), Z(-10), Z(-12), Z(-10))
            .Rot(FArmL, t, Z(10), Z(10), Z(10), Z(10), Z(10))
            .Rot(UArmR, t, Z(150), Z(160), Z(150), Z(160), Z(150))
            .Rot(FArmR, t, Z(-30), Z(30), Z(-30), Z(30), Z(-30))
            .Loop().Save();
    }
}
