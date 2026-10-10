using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0524_Bloom
{
    [GlossaryScene("0524")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0524_Bloom")]
    public static void Build()
    {
        var ctx = Stage("0524", "↑ / ↓ : Bloom Threshold ±0.1   B : Bloom ON/OFF\n버튼 : Threshold 0.5(낮음) / 1.1(Emission 만)\nThreshold 를 1 위로 올리면 HDR Emission 구만 빛나는지 관찰",
            new Vector3(-1f, 4f, -9f), new Vector3(18f, 0f, 0f));
        EnablePostProcessing(ctx.Camera);
        ctx.Light.intensity = 1.1f;
        PPShowcase(new Vector3(0.5f, 0f, 0f), 4f);

        var profile = Profile("Profile_0524_Bloom");
        var bloom = AddOverride<Bloom>(profile);
        bloom.threshold.Override(1f);
        bloom.intensity.Override(1.5f);
        bloom.scatter.Override(0.75f);
        bloom.highQualityFiltering.Override(true);
        SaveProfile(profile);
        var volume = GlobalVolume("Global Volume", profile);

        var demo = volume.gameObject.AddComponent<BloomThresholdDemo>();
        Set(demo, "volume", volume);
        var th = RightSlider(ctx, 1, "Threshold", 0f, 3f, 1f);
        OnValueChanged(th, demo.SetThreshold);
        Set(demo, "thresholdSlider", th);
        OnValueChanged(RightSlider(ctx, 0, "Intensity", 0f, 5f, 1.5f), demo.SetIntensity);
        var b = Buttons(ctx, "Threshold 0.5", "Emission 만", "Bloom ON/OFF");
        OnClick(b[0], demo.PresetLow);
        OnClick(b[1], demo.PresetEmissionOnly);
        OnClick(b[2], demo.ToggleBloom);

        InfoCard(ctx, "Emission 만 빛나게 하는 조건",
            "1. 머티리얼 Emission 체크 + HDR 색 강도 > 1\n   (이 씬: 강도 4)\n" +
            "2. URP 에셋 HDR 켜기 (밝기 1 초과 값을 보존)\n" +
            "3. Bloom Threshold ≈ 1 (일반 물체 최대 밝기 위)\n" +
            "4. Intensity 로 번짐 세기, Scatter 로 퍼지는 범위", 500f, 230f);
        Save(ctx);
    }
}
