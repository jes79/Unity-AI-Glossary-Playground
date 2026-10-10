using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0525_Vignette
{
    [GlossaryScene("0525")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0525_Vignette")]
    public static void Build()
    {
        var ctx = Stage("0525", "Space / D : 피해 -10   H : 회복 +10\nP : 맥박 ON/OFF (OFF 면 고정된 빨간 테두리)\n체력 30% 이하에서 화면 가장자리가 빨갛게 박동하는지 관찰",
            new Vector3(-1f, 4f, -9f), new Vector3(18f, 0f, 0f));
        EnablePostProcessing(ctx.Camera);
        PPShowcase(new Vector3(0.5f, 0f, 0f), 2f);

        var profile = Profile("Profile_0525_Vignette");
        var v = AddOverride<Vignette>(profile);
        v.color.Override(Color.black);
        v.intensity.Override(0.2f);
        v.smoothness.Override(0.45f);
        v.rounded.Override(false);
        SaveProfile(profile);
        var volume = GlobalVolume("Global Volume", profile);

        // HP 바 (아래 가운데)
        var canvas = EnsureCanvas(ctx).transform;
        var bar = CreateImage("HpBar", canvas, new Vector2(0f, 150f), new Vector2(600f, 34f), new Color32(30, 33, 42, 255));
        AnchorBottom(bar.rectTransform);
        var fill = CreateImage("Fill", bar.transform, Vector2.zero, Vector2.zero, new Color(0.35f, 0.85f, 0.4f), WhiteSprite);
        Stretch(fill.rectTransform, 4f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        var mark = CreateImage("Mark30", bar.transform, new Vector2(-300f + 600f * 0.3f, 0f), new Vector2(3f, 44f), new Color(1f, 0.4f, 0.4f));
        mark.raycastTarget = false;
        CreateText("HpLabel", bar.transform, new Vector2(0f, 36f), new Vector2(600f, 30f), "HP   (빨간 선 = 30%)", 22f, TextColor);

        var demo = volume.gameObject.AddComponent<LowHpVignetteDemo>();
        Set(demo, "volume", volume);
        Set(demo, "hpFill", fill);
        var b = Buttons(ctx, "피해 -10", "회복 +10", "맥박 ON/OFF");
        OnClick(b[0], demo.Damage);
        OnClick(b[1], demo.Heal);
        OnClick(b[2], demo.TogglePulse);
        Save(ctx);
    }
}
