using GlossaryDemo.Movement;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0111_Cinemachine
{
    [GlossaryScene("0111")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0111_Cinemachine")]
    public static void Build()
    {
        var ctx = NewScene3D("0111",
            "WASD : 이동(카메라 기준)   Space : 점프\n마우스 오른쪽 드래그 : 궤도 회전   휠 : 거리\nC : 고정 전경 카메라 ↔ 3인칭 카메라 (Priority 전환 → Brain 이 1초 블렌드)",
            true, 50f);

        // 주변 기둥 (회전할 때 배경 변화가 잘 보이게)
        for (int i = 0; i < 12; i++)
        {
            float a = i * 30f * Mathf.Deg2Rad;
            var pos = new Vector3(Mathf.Cos(a) * 10f, 1.5f, Mathf.Sin(a) * 10f);
            Prim(PrimitiveType.Cube, $"Pillar_{i}", pos, new Vector3(1f, 3f, 1f), Color.HSVToRGB(i / 12f, 0.5f, 0.85f));
        }
        Prim(PrimitiveType.Cube, "Block", new Vector3(3f, 0.5f, 3f), new Vector3(2f, 1f, 2f), new Color32(110, 120, 150, 255));

        var player = PlayerCapsule(new Vector3(0f, 1.1f, 0f));
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        var cc = player.AddComponent<CharacterController>();
        cc.center = Vector3.zero;
        var mover = player.AddComponent<CharacterControllerMover>();
        SetBool(mover, "slopeKeys", false);
        SetBool(mover, "writeStatus", false);

        // 1) 실제 카메라에 CinemachineBrain: 라이브 CinemachineCamera 의 결과를 받아 Camera 를 움직임
        var brain = ctx.Camera.gameObject.AddComponent<CinemachineBrain>();
        brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1f);

        // 2) 3인칭 궤도 카메라: CinemachineCamera + OrbitalFollow(Body, 위치) + RotationComposer(Aim, 회전)
        var orbitGo = new GameObject("CM Orbit (3인칭)");
        var orbitCam = orbitGo.AddComponent<CinemachineCamera>();
        orbitCam.Target.TrackingTarget = player.transform;
        orbitCam.Lens = LensSettings.Default;
        orbitCam.Lens.FieldOfView = 55f;
        orbitCam.Priority = 10;

        var orbital = orbitGo.AddComponent<CinemachineOrbitalFollow>();
        orbital.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
        orbital.Radius = 7f;
        orbital.TargetOffset = new Vector3(0f, 0.5f, 0f);
        orbital.HorizontalAxis = new InputAxis { Value = 0f, Center = 0f, Range = new Vector2(-180f, 180f), Wrap = true };
        orbital.VerticalAxis = new InputAxis { Value = 20f, Center = 20f, Range = new Vector2(-10f, 70f), Wrap = false };
        orbital.RadialAxis = new InputAxis { Value = 1f, Center = 1f, Range = new Vector2(0.5f, 2f), Wrap = false };

        var composer = orbitGo.AddComponent<CinemachineRotationComposer>();
        composer.TargetOffset = new Vector3(0f, 0.5f, 0f);
        composer.Damping = new Vector2(0.3f, 0.3f);

        // 3) 비교용 고정 카메라: Body 없이 제자리에서 플레이어만 바라봄 (Aim 만)
        var overviewGo = new GameObject("CM Overview (고정)");
        overviewGo.transform.position = new Vector3(0f, 16f, -16f);
        var overviewCam = overviewGo.AddComponent<CinemachineCamera>();
        overviewCam.Target.TrackingTarget = player.transform;
        overviewCam.Lens = LensSettings.Default;
        overviewCam.Lens.FieldOfView = 50f;
        overviewCam.Priority = 0;
        overviewGo.AddComponent<CinemachineRotationComposer>();

        var input = new GameObject("OrbitCameraInput").AddComponent<OrbitCameraInput>();
        Set(input, "orbital", orbital);
        Set(input, "orbitCamera", orbitCam);
        Set(input, "overviewCamera", overviewCam);
        Set(input, "brain", brain);

        EditorUtility.SetDirty(orbitCam);
        EditorUtility.SetDirty(orbital);
        EditorUtility.SetDirty(overviewCam);
        Save(ctx);
    }
}
