using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0612_PauseMenu
{
    [GlossaryScene("0612")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0612_PauseMenu")]
    public static void Build()
    {
        var ctx = NewScene2D("0612",
            "ESC : 일시정지 메뉴 (계속 / 설정 / 메인으로)\n도형·공·음악이 멈추고, 메뉴의 회전 장식은 계속 도는지 보세요 (unscaled)\n" +
            "T : timeScale 사용 OFF → 메뉴가 열려도 게임이 계속 진행되는 문제 확인");

        // 움직이는 오브젝트 (Time.deltaTime 기반)
        Color[] colors = { new Color(0.35f, 0.6f, 1f), new Color(1f, 0.6f, 0.3f), new Color(0.5f, 0.9f, 0.5f) };
        for (int i = 0; i < 3; i++)
        {
            var sr = CreateSpriteObject($"Mover_{i}", SquareSprite, new Vector3(3f, 2.2f - i * 1.6f, 0f), new Vector2(0.9f, 0.9f), colors[i], order: 1);
            var m = sr.gameObject.AddComponent<PauseDemoMover>();
            SetFloat(m, "amplitude", 3.5f);
            SetFloat(m, "speed", 1f + i * 0.6f);
            SetFloat(m, "spin", 60f + i * 70f);
        }

        // 물리로 튀는 공 (timeScale 0 이면 물리도 멈춤)
        var floor = CreateSpriteObject("Floor", SquareSprite, new Vector3(2.5f, -4.6f, 0f), new Vector2(14f, 0.5f), GroundColor);
        floor.gameObject.AddComponent<BoxCollider2D>();
        var bouncy = SaveAsset(new PhysicsMaterial2D("Bouncy") { bounciness = 0.95f, friction = 0f }, "06_UI", "Bouncy0612.physicsMaterial2D");
        for (int i = 0; i < 4; i++)
        {
            var ball = CreateSpriteObject($"Ball_{i}", CircleSprite, new Vector3(-1f + i * 2f, -1f + i * 0.6f, 0f), new Vector2(0.6f, 0.6f), new Color(1f, 0.85f, 0.3f), order: 2);
            var rb = ball.gameObject.AddComponent<Rigidbody2D>();
            rb.sharedMaterial = bouncy;
            var col = ball.gameObject.AddComponent<CircleCollider2D>();
            col.sharedMaterial = bouncy;
        }

        // 배경 음악 (일시정지 시 AudioListener.pause)
        var bgm = new GameObject("BGM").AddComponent<AudioSource>();
        bgm.clip = CreateMelodyClip("BGM_PauseDemo", new[] { 523f, 659f, 784f, 659f, 587f, 698f, 880f, 698f }, 0.22f, Wave.Square, 0.08f);
        bgm.loop = true;
        bgm.playOnAwake = true;

        var canvas = CreateCanvas("UI Canvas");
        var root = canvas.transform;
        var pauseBtn = CreateButton("PauseButton", root, Vector2.zero, new Vector2(220f, 70f), "II  일시정지 (ESC)", new Color32(70, 80, 105, 255), 24f);
        Anchor((RectTransform)pauseBtn.transform, Vector2.one, Vector2.one, new Vector2(-30f, -24f));
        var modeBtn = CreateButtonRow(root, 60f, "timeScale 0 사용 (T)")[0];

        // ── 일시정지 메뉴 ──
        var menu = CreateImage("PauseMenu", root, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.65f));
        Stretch(menu.rectTransform);
        var panel = Panel(menu.transform, "MenuPanel", new Vector2(200f, 0f), new Vector2(460f, 520f), new Color32(40, 44, 58, 250));
        CreateText("Title", panel.transform, new Vector2(0f, 200f), new Vector2(400f, 70f), "일시정지", 48f, Color.white).fontStyle = FontStyles.Bold;
        var spinner = CreateImage("Spinner", panel.transform, new Vector2(170f, 200f), new Vector2(46f, 46f), AccentColor, RoundedSprite, true);
        var menuButtons = ButtonColumn(panel.transform, new Vector2(0f, 90f), 340f, 78f, 28f, "계속", "설정", "메인으로");
        CreateText("Hint", panel.transform, new Vector2(0f, -215f), new Vector2(420f, 40f), "ESC : 닫기", 20f, SubTextColor);

        var settings = Panel(menu.transform, "SettingsPanel", new Vector2(700f, 0f), new Vector2(460f, 300f), new Color32(50, 55, 72, 250));
        CreateText("Title", settings.transform, new Vector2(0f, 105f), new Vector2(400f, 50f), "설정", 34f, Color.white).fontStyle = FontStyles.Bold;
        var volText = CreateText("VolumeText", settings.transform, new Vector2(0f, 40f), new Vector2(400f, 40f), "전체 볼륨", 24f, TextColor);
        var vol = CreateSlider("VolumeSlider", settings.transform, new Vector2(0f, -20f), 360f, 0f, 1f, 1f);
        var close = CreateButton("CloseSettings", settings.transform, new Vector2(0f, -100f), new Vector2(200f, 60f), "닫기", null, 24f);

        var ctrl = new GameObject("PauseMenuController").AddComponent<PauseMenuController>();
        Set(ctrl, "menuRoot", menu.gameObject);
        Set(ctrl, "settingsPanel", settings.gameObject);
        Set(ctrl, "menuSpinner", spinner.rectTransform);
        Set(ctrl, "volumeSlider", vol);
        Set(ctrl, "volumeText", volText);
        Set(ctrl, "modeLabel", LabelOf(modeBtn));
        OnClick(pauseBtn, ctrl.TogglePause);
        OnClick(modeBtn, ctrl.ToggleUseTimeScale);
        OnClick(menuButtons[0], ctrl.Resume);
        OnClick(menuButtons[1], ctrl.ToggleSettings);
        OnClick(menuButtons[2], ctrl.GoToMain);
        OnClick(close, ctrl.ToggleSettings);
        OnValueChanged(vol, ctrl.SetVolume);

        Save(ctx);
    }
}
