using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0516_AuraBuff
{
    [GlossaryScene("0516")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0516_AuraBuff")]
    public static void Build()
    {
        var ctx = Stage("0516", "B / Space : 버프 ON/OFF (10초 지속)\nWASD : 이동 — 마법진과 입자가 캐릭터를 따라다님\n발밑 회전 마법진 + 위로 오르는 입자가 버프 동안 계속 재생",
            new Vector3(0f, 5f, -8f), new Vector3(30f, 0f, 0f));
        ctx.Light.intensity = 0.4f;

        var player = Prim(PrimitiveType.Capsule, "Player", new Vector3(1f, 1f, 0f), Vector3.one, PlayerColor);
        player.tag = "Player";

        // 마법진: 바닥에 눕힌 Quad (가산 Unlit + 생성 텍스처)
        var circle = GameObject.CreatePrimitive(PrimitiveType.Quad);
        circle.name = "MagicCircle";
        NoCollider(circle);
        circle.transform.SetParent(player.transform, false);
        circle.transform.localPosition = new Vector3(0f, -0.97f, 0f);
        circle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        circle.transform.localScale = Vector3.one * 2.8f;
        circle.GetComponent<Renderer>().sharedMaterial = UnlitTex("MagicCircle", MagicCircleTex, new Color(0.4f, 0.85f, 1f, 1f), true);
        circle.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // 위로 오르는 입자: 원 둘레(바닥과 평행)에서 생성 → 위로 1.5m/s
        var rising = PS("RisingParticles", player.transform, new Vector3(0f, -0.9f, 0f), Glow("AuraRise", new Color(0.5f, 0.9f, 1f)));
        var m = rising.main;
        m.startLifetime = Range(1f, 1.6f);
        m.startSpeed = 0f;
        m.startSize = Range(0.06f, 0.14f);
        var em = rising.emission;
        em.rateOverTime = 40f;
        var sh = rising.shape;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius = 1.1f;
        sh.radiusThickness = 0.3f;
        sh.rotation = new Vector3(90f, 0f, 0f);
        var vel = rising.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.y = Range(1.2f, 2f);
        vel.x = Range(0f, 0f); vel.z = Range(0f, 0f); // x/y/z 는 같은 모드(Two Constants)여야 함
        ColorOverLife(rising, (0f, new Color(1f, 1f, 1f, 0f)), (0.2f, Color.white), (1f, new Color(0.4f, 0.6f, 1f, 0f)));
        SizeOverLife(rising, 0f, 1f, 1f, 0.3f);

        var lightGo = new GameObject("AuraLight");
        lightGo.transform.SetParent(player.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, -0.4f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point; light.color = new Color(0.4f, 0.8f, 1f); light.range = 4f; light.intensity = 2.5f;

        var demo = player.AddComponent<AuraBuffDemo>();
        Set(demo, "magicCircle", circle.transform);
        Set(demo, "risingParticles", rising);
        Set(demo, "auraLight", light);
        var b = Buttons(ctx, "버프 ON/OFF");
        OnClick(b[0], demo.ToggleBuff);
        Save(ctx);
    }
}
