using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0601_CanvasUGUI
{
    [GlossaryScene("0601")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0601_CanvasUGUI")]
    public static void Build()
    {
        var ctx = NewScene2D("0601",
            "H : 피해   J : 회복   K : 점수 +10\nP/ESC 또는 오른쪽 위 II : 일시정지 (회전 상자가 멈춤)\n" +
            "M : Render Mode Overlay ↔ Camera (Camera 모드에선 상자가 UI 앞으로 나옴)");

        // 월드 오브젝트: 일시정지·Render Mode 차이를 보기 위한 회전 상자
        var spinner = CreateSpriteObject("WorldSpinner", SquareSprite, new Vector3(3f, -0.3f, 0f), new Vector2(3.2f, 3.2f), new Color(0.35f, 0.55f, 1f, 1f));

        // HUD 전용 Canvas (배경 없음 → 월드가 보임)
        var canvas = CreateCanvas("HUD Canvas");
        var root = canvas.transform;

        // 체력바: 왼쪽 아래 고정
        var hpBg = Panel(root, "HPBar", Vector2.zero, new Vector2(480f, 46f), new Color(0.1f, 0.1f, 0.12f, 0.9f));
        Anchor(hpBg.rectTransform, Vector2.zero, Vector2.zero, new Vector2(40f, 150f));
        var hpFill = Filled(hpBg.transform, "Fill", Vector2.zero, Vector2.zero, new Color(0.3f, 0.85f, 0.4f), Image.FillMethod.Horizontal);
        Stretch(hpFill.rectTransform, 6f);
        var hpText = CreateText("HPText", hpBg.transform, Vector2.zero, Vector2.zero, "HP", 24f, Color.white);
        Stretch(hpText.rectTransform);
        hpText.fontStyle = FontStyles.Bold;

        // 점수: 오른쪽 위 고정
        var score = CreateText("ScoreText", root, Vector2.zero, new Vector2(460f, 64f), "SCORE 0", 46f, Color.white, TextAlignmentOptions.Right);
        Anchor(score.rectTransform, Vector2.one, Vector2.one, new Vector2(-140f, -26f));
        score.fontStyle = FontStyles.Bold;

        var modeLabel = CreateText("RenderModeLabel", root, Vector2.zero, new Vector2(460f, 40f), "Overlay 모드", 24f, SubTextColor, TextAlignmentOptions.Right);
        Anchor(modeLabel.rectTransform, Vector2.one, Vector2.one, new Vector2(-40f, -110f));

        // 일시정지 버튼 (Button 컴포넌트)
        var pauseBtn = CreateButton("PauseButton", root, Vector2.zero, new Vector2(90f, 72f), "II", new Color32(70, 80, 105, 255), 34f);
        Anchor((RectTransform)pauseBtn.transform, Vector2.one, Vector2.one, new Vector2(-30f, -22f));

        var buttons = CreateButtonRow(root, 60f, "피해 (H)", "회복 (J)", "점수 +10 (K)", "Render Mode (M)");

        // 일시정지 오버레이 (마지막 자식 → 가장 위에 그려짐)
        var overlay = CreateImage("PauseOverlay", root, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.6f));
        Stretch(overlay.rectTransform);
        CreateText("PauseTitle", overlay.transform, new Vector2(0f, 80f), new Vector2(800f, 100f), "일시정지", 72f, Color.white).fontStyle = FontStyles.Bold;
        var resume = CreateButton("ResumeButton", overlay.transform, new Vector2(0f, -60f), new Vector2(260f, 80f), "계속");

        var hud = new GameObject("HudController").AddComponent<HudController>();
        Set(hud, "hudCanvas", canvas);
        Set(hud, "uiCamera", ctx.Camera);
        Set(hud, "hpFill", hpFill);
        Set(hud, "hpText", hpText);
        Set(hud, "scoreText", score);
        Set(hud, "pauseOverlay", overlay.gameObject);
        Set(hud, "renderModeLabel", modeLabel);
        Set(hud, "spinner", spinner.transform);

        OnClick(buttons[0], hud.Damage);
        OnClick(buttons[1], hud.Heal);
        OnClick(buttons[2], hud.AddScore);
        OnClick(buttons[3], hud.ToggleRenderMode);
        OnClick(pauseBtn, hud.TogglePause);
        OnClick(resume, hud.TogglePause);

        Save(ctx);
    }
}
