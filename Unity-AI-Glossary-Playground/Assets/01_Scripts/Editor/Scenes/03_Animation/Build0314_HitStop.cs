using GlossaryDemo.AnimationDemo;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0314_HitStop
{
    [GlossaryScene("0314")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0314_HitStop")]
    public static void Build()
    {
        var ctx = NewScene3D("0314", "마우스 왼쪽 / Space : 공격\nH : 히트스톱 ON/OFF\n1/2/3/4 : 정지 시간 0.05 / 0.1 / 0.2 / 0.4초 (길게 하면 원리가 잘 보임)\n맞는 순간 화면 전체가 '탁' 멈추는 타격감을 비교해 보세요");
        FrameCamera(ctx.Camera, new Vector3(-1.2f, 2.6f, -6.5f), new Vector3(0.5f, 1.2f, 0f));
        var canvas = EnsureCanvas(ctx);

        var evt = new AnimationEvent { time = 0.4f, functionName = "OnAttackHit" };
        var idle = IdleClip("0314");
        var attack = AttackClip("0314", evt);
        var ctrl = NewController("0314_HitStop");
        ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        var sm = ctrl.layers[0].stateMachine;
        var sIdle = AddState(sm, "Idle", idle, new Vector3(300, 0));
        var sAttack = AddState(sm, "Attack", attack, new Vector3(300, 120));
        sm.defaultState = sIdle;
        Go(sIdle, sAttack, 0.05f).AddCondition(AnimatorConditionMode.If, 0f, "Attack");
        After(sAttack, sIdle, 0.95f, 0.1f);

        var player = CreateRig("Player", new Vector3(-0.6f, 0f, 0f), PlayerColor, withSword: true, yaw: 90f);
        player.Animator.runtimeAnimatorController = ctrl;

        // 적: 같은 리그를 빨간색으로 (애니메이션 없음)
        var enemy = CreateRig("Enemy", new Vector3(0.95f, 0f, 0f), EnemyColor, yaw: -90f);
        Object.DestroyImmediate(enemy.Animator);
        var renderers = enemy.Root.GetComponentsInChildren<Renderer>();
        var flash = UnlitMat("HitFlash_White", Color.white);

        var demo = player.Root.AddComponent<HitStopDemo>();
        Set(demo, "enemy", enemy.Root.transform);
        SetArray(demo, "enemyRenderers", renderers);
        Set(demo, "flashMaterial", flash);
        Set(demo, "cameraTransform", ctx.Camera.transform);

        ExplainPanel(canvas, "히트스톱 구현 포인트",
            "<color=#9FD0FF>IEnumerator HitStop(float d) {\n" +
            "  Time.timeScale = 0f;\n" +
            "  yield return new WaitForSecondsRealtime(d);\n" +
            "  Time.timeScale = 1f;\n}</color>\n\n" +
            "• 멈춘 동안 Time.deltaTime = 0 → 대기는 <b>Realtime</b>\n" +
            "• UI·카메라 흔들림은 unscaledDeltaTime 으로 계속 움직임\n" +
            "• 판정 타이밍은 Animation Event (칼이 닿는 프레임)\n" +
            "• 전체 대신 맞은 두 캐릭터만 animator.speed = 0 하는 방법도 있음", 540f, 340f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }
}
