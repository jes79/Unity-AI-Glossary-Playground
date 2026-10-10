using GlossaryDemo.AnimationDemo;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0303_AnimatorParameters
{
    [GlossaryScene("0303")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0303_AnimatorParameters")]
    public static void Build()
    {
        var ctx = NewScene3D("0303", "A/D : 이동 → Speed(Float)\nSpace : 점프 → IsGrounded(Bool)\n마우스 왼쪽 / J : 공격 → Attack(Trigger)\n정보 패널 아래에서 파라미터 값이 바뀌는 것을 보세요");
        FrameCamera(ctx.Camera, new Vector3(0f, 3.2f, -10f), new Vector3(0f, 1.4f, 0f));
        var canvas = EnsureCanvas(ctx);

        var idle = IdleClip("0303");
        var run = RunClip("0303");
        var air = JumpClip("0303");
        var attack = AttackClip("0303");

        var ctrl = NewController("0303_Parameters");
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        // Bool 기본값 true (시작할 때 땅 위)
        var ps = ctrl.parameters;
        foreach (var p in ps) if (p.name == "IsGrounded") p.defaultBool = true;
        ctrl.parameters = ps;

        var sm = ctrl.layers[0].stateMachine;
        var sIdle = AddState(sm, "Idle", idle, new Vector3(300, 0));
        var sRun = AddState(sm, "Run", run, new Vector3(300, 120));
        var sAir = AddState(sm, "Air", air, new Vector3(600, 0));
        var sAttack = AddState(sm, "Attack", attack, new Vector3(600, 120));
        sm.defaultState = sIdle;

        Go(sIdle, sRun).AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        Go(sRun, sIdle).AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        // IsGrounded == false 인 동안 Air, true 가 되면 Idle 로
        foreach (var from in new[] { sIdle, sRun, sAttack })
            Go(from, sAir, 0.05f).AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
        Go(sAir, sIdle, 0.1f).AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");

        // Attack 트리거 → 공격 1회, 끝나면 Idle
        foreach (var from in new[] { sIdle, sRun })
            Go(from, sAttack, 0.05f).AddCondition(AnimatorConditionMode.If, 0f, "Attack");
        After(sAttack, sIdle, 0.9f, 0.1f);

        var rig = CreateRig("Character", Vector3.zero, PlayerColor, withSword: true, yaw: 90f);
        rig.Animator.runtimeAnimatorController = ctrl;

        var display = StateDiagram(canvas, rig.Animator, 50f, "Idle", "Run", "Air", "Attack");
        var demo = rig.Root.AddComponent<AnimatorParameterDemo>();
        Set(demo, "display", display);

        ExplainPanel(canvas, "세 가지 파라미터",
            "<b>Float</b>  animator.SetFloat(\"Speed\", 3.2f)\n  연속 값. 'Speed > 0.1' 같은 비교 조건\n\n" +
            "<b>Bool</b>  animator.SetBool(\"IsGrounded\", false)\n  켜고 끌 때까지 유지되는 상태\n\n" +
            "<b>Trigger</b>  animator.SetTrigger(\"Attack\")\n  전환에 한 번 쓰이면 자동으로 꺼짐 (1회성 동작)\n\n" +
            "팁: Animator.StringToHash(\"Speed\") 로 해시를 캐싱", 520f, 360f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }
}
