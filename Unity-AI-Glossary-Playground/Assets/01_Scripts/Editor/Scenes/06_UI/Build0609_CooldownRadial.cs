using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0609_CooldownRadial
{
    [GlossaryScene("0609")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0609_CooldownRadial")]
    public static void Build()
    {
        var ctx = NewSceneUI("0609",
            "Q / W / E 또는 클릭 : 스킬 사용 (쿨타임 1.5 / 4 / 8초)\nF : Fill Method 변경 (Radial 360 ↔ 90 ↔ Vertical ↔ Horizontal)\n" +
            "R : 표시 방향 (남은 만큼 덮기 ↔ 차오르기)");
        var root = ctx.Canvas.transform;

        var methodLabel = Body(root, "MethodLabel", new Vector2(230f, 290f), new Vector2(900f, 44f), "", 28f, TextAlignmentOptions.Center, TitleColor);

        // 쿨타임 원형 아이콘 (Knob 스프라이트를 원 모양 아이콘 배경으로 사용)
        Sprite iconSprite = CreateSprite("UI_SkillIcon", 128, 128, (u, v) =>
        {
            float d = new Vector2(u - 0.5f, v - 0.5f).magnitude * 2f;
            float a = Mathf.Clamp01((1f - d) * 40f);
            float shade = Mathf.Lerp(1f, 0.65f, d); // 가운데가 밝은 구슬 느낌
            return new Color(shade, shade, shade, a);
        });

        string[] names = { "파이어볼", "대시", "궁극기" };
        string[] keys = { "Q", "W", "E" };
        string[] glyphs = { "화", "질", "궁" };
        float[] cooldowns = { 1.5f, 4f, 8f };
        Color[] colors = { new Color(0.95f, 0.45f, 0.25f), new Color(0.3f, 0.7f, 0.95f), new Color(0.75f, 0.45f, 0.95f) };
        var slots = new SkillCooldownSlot[3];
        var buttons = new Button[3];

        for (int i = 0; i < 3; i++)
        {
            var slotRt = CreateRect($"Skill_{keys[i]}_{names[i]}", root, new Vector2(230f + (i - 1) * 300f, 40f), new Vector2(220f, 220f));
            var hit = slotRt.gameObject.AddComponent<Image>();
            hit.color = new Color(0f, 0f, 0f, 0f); // 클릭 영역
            buttons[i] = slotRt.gameObject.AddComponent<Button>();
            buttons[i].targetGraphic = hit;

            var iconRoot = CreateRect("IconRoot", slotRt, Vector2.zero, new Vector2(220f, 220f));
            CreateImage("Icon", iconRoot, Vector2.zero, new Vector2(220f, 220f), colors[i], iconSprite);
            CreateText("Glyph", iconRoot, Vector2.zero, new Vector2(200f, 140f), glyphs[i], 96f, new Color(1f, 1f, 1f, 0.9f)).fontStyle = FontStyles.Bold;

            // 핵심: Filled + Radial360 + Origin Top + Clockwise
            var overlay = CreateImage("CooldownOverlay (Filled Radial360)", iconRoot, Vector2.zero, new Vector2(220f, 220f), new Color(0f, 0f, 0f, 0.72f), iconSprite);
            overlay.type = Image.Type.Filled;
            overlay.fillMethod = Image.FillMethod.Radial360;
            overlay.fillOrigin = (int)Image.Origin360.Top;
            overlay.fillClockwise = true;
            overlay.fillAmount = 0f;

            var time = CreateText("TimeText", iconRoot, Vector2.zero, new Vector2(200f, 120f), "", 72f, Color.white);
            time.fontStyle = FontStyles.Bold;

            var key = Panel(iconRoot, "KeyBadge", new Vector2(-82f, 82f), new Vector2(54f, 54f), new Color32(30, 33, 42, 240));
            CreateText("Key", key.transform, Vector2.zero, new Vector2(54f, 54f), keys[i], 28f, Color.white).fontStyle = FontStyles.Bold;

            CreateText("Name", slotRt, new Vector2(0f, -150f), new Vector2(260f, 40f), $"{names[i]}  ({cooldowns[i]:0.#}초)", 26f, TextColor);

            var slot = slotRt.gameObject.AddComponent<SkillCooldownSlot>();
            SetString(slot, "skillName", names[i]);
            SetFloat(slot, "cooldown", cooldowns[i]);
            Set(slot, "cooldownOverlay", overlay);
            Set(slot, "timeText", time);
            Set(slot, "iconRoot", iconRoot);
            slots[i] = slot;
        }

        InfoBox(root, "HowTo", new Vector2(230f, -300f), new Vector2(900f, 190f), "Image 설정 (인스펙터)",
            "Image Type = <b>Filled</b>   Fill Method = <b>Radial 360</b>   Fill Origin = <b>Top</b>   Clockwise = 체크\n" +
            "코드: <color=#9BE89B>overlay.fillAmount = remaining / cooldown;</color>  (1 = 다 덮임, 0 = 다 걷힘)\n" +
            "아이콘 위에 반투명 검은 이미지를 겹쳐 두고 fillAmount 만 줄이면 시계 방향으로 걷힙니다.", 20f);

        var row = ButtonRow(root, new Vector2(0f, -480f), 260f, 64f, 22f, "Fill Method 변경 (F)", "표시 방향 (R)");

        var demo = new GameObject("CooldownDemo").AddComponent<CooldownDemo>();
        SetArray(demo, "slots", slots);
        Set(demo, "methodLabel", methodLabel);
        for (int i = 0; i < 3; i++) OnClick(buttons[i], demo.UseSlot, i);
        OnClick(row[0], demo.NextFillMethod);
        OnClick(row[1], demo.ToggleDirection);

        Save(ctx);
    }
}
