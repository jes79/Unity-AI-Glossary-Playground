using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0512_AdditiveAlphaBlend
{
    [GlossaryScene("0512")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0512_AdditiveAlphaBlend")]
    public static void Build()
    {
        var ctx = Stage("0512", "1 : 왼쪽 Alpha / 오른쪽 Additive\n2 : 모두 Alpha Blend   3 : 모두 Additive\n위쪽(어두운 배경)과 아래쪽(흰 배경)에서 보이는 차이를 관찰",
            new Vector3(0f, 2f, -8f), new Vector3(0f, 0f, 0f), ground: false);

        // 배경: 위 절반 어둡게, 아래 절반 밝게
        var dark = Prim(PrimitiveType.Quad, "DarkBg", new Vector3(0.5f, 3.2f, 2f), new Vector3(14f, 3.2f, 1f), new Color32(18, 20, 30, 255));
        var light = Prim(PrimitiveType.Quad, "LightBg", new Vector3(0.5f, 0.4f, 2f), new Vector3(14f, 2.4f, 1f), new Color32(235, 235, 235, 255));
        dark.GetComponent<Renderer>().sharedMaterial = UnlitMat("FX05_BgDark", new Color32(18, 20, 30, 255));
        light.GetComponent<Renderer>().sharedMaterial = UnlitMat("FX05_BgLight", new Color32(235, 235, 235, 255));

        var color = new Color(0.45f, 0.35f, 1f, 0.6f);
        var alphaMat = ParticleMat("FX05_Magic_Alpha", color, false, GlowTex);
        var addMat = ParticleMat("FX05_Magic_Additive", color, true, GlowTex);

        ParticleSystem Orb(string name, float x)
        {
            // 세로로 긴 마법 기둥: 위는 어두운 배경, 아래는 흰 배경에 걸침
            var ps = PS(name, null, new Vector3(x, 0f, 0f), alphaMat);
            var m = ps.main;
            m.startLifetime = Range(1.2f, 1.8f);
            m.startSpeed = Range(1.5f, 2.2f);
            m.startSize = Range(0.5f, 0.9f);
            m.maxParticles = 400;
            var em = ps.emission;
            em.rateOverTime = 60f;
            ShapeUp(ps, ParticleSystemShapeType.Circle, 0f, 0.4f);
            var shape = ps.shape;
            shape.rotation = new Vector3(90f, 0f, 0f);
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            vel.x = 0f; vel.z = 0f;
            vel.y = 2f;
            vel.orbitalY = 2f;
            ColorOverLife(ps, (0f, new Color(1f, 1f, 1f, 0f)), (0.2f, Color.white), (1f, new Color(1f, 1f, 1f, 0f)));
            return ps;
        }

        var left = Orb("Magic_Left", -2f);
        var right = Orb("Magic_Right", 3f);
        Label3D("Alpha Blend", new Vector3(-2f, 4.3f, 0f), 3f);
        Label3D("Additive", new Vector3(3f, 4.3f, 0f), 3f);

        var demo = new GameObject("BlendModeDemo").AddComponent<BlendModeDemo>();
        Set(demo, "left", left.GetComponent<ParticleSystemRenderer>());
        Set(demo, "right", right.GetComponent<ParticleSystemRenderer>());
        Set(demo, "alphaMaterial", alphaMat);
        Set(demo, "additiveMaterial", addMat);
        var b = Buttons(ctx, "좌 Alpha / 우 Add", "모두 Alpha", "모두 Additive");
        for (int i = 0; i < b.Length; i++) OnClick(b[i], demo.SetLayout, i);
        Save(ctx);
    }
}
