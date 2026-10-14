using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0610_DialogueTypewriter
{
    [GlossaryScene("0610")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0610_DialogueTypewriter")]
    public static void Build()
    {
        var ctx = NewSceneUI("0610",
            "대화창 클릭 / Space : 출력 중이면 전부 표시, 다 나왔으면 다음 대사\n슬라이더 : 글자 속도   S : '클릭 시 전부 표시' ON/OFF\n" +
            "색 글자(리치 텍스트 태그)도 한 글자씩 정상 출력되는지 보세요");
        var root = ctx.Canvas.transform;

        // 초상화
        var portrait = CreateImage("Portrait", root, Vector2.zero, new Vector2(240f, 240f), Color.white, CircleSprite);
        Anchor(portrait.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-230f, 480f)); // 왼쪽 위 정보 패널을 피함
        var glyph = CreateText("Glyph", portrait.transform, Vector2.zero, new Vector2(220f, 220f), "촌", 110f, Color.white);
        glyph.fontStyle = FontStyles.Bold;

        // 대화창 (DialogueTypewriter 가 IPointerClickHandler 로 클릭을 받음)
        var box = Panel(root, "DialogueBox", Vector2.zero, new Vector2(1500f, 280f), new Color(0.07f, 0.08f, 0.11f, 0.95f));
        Anchor(box.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f));
        box.rectTransform.pivot = new Vector2(0.5f, 0f);
        box.rectTransform.anchoredPosition = new Vector2(0f, 60f);
        Outline(box.rectTransform, new Color(1f, 1f, 1f, 0.25f), 3f);

        var nameTag = Panel(box.transform, "NameTag", Vector2.zero, new Vector2(240f, 60f), new Color32(88, 140, 255, 255));
        Anchor(nameTag.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(40f, -12f));
        var nameText = CreateText("Name", nameTag.transform, Vector2.zero, new Vector2(230f, 60f), "촌장", 30f, Color.white);
        nameText.fontStyle = FontStyles.Bold;

        var body = CreateText("Body", box.transform, Vector2.zero, Vector2.zero, "", 36f, Color.white, TextAlignmentOptions.TopLeft);
        Stretch(body.rectTransform);
        body.rectTransform.offsetMin = new Vector2(48f, 40f);
        body.rectTransform.offsetMax = new Vector2(-48f, -50f);
        body.richText = true;

        var next = CreateText("NextIndicator", box.transform, Vector2.zero, new Vector2(60f, 50f), "▼", 34f, new Color(1f, 0.85f, 0.4f));
        Anchor(next.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 20f));

        // 속도 슬라이더 + 토글
        var slider = CreateSlider("SpeedSlider", root, new Vector2(600f, 360f), 420f, 5f, 100f, 30f, "글자 속도 (글자/초)");
        var skip = CreateButton("SkipToggle", root, new Vector2(600f, 270f), new Vector2(420f, 64f), "클릭 시 전부 표시 ON (S)", null, 24f);

        var blip = CreateToneClip("UI_TypeBlip", 900f, 0.035f, Wave.Square, 0.12f, -1f, 0.002f, 0.02f);
        var src = box.gameObject.AddComponent<AudioSource>();
        src.clip = blip;
        src.playOnAwake = false;

        var typer = box.gameObject.AddComponent<DialogueTypewriter>();
        Set(typer, "nameText", nameText);
        Set(typer, "bodyText", body);
        Set(typer, "nextIndicator", next.gameObject);
        Set(typer, "portrait", portrait);
        Set(typer, "portraitGlyph", glyph);
        Set(typer, "speedSlider", slider);
        Set(typer, "skipLabel", LabelOf(skip));
        Set(typer, "blipSource", src);
        SetStrings(typer, "speakers", "촌장", "용사");
        SetStrings(typer, "lines",
            "어서 오게, 젊은 용사여. 우리 마을에 온 걸 환영하네.",
            "안녕하세요! 이 근처에 <color=#FFD54F>몬스터</color>가 나온다고 들었어요.",
            "그렇다네... 북쪽 동굴에서 매일 밤 <color=#FF7043>이상한 울음소리</color>가 들려온다네.",
            "제가 가서 확인해 볼게요. 혹시 챙겨 갈 물건이 있을까요?",
            "이 <b>낡은 횃불</b>을 가져가게. 동굴 안은 아주 어둡다네. 부디 조심하게!");
        var so = new SerializedObject(typer);
        var colors = so.FindProperty("speakerColors");
        colors.arraySize = 2;
        colors.GetArrayElementAtIndex(0).colorValue = new Color(0.85f, 0.55f, 0.3f);
        colors.GetArrayElementAtIndex(1).colorValue = new Color(0.3f, 0.6f, 0.95f);
        so.ApplyModifiedPropertiesWithoutUndo();

        OnValueChanged(slider, typer.SetSpeed);
        OnClick(skip, typer.ToggleClickToComplete);

        Save(ctx);
    }
}
