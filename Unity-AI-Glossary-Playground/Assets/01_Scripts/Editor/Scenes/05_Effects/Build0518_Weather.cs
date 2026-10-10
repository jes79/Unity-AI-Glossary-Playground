using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0518_Weather
{
    [GlossaryScene("0518")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0518_Weather")]
    public static void Build()
    {
        var ctx = Stage("0518", "WASD : 카메라 이동\nR : 비 ↔ 눈   F : 카메라 따라가기 ON/OFF\n넓은 맵을 돌아다녀도 비가 항상 화면 주변에만 내리고, 바닥에 닿으면 물이 튐",
            new Vector3(0f, 5f, -10f), new Vector3(20f, 0f, 0f), true, 160f);
        ctx.Light.intensity = 0.5f;
        ctx.Camera.farClipPlane = 200f;

        // 카메라 리그: 이 오브젝트를 움직이면 카메라가 따라옴
        var rig = new GameObject("CameraRig").transform;
        ctx.Camera.transform.SetParent(rig, true);

        // 넓은 맵에 기둥을 격자로 (움직이는 게 보이도록)
        var pillar = new Color32(90, 100, 115, 255);
        for (int x = -6; x <= 6; x++)
            for (int z = -6; z <= 6; z++)
                if ((x + z) % 2 == 0)
                    Prim(PrimitiveType.Cube, $"Pillar_{x}_{z}", new Vector3(x * 10f + 3f, 1f, z * 10f + 5f), new Vector3(1f, 2f, 1f), pillar).isStatic = true;

        var rain = Rain("Rain", new Vector3(0f, 10f, 6f), new Vector3(24f, 0.5f, 20f), 1500f, out _);

        // 눈: 느리고 Noise 로 흔들리며, 바닥에 닿으면 잠깐 쌓였다(감속 후 정지) 사라짐
        var snow = PS("Snow", null, new Vector3(0f, 10f, 6f), Glow("Snow", new Color(1f, 1f, 1f, 0.9f), false));
        var m = snow.main;
        m.startLifetime = 8f;
        m.startSpeed = Range(1f, 1.8f);
        m.startSize = Range(0.06f, 0.14f);
        m.maxParticles = 5000;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.playOnAwake = false;
        var em = snow.emission;
        em.rateOverTime = 500f;
        var sh = snow.shape;
        sh.shapeType = ParticleSystemShapeType.Box;
        sh.scale = new Vector3(24f, 20f, 0.5f);
        sh.rotation = new Vector3(90f, 0f, 0f);
        var noise = snow.noise;
        noise.enabled = true; noise.strength = 0.6f; noise.frequency = 0.3f; noise.scrollSpeed = 0.3f;
        var col = snow.collision;
        col.enabled = true; col.type = ParticleSystemCollisionType.World; col.bounce = 0f; col.dampen = 1f; col.lifetimeLoss = 0.3f;
        ColorOverLife(snow, (0f, new Color(1f, 1f, 1f, 0f)), (0.05f, Color.white), (0.9f, Color.white), (1f, new Color(1f, 1f, 1f, 0f)));

        var demo = rig.gameObject.AddComponent<WeatherDemo>();
        Set(demo, "cameraRig", rig);
        Set(demo, "rain", rain);
        Set(demo, "snow", snow);
        var b = Buttons(ctx, "비 ↔ 눈", "따라가기 ON/OFF");
        OnClick(b[0], demo.ToggleWeather);
        OnClick(b[1], demo.ToggleFollow);
        Save(ctx);
    }
}
