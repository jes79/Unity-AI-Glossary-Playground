using GlossaryDemo.AnimationDemo;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0306_AnimationLayerAvatarMask
{
    [GlossaryScene("0306")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0306_AnimationLayerAvatarMask")]
    public static void Build()
    {
        var ctx = NewScene3D("0306", "마우스 왼쪽 / Space 누르고 있기 : 사격 (상체 레이어 Weight 1)\nF : 사격 고정 ON/OFF\n왼쪽(마스크 있음)은 다리가 계속 달리고, 오른쪽(마스크 없음)은 다리까지 사격 자세로 굳습니다");
        FrameCamera(ctx.Camera, new Vector3(0f, 3f, -9f), new Vector3(0f, 1.2f, 0f));
        var canvas = EnsureCanvas(ctx);

        var run = RunClip("0306");
        var shoot = ShootClip("0306");

        // 상체 마스크: Chest 아래(머리·팔)만 켜고 엉덩이·다리는 끔
        var mask = new AvatarMask { name = "0306_UpperBody" };
        string[] paths = { "", Hips, Chest, Head, ArmL, ArmR, LegL, LegR };
        bool[] active = { false, false, true, true, true, true, false, false };
        mask.transformCount = paths.Length;
        for (int i = 0; i < paths.Length; i++)
        {
            mask.SetTransformPath(i, paths[i]);
            mask.SetTransformActive(i, active[i]);
        }
        SaveAsset(mask, Sub, "0306_UpperBody.mask");

        var masked = MakeController("0306_WithMask", run, shoot, mask);
        var unmasked = MakeController("0306_NoMask", run, shoot, null);

        var left = CreateRig("WithMask", new Vector3(-2.2f, 0f, 0f), PlayerColor, yaw: 150f);
        left.Animator.runtimeAnimatorController = masked;
        AddGun(left);
        var right = CreateRig("NoMask", new Vector3(2.2f, 0f, 0f), new Color(0.95f, 0.6f, 0.3f), yaw: 150f);
        right.Animator.runtimeAnimatorController = unmasked;
        AddGun(right);

        Label(canvas, "Avatar Mask 있음 (상체만)", new Vector2(-330f, 140f), 28f);
        Label(canvas, "마스크 없음 (전신 덮어씀)", new Vector2(330f, 140f), 28f);

        var demo = new GameObject("AvatarMaskDemo").AddComponent<AvatarMaskDemo>();
        Set(demo, "withMask", left.Animator);
        Set(demo, "withoutMask", right.Animator);

        ExplainPanel(canvas, "상체 레이어 + Avatar Mask",
            "1. Project 창 → Create → <b>Avatar Mask</b>\n" +
            "   (Humanoid: 몸 그림에서 다리 끄기 / Generic: Transform 에서 상체 본만 체크)\n" +
            "2. Animator 창 Layers 탭 → + 로 <b>UpperBody</b> 레이어 추가\n" +
            "3. 톱니바퀴 → Mask 에 만든 마스크, Blending Override\n" +
            "4. 레이어 안에 Shoot 상태 배치\n" +
            "5. 코드: animator.SetLayerWeight(1, 1f) 로 켜기\n\n" +
            "생성 에셋: 09_Generated/Animation/0306_UpperBody.mask", 540f, 340f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }

    // 사격 자세: 두 팔을 앞으로, 반동으로 가슴이 살짝 뒤로. 다리는 서 있는 자세(마스크 없으면 이게 다리를 덮어씀)
    private static AnimationClip ShootClip(string prefix)
    {
        var t = T(0f, 0.06f, 0.3f);
        return new ClipBuilder($"{prefix}_Shoot")
            .Pos(Hips, t, V(0, 1f, 0), V(0, 1f, 0), V(0, 1f, 0))
            .Rot(Chest, t, V(0, -10, 0), V(-6, -10, 0), V(0, -10, 0))
            .Rot(Head, t, V(0, 10, 0), V(0, 10, 0), V(0, 10, 0))
            .Rot(ArmR, t, V(-90, 0, 0), V(-100, 0, 0), V(-90, 0, 0))
            .Rot(ArmL, t, V(-85, 25, 0), V(-95, 25, 0), V(-85, 25, 0))
            .Rot(LegL, t, V(0, 0, 0), V(0, 0, 0), V(0, 0, 0))
            .Rot(LegR, t, V(0, 0, 0), V(0, 0, 0), V(0, 0, 0))
            .Loop().Save();
    }

    private static AnimatorController MakeController(string name, AnimationClip run, AnimationClip shoot, AvatarMask mask)
    {
        var ctrl = NewController(name);
        var sm = ctrl.layers[0].stateMachine;
        sm.defaultState = AddState(sm, "Run", run, new Vector3(300, 0));

        ctrl.AddLayer("UpperBody");
        var layers = ctrl.layers; // 배열 복사본 → 수정 후 다시 대입해야 저장됨
        layers[1].avatarMask = mask;
        layers[1].blendingMode = AnimatorLayerBlendingMode.Override;
        layers[1].defaultWeight = 0f;
        ctrl.layers = layers;
        var upper = ctrl.layers[1].stateMachine;
        upper.defaultState = AddState(upper, "Shoot", shoot, new Vector3(300, 0));
        return ctrl;
    }

    private static void AddGun(Rig rig)
    {
        var gun = Bone("Gun", rig.ArmR, new Vector3(0f, -0.62f, 0f));
        Visual(PrimitiveType.Cube, gun, new Vector3(0f, -0.15f, 0.06f), new Vector3(0.1f, 0.4f, 0.14f), new Color(0.2f, 0.2f, 0.25f));
    }
}
