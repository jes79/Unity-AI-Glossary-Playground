using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0815_GameManagerState
{
    [GlossaryScene("0815")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0815_GameManagerState")]
    public static void Build()
    {
        var ctx = NewScene2D("0815", "Ready : Space 로 시작\nPlaying : A/D 로 떨어지는 블록 피하기   P / Esc : 일시정지\nPaused : Time.timeScale = 0 (모든 움직임 정지)   P / Esc : 계속\nGameOver : R 로 Ready 상태로 돌아가기 — 상태마다 화면이 바뀌는 것 관찰");

        CreateSpriteObject("Playfield", SquareSprite, new Vector3(3f, 0f, 1f), new Vector2(11f, 10.8f), new Color32(32, 36, 46, 255), null, -5);
        var player = CreateSpriteObject("Player", SquareSprite, new Vector3(3f, -4.2f), new Vector2(0.7f, 0.7f), PlayerColor, null, 2);

        var gmGo = new GameObject("GameManager");
        var gm = gmGo.AddComponent<GameManager>();

        ctx.Canvas = CreateCanvas("Canvas");
        var root = ctx.Canvas.transform;
        var hud = Label(root, "HUD", new Vector2(CenterX, -40f), new Vector2(800f, 50f), "", 30f, TextAlignmentOptions.Center);
        AnchorTop(hud.rectTransform);

        GameObject MakeScreen(string name, string title, Color color, string buttonLabel, UnityEngine.Events.UnityAction action, out TextMeshProUGUI text)
        {
            var p = Panel(root, name, new Vector2(CenterX, 20f), new Vector2(620f, 330f), color);
            text = Label(p.transform, "Text", new Vector2(0f, 45f), new Vector2(580f, 220f), title, 56f, TextAlignmentOptions.Center);
            text.fontStyle = FontStyles.Bold;
            OnClick(SmallButton(p.transform, "Button", new Vector2(0f, -110f), new Vector2(260f, 64f), buttonLabel), action);
            return p.gameObject;
        }

        var ready = MakeScreen("ReadyScreen", "READY\n<size=50%>Space 로 시작</size>", new Color32(40, 70, 120, 230), "시작", gm.StartGame, out _);
        var paused = MakeScreen("PausedScreen", "PAUSED\n<size=50%>P / Esc 로 계속</size>", new Color32(70, 70, 80, 230), "계속하기", gm.TogglePause, out _);
        var over = MakeScreen("GameOverScreen", "GAME OVER", new Color32(120, 40, 45, 230), "다시 (Ready)", gm.Retry, out var overText);
        paused.SetActive(false);
        over.SetActive(false);

        var pauseBtn = SmallButton(root, "PauseButton", new Vector2(CenterX, 60f), new Vector2(220f, 60f), "일시정지");
        AnchorBottom(pauseBtn.GetComponent<RectTransform>());
        OnClick(pauseBtn, gm.TogglePause);

        var game = gmGo.AddComponent<DodgeGame>();
        Set(game, "player", player.transform);
        Set(game, "blockSprite", SquareSprite);
        Set(game, "readyScreen", ready);
        Set(game, "pausedScreen", paused);
        Set(game, "gameOverScreen", over);
        Set(game, "gameOverText", overText);
        Set(game, "hudText", hud);

        Save(ctx);
    }
}
