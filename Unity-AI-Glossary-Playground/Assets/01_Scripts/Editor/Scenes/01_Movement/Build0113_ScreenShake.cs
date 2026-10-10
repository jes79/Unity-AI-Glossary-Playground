using GlossaryDemo.Movement;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0113_ScreenShake
{
    [GlossaryScene("0113")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0113_ScreenShake")]
    public static void Build()
    {
        var ctx = NewScene3D("0113",
            "Space : 피격 (직접 발생)   적이 2.5초마다 돌진해서 때림\n1 : 쉐이크 ON/OFF   ↑/↓ : 세기   S : 모양(Bump/Recoil/Explosion/Rumble)\n맞는 순간 0.2초 동안 화면이 흔들리는지, OFF 일 때 타격감이 얼마나 줄어드는지 비교하세요",
            true, 30f);

        for (int i = 0; i < 8; i++)
            Prim(PrimitiveType.Cube, $"Crate_{i}", new Vector3(-7f + i * 2f, 0.5f, 4f + (i % 2) * 1.5f), Vector3.one, new Color32(150, 120, 90, 255));

        var player = PlayerCapsule(new Vector3(0f, 1f, 0f));
        var enemy = Prim(PrimitiveType.Cube, "Enemy", new Vector3(4.5f, 0.75f, 1.5f), Vector3.one * 1.5f, EnemyColor);
        enemy.tag = "Enemy";
        Object.DestroyImmediate(enemy.GetComponent<Collider>());

        var brain = ctx.Camera.gameObject.AddComponent<CinemachineBrain>();

        // 추적 카메라 + Impulse Listener (확장 컴포넌트: 같은 오브젝트에 붙이면 자동 적용)
        var cmGo = new GameObject("CM Follow + ImpulseListener");
        var cm = cmGo.AddComponent<CinemachineCamera>();
        cm.Target.TrackingTarget = player.transform;
        cm.Lens = LensSettings.Default;
        cm.Lens.FieldOfView = 50f;
        var follow = cmGo.AddComponent<CinemachineFollow>();
        follow.FollowOffset = new Vector3(0f, 6f, -9f);
        cmGo.AddComponent<CinemachineRotationComposer>();
        var listener = cmGo.AddComponent<CinemachineImpulseListener>();
        listener.ChannelMask = 1;
        listener.Gain = 1f;
        listener.UseCameraSpace = true;
        listener.ApplyAfter = CinemachineCore.Stage.Noise;
        listener.ReactionSettings = new CinemachineImpulseListener.ImpulseReaction { AmplitudeGain = 1f, FrequencyGain = 1f, Duration = 1f };
        cmGo.transform.position = new Vector3(0f, 7f, -9f);

        // Impulse Source: 신호를 보내는 쪽 (보통 무기·폭발·피격 대상에 붙임)
        var demoGo = new GameObject("ScreenShakeDemo");
        var source = demoGo.AddComponent<CinemachineImpulseSource>();
        source.ImpulseDefinition = new CinemachineImpulseDefinition
        {
            ImpulseChannel = 1,
            ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump,
            CustomImpulseShape = new AnimationCurve(),
            ImpulseDuration = 0.2f,
            ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform,
            DissipationDistance = 100f,
            DissipationRate = 0.25f,
            PropagationSpeed = 343f
        };
        source.DefaultVelocity = new Vector3(-0.6f, -1f, 0f).normalized;

        var demo = demoGo.AddComponent<ScreenShakeDemo>();
        Set(demo, "impulseSource", source);
        Set(demo, "player", player.transform);
        Set(demo, "playerRenderer", player.GetComponent<Renderer>());
        Set(demo, "enemy", enemy.transform);

        EditorUtility.SetDirty(cm);
        EditorUtility.SetDirty(follow);
        EditorUtility.SetDirty(listener);
        EditorUtility.SetDirty(source);
        Save(ctx);
    }
}
