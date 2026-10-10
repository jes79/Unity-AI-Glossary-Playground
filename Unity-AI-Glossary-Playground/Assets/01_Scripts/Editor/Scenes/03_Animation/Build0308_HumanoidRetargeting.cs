using GlossaryDemo.AnimationDemo;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

// 설명 + 대체 구현: Humanoid 모델이 없으므로 '본 이름 매핑 표'로 체형이 다른 리그에 애니메이션을 옮겨 입힘
public static class Build0308_HumanoidRetargeting
{
    [GlossaryScene("0308")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0308_HumanoidRetargeting")]
    public static void Build()
    {
        var ctx = NewScene3D("0308", "M : 리타게팅 ON/OFF\n1 : 춤   2 : 달리기 (원본 클립 교체)\n왼쪽 원본 리그의 애니메이션이 본 이름·체형이 다른 오른쪽 캐릭터에 그대로 입혀지는지 보세요");
        FrameCamera(ctx.Camera, new Vector3(0f, 2.8f, -8f), new Vector3(0f, 1.1f, 0f));
        var canvas = EnsureCanvas(ctx);

        var dance = DanceClip("0308");
        var run = RunClip("0308");
        var ctrl = NewController("0308_Source");
        var sm = ctrl.layers[0].stateMachine;
        sm.defaultState = AddState(sm, "Dance", dance, new Vector3(300, 0));
        AddState(sm, "Run", run, new Vector3(300, 120));

        // 원본: 표준 리그 (Hips/Chest/ArmL …)
        var src = CreateRig("Source_Rig", new Vector3(-1.8f, 0f, 0f), PlayerColor, yaw: 180f);
        src.Animator.runtimeAnimatorController = ctrl;

        // 대상: 짧은 다리·긴 팔·큰 머리, 본 이름도 다름 (pelvis/spine/upperarm_l …)
        var tgtRoot = new GameObject("Target_Chibi");
        tgtRoot.transform.position = new Vector3(1.8f, 0f, 0f);
        tgtRoot.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        Color c = new Color(0.95f, 0.55f, 0.75f), d = new Color(0.6f, 0.35f, 0.5f);
        var pelvis = Bone("pelvis", tgtRoot.transform, new Vector3(0f, 0.6f, 0f));
        Visual(PrimitiveType.Cube, pelvis, new Vector3(0f, 0.02f, 0f), new Vector3(0.7f, 0.25f, 0.4f), d);
        var spine = Bone("spine", pelvis, new Vector3(0f, 0.12f, 0f));
        Visual(PrimitiveType.Cube, spine, new Vector3(0f, 0.25f, 0f), new Vector3(0.8f, 0.5f, 0.45f), c);
        var head = Bone("head", spine, new Vector3(0f, 0.52f, 0f));
        Visual(PrimitiveType.Sphere, head, new Vector3(0f, 0.32f, 0f), Vector3.one * 0.68f, SkinColor);
        Visual(PrimitiveType.Cube, head, new Vector3(0f, 0.36f, 0.3f), new Vector3(0.4f, 0.1f, 0.1f), new Color(0.1f, 0.1f, 0.15f));
        var armL = Bone("upperarm_l", spine, new Vector3(-0.49f, 0.45f, 0f));
        Visual(PrimitiveType.Cube, armL, new Vector3(0f, -0.42f, 0f), new Vector3(0.18f, 0.85f, 0.18f), SkinColor);
        var armR = Bone("upperarm_r", spine, new Vector3(0.49f, 0.45f, 0f));
        Visual(PrimitiveType.Cube, armR, new Vector3(0f, -0.42f, 0f), new Vector3(0.18f, 0.85f, 0.18f), SkinColor);
        var legL = Bone("thigh_l", pelvis, new Vector3(-0.2f, -0.05f, 0f));
        Visual(PrimitiveType.Cube, legL, new Vector3(0f, -0.275f, 0f), new Vector3(0.26f, 0.55f, 0.26f), d);
        var legR = Bone("thigh_r", pelvis, new Vector3(0.2f, -0.05f, 0f));
        Visual(PrimitiveType.Cube, legR, new Vector3(0f, -0.275f, 0f), new Vector3(0.26f, 0.55f, 0.26f), d);
        // 같은 컨트롤러를 그대로 붙여도 경로(Hips/Chest/ArmL)가 없어 아무 본도 안 움직임 → 리타게팅 OFF 때 확인
        var tgtAnimator = tgtRoot.AddComponent<Animator>();
        tgtAnimator.runtimeAnimatorController = ctrl;

        var retargeter = new GameObject("BoneRetargeter").AddComponent<BoneRetargeter>();
        var map = new (string human, Transform s, Transform t)[]
        {
            ("Hips", src.Hips, pelvis), ("Spine/Chest", src.Chest, spine), ("Head", src.Head, head),
            ("LeftUpperArm", src.ArmL, armL), ("RightUpperArm", src.ArmR, armR),
            ("LeftUpperLeg", src.LegL, legL), ("RightUpperLeg", src.LegR, legR),
        };
        var so = new SerializedObject(retargeter);
        var arr = so.FindProperty("bones");
        arr.arraySize = map.Length;
        for (int i = 0; i < map.Length; i++)
        {
            var e = arr.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("humanName").stringValue = map[i].human;
            e.FindPropertyRelative("source").objectReferenceValue = map[i].s;
            e.FindPropertyRelative("target").objectReferenceValue = map[i].t;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        Set(retargeter, "sourceHips", src.Hips);
        Set(retargeter, "targetHips", pelvis);
        SetFloat(retargeter, "sourceLegLength", 0.95f);
        SetFloat(retargeter, "targetLegLength", 0.55f);
        Set(retargeter, "sourceAnimator", src.Animator);

        Label(canvas, "원본 리그 (Hips, Chest, ArmL …)", new Vector2(-330f, 60f), 24f);
        Label(canvas, "대상 리그 (pelvis, spine, upperarm_l …)", new Vector2(330f, 60f), 24f);

        ExplainPanel(canvas, "실제 Humanoid 리타게팅 (Mixamo)",
            "1. Mixamo 에서 FBX 다운로드 (캐릭터 / 'Without Skin' 애니메이션)\n" +
            "2. 모델 FBX 선택 → Rig 탭 → Animation Type <b>Humanoid</b>\n   → Avatar Definition: Create From This Model → Apply\n" +
            "3. Configure 로 본 매핑(Hips, Spine, LeftUpperArm…) 확인\n" +
            "4. 애니메이션 FBX 도 Humanoid 로, Avatar Definition:\n   <b>Copy From Other Avatar</b> (또는 각자 Create)\n" +
            "5. 내 캐릭터 Animator 의 Avatar = 내 모델 Avatar,\n   컨트롤러에 Mixamo 클립 상태 추가\n\n" +
            "<color=#9BE89B>이 씬</color>: 매핑 표(왼쪽 Hips ↔ 오른쪽 pelvis …)로\n회전 변화량을 복사 + 엉덩이 이동을 다리 길이 비율로 보정\n" +
            "= Humanoid Avatar 가 내부에서 하는 일의 축소판", 560f, 420f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }

    private static AnimationClip DanceClip(string prefix)
    {
        var t = T(0f, 0.5f, 1f, 1.5f, 2f);
        return new ClipBuilder($"{prefix}_Dance")
            .Pos(Hips, t, V(0, 1f, 0), V(0, 0.85f, 0), V(0, 1f, 0), V(0, 0.85f, 0), V(0, 1f, 0))
            .Rot(Hips, t, V(0, 20, 0), V(0, 0, 0), V(0, -20, 0), V(0, 0, 0), V(0, 20, 0))
            .Rot(Chest, t, V(0, 0, 8), V(10, 0, 0), V(0, 0, -8), V(10, 0, 0), V(0, 0, 8))
            .Rot(Head, t, V(0, 0, -15), V(0, 0, 0), V(0, 0, 15), V(0, 0, 0), V(0, 0, -15))
            .Rot(ArmL, t, V(0, 0, -160), V(-40, 0, -40), V(0, 0, -20), V(-90, 0, -30), V(0, 0, -160))
            .Rot(ArmR, t, V(0, 0, 20), V(-90, 0, 30), V(0, 0, 160), V(-40, 0, 40), V(0, 0, 20))
            .Rot(LegL, t, V(0, 0, -10), V(-30, 0, -5), V(0, 0, -10), V(-30, 0, -5), V(0, 0, -10))
            .Rot(LegR, t, V(0, 0, 10), V(-30, 0, 5), V(0, 0, 10), V(-30, 0, 5), V(0, 0, 10))
            .Loop().Save();
    }
}
