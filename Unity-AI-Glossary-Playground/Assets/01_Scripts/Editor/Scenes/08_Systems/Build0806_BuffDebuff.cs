using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0806_BuffDebuff
{
    [GlossaryScene("0806")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0806_BuffDebuff")]
    public static void Build()
    {
        var ctx = NewScene2D("0806", "P : 독 (5초 동안 1초마다 3 데미지)   H : 가속 버프 (5초, 속도 ×1.5)   F : 빙결 (2초)\nS : 독 재적용 규칙 전환 (갱신 → 무시 → 중첩 허용)   R : 초기화\n독을 여러 번 눌러 '중첩 불가'가 어떻게 동작하는지 관찰\n위쪽 아이콘의 원형 게이지 = 남은 시간");
        ctx.Camera.transform.position = new Vector3(2.5f, 0f, -10f);

        Platform2D("Floor", new Vector3(2.5f, -2.2f), new Vector2(14f, 0.4f));

        var slime = CreateSpriteObject("Slime (대상)", CircleSprite, new Vector3(2.5f, -1.4f), new Vector2(1.4f, 1.1f), new Color(0.4f, 0.85f, 0.45f), null, 1);
        var demo = slime.gameObject.AddComponent<StatusEffectDemo>();
        Set(demo, "body", slime);

        ctx.Canvas = CreateCanvas("Canvas");
        var root = ctx.Canvas.transform;

        // 효과 아이콘 6칸 (위쪽 가운데)
        var icons = new Image[6];
        var texts = new TextMeshProUGUI[6];
        for (int i = 0; i < 6; i++)
        {
            var slot = Panel(root, $"EffectSlot_{i}", new Vector2(CenterX - 300f + i * 120f, -90f), new Vector2(105f, 105f), new Color32(20, 22, 28, 230));
            AnchorTop(slot.rectTransform);
            var fill = CreateImage("Fill", slot.transform, Vector2.zero, new Vector2(95f, 95f), Color.white, WhiteSprite);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Radial360;
            fill.fillOrigin = (int)Image.Origin360.Top;
            fill.fillClockwise = false;
            var t = Label(slot.transform, "Text", Vector2.zero, new Vector2(100f, 100f), "", 22f, TextAlignmentOptions.Center, Color.black);
            t.fontStyle = FontStyles.Bold;
            icons[i] = fill;
            texts[i] = t;
            slot.gameObject.SetActive(false);
        }

        var hp = Bar(root, "HpBar", new Vector2(CenterX, -190f), new Vector2(600f, 28f), new Color(0.9f, 0.3f, 0.3f));
        AnchorTop(hp.transform.parent.GetComponent<RectTransform>());
        var log = Label(root, "Log", new Vector2(CenterX, 120f), new Vector2(1100f, 50f), "", 26f, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.4f));
        AnchorBottom(log.rectTransform);

        // 버튼 (키와 같은 동작)
        var buttons = CreateButtonRow(root, 50f, "독 (P)", "가속 (H)", "빙결 (F)");
        foreach (var b in buttons) b.GetComponent<RectTransform>().anchoredPosition += new Vector2(CenterX, 0f);
        OnClick(buttons[0], demo.ApplyPoison);
        OnClick(buttons[1], demo.ApplyHaste);
        OnClick(buttons[2], demo.ApplyFreeze);

        SetArray(demo, "effectIcons", icons);
        SetArray(demo, "effectTexts", texts);
        Set(demo, "hpFill", hp);
        Set(demo, "logText", log);

        Save(ctx);
    }
}
