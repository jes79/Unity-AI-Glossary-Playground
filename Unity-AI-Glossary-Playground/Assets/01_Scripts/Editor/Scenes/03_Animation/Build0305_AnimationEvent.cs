using GlossaryDemo.AnimationDemo;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0305_AnimationEvent
{
    public const int HitFrame = 12;

    [GlossaryScene("0305")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0305_AnimationEvent")]
    public static void Build()
    {
        var ctx = NewScene3D("0305", "마우스 왼쪽 / Space : 공격\nE : 판정 방식 전환 (Animation Event ↔ 버튼 즉시)\n막대의 노란 선(12프레임)에 칼이 닿는 순간 데미지가 들어가는지 보세요.  R : HP 리셋");
        FrameCamera(ctx.Camera, new Vector3(-1.5f, 3f, -7.5f), new Vector3(0.6f, 1.2f, 0f));
        var canvas = EnsureCanvas(ctx);

        // 12프레임(30fps 기준 0.4초)에 OnAttackHit 호출
        var evt = new AnimationEvent { time = HitFrame / 30f, functionName = "OnAttackHit" };
        var idle = IdleClip("0305");
        var attack = AttackClip("0305", evt);

        var ctrl = NewController("0305_AttackEvent");
        ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        var sm = ctrl.layers[0].stateMachine;
        var sIdle = AddState(sm, "Idle", idle, new Vector3(300, 0));
        var sAttack = AddState(sm, "Attack", attack, new Vector3(300, 120));
        sm.defaultState = sIdle;
        Go(sIdle, sAttack, 0.05f).AddCondition(AnimatorConditionMode.If, 0f, "Attack");
        After(sAttack, sIdle, 0.95f, 0.1f);

        var rig = CreateRig("Character", new Vector3(-0.6f, 0f, 0f), PlayerColor, withSword: true, yaw: 90f);
        rig.Animator.runtimeAnimatorController = ctrl;

        // 허수아비
        var dummy = new GameObject("Dummy");
        dummy.transform.position = new Vector3(0.9f, 0f, 0f);
        var post = Visual(PrimitiveType.Cylinder, dummy.transform, new Vector3(0f, 0.6f, 0f), new Vector3(0.12f, 0.6f, 0.12f), new Color(0.55f, 0.4f, 0.25f), "Post");
        var body = Visual(PrimitiveType.Capsule, dummy.transform, new Vector3(0f, 1.3f, 0f), new Vector3(0.6f, 0.55f, 0.6f), new Color(0.85f, 0.75f, 0.45f), "Body");

        // 진행 막대 (화면 아래)
        var bar = CreateImage("AttackProgress", canvas.transform, new Vector2(0f, 80f), new Vector2(800f, 36f), PanelColor, WhiteSprite);
        AnchorBottom(bar.rectTransform);
        var fill = CreateImage("Fill", bar.transform, Vector2.zero, Vector2.zero, AccentColor, WhiteSprite);
        Stretch(fill.rectTransform);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 0f;
        var marker = CreateImage("HitMarker", bar.transform, Vector2.zero, new Vector2(6f, 56f), new Color(1f, 0.85f, 0.2f));
        var markerLabel = CreateText("Text", marker.transform, new Vector2(0f, 44f), new Vector2(240f, 30f), $"Event : {HitFrame}프레임", 20f, new Color(1f, 0.85f, 0.2f));
        Label(canvas, "공격 클립 진행도 (0 ~ 24프레임)", new Vector2(0f, 130f), 22f);

        var demo = rig.Root.AddComponent<AnimationEventDemo>();
        SetInt(demo, "hitFrame", HitFrame);
        Set(demo, "target", dummy.transform);
        Set(demo, "targetRenderer", body.GetComponent<Renderer>());
        Set(demo, "progressFill", fill);
        Set(demo, "hitMarker", marker.rectTransform);

        ExplainPanel(canvas, "Animation Event 넣는 법",
            "1. Animation 창에서 공격 클립 선택\n" +
            "2. 타임라인을 <b>12프레임</b>으로 이동\n" +
            "3. Add Event 버튼(깃발 아이콘) 클릭\n" +
            "4. Function 에 <b>OnAttackHit</b> 입력\n" +
            "5. Animator 가 붙은 오브젝트의 스크립트에\n   public void OnAttackHit() 작성\n\n" +
            "코드로: AnimationUtility.SetAnimationEvents(clip, events)\n" +
            "모델 FBX 클립은 Import 설정 Animation 탭 → Events 에서 추가", 520f, 340f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }
}
