using GlossaryDemo.Movement;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0112_CameraDeadZoneDamping
{
    [GlossaryScene("0112")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0112_CameraDeadZoneDamping")]
    public static void Build()
    {
        var ctx = NewScene2D("0112",
            "WASD : 이동\nZ : Dead Zone ON/OFF   X : Damping 0.5 ↔ 0   + / - : Dead Zone 크기\n노란 사각형 안에서 조금 움직이면 카메라가 가만히 있고, 벗어나면 0.5초쯤 늦게 부드럽게 따라옵니다");

        GridBackground(Vector2.zero, new Vector2(64f, 44f));
        // 위치 기준이 되는 표지물
        for (int i = 0; i < 24; i++)
        {
            var rnd = new System.Random(i * 7919);
            var pos = new Vector3((float)rnd.NextDouble() * 56f - 28f, (float)rnd.NextDouble() * 36f - 18f, 0f);
            CreateSpriteObject($"Landmark_{i}", i % 2 == 0 ? SquareSprite : CircleSprite, pos, Vector2.one * (0.6f + (float)rnd.NextDouble()),
                Color.HSVToRGB(i / 24f, 0.45f, 0.8f), order: -5);
        }

        var player = CreateSpriteObject("Player", CircleSprite, Vector3.zero, Vector2.one * 0.8f, PlayerColor, order: 1);
        player.gameObject.AddComponent<SimpleMover2D>();

        // Brain: 실제 Camera 에 붙여 CinemachineCamera 의 결과를 적용
        var brain = ctx.Camera.gameObject.AddComponent<CinemachineBrain>();
        brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);

        // 2D 추적 카메라: CinemachineCamera(직교 렌즈) + PositionComposer (화면 구도 기준으로 위치만 이동)
        var cmGo = new GameObject("CM 2D Follow");
        cmGo.transform.position = new Vector3(0f, 0f, -10f);
        var cm = cmGo.AddComponent<CinemachineCamera>();
        cm.Target.TrackingTarget = player.transform;
        cm.Lens = LensSettings.Default;
        cm.Lens.OrthographicSize = 5.4f;
        cm.Lens.NearClipPlane = 0.1f;
        cm.Lens.FarClipPlane = 100f;
        var composer = cmGo.AddComponent<CinemachinePositionComposer>();
        composer.CameraDistance = 10f;
        var comp = ScreenComposerSettings.Default;
        comp.DeadZone.Enabled = true;
        comp.DeadZone.Size = new Vector2(0.3f, 0.3f);
        composer.Composition = comp;
        composer.Damping = new Vector3(0.5f, 0.5f, 0f);
        EditorUtility.SetDirty(cm);
        EditorUtility.SetDirty(composer);

        // Dead Zone 표시용 화면 사각형
        ctx.Canvas = CreateCanvas("DemoCanvas");
        var outline = ScreenOutline("DeadZoneOutline", ctx.Canvas.transform, new Color(1f, 0.85f, 0.3f, 0.9f));
        var label = CreateText("Label", outline, new Vector2(0f, 18f), new Vector2(300f, 30f), "Dead Zone", 20f, new Color(1f, 0.85f, 0.3f));
        label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(0.5f, 1f);

        var demo = new GameObject("DeadZoneDemo").AddComponent<DeadZoneDemo>();
        Set(demo, "composer", composer);
        Set(demo, "deadZoneOutline", outline);
        Save(ctx);
    }
}
