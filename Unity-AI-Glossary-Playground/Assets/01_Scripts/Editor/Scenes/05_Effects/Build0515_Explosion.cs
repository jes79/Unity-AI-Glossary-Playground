using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0515_Explosion
{
    [GlossaryScene("0515")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0515_Explosion")]
    public static void Build()
    {
        var ctx = Stage("0515", "Space : 가운데 폭발   바닥 클릭 : 그 자리 폭발\n1 섬광  2 화염 구  3 연기  4 파편  5 충격파 링 (레이어별 ON/OFF)\n레이어를 하나씩 빼 보며 각자의 역할을 확인",
            new Vector3(0f, 6f, -12f), new Vector3(22f, 0f, 0f));
        ctx.Light.intensity = 0.5f;

        var root = new GameObject("Explosion").transform;
        root.position = new Vector3(1f, 0.8f, 0f);

        // 1 섬광: 0.1초, 아주 크게
        var flash = PS("1_Flash", root, Vector3.zero, ParticleMat("FX05_ExpFlash", new Color(1f, 0.95f, 0.8f), true, StarTex), false, false);
        var m = flash.main;
        m.duration = 0.2f; m.startLifetime = 0.12f; m.startSpeed = 0f; m.startSize = 7f;
        m.startRotation = Range(0f, Mathf.PI * 2f);
        Burst(flash, 1);
        var sh = flash.shape; sh.enabled = false;
        ColorOverLife(flash, (0f, Color.white), (1f, new Color(1f, 0.8f, 0.5f, 0f)));

        // 2 화염 구: 가산, 흰→노랑→주황→암적→투명, 커졌다 줄어듦
        var fire = PS("2_Fireball", root, Vector3.zero, ParticleMat("FX05_ExpFire", Color.white, true, SmokeTex), false, false);
        m = fire.main;
        m.duration = 0.5f; m.startLifetime = Range(0.5f, 0.9f); m.startSpeed = Range(1f, 3f); m.startSize = Range(1.5f, 2.5f);
        m.startRotation = Range(0f, Mathf.PI * 2f);
        Burst(fire, 18);
        sh = fire.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.5f;
        ColorOverLife(fire, (0f, new Color(1f, 1f, 0.9f, 1f)), (0.2f, new Color(1f, 0.8f, 0.3f, 1f)), (0.5f, new Color(1f, 0.4f, 0.1f, 0.8f)), (1f, new Color(0.4f, 0.05f, 0f, 0f)));
        SizeOverLife(fire, 0f, 0.6f, 0.3f, 1f, 1f, 0.7f);

        // 3 연기: 알파, 0.15초 늦게 시작, 위로 천천히 오르며 커짐
        var smoke = PS("3_Smoke", root, Vector3.zero, ParticleMat("FX05_ExpSmoke", Color.white, false, SmokeTex), false, false);
        m = smoke.main;
        m.duration = 1f; m.startLifetime = Range(2f, 3f); m.startSpeed = Range(0.5f, 1.5f); m.startSize = Range(1.5f, 2.5f);
        m.startRotation = Range(0f, Mathf.PI * 2f);
        m.startColor = new Color(0.25f, 0.24f, 0.24f);
        Burst(smoke, 14, 0.15f);
        sh = smoke.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.8f;
        var vel = smoke.velocityOverLifetime;
        vel.enabled = true; vel.x = 0f; vel.y = 0.8f; vel.z = 0f;
        ColorOverLife(smoke, (0f, new Color(1f, 1f, 1f, 0f)), (0.15f, new Color(1f, 1f, 1f, 0.8f)), (1f, new Color(1f, 1f, 1f, 0f)));
        SizeOverLife(smoke, 0f, 0.7f, 1f, 2f);

        // 4 파편: 길쭉한 불똥, 중력 + 바닥 충돌
        var debris = PS("4_Debris", root, Vector3.zero, Glow("ExpDebris", new Color(1f, 0.7f, 0.3f)), false, false);
        m = debris.main;
        m.duration = 0.5f; m.startLifetime = Range(1f, 1.6f); m.startSpeed = Range(6f, 13f); m.startSize = Range(0.06f, 0.12f);
        m.gravityModifier = 2f;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        Burst(debris, 30);
        ShapeUp(debris, ParticleSystemShapeType.Hemisphere, 0f, 0.3f); // 반구를 위로 향하게
        Stretched(debris, 0.04f, 1.2f);
        var col = debris.collision;
        col.enabled = true; col.type = ParticleSystemCollisionType.World; col.bounce = 0.3f; col.dampen = 0.4f;
        ColorOverLife(debris, (0f, Color.white), (0.7f, new Color(1f, 0.5f, 0.1f, 1f)), (1f, new Color(1f, 0.2f, 0f, 0f)));

        // 5 충격파 링: 바닥과 평행(Horizontal Billboard), 0.4초 동안 1 → 10m 로 퍼지며 사라짐
        var ring = PS("5_Shockwave", root, new Vector3(0f, -0.7f, 0f), ParticleMat("FX05_ExpRing", new Color(1f, 0.85f, 0.6f), true, RingTex), false, false);
        m = ring.main;
        m.duration = 0.5f; m.startLifetime = 0.45f; m.startSpeed = 0f; m.startSize = 10f;
        Burst(ring, 1);
        sh = ring.shape; sh.enabled = false;
        ring.GetComponent<ParticleSystemRenderer>().renderMode = ParticleSystemRenderMode.HorizontalBillboard;
        SizeOverLife(ring, 0f, 0.1f, 1f, 1f);
        ColorOverLife(ring, (0f, Color.white), (1f, new Color(1f, 1f, 1f, 0f)));

        var lightGo = new GameObject("FlashLight");
        lightGo.transform.SetParent(root, false);
        lightGo.transform.localPosition = Vector3.up * 0.5f;
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point; light.color = new Color(1f, 0.7f, 0.35f); light.range = 12f; light.intensity = 0f;

        // 주변 소품 (파편이 튕기고 빛을 받는 대상)
        for (int i = 0; i < 6; i++)
        {
            float a = i * Mathf.PI / 3f;
            Prim(PrimitiveType.Cube, $"Crate_{i}", new Vector3(1f + Mathf.Cos(a) * 4.5f, 0.5f, Mathf.Sin(a) * 4.5f), Vector3.one, new Color32(130, 100, 70, 255));
        }

        var shake = ctx.Camera.gameObject.AddComponent<EffectsCameraShake>();
        var audio = root.gameObject.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.clip = CreateToneClip("FX05_Boom", 120f, 0.8f, Wave.Noise, 0.6f, 30f, 0.005f, 0.6f);

        var demo = root.gameObject.AddComponent<ExplosionDemo>();
        SetArray(demo, "layers", flash, fire, smoke, debris, ring);
        Set(demo, "flashLight", light);
        Set(demo, "cameraShake", shake);
        Set(demo, "boomSound", audio);

        var b = Buttons(ctx, "폭발!", "1 섬광", "2 화염", "3 연기", "4 파편", "5 링");
        OnClick(b[0], demo.ExplodeCenter);
        for (int i = 0; i < 5; i++) OnClick(b[i + 1], demo.ToggleLayer, i);
        Save(ctx);
    }
}
