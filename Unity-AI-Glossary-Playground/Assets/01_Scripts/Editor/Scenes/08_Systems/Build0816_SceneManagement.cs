using System.IO;
using System.Linq;
using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GlossaryKit;
using static SystemsBuildUtil;

// 씬 관리: 타이틀(0816_SceneManagement) → 게임(_Game) → 결과(_Result) 세 개의 실제 씬
// 카탈로그에는 타이틀 씬만 있으므로 보조 씬 두 개는 이 빌더가 직접 만들고 Build Settings 끝에 추가함
public static class Build0816_SceneManagement
{
    const string Controls = "Space / 버튼 : 다음 씬으로 이동 (SceneManager.LoadSceneAsync)\n게임 씬: 10초 동안 노란 과녁 클릭 → 결과 씬으로 점수 전달\n점수는 static 변수(SceneFlow.LastScore)로 넘김\n화면 위 정보 패널의 '현재 씬' 이름이 바뀌는 것을 확인";

    public static string GamePath => $"{SceneFlow.SceneFolder}/{SceneFlow.GameScene}.unity";
    public static string ResultPath => $"{SceneFlow.SceneFolder}/{SceneFlow.ResultScene}.unity";

    [GlossaryScene("0816")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0816_SceneManagement")]
    public static void Build()
    {
        EnsureFolder(SceneFlow.SceneFolder);

        // ② 게임 씬
        {
            var ctx = NewSceneUI("0816", Controls);
            var screen = Common(ctx, SceneFlowScreen.Role.Game, new Color32(30, 50, 40, 255), "GAME 씬");
            var area = CreateRect("TargetArea", ctx.Canvas.transform, new Vector2(CenterX, -60f), new Vector2(1100f, 700f));
            CreateImage("AreaBg", area, Vector2.zero, new Vector2(1100f, 700f), new Color(0f, 0f, 0f, 0.25f), RoundedSprite, true).raycastTarget = false;
            var target = CreateButton("Target", area, Vector2.zero, new Vector2(110f, 110f), "", new Color(1f, 0.8f, 0.25f));
            target.GetComponent<Image>().sprite = KnobSprite;
            target.GetComponent<Image>().type = Image.Type.Simple;
            Set(screen, "targetButton", target);
            Set(screen, "targetArea", area);
            SetFloat(screen, "gameTime", 10f);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), GamePath);
        }

        // ③ 결과 씬
        {
            var ctx = NewSceneUI("0816", Controls);
            var screen = Common(ctx, SceneFlowScreen.Role.Result, new Color32(55, 40, 70, 255), "RESULT 씬");
            var btns = CreateButtonRow(ctx.Canvas.transform, 120f, "다시 하기 (Game)", "타이틀로 (Title)");
            foreach (var b in btns) b.GetComponent<RectTransform>().anchoredPosition += new Vector2(CenterX, 0f);
            OnClick(btns[0], screen.GoGame);
            OnClick(btns[1], screen.GoTitle);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ResultPath);
        }

        // ① 타이틀 씬 (카탈로그 씬) — 마지막에 만들어 열린 상태로 둠
        {
            var ctx = NewSceneUI("0816", Controls);
            var screen = Common(ctx, SceneFlowScreen.Role.Title, new Color32(30, 40, 70, 255), "TITLE 씬");
            var start = CreateButton("StartButton", ctx.Canvas.transform, new Vector2(CenterX, 120f), new Vector2(320f, 80f), "게임 시작 (Space)");
            AnchorBottom(start.GetComponent<RectTransform>());
            OnClick(start, screen.GoGame);

            var help = Panel(ctx.Canvas.transform, "HowItWorks", new Vector2(CenterX, -230f), new Vector2(1100f, 300f), new Color32(20, 22, 28, 230));
            Label(help.transform, "Text", Vector2.zero, new Vector2(1050f, 280f),
                "<b>씬 이동 방법</b>\n" +
                "• <color=#7FB2FF>SceneManager.LoadScene(\"이름\")</color> : 즉시 전환 (잠깐 멈출 수 있음)\n" +
                "• <color=#7FB2FF>SceneManager.LoadSceneAsync(\"이름\")</color> : 백그라운드 로드 → 로딩 막대 표시 가능 (이 데모)\n" +
                "• 씬은 <color=#FFD54A>File > Build Profiles(Build Settings) 의 Scene List</color> 에 있어야 로드됨\n" +
                "<b>값 넘기기</b>: ① static 변수 (이 데모)  ② DontDestroyOnLoad 오브젝트  ③ PlayerPrefs / 파일",
                21f, TextAlignmentOptions.TopLeft);

            Save(ctx);
        }

        AppendHelperScenes();
    }

    // 타이틀/게임/결과 공통: 배경색, 큰 글자, 로딩 오버레이, SceneFlowScreen
    private static SceneFlowScreen Common(SceneContext ctx, SceneFlowScreen.Role role, Color bg, string sceneLabel)
    {
        var root = ctx.Canvas.transform;
        root.Find("Background").GetComponent<Image>().color = bg;
        var tag = Label(root, "SceneTag", new Vector2(-40f, -40f), new Vector2(400f, 50f), sceneLabel, 30f, TextAlignmentOptions.Right, new Color(1f, 1f, 1f, 0.5f));
        AnchorTopRight(tag.rectTransform);
        tag.rectTransform.anchoredPosition = new Vector2(-220f, -40f);

        float y = role == SceneFlowScreen.Role.Game ? 360f : 200f;
        var main = Label(root, "MainText", new Vector2(CenterX, y), new Vector2(1100f, role == SceneFlowScreen.Role.Game ? 80f : 300f), "", 40f, TextAlignmentOptions.Center);

        // 로딩 오버레이 (전체 화면, 처음엔 투명)
        var overlay = CreateImage("LoadingOverlay", root, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.92f));
        Stretch(overlay.rectTransform);
        overlay.raycastTarget = false;
        var group = overlay.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        Label(overlay.transform, "Loading", new Vector2(0f, 40f), new Vector2(600f, 60f), "Loading...", 40f, TextAlignmentOptions.Center);
        var bar = Bar(overlay.transform, "LoadingBar", new Vector2(0f, -30f), new Vector2(600f, 24f), AccentColor, 0f);

        var screen = new GameObject($"SceneFlowScreen ({role})").AddComponent<SceneFlowScreen>();
        Modify(screen, "role", (int)role);
        Set(screen, "mainText", main);
        Set(screen, "loadingOverlay", group);
        Set(screen, "loadingBar", bar);
        return screen;
    }

    private static void Modify(Object target, string field, int enumValue)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).intValue = enumValue;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Build Settings 끝에 보조 씬 추가 (기존 목록은 유지)
    public static void AppendHelperScenes()
    {
        var list = EditorBuildSettings.scenes.ToList();
        bool changed = false;
        foreach (var path in new[] { GamePath, ResultPath })
        {
            if (!File.Exists(path) || list.Any(s => s.path == path)) continue;
            list.Add(new EditorBuildSettingsScene(path, true));
            changed = true;
        }
        if (changed) EditorBuildSettings.scenes = list.ToArray();
    }
}

// "Build All" / "Refresh Build Settings" 가 목록을 카탈로그 기준으로 다시 쓰면 보조 씬이 빠지므로
// 목록이 바뀔 때마다 타이틀 씬이 있으면 보조 씬을 다시 붙임
[InitializeOnLoad]
internal static class Build0816_BuildSettingsHook
{
    private static bool busy;

    static Build0816_BuildSettingsHook()
    {
        EditorBuildSettings.sceneListChanged += OnChanged;
    }

    private static void OnChanged()
    {
        if (busy) return;
        string title = $"{SceneFlow.SceneFolder}/{SceneFlow.TitleScene}.unity";
        if (!EditorBuildSettings.scenes.Any(s => s.path == title)) return;
        busy = true;
        try { Build0816_SceneManagement.AppendHelperScenes(); }
        finally { busy = false; }
    }
}
