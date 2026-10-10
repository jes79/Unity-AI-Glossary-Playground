using GlossaryDemo.AudioDemo;
using UnityEditor;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0904_SpatialSound
{
    [GlossaryScene("0904")]
    [MenuItem("Tools/Glossary/Build Scene/09 Audio/0904_SpatialSound")]
    public static void Build()
    {
        var ctx = NewScene3D("0904", "WASD : 이동 (귀 = 플레이어)\n1 : 3D ↔ 2D   2 : Linear ↔ Logarithmic 감쇠\n노란 원(10m) 밖으로 나가면 모닥불 소리가 완전히 사라집니다. 좌우로 돌면 소리 방향도 바뀝니다");
        var canvas = EnsureCanvas(ctx);

        // ── 모닥불 ──
        var fire = new GameObject("Campfire");
        for (int i = 0; i < 4; i++)
        {
            var log = Visual(PrimitiveType.Cylinder, fire.transform, new Vector3(0f, 0.15f, 0f), new Vector3(0.18f, 0.6f, 0.18f), new Color(0.4f, 0.25f, 0.12f), $"Log_{i}");
            log.transform.localRotation = Quaternion.Euler(80f, i * 45f, 0f);
        }
        var flameMat = UnlitMat("0904_Flame", new Color(1f, 0.55f, 0.15f));
        var flame = Visual(PrimitiveType.Sphere, fire.transform, new Vector3(0f, 0.55f, 0f), new Vector3(0.6f, 0.9f, 0.6f), Color.white, "Flame");
        flame.GetComponent<Renderer>().sharedMaterial = flameMat;
        var lightGo = new GameObject("FireLight");
        lightGo.transform.SetParent(fire.transform, false);
        lightGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.6f, 0.25f);
        light.range = 8f;
        light.intensity = 8f;

        // 모닥불 소리: 3D, Linear, 1m ~ 10m
        var crackle = CreateToneClip("0904_FireCrackle", 100f, 2f, Wave.Noise, 0.3f, attack: 0.01f, release: 0.01f);
        var src = fire.AddComponent<AudioSource>();
        src.clip = crackle;
        src.loop = true;
        src.playOnAwake = true;
        src.spatialBlend = 1f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 1f;
        src.maxDistance = 10f;
        src.dopplerLevel = 0f;

        // 10m / 5m 원
        Ring("Ring_10m", 10f, new Color(1f, 0.85f, 0.2f), 0.08f);
        Ring("Ring_5m", 5f, new Color(1f, 0.85f, 0.2f, 0.5f), 0.04f);

        // ── 플레이어 = 귀 (AudioListener 를 카메라에서 플레이어로 옮김) ──
        Object.DestroyImmediate(ctx.Camera.GetComponent<AudioListener>());
        var player = new GameObject("Player (AudioListener)");
        player.transform.position = new Vector3(0f, 0f, -4f);
        Visual(PrimitiveType.Capsule, player.transform, new Vector3(0f, 1f, 0f), Vector3.one, PlayerColor, "Body");
        Visual(PrimitiveType.Cube, player.transform, new Vector3(0f, 1.5f, 0.4f), new Vector3(0.5f, 0.15f, 0.2f), new Color(0.1f, 0.1f, 0.15f), "Visor");
        // 귀 표시: 양옆 작은 구 (왼쪽/오른쪽 스피커)
        Visual(PrimitiveType.Sphere, player.transform, new Vector3(-0.55f, 1.4f, 0f), Vector3.one * 0.2f, SkinColor, "EarL");
        Visual(PrimitiveType.Sphere, player.transform, new Vector3(0.55f, 1.4f, 0f), Vector3.one * 0.2f, SkinColor, "EarR");
        player.AddComponent<AudioListener>();

        var demo = player.AddComponent<SpatialSoundDemo>();
        Set(demo, "fireSource", src);
        Set(demo, "fireLight", light);
        Set(demo, "cameraTransform", ctx.Camera.transform);
        ctx.Light.intensity = 0.4f; // 밤 분위기

        ExplainPanel(canvas, "3D 사운드 설정 (AudioSource)",
            "• <b>Spatial Blend</b> = 1 (3D)\n" +
            "• 3D Sound Settings → Volume Rolloff = <b>Linear Rolloff</b>\n" +
            "• Min Distance 1 (이 안은 최대 볼륨)\n" +
            "• <b>Max Distance 10</b> (Linear 는 여기서 0)\n" +
            "  ※ Logarithmic 은 Max 밖에서도 작게 들림 → '안 들리게'는 Linear\n" +
            "     또는 Custom Rolloff 커브 끝을 0 으로\n" +
            "• AudioListener(귀)는 씬에 하나. 3인칭이면 보통 카메라,\n  여기서는 거리 체감을 위해 플레이어에 붙임\n" +
            "• Doppler Level 0 : 움직일 때 음높이 변화 끔", 560f, 360f);

        Save(ctx);
    }

    private static void Ring(string name, float radius, Color color, float width)
    {
        var go = new GameObject(name);
        var lr = go.AddComponent<LineRenderer>();
        lr.loop = true;
        lr.useWorldSpace = true;
        lr.widthMultiplier = width * 2f;
        const int n = 96;
        lr.positionCount = n;
        for (int i = 0; i < n; i++)
        {
            float a = i / (float)n * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0.03f, Mathf.Sin(a) * radius));
        }
        var mat = UnlitMat($"0904_{name}", color);
        if (color.a < 1f) MakeTransparentUnlit(mat);
        lr.sharedMaterial = mat;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private static void MakeTransparentUnlit(Material m)
    {
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 0f);
        m.SetFloat("_ZWrite", 0f);
        m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        m.SetOverrideTag("RenderType", "Transparent");
        EditorUtility.SetDirty(m);
    }
}
