using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0508_TrailRenderer
{
    [GlossaryScene("0508")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0508_TrailRenderer")]
    public static void Build()
    {
        var ctx = Stage("0508", "Space / 클릭 : 칼 휘두르기\nT : Trail Renderer ON/OFF   ↑/↓ : time ±0.05\nP : 오른쪽 혜성 파티클의 Trails 모듈 ON/OFF",
            new Vector3(0.5f, 2f, -7f), new Vector3(5f, 0f, 0f));

        // 칼: 피벗(손잡이) 기준으로 Z 축 회전
        var pivot = new GameObject("SwordPivot").transform;
        pivot.position = new Vector3(-0.5f, 1.2f, 0f);
        Prim(PrimitiveType.Cube, "Handle", new Vector3(0f, 0.2f, 0f), new Vector3(0.1f, 0.4f, 0.1f), new Color32(90, 60, 40, 255), pivot);
        Prim(PrimitiveType.Cube, "Guard", new Vector3(0f, 0.42f, 0f), new Vector3(0.45f, 0.06f, 0.12f), new Color32(200, 170, 80, 255), pivot);
        Prim(PrimitiveType.Cube, "Blade", new Vector3(0f, 1.25f, 0f), new Vector3(0.14f, 1.6f, 0.04f), new Color32(210, 220, 235, 255), pivot);
        foreach (var c in pivot.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);

        // 칼날 가운데에 Trail Renderer: 폭 1.6 ≈ 칼날 길이 → 칼날 전체가 띠를 남김
        var trailGo = new GameObject("BladeTrail");
        trailGo.transform.SetParent(pivot, false);
        trailGo.transform.localPosition = new Vector3(0f, 1.3f, 0f);
        var trail = trailGo.AddComponent<TrailRenderer>();
        trail.time = 0.25f;
        trail.minVertexDistance = 0.05f;
        trail.widthMultiplier = 1.6f;
        trail.widthCurve = Curve(0f, 1f, 1f, 0.2f);
        trail.colorGradient = Grad((0f, new Color(0.8f, 0.95f, 1f, 0.9f)), (0.5f, new Color(0.3f, 0.6f, 1f, 0.5f)), (1f, new Color(0.2f, 0.3f, 1f, 0f)));
        trail.sharedMaterial = ParticleMat("FX05_SwordTrail", Color.white, true, BeamTex);
        trail.alignment = LineAlignment.View;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // 오른쪽: Trails 모듈 혜성
        var comets = PS("Comets", null, new Vector3(3.5f, 1.2f, 0f), Glow("Comet", new Color(1f, 0.8f, 0.4f)));
        var m = comets.main;
        m.startLifetime = 1.6f;
        m.startSpeed = Range(1.5f, 2.5f);
        m.startSize = 0.15f;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = comets.emission;
        em.rateOverTime = 6f;
        var shape = comets.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;
        var vel = comets.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.orbitalZ = 3f;
        vel.x = 0f; vel.y = 0f; vel.z = 0f;
        var tr = comets.trails;
        tr.enabled = true;
        tr.lifetime = 0.4f;
        tr.inheritParticleColor = true;
        tr.widthOverTrail = CurveMM(1f, 0f, 1f, 1f, 0f);
        comets.GetComponent<ParticleSystemRenderer>().trailMaterial = ParticleMat("FX05_CometTrail", Color.white, true, BeamTex);
        Label3D("Particle Trails", new Vector3(3.5f, -0.3f, 0f), 2.5f);

        var demo = pivot.gameObject.AddComponent<SwordTrailDemo>();
        Set(demo, "swordPivot", pivot);
        Set(demo, "trail", trail);
        Set(demo, "comets", comets);
        var b = Buttons(ctx, "휘두르기", "Trail ON/OFF", "Particle Trails");
        OnClick(b[0], demo.Swing);
        OnClick(b[1], demo.ToggleTrail);
        OnClick(b[2], demo.ToggleParticleTrails);
        Save(ctx);
    }
}
