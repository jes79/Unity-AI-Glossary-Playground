using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0501_ParticleSystem
{
    [GlossaryScene("0501")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0501_ParticleSystem")]
    public static void Build()
    {
        var ctx = Stage("0501", "1 / 2 / 3 : 불꽃 / 불씨 / 연기 레이어 ON/OFF\n↑ / ↓ : 불꽃 Rate over Time ±10\n모닥불은 파티클 시스템 3개를 겹친 것 — 하나씩 꺼 보며 역할을 비교",
            new Vector3(-1.2f, 2.4f, -6f), new Vector3(12f, 0f, 0f));
        ctx.Light.intensity = 0.25f;

        var fire = new GameObject("Campfire").transform;
        fire.position = new Vector3(0.8f, 0f, 0f);
        var wood = new Color32(90, 55, 35, 255);
        for (int i = 0; i < 4; i++)
        {
            var log = Prim(PrimitiveType.Cylinder, $"Log_{i}", new Vector3(0f, 0.15f, 0f), new Vector3(0.18f, 0.6f, 0.18f), wood, fire);
            log.transform.localRotation = Quaternion.Euler(70f, i * 90f + 20f, 0f);
        }

        // 불꽃: Cone 15°, 수명 0.6~1.0, 속도 1~2, 크기 0.4~0.8, 초당 40개, 가산 블렌딩
        var flame = PS("Flame", fire, new Vector3(0f, 0.2f, 0f), Glow("CampFlame", Color.white));
        var m = flame.main;
        m.startLifetime = Range(0.6f, 1.0f);
        m.startSpeed = Range(1f, 2f);
        m.startSize = Range(0.4f, 0.8f);
        m.startColor = new Color(1f, 0.7f, 0.3f);
        m.maxParticles = 300;
        var em = flame.emission;
        em.rateOverTime = 40f;
        ShapeUp(flame, ParticleSystemShapeType.Cone, 15f, 0.3f);
        ColorOverLife(flame, (0f, new Color(1f, 0.95f, 0.6f, 0f)), (0.15f, new Color(1f, 0.75f, 0.25f, 1f)),
            (0.6f, new Color(1f, 0.3f, 0.05f, 0.6f)), (1f, new Color(0.5f, 0.1f, 0f, 0f)));
        SizeOverLife(flame, 0f, 1f, 1f, 0.3f);

        // 불씨: 작고 오래 살며 Noise 로 흔들림
        var embers = PS("Embers", fire, new Vector3(0f, 0.3f, 0f), Glow("CampEmber", Color.white));
        m = embers.main;
        m.startLifetime = Range(1.5f, 2.5f);
        m.startSpeed = Range(1.5f, 3f);
        m.startSize = Range(0.04f, 0.09f);
        m.startColor = new Color(1f, 0.6f, 0.2f);
        m.gravityModifier = -0.05f;
        em = embers.emission;
        em.rateOverTime = 8f;
        ShapeUp(embers, ParticleSystemShapeType.Cone, 20f, 0.3f);
        var noise = embers.noise;
        noise.enabled = true;
        noise.strength = 0.6f;
        noise.frequency = 0.8f;
        ColorOverLife(embers, (0f, Color.white), (0.7f, new Color(1f, 0.5f, 0.1f, 1f)), (1f, new Color(1f, 0.2f, 0f, 0f)));

        // 연기: 알파 블렌딩, 점점 커지며 옅어짐
        var smoke = PS("Smoke", fire, new Vector3(0f, 1.2f, 0f), ParticleMat("FX05_CampSmoke", Color.white, false, SmokeTex));
        m = smoke.main;
        m.startLifetime = Range(3f, 4f);
        m.startSpeed = Range(0.8f, 1.2f);
        m.startSize = Range(0.6f, 1f);
        m.startRotation = Range(0f, Mathf.PI * 2f);
        m.startColor = new Color(0.35f, 0.35f, 0.38f);
        em = smoke.emission;
        em.rateOverTime = 6f;
        ShapeUp(smoke, ParticleSystemShapeType.Cone, 10f, 0.2f);
        ColorOverLife(smoke, (0f, new Color(1f, 1f, 1f, 0f)), (0.2f, new Color(1f, 1f, 1f, 0.45f)), (1f, new Color(1f, 1f, 1f, 0f)));
        SizeOverLife(smoke, 0f, 1f, 1f, 3f);

        var lightGo = new GameObject("FireLight");
        lightGo.transform.SetParent(fire, false);
        lightGo.transform.localPosition = new Vector3(0f, 0.8f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.6f, 0.25f);
        light.intensity = 3f;
        light.range = 6f;

        var demo = fire.gameObject.AddComponent<CampfireDemo>();
        Set(demo, "flame", flame);
        Set(demo, "embers", embers);
        Set(demo, "smoke", smoke);
        Set(demo, "fireLight", light);

        var b = Buttons(ctx, "불꽃", "불씨", "연기");
        OnClick(b[0], demo.ToggleFlame);
        OnClick(b[1], demo.ToggleEmbers);
        OnClick(b[2], demo.ToggleSmoke);

        InfoCard(ctx, "모닥불 설정값",
            "<b>불꽃</b>  Cone 15° r0.3 / Rate 40\n Lifetime 0.6~1.0  Speed 1~2  Size 0.4~0.8\n Color: 노랑→주황→빨강→투명, Size 1→0.3\n 머티리얼 Additive\n" +
            "<b>불씨</b>  Rate 8 / Lifetime 1.5~2.5 / Size 0.04~0.09\n Gravity -0.05 + Noise 0.6\n" +
            "<b>연기</b>  Rate 6 / Lifetime 3~4 / Size ×3 증가\n 머티리얼 Alpha Blend, 투명→0.45→투명\n" +
            "<b>Point Light</b>  주황, Perlin 노이즈로 깜빡임", 520f, 380f);
        Save(ctx);
    }
}
