using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0509_ParticleCollision
{
    [GlossaryScene("0509")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0509_ParticleCollision")]
    public static void Build()
    {
        var ctx = Stage("0509", "C : Collision 모듈 ON/OFF (OFF 면 바닥을 뚫고 지나감)\nB : 소멸+물튀김 ↔ 튕김(Bounce) 전환\n↑ / ↓ : 비 양 ±200",
            new Vector3(-1f, 5f, -10f), new Vector3(22f, 0f, 0f));
        ctx.Light.intensity = 0.6f;

        // 빗방울이 부딪힐 물체들
        Prim(PrimitiveType.Cube, "Box", new Vector3(-1.5f, 0.5f, 1f), Vector3.one, new Color32(120, 100, 80, 255));
        var plank = Prim(PrimitiveType.Cube, "Plank", new Vector3(2.5f, 1.5f, 0f), new Vector3(3f, 0.1f, 2f), new Color32(140, 110, 80, 255));
        plank.transform.rotation = Quaternion.Euler(0f, 0f, 20f);
        Prim(PrimitiveType.Sphere, "Ball", new Vector3(0.5f, 0.8f, -1f), Vector3.one * 1.6f, new Color32(90, 120, 160, 255));

        var rain = Rain("Rain", new Vector3(0.5f, 9f, 0f), new Vector3(10f, 0.5f, 8f), 600f, out _);
        var demo = rain.gameObject.AddComponent<ParticleCollisionDemo>();
        Set(demo, "rain", rain);
        var b = Buttons(ctx, "Collision ON/OFF", "소멸 ↔ 튕김");
        OnClick(b[0], demo.ToggleCollision);
        OnClick(b[1], demo.ToggleBounce);
        Save(ctx);
    }
}
