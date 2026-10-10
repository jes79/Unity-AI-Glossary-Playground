using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0519_DamagePopup
{
    [GlossaryScene("0519")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0519_DamagePopup")]
    public static void Build()
    {
        var ctx = Stage("0519", "표적 클릭 : 맞은 위치에 데미지 숫자   Space : 랜덤 표적 타격\nC : 치명타 강제   A : 자동 연타 ON/OFF\n치명타(20%)는 2배 데미지 + 1.6배 크기 + 빨간색",
            new Vector3(0f, 2.8f, -7f), new Vector3(12f, 0f, 0f));

        var cols = new Collider[3];
        for (int i = 0; i < 3; i++)
        {
            var d = Prim(PrimitiveType.Capsule, $"Dummy_{i}", new Vector3(-1f + i * 2.2f, 1f, 0f), Vector3.one, EnemyColor);
            cols[i] = d.GetComponent<Collider>();
        }

        var spark = PS("HitSpark", null, Vector3.zero, Glow("PopupSpark", new Color(1f, 0.85f, 0.5f)), true, true); // Rate 0 으로 재생만 해 두고 Emit() 으로 뿌림
        var m = spark.main;
        m.startLifetime = Range(0.2f, 0.4f);
        m.startSpeed = Range(3f, 6f);
        m.startSize = Range(0.05f, 0.1f);
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = spark.emission;
        em.rateOverTime = 0f;
        var sh = spark.shape;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius = 0.05f;
        Stretched(spark, 0.04f, 1.2f);

        var demo = new GameObject("DamagePopupDemo").AddComponent<DamagePopupDemo>();
        SetArray(demo, "targets", cols);
        Set(demo, "hitSpark", spark);
        var b = Buttons(ctx, "타격", "치명타", "자동 연타");
        OnClick(b[0], demo.HitRandom);
        OnClick(b[1], demo.ForceCritical);
        OnClick(b[2], demo.ToggleAuto);
        Save(ctx);
    }
}
