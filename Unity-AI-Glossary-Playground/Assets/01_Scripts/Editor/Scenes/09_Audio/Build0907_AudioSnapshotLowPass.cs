using GlossaryDemo.AudioDemo;
using UnityEditor;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0907_AudioSnapshotLowPass
{
    [GlossaryScene("0907")]
    [MenuItem("Tools/Glossary/Build Scene/09 Audio/0907_AudioSnapshotLowPass")]
    public static void Build()
    {
        var ctx = NewScene2D("0907", "Esc / P / 버튼 : 일시정지 ↔ 계속 (Paused 스냅샷 ↔ Normal 스냅샷)\nF : 로우패스 필터 ON/OFF (OFF 면 볼륨만 줄어듦)\n일시정지하면 BGM 이 벽 너머처럼 먹먹해지는 것을 들어 보세요");

        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f, 783.99f, 659.25f, 587.33f, 698.46f, 880f, 1174.66f, 880f, 698.46f };
        var bgmClip = CreateMelodyClip("0907_BrightBGM", notes, 0.2f, Wave.Saw, 0.2f); // 고음이 많은 톱니파 → 필터 효과가 잘 들림

        var bgmGo = new GameObject("BGM");
        var bgm = bgmGo.AddComponent<AudioSource>();
        bgm.clip = bgmClip;
        bgm.loop = true;
        bgm.playOnAwake = true;
        bgm.ignoreListenerPause = true;
        var lowPass = bgmGo.AddComponent<AudioLowPassFilter>(); // AudioSource 와 같은 오브젝트에 붙이면 그 소리에만 적용
        lowPass.cutoffFrequency = 22000f;

        // 튀는 공 (일시정지 시각 확인)
        CreateSpriteObject("Ground", SquareSprite, new Vector3(0f, -2.3f), new Vector2(8f, 0.3f), GroundColor);
        var bouncerRoot = new GameObject("BouncerRoot");
        bouncerRoot.transform.position = new Vector3(0f, -1.65f, 0f);
        var ball = CreateSpriteObject("Ball", CircleSprite, Vector3.zero, Vector2.one, PlayerColor, bouncerRoot.transform, 1);

        var canvas = CreateCanvas("Canvas");
        var overlay = CreateImage("PauseOverlay", canvas.transform, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0.05f, 0.55f));
        Stretch(overlay.rectTransform);
        overlay.raycastTarget = false;
        CreateText("Text", overlay.transform, new Vector2(0f, 60f), new Vector2(800f, 120f), "일시정지", 80f, Color.white).fontStyle = TMPro.FontStyles.Bold;

        var demo = new GameObject("AudioSnapshotDemo").AddComponent<AudioSnapshotDemo>();
        Set(demo, "bgm", bgm);
        Set(demo, "lowPass", lowPass);
        Set(demo, "pauseOverlay", overlay.gameObject);
        Set(demo, "bouncer", ball.transform);

        var buttons = CreateButtonRow(canvas.transform, 60f, "일시정지 / 계속", "필터 ON/OFF");
        OnClick(buttons[0], demo.TogglePause);
        OnClick(buttons[1], demo.ToggleLowPass);

        ExplainPanel(canvas, "실제 Audio Mixer 스냅샷",
            "1. Audio Mixer 의 BGM 그룹 → Add Effect → <b>Lowpass</b>\n" +
            "2. Snapshots 의 + 로 <b>Normal</b>, <b>Paused</b> 두 개 만들기\n" +
            "3. Paused 선택 상태에서 Lowpass Cutoff 800Hz, Volume -4dB\n" +
            "   (Normal 은 22000Hz, 0dB)\n" +
            "4. 코드:\n<color=#9FD0FF>[SerializeField] AudioMixerSnapshot normal, paused;\n" +
            "void Pause(bool on) {\n  Time.timeScale = on ? 0 : 1;\n" +
            "  (on ? paused : normal).TransitionTo(0.4f);\n}</color>\n" +
            "   (Audio Mixer Update Mode 를 Unscaled Time 으로)\n\n" +
            "<color=#9BE89B>이 씬</color>: AudioLowPassFilter + 스냅샷 값 보간으로 같은 효과", 560f, 420f);

        Save(ctx);
    }
}
