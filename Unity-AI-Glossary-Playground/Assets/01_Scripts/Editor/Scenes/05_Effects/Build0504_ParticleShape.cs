using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0504_ParticleShape
{
    [GlossaryScene("0504")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0504_ParticleShape")]
    public static void Build()
    {
        var ctx = Stage("0504", "1 Cone   2 Sphere   3 Edge   4 Box\n슬라이더 : Cone 각도 (요청값 30°)\n같은 연기가 모양(Shape)에 따라 어디서 어떻게 나오는지 관찰",
            new Vector3(-1f, 3.5f, -9f), new Vector3(12f, 0f, 0f));

        // 굴뚝
        Prim(PrimitiveType.Cylinder, "Chimney", new Vector3(0.8f, 0.5f, 0f), new Vector3(0.7f, 0.5f, 0.7f), new Color32(110, 80, 70, 255));

        var ps = PS("Smoke", null, new Vector3(0.8f, 1.05f, 0f), ParticleMat("FX05_ShapeSmoke", Color.white, false, SmokeTex));
        var m = ps.main;
        m.startLifetime = Range(2.5f, 3.5f);
        m.startSpeed = Range(1.5f, 2f);
        m.startSize = Range(0.4f, 0.7f);
        m.startRotation = Range(0f, Mathf.PI * 2f);
        m.startColor = new Color(0.75f, 0.77f, 0.8f);
        m.maxParticles = 600;
        var em = ps.emission;
        em.rateOverTime = 25f;
        ShapeUp(ps, ParticleSystemShapeType.Cone, 30f, 0.3f);
        ColorOverLife(ps, (0f, new Color(1f, 1f, 1f, 0f)), (0.15f, new Color(1f, 1f, 1f, 0.6f)), (1f, new Color(1f, 1f, 1f, 0f)));
        SizeOverLife(ps, 0f, 0.6f, 1f, 2.5f);

        var demo = ps.gameObject.AddComponent<ShapeModuleDemo>();
        Set(demo, "ps", ps);

        var b = Buttons(ctx, "Cone", "Sphere", "Edge", "Box");
        for (int i = 0; i < b.Length; i++) OnClick(b[i], demo.SetKind, i);
        var s = RightSlider(ctx, 0, "Cone 각도 (°)", 0f, 90f, 30f);
        OnValueChanged(s, demo.SetConeAngle);
        Save(ctx);
    }
}
