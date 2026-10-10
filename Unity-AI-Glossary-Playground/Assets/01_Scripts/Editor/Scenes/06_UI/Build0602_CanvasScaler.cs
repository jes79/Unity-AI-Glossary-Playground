using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0602_CanvasScaler
{
    [GlossaryScene("0602")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0602_CanvasScaler")]
    public static void Build()
    {
        var ctx = NewSceneUI("0602",
            "R : 가상 기기 해상도 변경   M : Constant Pixel ↔ Scale With Screen\n슬라이더 : Match (가로 0 ↔ 세로 1)\n" +
            "Game 뷰 해상도(1280x720, 2560x1440 등)를 바꿔 실제 Canvas 도 비교해 보세요");
        var root = ctx.Canvas.transform;

        var modeLabel = Body(root, "ModeLabel", new Vector2(230f, 360f), new Vector2(1060f, 40f), "", 24f, TextAlignmentOptions.Left, TitleColor);
        var deviceLabel = Body(root, "DeviceLabel", new Vector2(230f, 318f), new Vector2(1060f, 40f), "", 22f, TextAlignmentOptions.Left);

        // 가상 기기 화면 (RectMask2D 로 화면 밖으로 넘친 UI 는 잘림)
        var frame = CreateImage("DeviceFrame", root, new Vector2(230f, -10f), new Vector2(1060f, 560f), new Color(0.03f, 0.03f, 0.05f, 1f));
        frame.gameObject.AddComponent<RectMask2D>();
        var frameRt = frame.rectTransform;

        // 가상 Canvas: 자식들은 1920x1080 기준 크기로 만들고, 스크립트가 scaleFactor 만큼 축소/확대
        var vc = CreateImage("VirtualCanvas", frameRt, Vector2.zero, new Vector2(1920f, 1080f), new Color(0.16f, 0.24f, 0.36f, 1f));
        var vcRt = vc.rectTransform;

        var hp = CreateImage("HPBar", vcRt, Vector2.zero, new Vector2(460f, 48f), new Color(0.1f, 0.1f, 0.1f, 1f));
        Anchor(hp.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -30f));
        var hpFill = CreateImage("Fill", hp.transform, Vector2.zero, Vector2.zero, new Color(0.9f, 0.3f, 0.3f));
        Stretch(hpFill.rectTransform, 6f);
        hpFill.rectTransform.anchorMax = new Vector2(0.7f, 1f);

        var score = CreateText("Score", vcRt, Vector2.zero, new Vector2(460f, 70f), "SCORE 12,345", 50f, Color.white, TextAlignmentOptions.Right);
        Anchor(score.rectTransform, Vector2.one, Vector2.one, new Vector2(-30f, -26f));
        score.fontStyle = FontStyles.Bold;

        var stage = CreateText("Stage", vcRt, Vector2.zero, new Vector2(800f, 120f), "STAGE 1", 90f, new Color(1f, 1f, 1f, 0.85f));
        stage.fontStyle = FontStyles.Bold;

        var joy = CreateImage("Joystick", vcRt, Vector2.zero, new Vector2(260f, 260f), new Color(1f, 1f, 1f, 0.25f), CircleSprite);
        Anchor(joy.rectTransform, Vector2.zero, Vector2.zero, new Vector2(50f, 50f));

        var attack = CreateImage("AttackButton", vcRt, Vector2.zero, new Vector2(220f, 220f), new Color(0.95f, 0.5f, 0.3f, 0.9f), CircleSprite);
        Anchor(attack.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-50f, 50f));
        CreateText("Label", attack.transform, Vector2.zero, new Vector2(200f, 80f), "공격", 52f, Color.white).fontStyle = FontStyles.Bold;

        Outline(frameRt, AccentColor, 4f);

        var buttons = CreateButtonRow(root, 60f, "해상도 변경 (R)", "모드 전환 (M)");
        var slider = CreateSlider("MatchSlider", root, new Vector2(620f, 60f), 360f, 0f, 1f, 0.5f, "Match : 가로 0 ↔ 세로 1");
        AnchorBottom((RectTransform)slider.transform);

        var demo = new GameObject("CanvasScalerDemo").AddComponent<CanvasScalerDemo>();
        Set(demo, "targetScaler", ctx.Canvas.GetComponent<CanvasScaler>());
        Set(demo, "deviceFrame", frameRt);
        Set(demo, "virtualCanvas", vcRt);
        Set(demo, "deviceLabel", deviceLabel);
        Set(demo, "modeLabel", modeLabel);
        Set(demo, "matchSlider", slider);
        OnClick(buttons[0], demo.NextResolution);
        OnClick(buttons[1], demo.ToggleMode);
        OnValueChanged(slider, demo.SetMatch);

        Save(ctx);
    }
}
