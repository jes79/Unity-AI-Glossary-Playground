using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0503_LifetimeSpeedSize
{
    [GlossaryScene("0503")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0503_LifetimeSpeedSize")]
    public static void Build()
    {
        var ctx = Stage("0503", "1 : 요청값 (수명 0.5 / 속도 5 / 크기 0.1~0.3)\n2 : 느리고 큼   3 : 빠르고 짧음\n오른쪽 슬라이더로 값 하나씩 바꿔 보기 — 퍼지는 반경 = 속도×수명",
            new Vector3(-1f, 4f, -9f), new Vector3(20f, 0f, 0f));

        var ps = PS("Particles", null, new Vector3(0.5f, 1.5f, 0f), Glow("LSS", new Color(0.5f, 0.9f, 1f)));
        var m = ps.main;
        m.startLifetime = 0.5f;
        m.startSpeed = 5f;
        m.startSize = Range(0.1f, 0.3f);
        m.maxParticles = 2000;
        var em = ps.emission;
        em.rateOverTime = 80f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        ColorOverLife(ps, (0f, Color.white), (0.8f, Color.white), (1f, new Color(1f, 1f, 1f, 0f)));

        var demo = ps.gameObject.AddComponent<LifetimeSpeedSizeDemo>();
        Set(demo, "ps", ps);
        var s0 = RightSlider(ctx, 3, "Start Lifetime (초)", 0.1f, 3f, 0.5f);
        var s1 = RightSlider(ctx, 2, "Start Speed", 0f, 15f, 5f);
        var s2 = RightSlider(ctx, 1, "Start Size 최소", 0.01f, 1f, 0.1f);
        var s3 = RightSlider(ctx, 0, "Start Size 최대", 0.01f, 1f, 0.3f);
        OnValueChanged(s0, demo.SetLifetime);
        OnValueChanged(s1, demo.SetSpeed);
        OnValueChanged(s2, demo.SetSizeMin);
        OnValueChanged(s3, demo.SetSizeMax);
        SetArray(demo, "sliders", s0, s1, s2, s3);

        var b = Buttons(ctx, "요청값", "느리고 큼", "빠르고 짧음");
        OnClick(b[0], demo.PresetRequest);
        OnClick(b[1], demo.PresetSlow);
        OnClick(b[2], demo.PresetFast);
        Save(ctx);
    }
}
