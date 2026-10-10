using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0513_HitEffect
{
    [GlossaryScene("0513")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0513_HitEffect")]
    public static void Build()
    {
        var ctx = Stage("0513", "Space / 표적 클릭 : 타격\n1 섬광   2 파편 Burst   3 히트스톱 0.05초   4 카메라 쉐이크 (각각 ON/OFF)\n하나씩 끄고 때려 보며 타격감 차이를 비교",
            new Vector3(-0.5f, 2.2f, -6f), new Vector3(8f, 0f, 0f));

        var dummy = Prim(PrimitiveType.Capsule, "Dummy", new Vector3(1f, 1f, 0f), new Vector3(0.9f, 1f, 0.9f), EnemyColor);

        // 섬광: 별 모양 큰 입자 1개, 0.08초
        var flash = PS("HitFlash", null, new Vector3(1f, 1.3f, -0.5f), ParticleMat("FX05_HitFlash", Color.white, true, StarTex), false, false);
        var m = flash.main;
        m.duration = 0.1f;
        m.startLifetime = 0.08f;
        m.startSpeed = 0f;
        m.startSize = Range(1.6f, 2.2f);
        m.startRotation = Range(0f, Mathf.PI * 2f);
        Burst(flash, 1);
        var fs = flash.shape;
        fs.enabled = false;
        SizeOverLife(flash, 0f, 0.6f, 0.3f, 1f, 1f, 0.8f);

        // 파편: 20개 Burst, 길쭉한 스파크 + 중력
        var debris = PS("HitDebris", null, new Vector3(1f, 1.3f, -0.5f), Glow("HitDebris", new Color(1f, 0.85f, 0.5f)), false, false);
        m = debris.main;
        m.duration = 0.5f;
        m.startLifetime = Range(0.25f, 0.5f);
        m.startSpeed = Range(5f, 10f);
        m.startSize = Range(0.05f, 0.1f);
        m.gravityModifier = 1.5f;
        Burst(debris, 20);
        var ds = debris.shape;
        ds.shapeType = ParticleSystemShapeType.Cone;
        ds.angle = 55f;
        ds.radius = 0.05f;
        Stretched(debris, 0.04f, 1.5f);
        ColorOverLife(debris, (0f, Color.white), (1f, new Color(1f, 0.4f, 0.1f, 0f)));

        var shake = ctx.Camera.gameObject.AddComponent<EffectsCameraShake>();
        var audio = dummy.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.clip = CreateToneClip("FX05_HitSound", 180f, 0.12f, Wave.Noise, 0.5f, 60f, 0.002f, 0.08f);

        var demo = dummy.AddComponent<HitEffectDemo>();
        Set(demo, "target", dummy.GetComponent<Renderer>());
        Set(demo, "flash", flash);
        Set(demo, "debris", debris);
        Set(demo, "cameraShake", shake);
        Set(demo, "hitSound", audio);

        var b = Buttons(ctx, "타격", "섬광", "파편", "히트스톱", "쉐이크");
        OnClick(b[0], demo.HitFront);
        OnClick(b[1], demo.ToggleFlash);
        OnClick(b[2], demo.ToggleDebris);
        OnClick(b[3], demo.ToggleHitStop);
        OnClick(b[4], demo.ToggleShake);
        Save(ctx);
    }
}
