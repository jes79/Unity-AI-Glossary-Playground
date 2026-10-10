using System;
using GlossaryDemo.ShaderDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static GlossaryKit;
using Object = UnityEngine.Object;

// 04_Shader 씬 빌더 전용 헬퍼 (GlossaryKit 은 공통 파일이라 여기에 분야 전용 기능을 둠)
public static class ShaderBuildUtil
{
    public const string ShaderPrefix = "Glossary/Shader/";
    public const string GeneratedSub = "04_Shader";

    // ───────────── 머티리얼 / 텍스처 ─────────────

    // Assets/02_Shaders/04_Shader 의 손으로 쓴 셰이더로 머티리얼 에셋 생성
    public static Material ShaderMat(string name, string shaderName, Action<Material> setup = null)
        => CreateMaterial(name, ShaderPrefix + shaderName, setup);

    // URP Lit 머티리얼 (텍스처/노멀/이미션 등은 setup 에서)
    public static Material UrpLit(string name, Color color, float smoothness = 0.4f, float metallic = 0f, Action<Material> setup = null)
    {
        return CreateMaterial(name, "Universal Render Pipeline/Lit", m =>
        {
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", metallic);
            setup?.Invoke(m);
        });
    }

    public static Material UrpUnlit(string name, Color color, Texture texture = null)
    {
        return CreateMaterial(name, "Universal Render Pipeline/Unlit", m =>
        {
            m.SetColor("_BaseColor", color);
            m.SetTexture("_BaseMap", texture);
        });
    }

    // URP Lit 을 Alpha Clipping(Cutout) 으로
    public static void MakeCutout(Material m, float cutoff)
    {
        m.SetFloat("_AlphaClip", 1f);
        m.SetFloat("_Cutoff", cutoff);
        m.EnableKeyword("_ALPHATEST_ON");
        m.renderQueue = (int)RenderQueue.AlphaTest;
        m.SetOverrideTag("RenderType", "TransparentCutout");
        EditorUtility.SetDirty(m);
    }

    // 양면 렌더링 (URP Lit: _Cull 0 = Off)
    public static void MakeDoubleSided(Material m)
    {
        m.SetFloat("_Cull", 0f);
        m.doubleSidedGI = true;
    }

    // 이음새 없이 반복되는 fBm 값 노이즈 (0~1)
    public static float TileNoise(float u, float v, int basePeriod, int octaves, int seed)
    {
        float sum = 0f, amp = 0.5f, norm = 0f;
        int period = basePeriod;
        for (int o = 0; o < octaves; o++)
        {
            sum += amp * ValueNoise(u * period, v * period, period, seed + o * 31);
            norm += amp;
            amp *= 0.5f;
            period *= 2;
        }
        return sum / norm;
    }

    private static float ValueNoise(float x, float y, int period, int seed)
    {
        int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
        float fx = x - x0, fy = y - y0;
        fx = fx * fx * (3f - 2f * fx);
        fy = fy * fy * (3f - 2f * fy);
        float a = Hash(x0, y0, period, seed), b = Hash(x0 + 1, y0, period, seed);
        float c = Hash(x0, y0 + 1, period, seed), d = Hash(x0 + 1, y0 + 1, period, seed);
        return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
    }

    private static float Hash(int x, int y, int period, int seed)
    {
        x = ((x % period) + period) % period;
        y = ((y % period) + period) % period;
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 2147483647);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0xFFFFFF;
        }
    }

    // 셰이더 데모 공용 노이즈 텍스처 (디졸브, 물, Shader Graph 레시피)
    public static Texture2D NoiseTexture()
    {
        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>($"{GeneratedFolder}/Textures/Shader_Noise.png");
        if (existing != null) return existing;
        return CreateTexture("Shader_Noise", 256, 256, (u, v) =>
        {
            float n = TileNoise(u, v, 4, 5, 7);
            n = Mathf.Clamp01((n - 0.5f) * 1.6f + 0.5f); // 대비를 조금 키워 0~1 을 고르게 쓰게
            return new Color(n, n, n, 1f);
        }, sRGB: false);
    }

    // ───────────── 씬 구성 ─────────────

    public static void FrameCamera(SceneContext ctx, Vector3 position, Vector3 lookAt, float fov = 50f)
    {
        ctx.Camera.transform.position = position;
        ctx.Camera.transform.rotation = Quaternion.LookRotation(lookAt - position);
        ctx.Camera.fieldOfView = fov;
    }

    // 어두운 배경(단색) 씬: 빛나는 효과(Emission, 홀로그램, 쉴드)가 잘 보이게
    public static void DarkBackground(SceneContext ctx, Color? color = null)
    {
        ctx.Camera.clearFlags = CameraClearFlags.SolidColor;
        ctx.Camera.backgroundColor = color ?? new Color(0.03f, 0.035f, 0.05f);
    }

    // 절차적 스카이박스 + 환경광 (금속 반사 등을 볼 때)
    public static void ProceduralSky()
    {
        var sky = CreateMaterial("Shader_Sky", "Skybox/Procedural", m =>
        {
            m.SetFloat("_SunSize", 0.04f);
            m.SetFloat("_AtmosphereThickness", 1.1f);
            m.SetColor("_SkyTint", new Color(0.5f, 0.6f, 0.75f));
            m.SetColor("_GroundColor", new Color(0.35f, 0.33f, 0.3f));
            m.SetFloat("_Exposure", 1.2f);
        });
        RenderSettings.skybox = sky;
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
    }

    // Depth Texture 가 필요한 셰이더(물, 쉴드)용: 파이프라인 에셋은 건드리지 않고 이 카메라만 켬
    public static void RequireDepthTexture(Camera cam)
    {
        var data = cam.GetComponent<UniversalAdditionalCameraData>() ?? cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
        data.requiresDepthOption = CameraOverrideOption.On;
    }

    // Bloom 볼륨 (HDR 색이 번져 보이게). 프로필은 Assets/09_Generated/04_Shader/{id}_Volume.asset
    public static Volume AddBloomVolume(SceneContext ctx, string id, float intensity = 1f, float threshold = 1f, Action<VolumeProfile> extra = null)
    {
        EnablePostProcessing(ctx.Camera);
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        var bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(intensity);
        bloom.threshold.Override(threshold);
        bloom.scatter.Override(0.7f);
        extra?.Invoke(profile);

        SaveAsset(profile, GeneratedSub, $"{id}_Volume.asset");
        foreach (var component in profile.components)
        {
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(component, profile);
        }
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        var go = new GameObject("Global Volume");
        var volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = profile;
        return volume;
    }

    // 캡슐 몸통 + 구 머리로 된 간단한 캐릭터. 모든 파츠에 같은 머티리얼
    public static GameObject Character(string name, Vector3 position, Material material, Transform parent = null, float scale = 1f)
    {
        var root = new GameObject(name);
        if (parent != null) root.transform.SetParent(parent, false);
        root.transform.localPosition = position;
        root.transform.localScale = Vector3.one * scale;

        var body = Part(PrimitiveType.Capsule, "Body", root.transform, new Vector3(0f, 0.8f, 0f), new Vector3(0.8f, 0.8f, 0.8f), material);
        var head = Part(PrimitiveType.Sphere, "Head", root.transform, new Vector3(0f, 1.9f, 0f), Vector3.one * 0.62f, material);
        Part(PrimitiveType.Capsule, "Arm_L", root.transform, new Vector3(-0.52f, 0.95f, 0f), new Vector3(0.22f, 0.4f, 0.22f), material).transform.localRotation = Quaternion.Euler(0f, 0f, 12f);
        Part(PrimitiveType.Capsule, "Arm_R", root.transform, new Vector3(0.52f, 0.95f, 0f), new Vector3(0.22f, 0.4f, 0.22f), material).transform.localRotation = Quaternion.Euler(0f, 0f, -12f);
        Part(PrimitiveType.Sphere, "Nose", root.transform, new Vector3(0f, 1.9f, -0.3f), Vector3.one * 0.14f, material);

        // 클릭 판정용으로 몸통/머리 콜라이더만 남김
        foreach (var c in root.GetComponentsInChildren<Collider>())
            if (c.gameObject != body && c.gameObject != head) Object.DestroyImmediate(c);
        return root;
    }

    public static GameObject Part(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Material material)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    public static ShaderDemoSpinner Spin(GameObject go, Vector3 degreesPerSecond, float bobHeight = 0f)
    {
        var s = go.AddComponent<ShaderDemoSpinner>();
        SetVector3(s, "degreesPerSecond", degreesPerSecond);
        SetFloat(s, "bobHeight", bobHeight);
        return s;
    }

    // 월드 공간 글자 (물체 이름표). 카메라 쪽을 바라보게 회전
    public static TextMeshPro WorldLabel(string text, Vector3 position, Camera facing, float fontSize = 3f, Color? color = null)
    {
        var go = new GameObject("Label_" + text.Split('\n')[0]);
        go.transform.position = position;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.font = GlossaryKit.Font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color ?? TextColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.rectTransform.sizeDelta = new Vector2(6f, 1.2f);
        if (facing != null)
            go.transform.rotation = Quaternion.LookRotation(position - facing.transform.position);
        return tmp;
    }

    // ───────────── 오른쪽 위 조작 패널 (정보 패널과 겹치지 않게) ─────────────

    public class Panel
    {
        public RectTransform Root;
        public float Width;
        public float Cursor; // 다음 요소가 놓일 위쪽 기준 y (양수, 아래로 증가)
    }

    public static Panel SidePanel(SceneContext ctx, string title, float width = 440f)
    {
        if (ctx.Canvas == null) ctx.Canvas = CreateCanvas("Canvas");
        var img = CreateImage("SidePanel", ctx.Canvas.transform, Vector2.zero, new Vector2(width, 100f), new Color(0.07f, 0.08f, 0.1f, 0.86f));
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-16f, -16f);
        var p = new Panel { Root = rt, Width = width, Cursor = 14f };
        var t = AddText(p, title, 24f, 34f);
        t.fontStyle = FontStyles.Bold;
        t.color = new Color(1f, 0.86f, 0.4f);
        return p;
    }

    private static void Place(Panel p, RectTransform rt, float height)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -p.Cursor);
        p.Cursor += height + 8f;
        p.Root.sizeDelta = new Vector2(p.Width, p.Cursor + 8f);
    }

    public static TextMeshProUGUI AddText(Panel p, string text, float fontSize, float height, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var t = CreateText("Text", p.Root, Vector2.zero, new Vector2(p.Width - 32f, height), text, fontSize, TextColor, align);
        t.richText = true;
        Place(p, t.rectTransform, height);
        return t;
    }

    public static Slider AddSlider(Panel p, string label, float min, float max, float value, UnityAction<float> onChanged)
    {
        p.Cursor += 34f; // 라벨 자리 (CreateSlider 가 슬라이더 위쪽에 라벨을 그림)
        var s = CreateSlider("Slider_" + label, p.Root, Vector2.zero, p.Width - 48f, min, max, value, label);
        Place(p, (RectTransform)s.transform, 30f);
        if (onChanged != null) OnValueChanged(s, onChanged);
        return s;
    }

    public static Button AddButton(Panel p, string label, UnityAction onClick, float height = 46f)
    {
        var b = CreateButton("Button_" + label, p.Root, Vector2.zero, new Vector2(p.Width - 48f, height), label, null, 22f);
        Place(p, (RectTransform)b.transform, height);
        if (onClick != null) OnClick(b, onClick);
        return b;
    }

    // 단계 설명 같은 긴 글용 큰 패널 (화면 오른쪽 아래)
    public static TextMeshProUGUI BottomRightText(SceneContext ctx, string name, Vector2 size, string text, float fontSize = 20f)
    {
        if (ctx.Canvas == null) ctx.Canvas = CreateCanvas("Canvas");
        var bg = CreateImage(name, ctx.Canvas.transform, Vector2.zero, size, new Color(0.07f, 0.08f, 0.1f, 0.88f));
        var rt = bg.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-16f, 16f);
        var t = CreateText("Text", rt, Vector2.zero, Vector2.zero, text, fontSize, TextColor, TextAlignmentOptions.TopLeft);
        Stretch(t.rectTransform, 16f);
        t.richText = true;
        return t;
    }
}
