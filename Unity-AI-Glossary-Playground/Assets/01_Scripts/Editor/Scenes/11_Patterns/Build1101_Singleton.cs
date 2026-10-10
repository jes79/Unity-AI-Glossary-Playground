using GlossaryDemo.Patterns;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build1101_Singleton
{
    [GlossaryScene("1101")]
    [MenuItem("Tools/Glossary/Build Scene/11 Patterns/1101_Singleton")]
    public static void Build()
    {
        var ctx = NewSceneUI("1101",
            "A : 점수 +10   R : 씬 다시 불러오기 → 점수가 유지되는지 확인\nD : DontDestroyOnLoad ON/OFF (OFF 면 다시 불러올 때 점수 0)\n" +
            "C : GameManager 를 하나 더 만들어 보기 → 중복은 스스로 Destroy");
        var root = ctx.Canvas.transform;

        // 씬에 배치된 GameManager (씬을 다시 불러오면 이것이 또 생김 → 중복 처리 확인)
        new GameObject("GameManager").AddComponent<GameManager>();

        var score = CreateText("Score", root, new Vector2(120f, 330f), new Vector2(700f, 90f), "SCORE 0", 72f, Color.white);
        score.fontStyle = FontStyles.Bold;
        var info = Body(root, "Info", new Vector2(120f, 235f), new Vector2(700f, 80f), "", 22f, TextAlignmentOptions.Center);

        var buttons = ButtonColumn(root, new Vector2(120f, 140f), 460f, 70f, 24f, "점수 +10 (A)", "씬 다시 불러오기 (R)", "DontDestroyOnLoad ON (D)", "중복 생성 시도 (C)");

        var logPanel = Panel(root, "LogPanel", new Vector2(120f, -340f), new Vector2(760f, 230f), DarkPanel);
        var log = CreateText("Log", logPanel.transform, Vector2.zero, Vector2.zero, "", 19f, TextColor, TextAlignmentOptions.TopLeft);
        Stretch(log.rectTransform, 16f);
        log.richText = true;

        InfoBox(root, "CodeInfo", new Vector2(730f, 60f), new Vector2(400f, 640f), "싱글톤 코드",
            "<color=#9BE89B>public static GameManager\n    Instance { get; private set; }</color>\n\n" +
            "<color=#9BE89B>void Awake()\n{\n  if (Instance != null &&\n      Instance != this)\n  {\n    Destroy(gameObject);\n    return;\n  }\n  Instance = this;\n  DontDestroyOnLoad(gameObject);\n}</color>\n\n" +
            "• 어디서나\n  GameManager.Instance.AddScore(10)\n" +
            "• 남용하면 의존이 얽히니\n  '정말 하나뿐인 관리자'에만", 18f);

        var ui = new GameObject("SingletonDemoUI").AddComponent<SingletonDemoUI>();
        Set(ui, "scoreText", score);
        Set(ui, "infoText", info);
        Set(ui, "logText", log);
        Set(ui, "ddolLabel", LabelOf(buttons[2]));
        OnClick(buttons[0], ui.AddScore);
        OnClick(buttons[1], ui.ReloadScene);
        OnClick(buttons[2], ui.ToggleDontDestroy);
        OnClick(buttons[3], ui.TryCreateDuplicate);

        Save(ctx);
    }
}
