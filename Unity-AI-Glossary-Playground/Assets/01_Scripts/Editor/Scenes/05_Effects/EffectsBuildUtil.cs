using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static GlossaryKit;
using Object = UnityEngine.Object;

// 05_Effects 씬 빌더 공통 헬퍼
// - 어두운 이펙트 스테이지, 파티클 생성/커브/그라디언트, 생성 텍스처(글로우·링·연기·폭발 시트 등)
// - Volume 프로파일 생성, Decal 렌더러 복제, 화면 오른쪽 설명 카드
public static class EffectsBuildUtil
{
    public const string Sub = "Effects";
    public static readonly Color StageBg = new Color32(14, 16, 22, 255);

    // ───────────────────────── 스테이지 ─────────────────────────

    public static SceneContext Stage(string id, string controls, Vector3 camPos, Vector3 camEuler, bool ground = true, float groundSize = 30f)
    {
        var ctx = NewScene3D(id, controls, ground, groundSize);
        ctx.Camera.clearFlags = CameraClearFlags.SolidColor;
        ctx.Camera.backgroundColor = StageBg;
        ctx.Camera.transform.position = camPos;
        ctx.Camera.transform.rotation = Quaternion.Euler(camEuler);
        if (ctx.Ground != null) ctx.Ground.GetComponent<Renderer>().sharedMaterial = ColorMat(new Color32(48, 52, 62, 255));
        return ctx;
    }

    public static Canvas EnsureCanvas(SceneContext ctx)
    {
        if (ctx.Canvas == null) ctx.Canvas = CreateCanvas("Canvas");
        return ctx.Canvas;
    }

    public static Button[] Buttons(SceneContext ctx, params string[] labels) => CreateButtonRow(EnsureCanvas(ctx).transform, 60f, labels);

    // 화면 오른쪽 아래에 쌓는 슬라이더 (index 0 이 가장 아래)
    public static Slider RightSlider(SceneContext ctx, int index, string label, float min, float max, float value)
    {
        var s = CreateSlider($"Slider_{index}", EnsureCanvas(ctx).transform, new Vector2(-230f, 170f + index * 80f), 380f, min, max, value, label);
        AnchorBottomRight((RectTransform)s.transform);
        return s;
    }

    // 오른쪽 위 설명 카드 (실제 설정 방법, 노드 레시피 등)
    public static TextMeshProUGUI InfoCard(SceneContext ctx, string title, string body, float width = 560f, float height = 420f)
    {
        var bg = CreateImage("InfoCard", EnsureCanvas(ctx).transform, new Vector2(-width * 0.5f - 16f, -height * 0.5f - 16f), new Vector2(width, height),
            new Color(0.07f, 0.08f, 0.1f, 0.86f));
        AnchorTopRight(bg.rectTransform);
        bg.raycastTarget = false;
        var t = CreateText("Text", bg.transform, Vector2.zero, Vector2.zero, $"<color=#FFD966><b>{title}</b></color>\n{body}", 19f, TextColor,
            TextAlignmentOptions.TopLeft);
        Stretch(t.rectTransform, 16f);
        return t;
    }

    // 월드 공간 라벨 (TextMeshPro 3D)
    public static TextMeshPro Label3D(string text, Vector3 pos, float size = 3f, Transform parent = null)
    {
        var go = new GameObject($"Label_{text}");
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.font = GlossaryKit.Font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = TextColor;
        tmp.rectTransform.sizeDelta = new Vector2(8f, 1.5f);
        return tmp;
    }

    public static void NoCollider(GameObject go)
    {
        var c = go.GetComponent<Collider>();
        if (c != null) Object.DestroyImmediate(c);
    }

    // ───────────────────────── 파티클 ─────────────────────────

    // 파티클 시스템 생성. 기본 모양(Cone)은 +Z 로 뿜으므로 위로 뿜을 땐 ShapeUp() 사용
    public static ParticleSystem PS(string name, Transform parent, Vector3 localPos, Material mat, bool playOnAwake = true, bool loop = true)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = playOnAwake;
        main.loop = loop;
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = mat;
        r.renderMode = ParticleSystemRenderMode.Billboard;
        return ps;
    }

    public static void ShapeUp(ParticleSystem ps, ParticleSystemShapeType type, float angle = 25f, float radius = 0.3f)
    {
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = type;
        shape.angle = angle;
        shape.radius = radius;
        shape.rotation = new Vector3(-90f, 0f, 0f); // +Z → +Y (위)
    }

    public static void Burst(ParticleSystem ps, int count, float time = 0f)
    {
        var em = ps.emission;
        em.enabled = true;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(time, (short)count) });
    }

    public static ParticleSystem.MinMaxCurve Range(float a, float b) => new ParticleSystem.MinMaxCurve(a, b);

    public static AnimationCurve Curve(params float[] timeValuePairs)
    {
        var keys = new Keyframe[timeValuePairs.Length / 2];
        for (int i = 0; i < keys.Length; i++) keys[i] = new Keyframe(timeValuePairs[i * 2], timeValuePairs[i * 2 + 1]);
        var c = new AnimationCurve(keys);
        for (int i = 0; i < keys.Length; i++) c.SmoothTangents(i, 0f);
        return c;
    }

    public static ParticleSystem.MinMaxCurve CurveMM(float multiplier, params float[] timeValuePairs) =>
        new ParticleSystem.MinMaxCurve(multiplier, Curve(timeValuePairs));

    // (시간, 색) 쌍으로 그라디언트. 알파는 색의 a 를 사용
    public static Gradient Grad(params (float t, Color c)[] keys)
    {
        var g = new Gradient();
        var ck = new GradientColorKey[keys.Length];
        var ak = new GradientAlphaKey[keys.Length];
        for (int i = 0; i < keys.Length; i++)
        {
            ck[i] = new GradientColorKey(keys[i].c, keys[i].t);
            ak[i] = new GradientAlphaKey(keys[i].c.a, keys[i].t);
        }
        g.SetKeys(ck, ak);
        return g;
    }

    public static void ColorOverLife(ParticleSystem ps, params (float t, Color c)[] keys)
    {
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(Grad(keys));
    }

    public static void SizeOverLife(ParticleSystem ps, params float[] timeValuePairs)
    {
        var s = ps.sizeOverLifetime;
        s.enabled = true;
        s.size = CurveMM(1f, timeValuePairs);
    }

    public static void Stretched(ParticleSystem ps, float velocityScale, float lengthScale)
    {
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Stretch;
        r.velocityScale = velocityScale;
        r.lengthScale = lengthScale;
    }

    // 빗방울(Stretched Billboard) + Collision 모듈 + 충돌 시 물튀김 Sub Emitter (0509, 0518 공용)
    public static ParticleSystem Rain(string name, Vector3 pos, Vector3 area, float rate, out ParticleSystem splash)
    {
        var rain = PS(name, null, pos, Glow("RainDrop", new Color(0.7f, 0.85f, 1f, 0.7f), false));
        var m = rain.main;
        m.startLifetime = 2f;
        m.startSpeed = Range(14f, 16f);
        m.startSize = Range(0.03f, 0.05f);
        m.maxParticles = 5000;
        m.simulationSpace = ParticleSystemSimulationSpace.World; // 충돌·카메라 추적 모두 월드 기준이어야 자연스러움
        var em = rain.emission;
        em.rateOverTime = rate;
        var shape = rain.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(area.x, area.z, area.y);
        shape.rotation = new Vector3(90f, 0f, 0f); // 상자의 +Z 를 아래(-Y)로 → 아래로 떨어짐
        Stretched(rain, 0.03f, 1f);

        var col = rain.collision;
        col.enabled = true;
        col.type = ParticleSystemCollisionType.World;
        col.mode = ParticleSystemCollisionMode.Collision3D;
        col.bounce = 0f;
        col.lifetimeLoss = 1f; // 닿으면 바로 죽음
        col.quality = ParticleSystemCollisionQuality.High;
        col.collidesWith = ~0;

        splash = PS("Splash", rain.transform, Vector3.zero, Glow("RainSplash", new Color(0.75f, 0.9f, 1f, 0.8f), false), false, false);
        m = splash.main;
        m.startLifetime = Range(0.25f, 0.45f);
        m.startSpeed = Range(1.5f, 3f);
        m.startSize = Range(0.03f, 0.06f);
        m.gravityModifier = 1f;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.maxParticles = 3000;
        Burst(splash, 5);
        var ss = splash.shape;
        ss.shapeType = ParticleSystemShapeType.Cone;
        ss.angle = 40f;
        ss.radius = 0.02f;
        ss.rotation = new Vector3(-90f, 0f, 0f);
        ColorOverLife(splash, (0f, Color.white), (1f, new Color(1f, 1f, 1f, 0f)));

        var sub = rain.subEmitters;
        sub.enabled = true;
        sub.AddSubEmitter(splash, ParticleSystemSubEmitterType.Collision, ParticleSystemSubEmitterProperties.InheritNothing);
        return rain;
    }

    // ───────────────────────── 머티리얼 ─────────────────────────

    public static Material Glow(string name, Color color, bool additive = true) => ParticleMat($"FX05_{name}", color, additive, GlowTex);

    public static Material EmissiveMat(string name, Color baseColor, Color emission, float intensity)
    {
        return CreateMaterial($"FX05_{name}", "Universal Render Pipeline/Lit", m =>
        {
            m.SetColor("_BaseColor", baseColor);
            m.SetFloat("_Smoothness", 0.5f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission * intensity); // HDR: 1 보다 큰 밝기 → Bloom 대상
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        });
    }

    // 메시(Quad 등)에 텍스처를 붙이는 반투명/가산 Unlit 머티리얼 (버텍스 컬러 불필요)
    public static Material UnlitTex(string name, Texture tex, Color color, bool additive)
    {
        return CreateMaterial($"FX05_{name}", "Universal Render Pipeline/Unlit", m =>
        {
            m.SetTexture("_BaseMap", tex);
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", additive ? 2f : 0f);
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_Cull", 0f);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
            m.SetOverrideTag("RenderType", "Transparent");
        });
    }

    // ───────────────────────── 생성 텍스처 ─────────────────────────

    public static Texture2D Tex(string name, int size, Func<float, float, Color> pixel) => Tex(name, size, size, pixel);

    public static Texture2D Tex(string name, int w, int h, Func<float, float, Color> pixel, bool sRGB = true)
    {
        string path = $"{GeneratedFolder}/Textures/{name}.png";
        var t = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        return t != null ? t : CreateTexture(name, w, h, pixel, false, TextureWrapMode.Clamp, sRGB);
    }

    private static float Len(float u, float v) => Mathf.Sqrt((u - 0.5f) * (u - 0.5f) + (v - 0.5f) * (v - 0.5f)) * 2f;
    private static Color White(float a) => new Color(1f, 1f, 1f, Mathf.Clamp01(a));

    // 가운데가 밝은 둥근 빛
    public static Texture2D GlowTex => Tex("FX05_Glow", 64, (u, v) =>
    {
        float d = Len(u, v);
        float a = Mathf.Pow(Mathf.Clamp01(1f - d), 1.6f) + Mathf.Exp(-d * d * 30f) * 0.5f;
        return White(a);
    });

    // 4갈래 별 (섬광, 머즐 플래시)
    public static Texture2D StarTex => Tex("FX05_Star", 128, (u, v) =>
    {
        float x = Mathf.Abs(u * 2f - 1f), y = Mathf.Abs(v * 2f - 1f), d = Len(u, v);
        float rays = Mathf.Clamp01(1f - y * 14f) * Mathf.Clamp01(1f - x) + Mathf.Clamp01(1f - x * 14f) * Mathf.Clamp01(1f - y);
        float core = Mathf.Pow(Mathf.Clamp01(1f - d * 1.6f), 2f);
        return White(core + rays * 0.9f);
    });

    // 충격파 링
    public static Texture2D RingTex => Tex("FX05_Ring", 128, (u, v) =>
    {
        float d = Len(u, v);
        float a = d > 1f ? 0f : Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(d - 0.82f) / 0.14f), 2f);
        return White(a);
    });

    // 연기 덩어리 (노이즈로 가장자리가 울퉁불퉁)
    public static Texture2D SmokeTex => Tex("FX05_Smoke", 128, (u, v) =>
    {
        float d = Len(u, v);
        float n = Fbm(u * 5f, v * 5f, 7);
        float a = Mathf.Pow(Mathf.Clamp01(1f - d), 1.2f) * (0.35f + 0.9f * n);
        return White(a);
    });

    // 트레일/레이저용: 폭 방향(v)으로 가운데가 진함
    public static Texture2D BeamTex => Tex("FX05_Beam", 64, (u, v) =>
    {
        float k = Mathf.Abs(v * 2f - 1f);
        return White(Mathf.Pow(1f - k, 1.5f) + Mathf.Exp(-k * k * 40f) * 0.5f);
    });

    // 힐 이펙트용 + 모양
    public static Texture2D CrossTex => Tex("FX05_Cross", 64, (u, v) =>
    {
        float x = Mathf.Abs(u * 2f - 1f), y = Mathf.Abs(v * 2f - 1f);
        float bar1 = Mathf.Clamp01((0.24f - x) * 20f) * Mathf.Clamp01((0.75f - y) * 20f);
        float bar2 = Mathf.Clamp01((0.24f - y) * 20f) * Mathf.Clamp01((0.75f - x) * 20f);
        return White(Mathf.Max(bar1, bar2));
    });

    // 총알 자국: 검은 구멍 + 그을음 + 방사형 금
    public static Texture2D BulletHoleTex => Tex("FX05_BulletHole", 128, (u, v) =>
    {
        float x = u * 2f - 1f, y = v * 2f - 1f, d = Mathf.Sqrt(x * x + y * y);
        float ang = Mathf.Atan2(y, x);
        float crack = Mathf.Pow(Mathf.Abs(Mathf.Cos(ang * 3.5f + Noise(d * 3f, ang, 3) * 2f)), 40f) * Mathf.Clamp01(1f - d) * (d > 0.15f ? 1f : 0f);
        float hole = Mathf.Clamp01((0.2f - d) * 30f);
        float scorch = Mathf.Clamp01(1f - d / 0.6f) * 0.7f;
        float a = Mathf.Clamp01(Mathf.Max(hole, Mathf.Max(scorch, crack * 0.9f)));
        float shade = Mathf.Lerp(0.18f, 0.02f, hole);
        return new Color(shade, shade * 0.95f, shade * 0.9f, a);
    });

    // 마법진: 동심원 + 육망성 + 눈금
    public static Texture2D MagicCircleTex => Tex("FX05_MagicCircle", 256, (u, v) =>
    {
        float x = u * 2f - 1f, y = v * 2f - 1f, d = Mathf.Sqrt(x * x + y * y);
        float a = 0f;
        a = Mathf.Max(a, Line(Mathf.Abs(d - 0.95f), 0.018f));
        a = Mathf.Max(a, Line(Mathf.Abs(d - 0.82f), 0.012f));
        a = Mathf.Max(a, Line(Mathf.Abs(d - 0.5f), 0.012f));
        // 바깥 고리 사이 눈금(룬 자리)
        float ang = Mathf.Atan2(y, x);
        if (d > 0.84f && d < 0.93f) a = Mathf.Max(a, Mathf.Pow(Mathf.Abs(Mathf.Sin(ang * 24f)), 12f) * 0.9f);
        // 육망성: 반지름 0.82 위의 정삼각형 두 개
        for (int tri = 0; tri < 2; tri++)
            for (int i = 0; i < 3; i++)
            {
                float a0 = Mathf.PI * 0.5f + tri * Mathf.PI / 3f + i * Mathf.PI * 2f / 3f;
                float a1 = a0 + Mathf.PI * 2f / 3f;
                var p0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * 0.82f;
                var p1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * 0.82f;
                a = Mathf.Max(a, Line(SegmentDistance(new Vector2(x, y), p0, p1), 0.012f));
            }
        a = Mathf.Max(a, Mathf.Clamp01(1f - d) * 0.15f); // 은은한 바탕
        return White(d > 1f ? 0f : a);
    });

    // 폭발 스프라이트 시트 4x4 (16프레임). 프레임 0 = 왼쪽 위, 오른쪽→아래 순서 (Texture Sheet Animation 규칙)
    public static Texture2D ExplosionSheetTex => Tex("FX05_ExplosionSheet4x4", 512, (u, v) =>
    {
        int col = Mathf.Min(3, (int)(u * 4f));
        int row = 3 - Mathf.Min(3, (int)(v * 4f));
        int frame = row * 4 + col;
        float k = frame / 15f;
        float lx = (u * 4f - col) * 2f - 1f;
        float ly = (v * 4f - (3 - row)) * 2f - 1f;
        float len = Mathf.Sqrt(lx * lx + ly * ly);
        float n = Fbm(lx * 2.5f + frame * 0.37f, ly * 2.5f - frame * 0.21f, 11);
        float radius = 0.3f + 0.6f * Mathf.Sqrt(k);
        float d = len / (radius * (0.75f + 0.5f * n));
        float body = Mathf.Clamp01((1f - d) * 3f);
        float heat = Mathf.Clamp01((1f - d) * 1.6f) * Mathf.Clamp01(1.3f - k * 1.5f);
        Color fire = heat > 0.66f ? Color.Lerp(new Color(1f, 0.85f, 0.4f), Color.white, (heat - 0.66f) * 3f)
            : heat > 0.33f ? Color.Lerp(new Color(1f, 0.45f, 0.1f), new Color(1f, 0.85f, 0.4f), (heat - 0.33f) * 3f)
            : Color.Lerp(new Color(0.35f, 0.08f, 0.03f), new Color(1f, 0.45f, 0.1f), heat * 3f);
        Color smoke = new Color(0.22f, 0.2f, 0.2f) * (0.7f + 0.6f * n);
        float fireMix = Mathf.Clamp01(heat * 2.2f);
        Color c = Color.Lerp(smoke, fire, fireMix);
        c.a = body * Mathf.Clamp01(1.15f - k) * (0.6f + 0.4f * n);
        return c;
    });

    // ColorLookup 용 32 크기 LUT (1024x32, 선형). 세피아 톤
    public static Texture2D SepiaLutTex
    {
        get
        {
            const int size = 32;
            var tex = Tex("FX05_SepiaLUT32", size * size, size, (u, v) =>
            {
                int px = Mathf.Min(size * size - 1, (int)(u * size * size));
                int py = Mathf.Min(size - 1, (int)(v * size));
                float r = (px % size) / (size - 1f), g = py / (size - 1f), b = (px / size) / (size - 1f);
                float sr = r * 0.393f + g * 0.769f + b * 0.189f;
                float sg = r * 0.349f + g * 0.686f + b * 0.168f;
                float sb = r * 0.272f + g * 0.534f + b * 0.131f;
                return new Color(Mathf.Clamp01(sr), Mathf.Clamp01(sg), Mathf.Clamp01(sb), 1f);
            }, false);
            var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
            if (importer != null && (importer.mipmapEnabled || importer.npotScale != TextureImporterNPOTScale.None))
            {
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
            return tex;
        }
    }

    private static float Line(float dist, float width) => Mathf.Clamp01(1f - dist / width);

    private static float SegmentDistance(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return Vector2.Distance(p, a + ab * t);
    }

    // ───────────────────────── 노이즈 ─────────────────────────

    private static float Hash(int x, int y, int seed)
    {
        unchecked
        {
            int h = x * 374761393 + y * 668265263 + seed * 1442695041;
            h = (h ^ (h >> 13)) * 1274126177;
            return ((h ^ (h >> 16)) & 0xffff) / 65535f;
        }
    }

    public static float Noise(float x, float y, int seed)
    {
        int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y);
        float fx = x - ix, fy = y - iy;
        fx = fx * fx * (3f - 2f * fx);
        fy = fy * fy * (3f - 2f * fy);
        float a = Mathf.Lerp(Hash(ix, iy, seed), Hash(ix + 1, iy, seed), fx);
        float b = Mathf.Lerp(Hash(ix, iy + 1, seed), Hash(ix + 1, iy + 1, seed), fx);
        return Mathf.Lerp(a, b, fy);
    }

    public static float Fbm(float x, float y, int seed) =>
        Noise(x, y, seed) * 0.5f + Noise(x * 2f, y * 2f, seed + 1) * 0.3f + Noise(x * 4f, y * 4f, seed + 2) * 0.2f;

    // ───────────────────────── Volume (포스트 프로세싱) ─────────────────────────

    // 빈 VolumeProfile 에셋 (Assets/09_Generated/Effects/{fileName}.asset)
    public static VolumeProfile Profile(string fileName)
    {
        var p = ScriptableObject.CreateInstance<VolumeProfile>();
        return SaveAsset(p, Sub, $"{fileName}.asset");
    }

    // 프로파일에 Override 추가 (서브 에셋으로 저장해야 씬을 다시 열어도 유지됨)
    public static T AddOverride<T>(VolumeProfile profile) where T : VolumeComponent
    {
        var c = profile.Add<T>(false);
        c.name = typeof(T).Name;
        c.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(c, profile);
        return c;
    }

    // 값을 다 넣은 뒤 호출: 프로파일과 컴포넌트를 저장
    public static void SaveProfile(VolumeProfile profile)
    {
        foreach (var c in profile.components) EditorUtility.SetDirty(c);
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
    }

    public static Volume GlobalVolume(string name, VolumeProfile profile, float priority = 0f, float weight = 1f)
    {
        var go = new GameObject(name);
        var v = go.AddComponent<Volume>();
        v.isGlobal = true;
        v.sharedProfile = profile;
        v.priority = priority;
        v.weight = weight;
        return v;
    }

    // 포스트 프로세싱 데모용 소품: 색이 다른 큐브들 + HDR 발광 구 3개 + 흰 큐브 (Bloom/색 보정 차이가 잘 보임)
    public static void PPShowcase(Vector3 center, float emissionIntensity = 4f)
    {
        var root = new GameObject("Showcase").transform;
        root.position = center;
        Color[] cols = { new Color32(220, 70, 70, 255), new Color32(80, 200, 100, 255), new Color32(70, 120, 230, 255), new Color32(235, 200, 80, 255) };
        for (int i = 0; i < cols.Length; i++)
        {
            var c = Prim(PrimitiveType.Cube, $"Cube_{i}", new Vector3(-3f + i * 2f, 0.6f, 1.5f), Vector3.one * 1.2f, cols[i], root);
            c.transform.rotation = Quaternion.Euler(0f, 25f + i * 15f, 0f);
        }
        Prim(PrimitiveType.Cube, "WhiteCube", new Vector3(3.5f, 0.75f, -0.5f), Vector3.one * 1.5f, new Color(0.8f, 0.8f, 0.8f), root);
        Color[] glow = { new Color(0.2f, 0.9f, 1f), new Color(1f, 0.3f, 0.9f), new Color(1f, 0.8f, 0.2f) };
        for (int i = 0; i < glow.Length; i++)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.name = $"Emissive_{i}";
            s.transform.SetParent(root, false);
            s.transform.localPosition = new Vector3(-2f + i * 2f, 0.5f, -1f);
            s.transform.localScale = Vector3.one * 0.9f;
            s.GetComponent<Renderer>().sharedMaterial = EmissiveMat($"Emissive_{i}_{emissionIntensity:0}", glow[i] * 0.5f, glow[i], emissionIntensity);
        }
    }

    // ───────────────────────── Decal 렌더러 ─────────────────────────

    // 공유 렌더러(PC_Renderer)는 건드리지 않고 복제본에 DecalRendererFeature 를 붙임.
    // 카메라는 파이프라인 에셋의 Renderer List 에 등록된 렌더러만 고를 수 있으므로
    // 파이프라인 에셋(PC_RPAsset)도 복제해 Renderer List 를 복제 렌더러로 바꿔 반환함.
    // 런타임(DecalDemo)이 이 씬에 있는 동안만 QualitySettings.renderPipeline 을 이 에셋으로 바꿨다가 되돌림.
    public static UniversalRenderPipelineAsset CreateDecalPipeline()
    {
        string folder = $"{GeneratedFolder}/{Sub}";
        EnsureFolder(folder);
        string rendererPath = $"{folder}/Decal_PC_Renderer.asset";
        string pipelinePath = $"{folder}/Decal_PC_RPAsset.asset";
        AssetDatabase.DeleteAsset(pipelinePath);
        AssetDatabase.DeleteAsset(rendererPath);

        if (!AssetDatabase.CopyAsset("Assets/Settings/PC_Renderer.asset", rendererPath)) return null;
        var data = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(rendererPath);
        if (data == null) return null;

        var feature = ScriptableObject.CreateInstance<DecalRendererFeature>();
        feature.name = "Decal";
        feature.hideFlags |= HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(feature, data);
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out string _, out long localId);

        var so = new SerializedObject(data);
        var list = so.FindProperty("m_RendererFeatures");
        var map = so.FindProperty("m_RendererFeatureMap");
        list.arraySize++;
        list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = feature;
        map.arraySize++;
        map.GetArrayElementAtIndex(map.arraySize - 1).longValue = localId;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);

        if (!AssetDatabase.CopyAsset("Assets/Settings/PC_RPAsset.asset", pipelinePath)) return null;
        var rp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
        if (rp == null) return null;
        var pso = new SerializedObject(rp);
        var renderers = pso.FindProperty("m_RendererDataList");
        renderers.arraySize = 1;
        renderers.GetArrayElementAtIndex(0).objectReferenceValue = data;
        pso.FindProperty("m_DefaultRendererIndex").intValue = 0;
        pso.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(rp);
        AssetDatabase.SaveAssets();
        return rp;
    }
}
