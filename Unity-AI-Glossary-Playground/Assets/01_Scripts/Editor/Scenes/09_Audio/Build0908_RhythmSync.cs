using GlossaryDemo.AudioDemo;
using UnityEditor;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0908_RhythmSync
{
    [GlossaryScene("0908")]
    [MenuItem("Tools/Glossary/Build Scene/09 Audio/0908_RhythmSync")]
    public static void Build()
    {
        var ctx = NewScene2D("0908", "Space / 클릭 : 노트가 흰 선에 닿을 때 입력 (±50ms Perfect)\nM : 메트로놈 ON/OFF   T : 기준 시계 dspTime ↔ Time.time\nL : 일부러 0.5초 렉 → Time.time 원(아래)은 박자에서 밀리고 dspTime 원(위)은 그대로");

        // BPM 120 → 한 박 0.5초. 8박(4초) 루프 음악
        float[] notes = { 261.63f, 0f, 329.63f, 392f, 349.23f, 0f, 293.66f, 392f };
        var music = CreateMelodyClip("0908_Loop120", notes, 0.5f, Wave.Sine, 0.3f);
        var tick = CreateToneClip("0908_Tick", 1500f, 0.04f, Wave.Square, 0.25f, attack: 0.001f, release: 0.035f);
        var accent = CreateToneClip("0908_TickAccent", 2200f, 0.05f, Wave.Square, 0.35f, attack: 0.001f, release: 0.045f);

        var audioGo = new GameObject("Audio");
        var musicSrc = audioGo.AddComponent<AudioSource>();
        musicSrc.clip = music;
        musicSrc.loop = true;
        musicSrc.playOnAwake = false;
        var tickA = audioGo.AddComponent<AudioSource>();
        tickA.playOnAwake = false;
        var tickB = audioGo.AddComponent<AudioSource>();
        tickB.playOnAwake = false;

        // 레인 + 판정선
        float laneY = -1.5f, hitX = -4f;
        CreateSpriteObject("Lane", SquareSprite, new Vector3(2f, laneY), new Vector2(14f, 1.2f), PanelColor);
        var hitLine = CreateSpriteObject("HitLine", SquareSprite, new Vector3(hitX, laneY), new Vector2(0.12f, 1.6f), Color.white, order: 3);
        var flash = CreateSpriteObject("JudgeFlash", CircleSprite, new Vector3(hitX, laneY), new Vector2(1.6f, 1.6f), new Color(1f, 1f, 1f, 0f), order: 2);
        var notesRoot = new GameObject("Notes");
        var template = CreateSpriteObject("Note", CircleSprite, new Vector3(9f, laneY), new Vector2(0.8f, 0.8f), new Color(1f, 0.5f, 0.35f), notesRoot.transform, 4);

        // 박자 원 (위: dspTime, 아래: Time.time)
        var pulseDsp = CreateSpriteObject("Pulse_dspTime", CircleSprite, new Vector3(1.5f, 2.3f), Vector2.one, new Color(0.4f, 0.9f, 0.5f), order: 1);
        var pulseTime = CreateSpriteObject("Pulse_TimeTime", CircleSprite, new Vector3(1.5f, 0.6f), Vector2.one, new Color(0.95f, 0.6f, 0.3f), order: 1);

        var canvas = CreateCanvas("Canvas");
        var l1 = CreateText("Label_dsp", canvas.transform, new Vector2(-150f, 230f), new Vector2(360f, 40f), "AudioSettings.dspTime", 24f, new Color(0.4f, 0.9f, 0.5f));
        var l2 = CreateText("Label_time", canvas.transform, new Vector2(-150f, 60f), new Vector2(360f, 40f), "Time.time", 24f, new Color(0.95f, 0.6f, 0.3f));

        var demo = new GameObject("RhythmSyncDemo").AddComponent<RhythmSyncDemo>();
        Set(demo, "music", musicSrc);
        SetArray(demo, "tickSources", tickA, tickB);
        Set(demo, "tick", tick);
        Set(demo, "tickAccent", accent);
        Set(demo, "noteTemplate", template);
        Set(demo, "hitLine", hitLine.transform);
        SetFloat(demo, "spawnX", 9f);
        Set(demo, "pulseDsp", pulseDsp.transform);
        Set(demo, "pulseTime", pulseTime.transform);
        Set(demo, "judgeFlash", flash);

        ExplainPanel(canvas, "dspTime 으로 박자 맞추기",
            "<color=#9FD0FF>double secPerBeat = 60.0 / 120;  // 0.5초\n" +
            "dspStart = AudioSettings.dspTime + 0.5;\n" +
            "music.PlayScheduled(dspStart);\n\n" +
            "double songPos = AudioSettings.dspTime - dspStart;\n" +
            "double beat = songPos / secPerBeat;\n" +
            "// 노트 x = 판정선 + (노트박자시각 - songPos) × 속도</color>\n\n" +
            "• Time.time 은 프레임 시계 → 렉·일시정지에 흔들림\n" +
            "• dspTime 은 오디오 시계 → 실제 소리와 항상 일치\n" +
            "• 효과음/틱도 PlayScheduled(정확한 dsp 시각) 로 예약", 560f, 380f);

        Save(ctx);
    }
}
