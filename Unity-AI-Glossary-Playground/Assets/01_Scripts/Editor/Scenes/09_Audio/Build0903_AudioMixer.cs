using System.Collections.Generic;
using GlossaryDemo.AudioDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static AnimationBuildUtil;
using static GlossaryKit;

// 설명 + 대체 구현: Audio Mixer 에셋 대신 SimpleAudioMixer(스크립트 그룹 트리) + 같은 모양의 SetFloat 코드
public static class Build0903_AudioMixer
{
    [GlossaryScene("0903")]
    [MenuItem("Tools/Glossary/Build Scene/09 Audio/0903_AudioMixer")]
    public static void Build()
    {
        var ctx = NewSceneUI("0903", "슬라이더 : Master / BGM / SFX 볼륨 (dB 로 변환해 믹서에 전달)\nMUTE 버튼 / M : 음소거   Space : 효과음   A : 자동 효과음 ON/OFF\nMaster 를 내리면 BGM·SFX 가 함께 줄어드는 것(부모 그룹)을 보세요");
        var canvas = ctx.Canvas;

        var bgm = CreateMelodyClip("0903_BGM", new[] { 261.63f, 329.63f, 392f, 523.25f, 392f, 329.63f, 293.66f, 349.23f, 440f, 349.23f, 293.66f, 246.94f }, 0.3f, Wave.Square, 0.2f);
        var sfx = CreateToneClip("0903_Beep", 880f, 0.12f, Wave.Sine, 0.6f, 1320f, 0.002f, 0.08f);

        var mixerGo = new GameObject("SimpleAudioMixer");
        var mixer = mixerGo.AddComponent<SimpleAudioMixer>();

        var bgmGo = new GameObject("BGM Source");
        var bgmSrc = bgmGo.AddComponent<AudioSource>();
        bgmSrc.clip = bgm;
        bgmSrc.loop = true;
        bgmSrc.playOnAwake = true;
        bgmSrc.volume = 0f; // 첫 프레임 큰 소리 방지 (믹서가 곧 볼륨을 적용)
        var bgmRoute = bgmGo.AddComponent<MixerRoutedSource>();
        SetString(bgmRoute, "groupName", "BGM");

        var sfxGo = new GameObject("SFX Source");
        var sfxSrc = sfxGo.AddComponent<AudioSource>();
        sfxSrc.playOnAwake = false;
        var sfxRoute = sfxGo.AddComponent<MixerRoutedSource>();
        SetString(sfxRoute, "groupName", "SFX");

        // ── 설정창 ──
        var window = CreateImage("SettingsWindow", canvas.transform, new Vector2(60f, -170f), new Vector2(820f, 470f), PanelColor, RoundedSprite, true);
        CreateText("Title", window.transform, new Vector2(0f, 190f), new Vector2(700f, 50f), "사운드 설정", 36f, Color.white).fontStyle = FontStyles.Bold;
        var demo = new GameObject("AudioMixerDemo").AddComponent<AudioMixerDemo>();
        string[] names = { "Master", "BGM", "SFX" };
        var sliders = new List<Slider>();
        var labels = new List<TextMeshProUGUI>();
        var meters = new List<Image>();
        for (int i = 0; i < names.Length; i++)
        {
            float y = 90f - i * 110f;
            CreateText($"Name_{i}", window.transform, new Vector2(-320f, y), new Vector2(140f, 40f), names[i], 30f, TextColor, TextAlignmentOptions.Left);
            var slider = CreateSlider($"Slider_{names[i]}", window.transform, new Vector2(-30f, y), 360f, 0.0001f, 1f, 0.8f);
            sliders.Add(slider);
            labels.Add(CreateText($"Db_{i}", window.transform, new Vector2(215f, y), new Vector2(130f, 40f), "0 dB", 24f, SubTextColor));
            var mute = CreateButton($"Mute_{i}", window.transform, new Vector2(330f, y), new Vector2(100f, 44f), "MUTE", new Color(0.4f, 0.42f, 0.5f), 20f);
            OnClick(mute, demo.ToggleMute, i);
            // 최종 볼륨 미터 (슬라이더 아래 얇은 막대)
            var meterBg = CreateImage($"Meter_{i}", window.transform, new Vector2(-30f, y - 34f), new Vector2(360f, 8f), new Color(0.15f, 0.16f, 0.2f), WhiteSprite);
            var fill = CreateImage("Fill", meterBg.transform, Vector2.zero, Vector2.zero, new Color(0.4f, 0.9f, 0.5f), WhiteSprite);
            Stretch(fill.rectTransform);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            meters.Add(fill);
        }

        Set(demo, "mixer", mixer);
        SetArray(demo, "sliders", sliders.ToArray());
        SetArray(demo, "dbLabels", labels.ToArray());
        SetArray(demo, "meters", meters.ToArray());
        Set(demo, "sfxSource", sfxSrc);
        Set(demo, "sfxClip", sfx);

        ExplainPanel(canvas, "실제 Audio Mixer 설정",
            "1. Project → Create → <b>Audio Mixer</b> → Window → Audio Mixer\n" +
            "2. Groups 의 + 로 Master 아래 <b>BGM</b>, <b>SFX</b> 추가\n" +
            "3. 각 AudioSource 의 <b>Output</b> 에 그룹 지정\n" +
            "4. 그룹 선택 → Inspector 의 Volume 우클릭\n   → <b>Expose ... to script</b> → 이름을 BGMVolume 으로\n" +
            "5. 코드:\n<color=#9FD0FF>[SerializeField] AudioMixer mixer;\n" +
            "void OnBgm(float v) =>\n  mixer.SetFloat(\"BGMVolume\", Mathf.Log10(v) * 20);</color>\n" +
            "   슬라이더 최소값은 0.0001 (Log10(0) = -∞ 방지)\n\n" +
            "<color=#9BE89B>이 씬</color>: SimpleAudioMixer 가 같은 SetFloat/그룹 트리를 흉내", 560f, 430f);

        Save(ctx);
    }
}
