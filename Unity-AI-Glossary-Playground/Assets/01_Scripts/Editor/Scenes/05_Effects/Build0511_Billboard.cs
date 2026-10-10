using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0511_Billboard
{
    [GlossaryScene("0511")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0511_Billboard")]
    public static void Build()
    {
        var ctx = Stage("0511", "1 Billboard   2 Stretched Billboard   3 Horizontal   4 Vertical\n슬라이더 : Stretched 의 Speed Scale\n같은 스파크가 둥근 점 ↔ 길쭉한 선으로 바뀌는 것을 관찰",
            new Vector3(-0.5f, 3f, -8f), new Vector3(14f, 0f, 0f));
        ctx.Light.intensity = 0.4f;

        // 그라인더 스파크: 0.6초마다 40개 Burst, 중력으로 포물선
        var ps = PS("Sparks", null, new Vector3(1f, 0.68f, 0f), Glow("Spark", Color.white));
        var m = ps.main;
        m.duration = 0.6f;
        m.loop = true;
        m.startLifetime = Range(0.5f, 0.9f);
        m.startSpeed = Range(6f, 12f);
        m.startSize = Range(0.06f, 0.1f);
        m.startColor = new Color(1f, 0.8f, 0.4f);
        m.gravityModifier = 1.2f;
        Burst(ps, 40);
        ShapeUp(ps, ParticleSystemShapeType.Cone, 35f, 0.05f);
        ColorOverLife(ps, (0f, Color.white), (0.5f, new Color(1f, 0.6f, 0.2f, 1f)), (1f, new Color(1f, 0.3f, 0f, 0f)));
        var col = ps.collision;
        col.enabled = true;
        col.type = ParticleSystemCollisionType.World;
        col.bounce = 0.35f;
        col.dampen = 0.3f;
        Prim(PrimitiveType.Cylinder, "Grinder", new Vector3(1f, 0.3f, 0f), new Vector3(0.6f, 0.3f, 0.6f), new Color32(80, 85, 95, 255));

        var demo = ps.gameObject.AddComponent<BillboardDemo>();
        Set(demo, "sparks", ps.GetComponent<ParticleSystemRenderer>());
        var b = Buttons(ctx, "Billboard", "Stretched", "Horizontal", "Vertical");
        for (int i = 0; i < b.Length; i++) OnClick(b[i], demo.SetMode, i);
        OnValueChanged(RightSlider(ctx, 0, "Speed Scale", 0f, 0.3f, 0.08f), demo.SetVelocityScale);
        Save(ctx);
    }
}
