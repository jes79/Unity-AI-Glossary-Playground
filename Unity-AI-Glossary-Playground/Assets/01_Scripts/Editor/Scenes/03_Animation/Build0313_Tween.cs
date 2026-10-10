using GlossaryDemo.AnimationDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static AnimationBuildUtil;
using static GlossaryKit;

// 설명 + 대체 구현: DOTween 미설치 → MiniTween(직접 만든 트윈 러너)으로 같은 효과
public static class Build0313_Tween
{
    [GlossaryScene("0313")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0313_Tween")]
    public static void Build()
    {
        var ctx = NewSceneUI("0313", "버튼 클릭 / Space : 0.2초 펀치 스케일\nT : 트윈 ON/OFF (OFF 면 반응 없이 눌림)\nE : 카드 이동 이징 바꾸기   V : 펀치 진동(vibrato) 1 ↔ 6\n연타해도 크기가 누적되지 않는지 보세요");
        var canvas = ctx.Canvas;

        var demo = new GameObject("TweenDemo").AddComponent<TweenDemo>();

        var buttons = CreateButtonRow(canvas.transform, 110f, "펀치!", "카드 이동", "색 바꾸기", "트윈 ON/OFF");

        // 큰 펀치 버튼 (가운데)
        var big = CreateButton("BigPunchButton", canvas.transform, new Vector2(-150f, 40f), new Vector2(360f, 140f), "눌러 보세요", AccentColor, 40f);

        // 펀치 대상 목록: 0 = 큰 버튼, 1~4 = 아래 버튼. 모든 버튼은 눌리면 자기 자신을 펀치
        var all = new RectTransform[buttons.Length + 1];
        all[0] = big.GetComponent<RectTransform>();
        for (int i = 0; i < buttons.Length; i++) all[i + 1] = buttons[i].GetComponent<RectTransform>();
        OnClick(big, demo.PunchIndex, 0);
        for (int i = 0; i < buttons.Length; i++) OnClick(buttons[i], demo.PunchIndex, i + 1);
        OnClick(buttons[1], demo.ToggleCard);
        OnClick(buttons[2], demo.NextColor);
        OnClick(buttons[3], demo.ToggleTween);

        // 색 바뀌는 원
        var circle = CreateImage("ColorCircle", canvas.transform, new Vector2(-150f, -170f), new Vector2(160f, 160f), AccentColor, KnobSprite);

        // 오른쪽에서 미끄러져 들어오는 카드
        var card = CreateImage("Card", canvas.transform, new Vector2(1250f, -150f), new Vector2(380f, 240f), PanelColor, RoundedSprite, true);
        card.rectTransform.anchorMin = card.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        CreateText("Text", card.transform, Vector2.zero, new Vector2(340f, 200f), "카드 이동\nMiniTween.MoveTo\n(이징 E 키로 변경)", 28f, Color.white);

        SetArray(demo, "buttons", all);
        Set(demo, "card", card.rectTransform);
        Set(demo, "colorTarget", circle);

        ExplainPanel(canvas, "DOTween 으로 하면",
            "<color=#9FD0FF>using DG.Tweening;\n\n" +
            "button.onClick.AddListener(() => {\n" +
            "  rect.DOKill(true);   // 연타 시 이전 트윈 정리\n" +
            "  rect.DOPunchScale(Vector3.one * 0.2f, 0.2f, 1, 0.5f);\n" +
            "});\n\n" +
            "card.DOAnchorPos(target, 0.5f).SetEase(Ease.OutBack);\n" +
            "image.DOColor(Color.red, 0.4f);</color>\n\n" +
            "설치: Asset Store 'DOTween (HOTween v2)' → Tools → Demigiant\n→ DOTween Utility Panel → Setup DOTween\n\n" +
            "<color=#9BE89B>이 씬</color>: MiniTween.PunchScale / MoveTo / ColorTo 가 같은 일", 600f, 420f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }
}
