using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0505_ColorSizeOverLifetime
{
    [GlossaryScene("0505")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0505_ColorSizeOverLifetime")]
    public static void Build()
    {
        var ctx = Stage("0505", "C : Color over Lifetime ON/OFF\nS : Size over Lifetime ON/OFF\n왼쪽(모듈 없음)과 오른쪽(노랑→빨강→투명, 점점 작게)을 비교",
            new Vector3(-0.5f, 3f, -9f), new Vector3(12f, 0f, 0f));

        var mat = Glow("OverLife", Color.white);
        ParticleSystem Make(string name, float x)
        {
            var ps = PS(name, null, new Vector3(x, 0.3f, 0f), mat);
            var m = ps.main;
            m.startLifetime = 1.6f;
            m.startSpeed = Range(2f, 2.5f);
            m.startSize = 0.6f;
            m.startColor = new Color(1f, 0.95f, 0.4f);
            var em = ps.emission;
            em.rateOverTime = 30f;
            ShapeUp(ps, ParticleSystemShapeType.Cone, 12f, 0.2f);
            return ps;
        }

        Make("Baseline (OFF)", -1.5f);
        var target = Make("OverLifetime (ON)", 2.5f);
        // 노랑 → 빨강 → 투명
        ColorOverLife(target, (0f, new Color(1f, 0.95f, 0.3f, 1f)), (0.5f, new Color(1f, 0.15f, 0.05f, 1f)), (1f, new Color(0.6f, 0f, 0f, 0f)));
        // 점점 작아지게 (1 → 0)
        SizeOverLife(target, 0f, 1f, 1f, 0f);

        Label3D("모듈 OFF", new Vector3(-1.5f, -0.1f, -1.2f), 3f).transform.rotation = Quaternion.Euler(12f, 0f, 0f);
        Label3D("Color/Size over Lifetime", new Vector3(2.5f, -0.1f, -1.2f), 3f).transform.rotation = Quaternion.Euler(12f, 0f, 0f);

        var demo = target.gameObject.AddComponent<OverLifetimeDemo>();
        Set(demo, "target", target);
        var b = Buttons(ctx, "Color ON/OFF", "Size ON/OFF");
        OnClick(b[0], demo.ToggleColor);
        OnClick(b[1], demo.ToggleSize);
        Save(ctx);
    }
}
