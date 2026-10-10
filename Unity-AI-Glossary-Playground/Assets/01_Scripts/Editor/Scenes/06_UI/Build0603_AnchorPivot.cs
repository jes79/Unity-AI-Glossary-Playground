using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0603_AnchorPivot
{
    [GlossaryScene("0603")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0603_AnchorPivot")]
    public static void Build()
    {
        var ctx = NewSceneUI("0603",
            "가상 화면이 커졌다 작아집니다 (Space : 멈춤/재생)\n초록 점수 = Anchor 오른쪽 위 → 항상 모서리에 붙음\n" +
            "빨강 점수 = Anchor 가운데 → 화면이 작아지면 밖으로 밀림\nP : Pivot 변경 (회전 중심이 바뀜)   노란 ◆ = Anchor 위치");
        var root = ctx.Canvas.transform;

        // 가상 화면
        var screen = CreateImage("SimScreen", root, new Vector2(230f, -80f), new Vector2(1200f, 640f), new Color(0.13f, 0.17f, 0.24f, 1f));
        var s = screen.rectTransform;
        Outline(s, AccentColor, 4f);
        CreateText("ScreenLabel", s, new Vector2(0f, 0f), new Vector2(500f, 40f), "가상 화면", 26f, new Color(1f, 1f, 1f, 0.25f));

        Color marker = new Color(1f, 0.85f, 0.2f);

        // 요청 예문: 점수는 오른쪽 위에 Anchor 로 고정
        var good = Panel(s, "Score_AnchorTopRight", Vector2.zero, new Vector2(400f, 58f), new Color32(40, 120, 70, 255));
        Anchor(good.rectTransform, Vector2.one, Vector2.one, new Vector2(-20f, -20f));
        CreateText("Text", good.transform, Vector2.zero, new Vector2(390f, 58f), "SCORE 1,234  (Anchor 오른쪽 위)", 22f, Color.white).fontStyle = FontStyles.Bold;
        AnchorMarker(s, Vector2.one, marker);

        // 비교: Anchor 가운데. 최대 크기 화면에서 같은 자리에 오도록 위치만 맞춤
        var bad = Panel(s, "Score_AnchorCenter", Vector2.zero, new Vector2(400f, 58f), new Color32(150, 50, 50, 255));
        Anchor(bad.rectTransform, new Vector2(0.5f, 0.5f), Vector2.one, new Vector2(580f, 220f));
        CreateText("Text", bad.transform, Vector2.zero, new Vector2(390f, 58f), "SCORE 1,234  (Anchor 가운데)", 22f, Color.white).fontStyle = FontStyles.Bold;
        AnchorMarker(s, new Vector2(0.5f, 0.5f), marker);

        // 다른 Anchor 프리셋 예시
        var hp = Panel(s, "HP_AnchorTopLeft", Vector2.zero, new Vector2(300f, 46f), new Color32(60, 70, 95, 255));
        Anchor(hp.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f));
        CreateText("Text", hp.transform, Vector2.zero, new Vector2(290f, 46f), "HP (Anchor 왼쪽 위)", 20f, Color.white);
        AnchorMarker(s, new Vector2(0f, 1f), marker);

        var btn = Panel(s, "Button_AnchorBottom", Vector2.zero, new Vector2(320f, 64f), new Color32(60, 70, 95, 255));
        Anchor(btn.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f));
        CreateText("Text", btn.transform, Vector2.zero, new Vector2(310f, 64f), "버튼 (Anchor 아래 가운데)", 20f, Color.white);
        AnchorMarker(s, new Vector2(0.5f, 0f), marker);

        // Stretch: 앵커 min/max 가 다르면 부모 크기에 맞춰 늘어남
        var bar = Panel(s, "Bar_StretchBottom", Vector2.zero, Vector2.zero, new Color32(70, 60, 110, 255));
        var barRt = bar.rectTransform;
        barRt.anchorMin = new Vector2(0f, 0f);
        barRt.anchorMax = new Vector2(1f, 0f);
        barRt.pivot = new Vector2(0.5f, 0f);
        barRt.offsetMin = new Vector2(20f, 96f);
        barRt.offsetMax = new Vector2(-20f, 130f);
        var barText = CreateText("Text", bar.transform, Vector2.zero, Vector2.zero, "Stretch (Anchor min x=0, max x=1 → 가로로 늘어남)", 18f, Color.white);
        Stretch(barText.rectTransform);

        // Pivot 상자
        var pivotBox = Panel(s, "PivotBox", Vector2.zero, new Vector2(170f, 100f), new Color32(88, 140, 255, 255));
        Anchor(pivotBox.rectTransform, new Vector2(0.3f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        CreateText("Text", pivotBox.transform, Vector2.zero, new Vector2(160f, 100f), "Pivot 상자", 22f, Color.white);
        var pivotMarker = CreateImage("PivotMarker", pivotBox.transform, Vector2.zero, new Vector2(18f, 18f), new Color(1f, 0.3f, 0.3f), CircleSprite);
        var pivotAnchor = CreateImage("PivotBoxAnchor", s, Vector2.zero, new Vector2(10f, 10f), Color.white);
        Anchor(pivotAnchor.rectTransform, new Vector2(0.3f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        var pivotLabel = CreateText("PivotLabel", s, Vector2.zero, new Vector2(360f, 36f), "Pivot", 22f, new Color(1f, 0.6f, 0.6f));
        Anchor(pivotLabel.rectTransform, new Vector2(0.3f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f));

        var buttons = CreateButtonRow(root, 50f, "크기 변경 멈춤 (Space)", "Pivot 변경 (P)");

        var demo = new GameObject("AnchorPivotDemo").AddComponent<AnchorPivotDemo>();
        Set(demo, "simScreen", s);
        Set(demo, "scoreTopRight", good.rectTransform);
        Set(demo, "scoreCenter", bad.rectTransform);
        Set(demo, "pivotBox", pivotBox.rectTransform);
        Set(demo, "pivotMarker", pivotMarker.rectTransform);
        Set(demo, "pivotLabel", pivotLabel);
        OnClick(buttons[0], demo.ToggleAutoResize);
        OnClick(buttons[1], demo.NextPivot);

        Save(ctx);
    }

    // Anchor 위치 표시용 노란 마름모
    private static void AnchorMarker(RectTransform parent, Vector2 anchor, Color color)
    {
        var m = CreateImage($"AnchorMarker_{anchor.x}_{anchor.y}", parent, Vector2.zero, new Vector2(16f, 16f), color);
        Anchor(m.rectTransform, anchor, new Vector2(0.5f, 0.5f), Vector2.zero);
        m.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        m.raycastTarget = false;
    }
}
