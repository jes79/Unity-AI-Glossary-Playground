using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0616_AsyncLoading
{
    [GlossaryScene("0616")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0616_AsyncLoading")]
    public static void Build()
    {
        var ctx = NewSceneUI("0616",
            "버튼 : 해당 씬을 LoadSceneAsync 로 불러오며 진행률 바 표시\nW : 로드 완료 후 '아무 키나 누르세요' 대기 ON/OFF (allowSceneActivation)\n" +
            "슬라이더 : 최소 표시 시간 (로컬 씬은 너무 빨라서 데모용으로 천천히 채움)");
        var root = ctx.Canvas.transform;

        Body(root, "Title", new Vector2(80f, 330f), new Vector2(560f, 44f), "불러올 씬 선택", 28f, TextAlignmentOptions.Center, TitleColor);
        var targets = ButtonColumn(root, new Vector2(80f, 250f), 520f, 76f, 26f, "허브 (0000_Hub)", "0601 Canvas / uGUI 씬", "이 씬 다시 (0616)");
        var option = CreateButton("WaitToggle", root, new Vector2(80f, -50f), new Vector2(520f, 70f), "완료 후 키 입력 대기 ON (W)", new Color32(70, 80, 105, 255), 24f);
        var minText = Body(root, "MinTimeText", new Vector2(80f, -130f), new Vector2(520f, 36f), "", 22f, TextAlignmentOptions.Center);
        var slider = CreateSlider("MinTimeSlider", root, new Vector2(80f, -180f), 480f, 0f, 5f, 2f);

        InfoBox(root, "CodeInfo", new Vector2(650f, 60f), new Vector2(520f, 560f), "핵심 코드",
            "<color=#9BE89B>var op = SceneManager.LoadSceneAsync(name);\nop.allowSceneActivation = false;</color>\n\n" +
            "<color=#9BE89B>while (op.progress < 0.9f)\n{\n    bar.fillAmount = op.progress / 0.9f;\n    yield return null;\n}</color>\n\n" +
            "// 준비 완료 → 키 입력 기다렸다가\n<color=#9BE89B>op.allowSceneActivation = true;</color>\n\n" +
            "• progress 는 <b>0.9 에서 멈춤</b> (활성화 대기)\n" +
            "• 씬은 Build Settings 에 등록되어 있어야 함\n" +
            "• 로딩 UI 는 unscaledDeltaTime 으로 움직임", 19f);

        // ── 로딩 화면 (전체 덮기) ──
        var screen = CreateImage("LoadingScreen", root, Vector2.zero, Vector2.zero, new Color(0.04f, 0.05f, 0.07f, 1f));
        Stretch(screen.rectTransform);
        CreateText("Loading", screen.transform, new Vector2(0f, 120f), new Vector2(900f, 100f), "LOADING", 80f, Color.white).fontStyle = FontStyles.Bold;
        var spinner = CreateImage("Spinner", screen.transform, new Vector2(0f, 240f), new Vector2(70f, 70f), AccentColor, RoundedSprite, true);
        var barBg = Panel(screen.transform, "ProgressBar", new Vector2(0f, 0f), new Vector2(1100f, 46f), new Color(0.15f, 0.16f, 0.2f, 1f));
        var fill = Filled(barBg.transform, "Fill", Vector2.zero, Vector2.zero, AccentColor, Image.FillMethod.Horizontal);
        Stretch(fill.rectTransform, 6f);
        fill.fillAmount = 0f;
        var percent = CreateText("Percent", barBg.transform, Vector2.zero, Vector2.zero, "0%", 28f, Color.white);
        Stretch(percent.rectTransform);
        percent.fontStyle = FontStyles.Bold;
        var detail = CreateText("Detail", screen.transform, new Vector2(0f, -60f), new Vector2(1400f, 40f), "", 22f, SubTextColor);
        var tip = CreateText("Tip", screen.transform, new Vector2(0f, -300f), new Vector2(1400f, 40f), "", 24f, TextColor);
        var prompt = CreateText("ContinuePrompt", screen.transform, new Vector2(0f, -150f), new Vector2(900f, 60f), "아무 키나 누르세요", 40f, new Color(1f, 0.85f, 0.4f));
        prompt.fontStyle = FontStyles.Bold;

        var loader = new GameObject("AsyncSceneLoader").AddComponent<AsyncSceneLoader>();
        SetStrings(loader, "targetScenes", "0000_Hub", "0601_CanvasUGUI", "0616_AsyncLoading");
        SetArray(loader, "targetButtons", targets);
        Set(loader, "loadingScreen", screen.gameObject);
        Set(loader, "progressFill", fill);
        Set(loader, "percentText", percent);
        Set(loader, "detailText", detail);
        Set(loader, "tipText", tip);
        Set(loader, "continuePrompt", prompt.gameObject);
        Set(loader, "spinner", spinner.rectTransform);
        Set(loader, "optionLabel", LabelOf(option));
        Set(loader, "minTimeSlider", slider);
        Set(loader, "minTimeText", minText);
        for (int i = 0; i < targets.Length; i++) OnClick(targets[i], loader.LoadTarget, i);
        OnClick(option, loader.ToggleWaitForInput);
        OnValueChanged(slider, loader.SetMinimumTime);

        Save(ctx);
    }
}
