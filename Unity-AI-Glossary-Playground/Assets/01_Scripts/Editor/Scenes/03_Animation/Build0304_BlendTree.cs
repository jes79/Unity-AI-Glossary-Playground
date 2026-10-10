using GlossaryDemo.AnimationDemo;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0304_BlendTree
{
    [GlossaryScene("0304")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0304_BlendTree")]
    public static void Build()
    {
        var ctx = NewScene3D("0304", "W/S 또는 슬라이더 : Speed 조절 (0~5)\n1/2/3/4 : Speed 0 / 2 / 3.5 / 5\n왼쪽(Blend Tree)은 부드럽게 섞이고, 오른쪽(상태 전환)은 문턱에서 툭 바뀝니다");
        FrameCamera(ctx.Camera, new Vector3(0f, 3f, -9f), new Vector3(0f, 1.2f, 0f));
        var canvas = EnsureCanvas(ctx);

        var idle = IdleClip("0304");
        var walk = WalkClip("0304");
        var run = RunClip("0304");

        // ── 1D Blend Tree ──
        var blend = NewController("0304_BlendTree");
        blend.AddParameter("Speed", AnimatorControllerParameterType.Float);
        var state = blend.CreateBlendTreeInController("Locomotion", out BlendTree tree, 0);
        tree.blendType = BlendTreeType.Simple1D;
        tree.blendParameter = "Speed";
        tree.useAutomaticThresholds = false;
        tree.AddChild(idle, 0f);
        tree.AddChild(walk, 2f);
        tree.AddChild(run, 5f);
        blend.layers[0].stateMachine.defaultState = state;

        // ── 비교용: 상태 3개를 문턱값으로 즉시 전환 ──
        var hard = NewController("0304_HardSwitch");
        hard.AddParameter("Speed", AnimatorControllerParameterType.Float);
        var sm = hard.layers[0].stateMachine;
        var sIdle = AddState(sm, "Idle", idle, new Vector3(300, 0));
        var sWalk = AddState(sm, "Walk", walk, new Vector3(300, 100));
        var sRun = AddState(sm, "Run", run, new Vector3(300, 200));
        sm.defaultState = sIdle;
        Go(sIdle, sWalk, 0f).AddCondition(AnimatorConditionMode.Greater, 1f, "Speed");
        Go(sIdle, sRun, 0f).AddCondition(AnimatorConditionMode.Greater, 3.5f, "Speed");
        Go(sWalk, sIdle, 0f).AddCondition(AnimatorConditionMode.Less, 1f, "Speed");
        Go(sWalk, sRun, 0f).AddCondition(AnimatorConditionMode.Greater, 3.5f, "Speed");
        Go(sRun, sWalk, 0f).AddCondition(AnimatorConditionMode.Less, 3.5f, "Speed");

        var left = CreateRig("BlendTree", new Vector3(-2.2f, 0f, 0f), PlayerColor, yaw: 150f);
        left.Animator.runtimeAnimatorController = blend;
        var right = CreateRig("HardSwitch", new Vector3(2.2f, 0f, 0f), new Color(0.95f, 0.6f, 0.3f), yaw: 150f);
        right.Animator.runtimeAnimatorController = hard;

        Label(canvas, "1D Blend Tree", new Vector2(-330f, 200f), 30f);
        Label(canvas, "상태 전환만 (비교)", new Vector2(330f, 200f), 30f);

        var slider = CreateSlider("SpeedSlider", canvas.transform, new Vector2(0f, 90f), 700f, 0f, 5f, 0f, "Speed");
        AnchorBottom(slider.GetComponent<RectTransform>());

        var demo = new GameObject("BlendTreeDemo").AddComponent<BlendTreeDemo>();
        Set(demo, "blendTree", left.Animator);
        Set(demo, "hardSwitch", right.Animator);
        Set(demo, "speedSlider", slider);

        ExplainPanel(canvas, "1D Blend Tree 만들기",
            "1. Animator 창 빈 곳 우클릭 → Create State → From New Blend Tree\n" +
            "2. 더블클릭 → Blend Type <b>1D</b>, Parameter <b>Speed</b>\n" +
            "3. Add Motion Field 로 Idle / Walk / Run 추가\n" +
            "4. Automate Thresholds 끄고 0 / 2 / 5 입력\n\n" +
            "Speed 3.5 → Walk 50% + Run 50% 로 섞임.\n" +
            "클립 길이가 달라도 정규화 시간으로 발이 맞춰짐(동기화).", 520f, 320f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }
}
