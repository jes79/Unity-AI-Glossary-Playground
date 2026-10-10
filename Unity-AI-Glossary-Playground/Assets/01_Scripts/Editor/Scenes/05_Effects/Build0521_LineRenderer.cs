using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0521_LineRenderer
{
    [GlossaryScene("0521")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0521_LineRenderer")]
    public static void Build()
    {
        var ctx = Stage("0521", "마우스 : 조준   왼쪽 버튼 누르고 있기 : 레이저 발사\nSpace : 계속 발사 ON/OFF   R : 반사 모드 ON/OFF\n맞은 지점에서 스파크가 튀고, 반사 모드에선 Line Renderer 점이 늘어남",
            new Vector3(0f, 12f, -9f), new Vector3(55f, 0f, 0f));
        ctx.Light.intensity = 0.4f;

        // 포탑
        var turret = new GameObject("Turret").transform;
        turret.position = new Vector3(-5f, 0f, 0f);
        Prim(PrimitiveType.Cylinder, "Base", new Vector3(0f, 0.4f, 0f), new Vector3(1f, 0.4f, 1f), new Color32(70, 75, 90, 255), turret);
        var barrel = Prim(PrimitiveType.Cube, "Barrel", new Vector3(0f, 1f, 0.5f), new Vector3(0.25f, 0.25f, 1f), new Color32(50, 55, 65, 255), turret);
        NoCollider(barrel);
        foreach (var c in turret.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
        var muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(turret, false);
        muzzle.localPosition = new Vector3(0f, 1f, 1.05f);

        // 레이저 Line Renderer: 가산 머티리얼, 빨강→주황, 끝으로 갈수록 살짝 가늘게
        var line = muzzle.gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.widthMultiplier = 0.12f;
        line.widthCurve = Curve(0f, 1f, 1f, 0.7f);
        line.colorGradient = Grad((0f, new Color(1f, 0.3f, 0.25f, 1f)), (1f, new Color(1f, 0.6f, 0.3f, 1f)));
        line.sharedMaterial = ParticleMat("FX05_Laser", new Color(1f, 0.5f, 0.4f), true, BeamTex);
        line.numCapVertices = 4;
        line.textureMode = LineTextureMode.Stretch;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.enabled = false;

        // 스파크: 맞은 면에서 반사 방향으로 튐
        var sparks = PS("HitSparks", null, Vector3.zero, Glow("LaserSpark", new Color(1f, 0.7f, 0.4f)));
        var m = sparks.main;
        m.startLifetime = Range(0.2f, 0.45f);
        m.startSpeed = Range(4f, 9f);
        m.startSize = Range(0.04f, 0.08f);
        m.gravityModifier = 1f;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = sparks.emission;
        em.rateOverTime = 90f;
        em.enabled = false;
        var sh = sparks.shape;
        sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle = 40f;
        sh.radius = 0.02f;
        Stretched(sparks, 0.04f, 1.2f);

        var light = new GameObject("HitLight").AddComponent<Light>();
        light.type = LightType.Point; light.color = new Color(1f, 0.4f, 0.3f); light.range = 3f; light.intensity = 4f; light.enabled = false;

        // 벽·장애물 (반사 대상)
        var wall = new Color32(110, 115, 130, 255);
        Prim(PrimitiveType.Cube, "Wall_N", new Vector3(0f, 1f, 6f), new Vector3(16f, 2f, 0.4f), wall);
        Prim(PrimitiveType.Cube, "Wall_E", new Vector3(8f, 1f, 0f), new Vector3(0.4f, 2f, 12f), wall);
        Prim(PrimitiveType.Cube, "Wall_S", new Vector3(0f, 1f, -6f), new Vector3(16f, 2f, 0.4f), wall);
        var mirror = Prim(PrimitiveType.Cube, "Mirror", new Vector3(3f, 1f, 3f), new Vector3(2f, 2f, 0.2f), new Color32(170, 200, 230, 255));
        mirror.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
        var movers = new Transform[2];
        for (int i = 0; i < 2; i++)
            movers[i] = Prim(PrimitiveType.Cube, $"Mover_{i}", new Vector3(-0.5f + i * 3f, 1f, -1f), new Vector3(0.8f, 2f, 0.8f), EnemyColor).transform;

        var demo = turret.gameObject.AddComponent<LaserBeamDemo>();
        Set(demo, "turret", turret);
        Set(demo, "muzzle", muzzle);
        Set(demo, "line", line);
        Set(demo, "sparks", sparks);
        Set(demo, "hitLight", light);
        SetArray(demo, "movers", movers);
        var b = Buttons(ctx, "계속 발사", "반사 ON/OFF");
        OnClick(b[0], demo.ToggleFire);
        OnClick(b[1], demo.ToggleReflect);
        Save(ctx);
    }
}
