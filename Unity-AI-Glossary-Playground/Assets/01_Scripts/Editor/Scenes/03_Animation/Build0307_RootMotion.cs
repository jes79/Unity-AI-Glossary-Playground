using GlossaryDemo.AnimationDemo;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0307_RootMotion
{
    [GlossaryScene("0307")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0307_RootMotion")]
    public static void Build()
    {
        var ctx = NewScene3D("0307", "A/D : 걷기 (코드로 이동)\nSpace : 구르기 (Root Motion 으로 이동)\nR : Root Motion ON/OFF — OFF 면 같은 구르기가 제자리에서 끝납니다\n바닥 눈금(1m)으로 이동 거리를 보세요");
        FrameCamera(ctx.Camera, new Vector3(0f, 4f, -11f), new Vector3(0f, 1f, 0f));
        var canvas = EnsureCanvas(ctx);

        var idle = Bake(IdleClip("0307"), true);
        var walk = Bake(WalkClip("0307"), true);
        var roll = Bake(RollClip("0307"), false);

        var ctrl = NewController("0307_RootMotion");
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Roll", AnimatorControllerParameterType.Trigger);
        var sm = ctrl.layers[0].stateMachine;
        var sIdle = AddState(sm, "Idle", idle, new Vector3(300, 0));
        var sWalk = AddState(sm, "Walk", walk, new Vector3(300, 120));
        var sRoll = AddState(sm, "Roll", roll, new Vector3(600, 60));
        sm.defaultState = sIdle;
        Go(sIdle, sWalk).AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        Go(sWalk, sIdle).AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        Go(sIdle, sRoll, 0.05f).AddCondition(AnimatorConditionMode.If, 0f, "Roll");
        Go(sWalk, sRoll, 0.05f).AddCondition(AnimatorConditionMode.If, 0f, "Roll");
        After(sRoll, sIdle, 0.95f, 0.08f);

        var rig = CreateRig("Character", new Vector3(-4f, 0f, 0f), PlayerColor, yaw: 90f);
        rig.Animator.runtimeAnimatorController = ctrl;

        // Generic 리그의 Avatar: Root Node 를 Hips 로 지정 → Hips 의 수평 이동이 루트 모션(deltaPosition)으로 추출됨
        var avatar = AvatarBuilder.BuildGenericAvatar(rig.Root, "Hips");
        avatar.name = "0307_GenericAvatar";
        SaveAsset(avatar, Sub, "0307_GenericAvatar.asset");
        rig.Animator.avatar = avatar;
        rig.Animator.applyRootMotion = true;

        // 1m 눈금
        for (int i = -9; i <= 9; i++)
            Visual(PrimitiveType.Cube, null, new Vector3(i, 0.005f, -0.8f), new Vector3(0.04f, 0.01f, i % 5 == 0 ? 0.8f : 0.4f), new Color(0.6f, 0.65f, 0.75f), $"Tick_{i}");
        var marker = Visual(PrimitiveType.Cube, null, new Vector3(-4f, 0.01f, 0f), new Vector3(0.08f, 0.02f, 1.6f), new Color(1f, 0.8f, 0.2f), "RollStartMarker");

        var demo = rig.Root.AddComponent<RootMotionDemo>();
        Set(demo, "startMarker", marker.transform);

        ExplainPanel(canvas, "Root Motion 설정",
            "• Animator 의 <b>Apply Root Motion</b> 체크 → 클립의 루트 이동이 캐릭터를 움직임\n" +
            "• 스크립트에 <b>OnAnimatorMove()</b> 가 있으면 적용을 직접 제어\n" +
            "  (여기선 구르기 상태일 때만 animator.deltaPosition 을 더함)\n" +
            "• Generic: Rig 탭 Root node 지정 / Humanoid: 자동(무게중심)\n" +
            "• 클립 Import 설정 Root Transform Position(Y)\n  'Bake Into Pose' = 높이는 자세로, 이동만 추출\n\n" +
            "걷기는 코드, 구르기는 애니메이션: 구르기 거리가\n애니메이션과 정확히 일치(발 미끄러짐 없음)", 540f, 360f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }

    // 구르기 0.9초: Hips 를 앞으로 2.5m, 한 바퀴(360도) 회전
    private static AnimationClip RollClip(string prefix)
    {
        var t = T(0f, 0.15f, 0.45f, 0.75f, 0.9f);
        var b = new ClipBuilder($"{prefix}_Roll")
            .Pos(Hips, t, V(0, 1f, 0), V(0, 0.6f, 0.4f), V(0, 0.5f, 1.3f), V(0, 0.6f, 2.2f), V(0, 1f, 2.5f))
            .Rot(Hips, t, V(0, 0, 0), V(60, 0, 0), V(200, 0, 0), V(330, 0, 0), V(360, 0, 0))
            .Rot(Chest, t, V(0, 0, 0), V(40, 0, 0), V(40, 0, 0), V(30, 0, 0), V(0, 0, 0))
            .Rot(Head, t, V(0, 0, 0), V(30, 0, 0), V(30, 0, 0), V(20, 0, 0), V(0, 0, 0))
            .Rot(LegL, t, V(0, 0, 0), V(-100, 0, 0), V(-120, 0, 0), V(-90, 0, 0), V(0, 0, 0))
            .Rot(LegR, t, V(0, 0, 0), V(-100, 0, 0), V(-120, 0, 0), V(-90, 0, 0), V(0, 0, 0))
            .Rot(ArmL, t, V(0, 0, -6), V(-70, 0, -10), V(-90, 0, -10), V(-60, 0, -10), V(0, 0, -6))
            .Rot(ArmR, t, V(0, 0, 6), V(-70, 0, 10), V(-90, 0, 10), V(-60, 0, 10), V(0, 0, 6));
        return b.Clip;
    }

    // 루트 모션 추출 설정: 높이(Y)와 회전은 자세에 굽고(Bake Into Pose), 수평 이동(XZ)만 루트 모션으로
    private static AnimationClip Bake(AnimationClip clip, bool bakeXZ)
    {
        bool loop = AnimationUtility.GetAnimationClipSettings(clip).loopTime;
        var s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = loop;
        s.loopBlendOrientation = true;
        s.keepOriginalOrientation = true;
        s.loopBlendPositionY = true;
        s.keepOriginalPositionY = true;
        s.loopBlendPositionXZ = bakeXZ;
        s.keepOriginalPositionXZ = true;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        if (!AssetDatabase.Contains(clip)) SaveAsset(clip, Sub, clip.name + ".anim");
        EditorUtility.SetDirty(clip);
        return clip;
    }
}
