using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0523_VolumePostProcessing
{
    [GlossaryScene("0523")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0523_VolumePostProcessing")]
    public static void Build()
    {
        var ctx = Stage("0523", "V : Global Volume ON/OFF   P : 카메라 Post Processing 체크 ON/OFF\nB : Bloom ON/OFF   C : Color Adjustments ON/OFF\n슬라이더 : Bloom Intensity / Saturation / Post Exposure",
            new Vector3(-1f, 4f, -9f), new Vector3(18f, 0f, 0f));
        EnablePostProcessing(ctx.Camera); // 카메라 Rendering ▸ Post Processing 체크
        PPShowcase(new Vector3(0.5f, 0f, 0f));

        // Volume Profile 에셋 + Override 2개 (Bloom, Color Adjustments)
        var profile = Profile("Profile_0523_Basics");
        var bloom = AddOverride<Bloom>(profile);
        bloom.intensity.Override(1.5f);
        bloom.threshold.Override(1f);
        bloom.scatter.Override(0.7f);
        var color = AddOverride<ColorAdjustments>(profile);
        color.postExposure.Override(0.2f);
        color.contrast.Override(15f);
        color.saturation.Override(20f);
        SaveProfile(profile);
        var volume = GlobalVolume("Global Volume", profile);

        var demo = volume.gameObject.AddComponent<VolumeBasicsDemo>();
        Set(demo, "volume", volume);
        Set(demo, "targetCamera", ctx.Camera);
        var b = Buttons(ctx, "Volume", "카메라 PP", "Bloom", "Color Adj.");
        OnClick(b[0], demo.ToggleVolume);
        OnClick(b[1], demo.ToggleCameraPP);
        OnClick(b[2], demo.ToggleBloom);
        OnClick(b[3], demo.ToggleColor);
        OnValueChanged(RightSlider(ctx, 2, "Bloom Intensity", 0f, 5f, 1.5f), demo.SetBloomIntensity);
        OnValueChanged(RightSlider(ctx, 1, "Saturation", -100f, 100f, 20f), demo.SetSaturation);
        OnValueChanged(RightSlider(ctx, 0, "Post Exposure", -2f, 2f, 0.2f), demo.SetExposure);
        Save(ctx);
    }
}
