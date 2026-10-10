using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0514_MuzzleFlash
{
    [GlossaryScene("0514")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0514_MuzzleFlash")]
    public static void Build()
    {
        var ctx = Stage("0514", "Space / 마우스 왼쪽 (누르고 있으면 연사) : 발사\nF : 섬광 ON/OFF   L : Point Light ON/OFF\n빛을 끄면 벽·바닥이 안 밝아져 발사감이 크게 줄어드는 것을 비교",
            new Vector3(0f, 2.2f, -6f), new Vector3(10f, 0f, 0f));
        ctx.Light.intensity = 0.15f;

        // 뒷벽 (빛 받는 면)
        Prim(PrimitiveType.Cube, "Wall", new Vector3(0f, 2f, 2.5f), new Vector3(12f, 4f, 0.3f), new Color32(110, 110, 120, 255));

        // 총: 오른쪽(+X)을 향함
        var gun = new GameObject("Gun").transform;
        gun.position = new Vector3(-1f, 1.2f, 0f);
        gun.rotation = Quaternion.Euler(0f, 90f, 0f);
        var dark = new Color32(40, 42, 48, 255);
        Prim(PrimitiveType.Cube, "Body", new Vector3(0f, 0f, 0f), new Vector3(0.18f, 0.25f, 0.8f), dark, gun);
        Prim(PrimitiveType.Cube, "Barrel", new Vector3(0f, 0.05f, 0.65f), new Vector3(0.08f, 0.08f, 0.6f), dark, gun);
        Prim(PrimitiveType.Cube, "Grip", new Vector3(0f, -0.25f, -0.2f), new Vector3(0.14f, 0.35f, 0.15f), dark, gun);

        var muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(gun, false);
        muzzle.localPosition = new Vector3(0f, 0.05f, 1f);

        // 섬광: 별 모양 1개(정면) + 원뿔 방향 작은 불꽃 6개, 수명 0.05초
        var flash = PS("MuzzleFlash", muzzle, Vector3.zero, ParticleMat("FX05_MuzzleStar", new Color(1f, 0.8f, 0.45f), true, StarTex), false, false);
        var m = flash.main;
        m.duration = 0.05f;
        m.startLifetime = 0.05f;
        m.startSpeed = 0f;
        m.startSize = Range(0.6f, 1f);
        m.startRotation = Range(0f, Mathf.PI * 2f);
        m.simulationSpace = ParticleSystemSimulationSpace.Local;
        Burst(flash, 1);
        var fs = flash.shape;
        fs.enabled = false;

        var cone = PS("MuzzleCone", flash.transform, Vector3.zero, Glow("MuzzleCone", new Color(1f, 0.7f, 0.3f)), false, false);
        m = cone.main;
        m.duration = 0.05f;
        m.startLifetime = 0.05f;
        m.startSpeed = Range(8f, 14f);
        m.startSize = Range(0.15f, 0.3f);
        Burst(cone, 6);
        var cs = cone.shape;
        cs.shapeType = ParticleSystemShapeType.Cone;
        cs.angle = 12f;
        cs.radius = 0.02f;
        Stretched(cone, 0.05f, 1.5f);

        var lightGo = new GameObject("MuzzleLight");
        lightGo.transform.SetParent(muzzle, false);
        lightGo.transform.localPosition = new Vector3(0f, 0f, 0.2f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.75f, 0.4f);
        light.range = 6f;
        light.intensity = 8f;
        light.enabled = false;

        var audio = gun.gameObject.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.clip = CreateToneClip("FX05_Shot", 400f, 0.1f, Wave.Noise, 0.35f, 80f, 0.001f, 0.08f);

        var demo = gun.gameObject.AddComponent<MuzzleFlashDemo>();
        Set(demo, "flash", flash);
        Set(demo, "muzzleLight", light);
        Set(demo, "gun", gun);
        Set(demo, "shotSound", audio);
        var b = Buttons(ctx, "발사", "섬광 ON/OFF", "Light ON/OFF");
        OnClick(b[0], demo.Fire);
        OnClick(b[1], demo.ToggleFlash);
        OnClick(b[2], demo.ToggleLight);
        Save(ctx);
    }
}
