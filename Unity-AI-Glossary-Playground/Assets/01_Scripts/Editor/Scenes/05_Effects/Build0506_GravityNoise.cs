using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0506_GravityNoise
{
    [GlossaryScene("0506")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0506_GravityNoise")]
    public static void Build()
    {
        var ctx = Stage("0506", "N : Noise ON/OFF (곧게 vs 하늘하늘)\nG : Gravity Modifier -0.15 → 0 → 0.5 순환\n슬라이더 : Noise Strength / Frequency",
            new Vector3(-1f, 3f, -8f), new Vector3(8f, 0f, 0f));
        ctx.Light.intensity = 0.3f;

        Prim(PrimitiveType.Cylinder, "Brazier", new Vector3(0.8f, 0.25f, 0f), new Vector3(1.2f, 0.25f, 1.2f), new Color32(60, 60, 70, 255));
        var ps = PS("Embers", null, new Vector3(0.8f, 0.55f, 0f), Glow("Ember", Color.white));
        var m = ps.main;
        m.startLifetime = Range(2.5f, 4f);
        m.startSpeed = Range(0.3f, 0.8f);
        m.startSize = Range(0.05f, 0.12f);
        m.startColor = new Color(1f, 0.6f, 0.2f);
        m.gravityModifier = -0.15f;
        m.maxParticles = 500;
        var em = ps.emission;
        em.rateOverTime = 35f;
        ShapeUp(ps, ParticleSystemShapeType.Cone, 10f, 0.5f);
        ColorOverLife(ps, (0f, new Color(1f, 1f, 0.7f, 0f)), (0.1f, new Color(1f, 0.8f, 0.3f, 1f)), (0.7f, new Color(1f, 0.35f, 0.05f, 1f)), (1f, new Color(1f, 0.1f, 0f, 0f)));
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.8f;
        noise.frequency = 0.6f;

        var light = new GameObject("GlowLight").AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.5f, 0.2f);
        light.intensity = 2f;
        light.range = 5f;
        light.transform.position = new Vector3(0.8f, 1f, 0f);

        var demo = ps.gameObject.AddComponent<GravityNoiseDemo>();
        Set(demo, "embers", ps);
        var b = Buttons(ctx, "Noise ON/OFF", "Gravity 변경");
        OnClick(b[0], demo.ToggleNoise);
        OnClick(b[1], demo.CycleGravity);
        OnValueChanged(RightSlider(ctx, 1, "Noise Strength", 0f, 3f, 0.8f), demo.SetStrength);
        OnValueChanged(RightSlider(ctx, 0, "Noise Frequency", 0.05f, 3f, 0.6f), demo.SetFrequency);
        Save(ctx);
    }
}
