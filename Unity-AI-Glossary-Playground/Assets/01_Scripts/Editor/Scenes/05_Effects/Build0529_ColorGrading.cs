using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0529_ColorGrading
{
    [GlossaryScene("0529")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0529_ColorGrading")]
    public static void Build()
    {
        var ctx = Stage("0529", "1 기본   2 밤(푸른 톤)   3 던전(채도 낮게)   4 LUT(세피아)\n프리셋마다 Volume 하나 — weight 를 1초 동안 교차 페이드\n같은 장면의 색감만 바뀌는 것을 관찰",
            new Vector3(-1f, 4f, -9f), new Vector3(18f, 0f, 0f));
        EnablePostProcessing(ctx.Camera);
        ctx.Camera.backgroundColor = new Color32(120, 160, 210, 255);
        PPShowcase(new Vector3(0.5f, 0f, 0f), 2.5f);
        // 피부·나무 같은 중간 톤 소품
        Prim(PrimitiveType.Capsule, "Character", new Vector3(-4f, 1f, -1.5f), Vector3.one, new Color32(230, 180, 140, 255));
        Prim(PrimitiveType.Cylinder, "Tree", new Vector3(5.5f, 1f, 2f), new Vector3(0.4f, 1f, 0.4f), new Color32(110, 75, 50, 255));
        Prim(PrimitiveType.Sphere, "TreeTop", new Vector3(5.5f, 2.6f, 2f), Vector3.one * 2f, new Color32(70, 150, 70, 255));

        // 공통 기본: 톤매핑만 (낮)
        var baseProfile = Profile("Profile_0529_Base");
        AddOverride<Tonemapping>(baseProfile).mode.Override(TonemappingMode.Neutral);
        SaveProfile(baseProfile);
        GlobalVolume("Base Volume", baseProfile, 0f);

        // 밤: 차가운 화이트 밸런스 + 파란 필터 + 어둡게
        var night = Profile("Profile_0529_Night");
        var wb = AddOverride<WhiteBalance>(night);
        wb.temperature.Override(-45f);
        wb.tint.Override(5f);
        var nca = AddOverride<ColorAdjustments>(night);
        nca.postExposure.Override(-1.1f);
        nca.colorFilter.Override(new Color(0.65f, 0.75f, 1f));
        nca.saturation.Override(-25f);
        nca.contrast.Override(10f);
        var lgg = AddOverride<LiftGammaGain>(night);
        lgg.lift.Override(new Vector4(0.9f, 0.95f, 1.15f, -0.05f)); // 어두운 부분을 푸르게
        SaveProfile(night);

        // 던전: 채도 크게 낮춤 + 대비 + 따뜻하고 어두운 그림자
        var dungeon = Profile("Profile_0529_Dungeon");
        var dca = AddOverride<ColorAdjustments>(dungeon);
        dca.saturation.Override(-65f);
        dca.contrast.Override(30f);
        dca.postExposure.Override(-0.5f);
        dca.colorFilter.Override(new Color(1f, 0.93f, 0.85f));
        var smh = AddOverride<ShadowsMidtonesHighlights>(dungeon);
        smh.shadows.Override(new Vector4(1f, 0.9f, 0.85f, -0.2f));
        var dv = AddOverride<Vignette>(dungeon);
        dv.intensity.Override(0.35f);
        SaveProfile(dungeon);

        // LUT: 32 크기 세피아 LUT 텍스처 (1024x32, 선형)
        var lut = Profile("Profile_0529_LUT");
        var cl = AddOverride<ColorLookup>(lut);
        cl.texture.Override(SepiaLutTex);
        cl.contribution.Override(1f);
        SaveProfile(lut);

        var vNight = GlobalVolume("Preset Night", night, 1f, 0f);
        var vDungeon = GlobalVolume("Preset Dungeon", dungeon, 1f, 0f);
        var vLut = GlobalVolume("Preset LUT", lut, 1f, 0f);

        var demo = new GameObject("ColorGradingDemo").AddComponent<ColorGradingDemo>();
        SetArray(demo, "presetVolumes", vNight, vDungeon, vLut);
        Set(demo, "sun", ctx.Light);
        var b = Buttons(ctx, "기본", "밤", "던전", "LUT");
        for (int i = 0; i < b.Length; i++) OnClick(b[i], demo.Select, i);

        InfoCard(ctx, "사용한 Override",
            "<b>밤</b>  White Balance(Temperature -45)\n  Color Adjustments(Exposure -1.1, Filter 파랑, Sat -25)\n  Lift Gamma Gain(Lift 파랑)\n" +
            "<b>던전</b>  Color Adjustments(Saturation -65, Contrast 30)\n  Shadows Midtones Highlights, Vignette 0.35\n" +
            "<b>LUT</b>  Color Lookup: 1024x32 LUT (URP 에셋 LUT 크기 32)\n  sRGB 끄기, 압축 없음, Mip 없음으로 임포트", 560f, 330f);
        Save(ctx);
    }
}
