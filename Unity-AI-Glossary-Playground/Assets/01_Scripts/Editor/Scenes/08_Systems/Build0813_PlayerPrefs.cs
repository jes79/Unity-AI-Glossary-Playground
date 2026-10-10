using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0813_PlayerPrefs
{
    [GlossaryScene("0813")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0813_PlayerPrefs")]
    public static void Build()
    {
        var ctx = NewSceneUI("0813", "슬라이더 : 배경음 볼륨 (바꾸는 즉시 PlayerPrefs 에 저장)\nEnter / [게임 시작] : 5초 동안 Space 연타 → 최고 점수면 저장\n플레이를 멈췄다가 다시 시작해도 볼륨과 최고 점수가 남아 있는지 확인\n[초기화] : DeleteKey 로 삭제");
        var root = ctx.Canvas.transform;

        // 배경음 (볼륨 확인용)
        var bgm = new GameObject("BGM").AddComponent<AudioSource>();
        bgm.clip = CreateMelodyClip("Prefs_BGM", new[] { 523f, 659f, 784f, 659f, 587f, 698f, 880f, 698f }, 0.25f, Wave.Square, 0.15f);
        bgm.loop = true;
        bgm.playOnAwake = true;

        var demo = new GameObject("PlayerPrefsDemo").AddComponent<PlayerPrefsDemo>();

        var soundPanel = Panel(root, "SoundPanel", new Vector2(CenterX - 260f, 280f), new Vector2(560f, 220f));
        Label(soundPanel.transform, "Title", new Vector2(0f, 70f), new Vector2(500f, 40f), "<b>사운드 설정</b>", 28f);
        var slider = CreateSlider("VolumeSlider", soundPanel.transform, new Vector2(0f, -10f), 480f, 0f, 1f, 0.8f);
        var volText = Label(soundPanel.transform, "VolumeText", new Vector2(0f, -65f), new Vector2(480f, 36f), "", 24f, TextAlignmentOptions.Center);
        OnValueChanged(slider, demo.OnVolumeChanged);

        var gamePanel = Panel(root, "GamePanel", new Vector2(CenterX - 260f, -110f), new Vector2(560f, 500f));
        Label(gamePanel.transform, "Title", new Vector2(0f, 210f), new Vector2(500f, 40f), "<b>미니 게임: Space 연타</b>", 28f);
        var game = Label(gamePanel.transform, "GameText", new Vector2(0f, 40f), new Vector2(520f, 260f), "", 34f, TextAlignmentOptions.Center);
        OnClick(SmallButton(gamePanel.transform, "Start", new Vector2(0f, -180f), new Vector2(300f, 70f), "게임 시작"), demo.StartRound);

        var prefsPanel = Panel(root, "PrefsPanel", new Vector2(CenterX + 330f, 80f), new Vector2(560f, 620f), new Color32(25, 28, 36, 255));
        var prefs = Label(prefsPanel.transform, "PrefsText", new Vector2(0f, 60f), new Vector2(520f, 460f), "", 22f, TextAlignmentOptions.TopLeft);
        OnClick(SmallButton(prefsPanel.transform, "Reset", new Vector2(0f, -250f), new Vector2(300f, 64f), "초기화 (DeleteKey)", EnemyColor), demo.ResetPrefs);

        Set(demo, "volumeSlider", slider);
        Set(demo, "volumeText", volText);
        Set(demo, "gameText", game);
        Set(demo, "prefsText", prefs);

        Save(ctx);
    }
}
