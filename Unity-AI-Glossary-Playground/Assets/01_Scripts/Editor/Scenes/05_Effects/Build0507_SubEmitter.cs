using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0507_SubEmitter
{
    [GlossaryScene("0507")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0507_SubEmitter")]
    public static void Build()
    {
        var ctx = Stage("0507", "Space : 로켓 1발 발사   A : 자동 발사 ON/OFF\nS : Sub Emitter ON/OFF\n로켓(부모 입자)이 죽는 순간 그 자리에서 폭죽(자식)이 터지는지 관찰",
            new Vector3(0f, 6f, -16f), new Vector3(-8f, 0f, 0f));
        ctx.Light.intensity = 0.2f;

        // 부모: 로켓. 위로 날아가다 수명(≈정점)에서 죽음
        var rockets = PS("Rockets", null, new Vector3(1f, 0f, 0f), Glow("Rocket", Color.white));
        var m = rockets.main;
        m.startLifetime = Range(1.1f, 1.4f);
        m.startSpeed = Range(11f, 13f);
        m.startSize = 0.25f;
        m.startColor = new Color(1f, 0.85f, 0.6f);
        m.gravityModifier = 0.5f;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = rockets.emission;
        em.rateOverTime = 1.2f;
        ShapeUp(rockets, ParticleSystemShapeType.Cone, 12f, 2f);
        var trails = rockets.trails;
        trails.enabled = true;
        trails.lifetime = 0.25f;
        trails.widthOverTrail = CurveMM(0.6f, 0f, 1f, 1f, 0f);
        trails.colorOverTrail = new ParticleSystem.MinMaxGradient(Grad((0f, new Color(1f, 0.7f, 0.3f, 1f)), (1f, new Color(1f, 0.3f, 0f, 0f))));
        var rr = rockets.GetComponent<ParticleSystemRenderer>();
        rr.trailMaterial = ParticleMat("FX05_RocketTrail", Color.white, true, BeamTex);

        // 자식: 폭죽 Burst (구 모양으로 사방, 무지개 랜덤 색, 꼬리)
        var burst = PS("FireworkBurst", rockets.transform, Vector3.zero, Glow("Firework", Color.white), false, false);
        m = burst.main;
        m.startLifetime = Range(1f, 1.6f);
        m.startSpeed = Range(4f, 6f);
        m.startSize = Range(0.12f, 0.2f);
        m.gravityModifier = 0.4f;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        var rainbow = Grad((0f, new Color(1f, 0.3f, 0.3f)), (0.33f, new Color(1f, 0.9f, 0.3f)), (0.66f, new Color(0.3f, 1f, 0.6f)), (1f, new Color(0.5f, 0.6f, 1f)));
        m.startColor = new ParticleSystem.MinMaxGradient(rainbow) { mode = ParticleSystemGradientMode.RandomColor };
        Burst(burst, 80);
        var shape = burst.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        var limit = burst.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.limit = 2.5f;
        limit.dampen = 0.08f; // 터진 뒤 공기 저항으로 감속
        ColorOverLife(burst, (0f, Color.white), (0.7f, Color.white), (1f, new Color(1f, 1f, 1f, 0f)));
        var bt = burst.trails;
        bt.enabled = true;
        bt.lifetime = 0.2f;
        bt.inheritParticleColor = true;
        bt.widthOverTrail = CurveMM(0.5f, 0f, 1f, 1f, 0f);
        burst.GetComponent<ParticleSystemRenderer>().trailMaterial = ParticleMat("FX05_FireworkTrail", Color.white, true, BeamTex);

        // 부모에 Sub Emitter 등록: 부모 입자가 Death 할 때 burst 재생
        var sub = rockets.subEmitters;
        sub.enabled = true;
        sub.AddSubEmitter(burst, ParticleSystemSubEmitterType.Death, ParticleSystemSubEmitterProperties.InheritNothing);

        var demo = rockets.gameObject.AddComponent<SubEmitterDemo>();
        Set(demo, "rockets", rockets);
        var b = Buttons(ctx, "로켓 발사", "Sub Emitter ON/OFF", "자동 발사");
        OnClick(b[0], demo.Launch);
        OnClick(b[1], demo.ToggleSubEmitter);
        OnClick(b[2], demo.ToggleAuto);
        Save(ctx);
    }
}
