using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0517_HealEffect
{
    [GlossaryScene("0517")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0517_HealEffect")]
    public static void Build()
    {
        var ctx = Stage("0517", "H / Space : 회복 (+25)   D : 피해 (-20)\nS : 나선(Orbital Velocity) ON/OFF\n초록 입자가 몸 둘레를 돌며 올라가는 나선 궤적을 관찰",
            new Vector3(-0.3f, 2.6f, -6f), new Vector3(12f, 0f, 0f));
        ctx.Light.intensity = 0.5f;

        var player = Prim(PrimitiveType.Capsule, "Player", new Vector3(1f, 1f, 0f), Vector3.one, PlayerColor);

        // 나선 입자: 원 둘레(반지름 0.7, 바닥과 평행)에서 0.8초 동안 생성
        // Velocity over Lifetime: 위로 2m/s + Orbital Y 4rad/s → 나선
        var spiral = PS("HealSpiral", player.transform, new Vector3(0f, -0.9f, 0f), Glow("HealSpiral", new Color(0.4f, 1f, 0.5f)), false, false);
        var m = spiral.main;
        m.duration = 0.8f;
        m.startLifetime = Range(1f, 1.3f);
        m.startSpeed = 0f;
        m.startSize = Range(0.08f, 0.16f);
        m.simulationSpace = ParticleSystemSimulationSpace.Local; // 공전 중심 = 캐릭터
        var em = spiral.emission;
        em.rateOverTime = 70f;
        var sh = spiral.shape;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius = 0.7f;
        sh.radiusThickness = 0f; // 둘레에서만
        sh.rotation = new Vector3(90f, 0f, 0f);
        var vel = spiral.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.x = 0f; vel.y = 2f; vel.z = 0f;
        vel.orbitalY = 4f;
        var tr = spiral.trails;
        tr.enabled = true;
        tr.lifetime = 0.15f;
        tr.inheritParticleColor = true;
        tr.widthOverTrail = CurveMM(1f, 0f, 1f, 1f, 0f);
        spiral.GetComponent<ParticleSystemRenderer>().trailMaterial = ParticleMat("FX05_HealTrail", Color.white, true, BeamTex);
        ColorOverLife(spiral, (0f, new Color(1f, 1f, 1f, 0f)), (0.15f, Color.white), (0.8f, new Color(0.6f, 1f, 0.6f, 1f)), (1f, new Color(1f, 1f, 1f, 0f)));

        // + 모양 입자 몇 개
        var crosses = PS("HealCrosses", player.transform, new Vector3(0f, 0f, 0f), ParticleMat("FX05_HealCross", new Color(0.5f, 1f, 0.55f), true, CrossTex), false, false);
        m = crosses.main;
        m.duration = 0.5f;
        m.startLifetime = Range(0.8f, 1.2f);
        m.startSpeed = Range(0.8f, 1.5f);
        m.startSize = Range(0.2f, 0.35f);
        Burst(crosses, 6);
        ShapeUp(crosses, ParticleSystemShapeType.Cone, 25f, 0.5f);
        ColorOverLife(crosses, (0f, new Color(1f, 1f, 1f, 0f)), (0.2f, Color.white), (1f, new Color(1f, 1f, 1f, 0f)));

        // HP 바
        var canvas = EnsureCanvas(ctx).transform;
        var bar = CreateImage("HpBar", canvas, new Vector2(0f, 150f), new Vector2(520f, 30f), new Color32(30, 33, 42, 255));
        AnchorBottom(bar.rectTransform);
        var fill = CreateImage("Fill", bar.transform, Vector2.zero, Vector2.zero, new Color32(90, 220, 110, 255), WhiteSprite);
        Stretch(fill.rectTransform, 4f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 0.35f;
        CreateText("HpLabel", bar.transform, new Vector2(0f, 32f), new Vector2(520f, 30f), "HP", 22f, TextColor);

        var demo = player.AddComponent<HealEffectDemo>();
        Set(demo, "spiral", spiral);
        Set(demo, "crosses", crosses);
        Set(demo, "hpFill", fill);
        Set(demo, "body", player.GetComponent<Renderer>());
        var b = Buttons(ctx, "회복", "피해", "나선 ON/OFF");
        OnClick(b[0], demo.Heal);
        OnClick(b[1], demo.Damage);
        OnClick(b[2], demo.ToggleSpiral);
        Save(ctx);
    }
}
