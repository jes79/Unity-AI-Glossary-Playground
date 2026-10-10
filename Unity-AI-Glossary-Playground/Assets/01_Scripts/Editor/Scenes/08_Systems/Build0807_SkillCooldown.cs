using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0807_SkillCooldown
{
    [GlossaryScene("0807")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0807_SkillCooldown")]
    public static void Build()
    {
        var ctx = NewScene2D("0807", "Q : 화염구 (쿨 2초)   W : 대시 (쿨 5초)   E : 광역기 (쿨 10초)\nC : 쿨다운 감소 50% ON/OFF\n아래 스킬 아이콘의 어두운 덮개가 시계 방향으로 걷히며 남은 시간을 보여 줌\n쿨다운 중에 누르면 '쿨다운 중!' 메시지");
        ctx.Camera.transform.position = new Vector3(3.5f, 0f, -10f);

        Platform2D("Floor", new Vector3(3.5f, -2f), new Vector2(16f, 0.4f));
        var hero = CreateSpriteObject("Hero", SquareSprite, new Vector3(-1f, -1.3f), new Vector2(0.8f, 1f), PlayerColor, null, 2);
        var dummy = CreateSpriteObject("Dummy (허수아비)", SquareSprite, new Vector3(8.5f, -1.1f), new Vector2(0.9f, 1.4f), new Color32(190, 150, 90, 255), null, 1);

        ctx.Canvas = CreateCanvas("Canvas");
        var root = ctx.Canvas.transform;

        string[] names = { "화염구", "대시", "광역기" };
        Key[] keys = { Key.Q, Key.W, Key.E };
        float[] cds = { 2f, 5f, 10f };
        Color[] colors = { new Color(1f, 0.5f, 0.2f), new Color(0.4f, 0.8f, 1f), new Color(0.8f, 0.4f, 1f) };
        var overlays = new Image[3];
        var timers = new TextMeshProUGUI[3];
        var buttons = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            var frame = Panel(root, $"Skill_{keys[i]}", new Vector2(CenterX + (i - 1) * 170f, 150f), new Vector2(140f, 140f), colors[i]);
            AnchorBottom(frame.rectTransform);
            buttons[i] = frame.gameObject.AddComponent<Button>();
            buttons[i].targetGraphic = frame;
            var keyLabel = Label(frame.transform, "Key", new Vector2(-45f, 50f), new Vector2(40f, 34f), keys[i].ToString(), 26f, TextAlignmentOptions.Center, Color.black);
            keyLabel.fontStyle = FontStyles.Bold;
            Label(frame.transform, "Name", new Vector2(0f, -95f), new Vector2(160f, 34f), $"{names[i]} ({cds[i]:0}초)", 22f, TextAlignmentOptions.Center);
            var ov = CreateImage("CooldownOverlay", frame.transform, Vector2.zero, new Vector2(140f, 140f), new Color(0f, 0f, 0f, 0.72f), WhiteSprite);
            ov.type = Image.Type.Filled;
            ov.fillMethod = Image.FillMethod.Radial360;
            ov.fillOrigin = (int)Image.Origin360.Top;
            ov.fillClockwise = false;
            ov.fillAmount = 0f;
            ov.raycastTarget = false;
            var timer = Label(frame.transform, "Timer", Vector2.zero, new Vector2(140f, 140f), "", 48f, TextAlignmentOptions.Center);
            timer.fontStyle = FontStyles.Bold;
            overlays[i] = ov;
            timers[i] = timer;
        }
        var msg = Label(root, "Message", new Vector2(CenterX, 290f), new Vector2(900f, 50f), "", 32f, TextAlignmentOptions.Center, new Color(1f, 0.45f, 0.4f));
        AnchorBottom(msg.rectTransform);

        var demo = new GameObject("SkillCooldownDemo").AddComponent<SkillCooldownDemo>();
        var so = new SerializedObject(demo);
        var arr = so.FindProperty("skills");
        arr.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            var e = arr.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("name").stringValue = names[i];
            e.FindPropertyRelative("key").intValue = (int)keys[i];
            e.FindPropertyRelative("cooldown").floatValue = cds[i];
            e.FindPropertyRelative("color").colorValue = colors[i];
            e.FindPropertyRelative("cooldownOverlay").objectReferenceValue = overlays[i];
            e.FindPropertyRelative("timerText").objectReferenceValue = timers[i];
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        Set(demo, "hero", hero.transform);
        Set(demo, "dummy", dummy.transform);
        Set(demo, "effectSprite", CircleSprite);
        Set(demo, "messageText", msg);
        for (int i = 0; i < 3; i++) OnClick(buttons[i], demo.TryUse, i);

        Save(ctx);
    }
}
