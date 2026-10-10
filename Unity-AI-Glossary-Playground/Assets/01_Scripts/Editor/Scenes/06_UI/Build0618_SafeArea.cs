using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0618_SafeArea
{
    [GlossaryScene("0618")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0618_SafeArea")]
    public static void Build()
    {
        var ctx = NewSceneUI("0618",
            "N : 기기(노치 모양) 바꾸기   S : Safe Area 적용 ON/OFF\nOFF 로 하면 HUD 가 노치·홈 바(빨간 영역)에 가려짐\n" +
            "초록 테두리 = Safe Area   (F1 로 이 패널을 접으면 왼쪽 위 HUD 도 보임)");
        var root = ctx.Canvas.transform;

        // ── 가운데: 조작 버튼 + 설명 ──
        var deviceBtn = CreateButton("DeviceButton", root, new Vector2(150f, 90f), new Vector2(660f, 76f), "기기", null, 26f);
        var applyBtn = CreateButton("ApplyButton", root, new Vector2(150f, 0f), new Vector2(660f, 76f), "Safe Area 적용 ON (S)", new Color32(60, 150, 90, 255), 26f);
        InfoBox(root, "CodeInfo", new Vector2(150f, -200f), new Vector2(820f, 250f), "SafeAreaFitter 핵심",
            "<color=#9BE89B>Rect a = Screen.safeArea;</color>  // 노치를 뺀 픽셀 사각형\n" +
            "<color=#9BE89B>rt.anchorMin = a.position / 화면크기;\nrt.anchorMax = (a.position + a.size) / 화면크기;</color>\n" +
            "HUD 를 모두 이 오브젝트 자식으로 → 노치를 자동으로 피함. 화면 회전 시 다시 계산.\n" +
            "에디터는 노치가 없으므로 여기선 비율로 흉내 냄 (Device Simulator 로 실제 기기 확인)", 19f);

        // ── Safe Area 루트 (HUD 의 부모) ──
        var safe = CreateRect("SafeArea", root, Vector2.zero, Vector2.zero);
        Stretch(safe);
        var fitter = safe.gameObject.AddComponent<SafeAreaFitter>();
        Outline(safe, new Color(0.3f, 0.95f, 0.45f, 0.9f), 4f);

        var hp = Panel(safe, "HPBar", Vector2.zero, new Vector2(380f, 46f), new Color32(30, 30, 36, 255));
        Anchor(hp.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f));
        var hpFill = CreateImage("Fill", hp.transform, Vector2.zero, Vector2.zero, new Color(0.9f, 0.3f, 0.3f));
        Stretch(hpFill.rectTransform, 5f);
        CreateText("Text", hp.transform, Vector2.zero, new Vector2(380f, 46f), "HP 100", 22f, Color.white).fontStyle = FontStyles.Bold;

        var score = CreateText("Score", safe, Vector2.zero, new Vector2(360f, 60f), "SCORE 9,999", 40f, Color.white, TextAlignmentOptions.Right);
        Anchor(score.rectTransform, Vector2.one, Vector2.one, new Vector2(-120f, -20f));
        score.fontStyle = FontStyles.Bold;
        var pause = Panel(safe, "PauseButton", Vector2.zero, new Vector2(80f, 70f), new Color32(70, 80, 105, 255));
        Anchor(pause.rectTransform, Vector2.one, Vector2.one, new Vector2(-20f, -20f));
        CreateText("II", pause.transform, Vector2.zero, new Vector2(80f, 70f), "II", 32f, Color.white);

        var joy = CreateImage("Joystick", safe, Vector2.zero, new Vector2(230f, 230f), new Color(1f, 1f, 1f, 0.3f), CircleSprite);
        Anchor(joy.rectTransform, Vector2.zero, Vector2.zero, new Vector2(30f, 30f));
        CreateImage("Handle", joy.transform, Vector2.zero, new Vector2(100f, 100f), new Color(1f, 1f, 1f, 0.7f), CircleSprite);

        var attack = CreateImage("Attack", safe, Vector2.zero, new Vector2(190f, 190f), new Color(0.95f, 0.5f, 0.3f, 0.85f), CircleSprite);
        Anchor(attack.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 30f));
        CreateText("Label", attack.transform, Vector2.zero, new Vector2(190f, 80f), "공격", 40f, Color.white).fontStyle = FontStyles.Bold;
        var skill = CreateImage("Skill", safe, Vector2.zero, new Vector2(120f, 120f), new Color(0.4f, 0.6f, 1f, 0.85f), CircleSprite);
        Anchor(skill.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-250f, 40f));
        CreateText("Label", skill.transform, Vector2.zero, new Vector2(120f, 60f), "스킬", 28f, Color.white);

        // ── 기기 하드웨어 흉내 (위에 덮는 Canvas) ──
        var device = CreateCanvas("DeviceOverlay Canvas", 500);
        var tints = new RectTransform[4];
        string[] tintNames = { "Unsafe_Left", "Unsafe_Right", "Unsafe_Bottom", "Unsafe_Top" };
        for (int i = 0; i < 4; i++)
        {
            var t = CreateImage(tintNames[i], device.transform, Vector2.zero, Vector2.zero, new Color(1f, 0.15f, 0.15f, 0.22f));
            t.raycastTarget = false;
            tints[i] = t.rectTransform;
        }
        var notch = CreateImage("Notch", device.transform, Vector2.zero, Vector2.zero, Color.black, RoundedSprite, true);
        notch.raycastTarget = false;
        var homeBar = CreateImage("HomeBar", device.transform, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.85f), RoundedSprite, true);
        homeBar.raycastTarget = false;
        var hb = homeBar.rectTransform;
        hb.anchorMin = new Vector2(0.4f, 0.012f);
        hb.anchorMax = new Vector2(0.6f, 0.02f);
        hb.offsetMin = hb.offsetMax = Vector2.zero;

        var demo = new GameObject("SafeAreaDemo").AddComponent<SafeAreaDemo>();
        Set(demo, "fitter", fitter);
        Set(demo, "notch", notch.rectTransform);
        Set(demo, "homeBar", hb);
        SetArray(demo, "unsafeTints", tints);
        Set(demo, "presetLabel", LabelOf(deviceBtn));
        Set(demo, "applyLabel", LabelOf(applyBtn));
        OnClick(deviceBtn, demo.NextPreset);
        OnClick(applyBtn, demo.ToggleApply);

        Save(ctx);
    }
}
