using GlossaryDemo.AnimationDemo;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0302_StateTransition
{
    [GlossaryScene("0302")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0302_StateTransition")]
    public static void Build()
    {
        var ctx = NewScene3D("0302", "Space : 두 캐릭터에 동시에 Jump 트리거\n왼쪽(Has Exit Time ON)은 달리기 한 바퀴가 끝날 때까지 기다렸다 점프,\n오른쪽(OFF)은 누르자마자 점프합니다.  R : 리셋");
        FrameCamera(ctx.Camera, new Vector3(0f, 3f, -9f), new Vector3(0f, 1.2f, 0f));
        var canvas = EnsureCanvas(ctx);

        var run = RunClip("0302");
        var jump = JumpClip("0302");

        var on = MakeController("0302_ExitTimeOn", run, jump, true);
        var off = MakeController("0302_ExitTimeOff", run, jump, false);

        var left = CreateRig("HasExitTime_ON", new Vector3(-2.2f, 0f, 0f), new Color(0.95f, 0.6f, 0.3f), yaw: 180f);
        left.Animator.runtimeAnimatorController = on;
        var right = CreateRig("HasExitTime_OFF", new Vector3(2.2f, 0f, 0f), PlayerColor, yaw: 180f);
        right.Animator.runtimeAnimatorController = off;

        var demo = new GameObject("StateTransitionDemo").AddComponent<StateTransitionDemo>();
        Set(demo, "exitTimeOn", left.Animator);
        Set(demo, "exitTimeOff", right.Animator);

        Label(canvas, "Has Exit Time  ON", new Vector2(-330f, 140f), 30f);
        Label(canvas, "Has Exit Time  OFF", new Vector2(330f, 140f), 30f);
        StateDiagram(canvas, left.Animator, 0, 60f, -330f, "Run", "Jump");
        StateDiagram(canvas, right.Animator, 0, 60f, 330f, "Run", "Jump");

        ExplainPanel(canvas, "Run → Jump 바로 넘어가게 하기",
            "1. Animator 창에서 Run → Jump 전환 화살표 선택\n" +
            "2. Inspector 의 <b>Has Exit Time</b> 체크 해제\n" +
            "3. Conditions 에 Jump(Trigger) 추가\n" +
            "4. Transition Duration 0~0.1 로 짧게\n\n" +
            "Has Exit Time 이 켜져 있으면 조건이 맞아도\n<b>Exit Time</b>(여기선 0.95 = 클립 95%)까지 기다립니다.\n" +
            "조건이 없는 전환은 Exit Time 이 필수(클립 끝나면 자동 복귀용).", 520f, 340f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }

    private static AnimatorController MakeController(string name, AnimationClip run, AnimationClip jump, bool hasExitTime)
    {
        var ctrl = NewController(name);
        ctrl.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        var sm = ctrl.layers[0].stateMachine;
        var sRun = AddState(sm, "Run", run, new Vector3(300, 0));
        var sJump = AddState(sm, "Jump", jump, new Vector3(300, 120));
        sm.defaultState = sRun;

        // 비교 대상: Run → Jump 전환의 Has Exit Time
        var t = sRun.AddTransition(sJump);
        t.hasExitTime = hasExitTime;
        t.exitTime = 0.95f;
        t.duration = 0.05f;
        t.AddCondition(AnimatorConditionMode.If, 0f, "Jump");

        After(sJump, sRun, 0.9f, 0.1f);
        return ctrl;
    }
}
