using GlossaryDemo.AnimationDemo;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0301_AnimatorController
{
    [GlossaryScene("0301")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0301_AnimatorController")]
    public static void Build()
    {
        var ctx = NewScene3D("0301", "A/D : 달리기 (Speed)   Space : 점프 (Trigger)\n아래 상태 상자에서 현재 상태(파랑)와 전환 중인 상태(주황)를 보세요\nR : Idle 로 리셋");
        FrameCamera(ctx.Camera, new Vector3(0f, 3f, -9f), new Vector3(0f, 1.2f, 0f));
        var canvas = EnsureCanvas(ctx);

        // 클립
        var idle = IdleClip("0301");
        var run = RunClip("0301");
        var jump = JumpClip("0301");

        // Animator Controller: Idle(기본) ↔ Run, Any State → Jump → Idle
        var ctrl = NewController("0301_Character");
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        var sm = ctrl.layers[0].stateMachine;
        var sIdle = AddState(sm, "Idle", idle, new Vector3(300, 0));
        var sRun = AddState(sm, "Run", run, new Vector3(300, 120));
        var sJump = AddState(sm, "Jump", jump, new Vector3(600, 60));
        sm.defaultState = sIdle;

        Go(sIdle, sRun).AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        Go(sRun, sIdle).AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        var any = sm.AddAnyStateTransition(sJump);
        any.hasExitTime = false;
        any.duration = 0.05f;
        any.canTransitionToSelf = false;
        any.AddCondition(AnimatorConditionMode.If, 0f, "Jump");
        After(sJump, sIdle, 0.9f, 0.1f);

        var rig = CreateRig("Character", Vector3.zero, PlayerColor, yaw: 90f);
        rig.Animator.runtimeAnimatorController = ctrl;

        var display = StateDiagram(canvas, rig.Animator, 50f, "Idle", "Run", "Jump");
        var demo = rig.Root.AddComponent<AnimatorControllerDemo>();
        Set(demo, "display", display);

        ExplainPanel(canvas, "Animator Controller 구성",
            "• <b>파라미터</b> Speed(Float), Jump(Trigger)\n" +
            "• <b>Idle</b>(기본 상태, 주황) → Run : Speed > 0.1\n" +
            "• <b>Run</b> → Idle : Speed < 0.1\n" +
            "• <b>Any State</b> → Jump : Jump 트리거\n" +
            "• <b>Jump</b> → Idle : Exit Time 0.9 (클립 끝나면 복귀)\n\n" +
            "에디터에서 보기: Assets/09_Generated/Animation/\n0301_Character.controller 더블클릭 → Animator 창\n" +
            "플레이 중 캐릭터를 선택하면 Animator 창에 진행 막대가 보입니다.", 520f, 330f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }
}
