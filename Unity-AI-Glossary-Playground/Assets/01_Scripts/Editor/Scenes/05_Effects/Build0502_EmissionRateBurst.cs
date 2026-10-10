using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0502_EmissionRateBurst
{
    [GlossaryScene("0502")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0502_EmissionRateBurst")]
    public static void Build()
    {
        var ctx = Stage("0502", "Space / 표적 클릭 : 피격 → Burst 20개\n↑ / ↓ : 왼쪽 Rate over Time ±10\n왼쪽은 꾸준히, 오른쪽은 한순간에 몰아서 나오는 차이를 관찰",
            new Vector3(0f, 3f, -9f), new Vector3(12f, 0f, 0f));

        // 왼쪽: Rate over Time 분수
        var rate = PS("RateFountain", null, new Vector3(-2.5f, 0f, 0f), Glow("EmitRate", new Color(0.4f, 0.8f, 1f)));
        var m = rate.main;
        m.startLifetime = 1.5f;
        m.startSpeed = Range(4f, 5f);
        m.startSize = 0.15f;
        m.gravityModifier = 0.6f;
        var em = rate.emission;
        em.rateOverTime = 10f;
        ShapeUp(rate, ParticleSystemShapeType.Cone, 12f, 0.1f);
        Label3D("Rate over Time", new Vector3(-2.5f, -0.2f, -1.2f), 3f).transform.rotation = Quaternion.Euler(12f, 0f, 0f);

        // 오른쪽: 표적 + Burst 파편
        var target = Prim(PrimitiveType.Cube, "Target", new Vector3(2.5f, 1f, 0f), new Vector3(1.2f, 2f, 1.2f), EnemyColor);
        var burst = PS("BurstDebris", null, new Vector3(2.5f, 1f, -0.6f), Glow("EmitBurst", new Color(1f, 0.8f, 0.4f)), false, false);
        m = burst.main;
        m.duration = 0.5f;
        m.startLifetime = Range(0.4f, 0.8f);
        m.startSpeed = Range(4f, 9f);
        m.startSize = Range(0.06f, 0.14f);
        m.gravityModifier = 1.2f;
        Burst(burst, 20);
        var shape = burst.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 45f;
        shape.radius = 0.05f;
        Stretched(burst, 0.05f, 1.5f);
        Label3D("Burst 20", new Vector3(2.5f, -0.2f, -1.2f), 3f).transform.rotation = Quaternion.Euler(12f, 0f, 0f);

        var demo = new GameObject("EmissionDemo").AddComponent<EmissionDemo>();
        Set(demo, "rateSystem", rate);
        Set(demo, "burstSystem", burst);
        Set(demo, "target", target.transform);

        var b = Buttons(ctx, "피격 (Burst)");
        OnClick(b[0], demo.HitButton);
        var s = RightSlider(ctx, 0, "Rate over Time (개/초)", 0f, 100f, 10f);
        OnValueChanged(s, demo.SetRate);
        Save(ctx);
    }
}
