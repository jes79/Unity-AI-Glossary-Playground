using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using GlossaryDemo;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// 씬 빌더 메서드에 붙이는 속성. id 는 GlossaryCatalog 의 항목 id ("0105")
// "Tools/Glossary/Build All Scenes" 가 이 속성이 붙은 메서드를 id 순서대로 실행함
[AttributeUsage(AttributeTargets.Method)]
public class GlossarySceneAttribute : Attribute
{
    public readonly string Id;
    public GlossarySceneAttribute(string id) { Id = id; }
}

// 용어집 데모 씬을 코드로 구성하기 위한 에디터 전용 헬퍼 모음
// - 씬 기본 구성(3D / 2D / UI) + 정보 패널
// - 프리미티브, 머티리얼, 스프라이트, 사운드 등 생성 에셋 저장
// - 직렬화 필드 연결, 버튼 이벤트 연결
public static class GlossaryKit
{
    public const string SceneRoot = "Assets/00_Scenes";
    public const string MaterialFolder = "Assets/04_Materials/Generated";
    public const string SpriteFolder = "Assets/05_Sprites/Generated";
    public const string AudioFolder = "Assets/08_Audio/Generated";
    public const string GeneratedFolder = "Assets/09_Generated"; // 컨트롤러, 클립, SO 등 그 밖의 생성 에셋

    private const string FontAssetPath = "Assets/Resources/Fonts/GlossaryFont.asset";
    private const string FontSourcePath = "Assets/06_Fonts/malgun.ttf";

    // 시스템에서 찾아볼 한글 폰트 (맑은 고딕이 없는 환경 대비)
    private static readonly string[] KoreanSystemFonts = { "malgun.ttf", "NanumGothic.ttf", "gulim.ttc", "batang.ttc", "AppleSDGothicNeo.ttc" };

    public static readonly Color BgColor = new Color32(24, 26, 33, 255);
    public static readonly Color PanelColor = new Color32(45, 49, 61, 255);
    public static readonly Color AccentColor = new Color32(88, 140, 255, 255);
    public static readonly Color TextColor = new Color32(235, 238, 245, 255);
    public static readonly Color SubTextColor = new Color32(150, 156, 170, 255);
    public static readonly Color GroundColor = new Color32(70, 76, 90, 255);
    public static readonly Color PlayerColor = new Color32(80, 170, 255, 255);
    public static readonly Color EnemyColor = new Color32(235, 80, 80, 255);

    // ───────────────────────── 메뉴 ─────────────────────────

    [MenuItem("Tools/Glossary/Build All Scenes")]
    public static void BuildAll() => BuildWhere(_ => true);

    public static void BuildCategory(int category) => BuildWhere(id => id.StartsWith($"{category:D2}"));

    [MenuItem("Tools/Glossary/Build Hub Scene")]
    public static void BuildHub()
    {
        EnsureKoreanFont();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = CreateCamera(false);
        cam.backgroundColor = BgColor;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        new GameObject("HubMenu").AddComponent<HubMenu>();
        EnsureFolder(SceneRoot);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), GlossaryCatalog.HubScenePath);
        RefreshBuildSettings();
    }

    // 명령줄 일괄 빌드용: Unity -batchmode -executeMethod GlossaryKit.BatchBuildAll -quit
    public static void BatchBuildAll()
    {
        BuildAll();
        BuildHub();
    }

    private static void BuildWhere(Func<string, bool> filter)
    {
        EnsureKoreanFont();
        var methods = TypeCache.GetMethodsWithAttribute<GlossarySceneAttribute>()
            .Where(m => filter(m.GetCustomAttribute<GlossarySceneAttribute>().Id))
            .OrderBy(m => m.GetCustomAttribute<GlossarySceneAttribute>().Id)
            .ToList();

        var failed = new List<string>();
        foreach (MethodInfo method in methods)
        {
            string id = method.GetCustomAttribute<GlossarySceneAttribute>().Id;
            try
            {
                method.Invoke(null, null);
                Debug.Log($"[GlossaryKit] Built {id}");
            }
            catch (Exception e)
            {
                var inner = e is TargetInvocationException tie && tie.InnerException != null ? tie.InnerException : e;
                failed.Add(id);
                Debug.LogError($"[GlossaryKit] FAILED {id}: {inner}");
            }
        }

        AssetDatabase.SaveAssets();
        RefreshBuildSettings();
        Debug.Log($"[GlossaryKit] Built {methods.Count - failed.Count}/{methods.Count} scenes. Failed: {string.Join(", ", failed)}");
    }

    // 허브 + 모든 데모 씬을 Build Settings 에 등록 (허브가 0번)
    [MenuItem("Tools/Glossary/Refresh Build Settings")]
    public static void RefreshBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>();
        if (File.Exists(GlossaryCatalog.HubScenePath))
            scenes.Add(new EditorBuildSettingsScene(GlossaryCatalog.HubScenePath, true));
        foreach (var e in GlossaryCatalog.Entries)
        {
            if (!File.Exists(e.ScenePath)) continue;
            scenes.Add(new EditorBuildSettingsScene(e.ScenePath, true));

            // 보조 씬({id}_{Name}_*.unity, 예: 0816 의 게임/결과 씬)도 함께 등록
            string dir = Path.GetDirectoryName(e.ScenePath)!.Replace('\\', '/');
            foreach (string extra in Directory.GetFiles(dir, $"{e.Scene}_*.unity").OrderBy(p => p))
                scenes.Add(new EditorBuildSettingsScene(extra.Replace('\\', '/'), true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ───────────────────────── 한글 폰트 ─────────────────────────

    // Windows 의 맑은 고딕을 프로젝트로 복사해 동적 TMP 폰트 에셋을 만들고 TMP 기본 폰트로 지정
    // (malgun.ttf 는 재배포 라이선스 문제로 .gitignore 처리, 없으면 이 메서드가 다시 복사함)
    //
    // GlossaryFont.asset 자체는 git 에 있지만 원본 ttf 는 없으므로, 새로 받은 프로젝트에서는
    // 폰트 에셋의 원본 참조가 끊긴 상태다. 동적 폰트는 원본이 없으면 글리프를 하나도 만들지 못해
    // 모든 TMP 텍스트가 '보이지 않는' 상태가 된다 → 에디터를 열 때 GlossaryFontGuard 가 이걸 호출해 고친다.
    [MenuItem("Tools/Glossary/Setup Korean Font")]
    public static TMP_FontAsset EnsureKoreanFont()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (existing != null && existing.sourceFontFile != null) return existing;

        EnsureFolder("Assets/06_Fonts");
        EnsureFolder("Assets/Resources/Fonts");

        var font = EnsureSourceFontFile(existing);
        if (font == null)
        {
            Debug.LogWarning("한글 폰트(맑은 고딕 등)를 찾지 못함. 한글이 □ 로 보이거나 안 보일 수 있음. " +
                             "한글 ttf 를 Assets/06_Fonts 에 넣고 Tools > Glossary > Setup Korean Font 를 다시 실행하세요");
            return TMP_Settings.defaultFontAsset;
        }

        // 원본만 다시 이어 붙여 고칠 수 있으면 그렇게 한다.
        // 에셋을 지우고 새로 만들면 GUID 가 바뀌어 이미 만들어 둔 182개 씬의 폰트 참조가 모두 끊긴다.
        if (existing != null)
        {
            // ttf 를 기록된 GUID 로 되돌려 넣었으면 다시 읽기만 해도 참조가 살아난다
            existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existing != null && existing.sourceFontFile == null) RelinkSourceFont(existing, font);
            if (existing.sourceFontFile != null)
            {
                Debug.Log($"[GlossaryKit] 한글 폰트 원본을 다시 연결했습니다: {AssetDatabase.GetAssetPath(font)}");
                SetTmpDefaultFont(existing);
                return existing;
            }
            AssetDatabase.DeleteAsset(FontAssetPath);
        }

        var fa = TMP_FontAsset.CreateFontAsset(font, 64, 6, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
        fa.name = "GlossaryFont";
        AssetDatabase.CreateAsset(fa, FontAssetPath);
        fa.atlasTextures[0].name = "GlossaryFont Atlas";
        AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
        fa.material.name = "GlossaryFont Material";
        AssetDatabase.AddObjectToAsset(fa.material, fa);
        EditorUtility.SetDirty(fa);
        AssetDatabase.SaveAssets();
        SetTmpDefaultFont(fa);
        return fa;
    }

    // Assets/06_Fonts 의 한글 ttf 를 돌려줌. 없으면 시스템 폰트에서 복사해 온다
    private static Font EnsureSourceFontFile(TMP_FontAsset existing)
    {
        if (File.Exists(FontSourcePath)) return AssetDatabase.LoadAssetAtPath<Font>(FontSourcePath);

        // 누군가 다른 한글 폰트를 직접 넣어 둔 경우
        foreach (string guid in AssetDatabase.FindAssets("t:Font", new[] { "Assets/06_Fonts" }))
        {
            var found = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(guid));
            if (found != null) return found;
        }

        string fontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        string sys = KoreanSystemFonts.Select(f => Path.Combine(fontsDir, f)).FirstOrDefault(File.Exists);
        if (sys == null) return null;

        string dest = $"Assets/06_Fonts/{Path.GetFileName(sys)}";
        File.Copy(sys, dest);

        // git 에 들어 있는 GlossaryFont.asset 이 기억하는 GUID 로 .meta 를 먼저 써 두면
        // import 하는 순간 끊겼던 참조가 그대로 이어진다 (폰트 에셋 파일을 건드리지 않아도 됨)
        string wantedGuid = existing != null ? RecordedSourceFontGuid() : null;
        if (!string.IsNullOrEmpty(wantedGuid) && !File.Exists(dest + ".meta"))
            File.WriteAllText(dest + ".meta",
                "fileFormatVersion: 2\n" +
                $"guid: {wantedGuid}\n" +
                "TrueTypeFontImporter:\n" +
                "  externalObjects: {}\n" +
                "  serializedVersion: 4\n" +
                "  fontSize: 16\n" +
                "  forceTextureCase: -2\n" +
                "  characterSpacing: 0\n" +
                "  characterPadding: 1\n" +
                "  includeFontData: 1\n" +
                "  fontNames: []\n" +
                "  fallbackFontReferences: []\n" +
                "  customCharacters: \n" +
                "  fontRenderingMode: 0\n" +
                "  ascentCalculationMode: 1\n" +
                "  useLegacyBoundsCalculation: 0\n" +
                "  shouldRoundAdvanceValue: 1\n" +
                "  userData: \n" +
                "  assetBundleName: \n" +
                "  assetBundleVariant: \n");

        AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<Font>(dest);
    }

    // GlossaryFont.asset 이 기록해 둔 원본 ttf 의 GUID (m_SourceFontFileGUID)
    private static string RecordedSourceFontGuid()
    {
        if (!File.Exists(FontAssetPath)) return null;
        var m = Regex.Match(File.ReadAllText(FontAssetPath), @"m_SourceFontFileGUID:\s*([0-9a-f]{32})");
        return m.Success ? m.Groups[1].Value : null;
    }

    // sourceFontFile 은 내부(internal) 세터라 SerializedObject 로 넣는다
    private static void RelinkSourceFont(TMP_FontAsset fontAsset, Font source)
    {
        var so = new SerializedObject(fontAsset);
        so.FindProperty("m_SourceFontFile").objectReferenceValue = source;
        var guidProp = so.FindProperty("m_SourceFontFileGUID");
        if (guidProp != null) guidProp.stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source));
        so.ApplyModifiedPropertiesWithoutUndo();

        fontAsset.ClearFontAssetData(true); // 원본이 바뀌었으니 남아 있던 글리프 기록을 비움
        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
    }

    // TMP 기본 폰트로 지정 → 새로 만드는 모든 TextMeshPro 가 한글을 표시
    private static void SetTmpDefaultFont(TMP_FontAsset fontAsset)
    {
        var settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>("Assets/TextMesh Pro/Resources/TMP Settings.asset");
        if (settings == null) return;
        var so = new SerializedObject(settings);
        var prop = so.FindProperty("m_defaultFontAsset");
        if (prop.objectReferenceValue == fontAsset) return;
        prop.objectReferenceValue = fontAsset;
        so.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
    }

    public static TMP_FontAsset Font => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath) ?? EnsureKoreanFont();

    // ───────────────────────── 씬 기본 구성 ─────────────────────────

    public class SceneContext
    {
        public GlossaryEntry Entry;
        public Camera Camera;
        public Light Light;       // 2D/UI 씬은 null
        public Canvas Canvas;     // 데모용 Canvas (CreateCanvas 또는 NewSceneUI 에서 생성)
        public GameObject Ground; // ground = true 일 때만
    }

    // 3D 씬: 원근 카메라(위에서 비스듬히), Directional Light, EventSystem, (선택) 바닥, 정보 패널
    public static SceneContext NewScene3D(string id, string controls, bool ground = true, float groundSize = 30f)
    {
        var ctx = BeginScene(id, controls);
        ctx.Camera = CreateCamera(false);
        ctx.Camera.transform.position = new Vector3(0f, 9f, -12f);
        ctx.Camera.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
        ctx.Camera.clearFlags = CameraClearFlags.Skybox;

        var lightGo = new GameObject("Directional Light");
        ctx.Light = lightGo.AddComponent<Light>();
        ctx.Light.type = LightType.Directional;
        ctx.Light.intensity = 1.2f;
        ctx.Light.shadows = LightShadows.Soft;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        if (ground)
        {
            ctx.Ground = Prim(PrimitiveType.Cube, "Ground", new Vector3(0f, -0.5f, 0f), new Vector3(groundSize, 1f, groundSize), GroundColor);
            ctx.Ground.layer = LayerMask.NameToLayer("Ground");
            ctx.Ground.isStatic = true;
        }
        return ctx;
    }

    // 2D 씬: 직교 카메라(size 5.4 = 1920x1080 기준 화면 세로 10.8 유닛), 단색 배경, EventSystem, 정보 패널
    public static SceneContext NewScene2D(string id, string controls)
    {
        var ctx = BeginScene(id, controls);
        ctx.Camera = CreateCamera(true);
        return ctx;
    }

    // UI 씬: 2D 카메라 + 데모용 Screen Space Overlay Canvas (1920x1080 기준)
    public static SceneContext NewSceneUI(string id, string controls, bool background = true)
    {
        var ctx = NewScene2D(id, controls);
        ctx.Canvas = CreateCanvas("Canvas");
        if (background)
        {
            var bg = CreateImage("Background", ctx.Canvas.transform, Vector2.zero, Vector2.zero, BgColor);
            Stretch(bg.rectTransform);
            bg.raycastTarget = false;
        }
        return ctx;
    }

    private static SceneContext BeginScene(string id, string controls)
    {
        EnsureKoreanFont();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var ctx = new SceneContext { Entry = GlossaryCatalog.Find(id) };

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var info = new GameObject("GlossaryInfo").AddComponent<GlossaryInfoPanel>();
        SetString(info, "entryId", id);
        SetString(info, "controls", controls ?? "");
        return ctx;
    }

    public static Camera CreateCamera(bool orthographic)
    {
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<UniversalAdditionalCameraData>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BgColor;
        cam.orthographic = orthographic;
        cam.orthographicSize = 5.4f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 500f;
        cam.fieldOfView = 55f;
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        return cam;
    }

    // 포스트 프로세싱(Volume) 데모에서 카메라 PP 켜기
    public static void EnablePostProcessing(Camera cam)
    {
        var data = cam.GetComponent<UniversalAdditionalCameraData>() ?? cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
        data.renderPostProcessing = true;
    }

    // 씬 저장 + Build Settings 등록. 경로는 카탈로그에서 결정 (Assets/00_Scenes/{분야폴더}/{id}_{Name}.unity)
    public static void Save(SceneContext ctx) => Save(ctx.Entry.Id);

    public static void Save(string id)
    {
        var entry = GlossaryCatalog.Find(id);
        EnsureFolder(Path.GetDirectoryName(entry.ScenePath)!.Replace('\\', '/'));
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), entry.ScenePath);
        RefreshBuildSettings();
    }

    // ───────────────────────── 3D 오브젝트 / 머티리얼 ─────────────────────────

    public static GameObject Prim(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Color color, Transform parent = null)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = ColorMat(color);
        return go;
    }

    // 색상별 URP Lit 머티리얼 (같은 색이면 재사용)
    public static Material ColorMat(Color color, float smoothness = 0.3f)
    {
        string name = $"Lit_{ColorUtility.ToHtmlStringRGBA(color)}_{Mathf.RoundToInt(smoothness * 100)}";
        return LitMat(name, color, smoothness);
    }

    public static Material LitMat(string name, Color color, float smoothness = 0.3f, float metallic = 0f)
    {
        return CreateMaterial(name, "Universal Render Pipeline/Lit", m =>
        {
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", metallic);
        });
    }

    public static Material UnlitMat(string name, Color color)
    {
        return CreateMaterial(name, "Universal Render Pipeline/Unlit", m => m.SetColor("_BaseColor", color));
    }

    // URP Lit 을 반투명으로 전환 (Surface Type = Transparent)
    public static void MakeTransparent(Material m)
    {
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 0f);
        m.SetFloat("_ZWrite", 0f);
        m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
        m.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        m.SetOverrideTag("RenderType", "Transparent");
        EditorUtility.SetDirty(m);
    }

    // 셰이더 이름으로 머티리얼 에셋 생성/갱신 (Assets/04_Materials/Generated/{name}.mat)
    public static Material CreateMaterial(string name, string shaderName, Action<Material> setup = null, string folder = MaterialFolder)
    {
        var shader = Shader.Find(shaderName);
        if (shader == null) throw new Exception($"셰이더를 찾을 수 없음: {shaderName}");

        EnsureFolder(folder);
        string path = $"{folder}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
        }

        setup?.Invoke(mat);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // 파티클용 머티리얼 (URP Particles/Unlit). additive = true 면 겹칠수록 밝아짐
    public static Material ParticleMat(string name, Color color, bool additive = false, Texture texture = null)
    {
        return CreateMaterial(name, "Universal Render Pipeline/Particles/Unlit", m =>
        {
            m.SetColor("_BaseColor", color);
            if (texture == null) texture = SoftCircleSprite.texture;
            m.SetTexture("_BaseMap", texture);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", additive ? 2f : 0f); // 0 Alpha, 1 Premultiply, 2 Additive, 3 Multiply
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", additive ? (float)UnityEngine.Rendering.BlendMode.One : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            m.SetOverrideTag("RenderType", "Transparent");
        });
    }

    // ───────────────────────── 스프라이트 / 텍스처 ─────────────────────────

    // 픽셀 함수(u, v → 색)로 PNG 를 그려 스프라이트로 저장
    public static Sprite CreateSprite(string name, int width, int height, Func<float, float, Color> pixel, float pixelsPerUnit = 100f,
        Vector4 border = default, FilterMode filter = FilterMode.Bilinear)
    {
        string path = WritePng(name, width, height, pixel, SpriteFolder);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = filter;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.spriteBorder = border;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // 픽셀 함수로 일반 텍스처(Default 타입) 저장. 셰이더 노이즈/노멀맵 등에 사용
    public static Texture2D CreateTexture(string name, int width, int height, Func<float, float, Color> pixel, bool normalMap = false,
        TextureWrapMode wrap = TextureWrapMode.Repeat, bool sRGB = true)
    {
        string path = WritePng(name, width, height, pixel, $"{GeneratedFolder}/Textures");
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
        importer.wrapMode = wrap;
        importer.sRGBTexture = sRGB && !normalMap;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static string WritePng(string name, int width, int height, Func<float, float, Color> pixel, string folder)
    {
        EnsureFolder(folder);
        string path = $"{folder}/{name}.png";
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                pixels[y * width + x] = pixel((x + 0.5f) / width, (y + 0.5f) / height);
        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
        return path;
    }

    private static Sprite Cached(string name, Func<Sprite> create) => AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteFolder}/{name}.png") ?? create();

    // 흰색 사각형 (1 유닛 = 100px). SpriteRenderer.color 로 색 지정
    public static Sprite SquareSprite => Cached("Square", () => CreateSprite("Square", 32, 32, (u, v) => Color.white, 32f));

    // 흰색 원 (지름 1 유닛)
    public static Sprite CircleSprite => Cached("Circle", () => CreateSprite("Circle", 128, 128, (u, v) =>
    {
        float d = new Vector2(u - 0.5f, v - 0.5f).magnitude * 2f;
        return new Color(1f, 1f, 1f, Mathf.Clamp01((1f - d) * 64f));
    }, 128f));

    // 가장자리로 갈수록 투명한 원 (파티클 기본 텍스처)
    public static Sprite SoftCircleSprite => Cached("SoftCircle", () => CreateSprite("SoftCircle", 64, 64, (u, v) =>
    {
        float d = new Vector2(u - 0.5f, v - 0.5f).magnitude * 2f;
        float a = Mathf.Clamp01(1f - d);
        return new Color(1f, 1f, 1f, a * a);
    }, 64f));

    // UI 용 흰색 스프라이트 (Image.Type.Filled 등에 필요)
    public static Sprite WhiteSprite => Cached("White", () => CreateSprite("White", 8, 8, (u, v) => Color.white));

    public static Sprite RoundedSprite => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    public static Sprite KnobSprite => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

    public static SpriteRenderer CreateSpriteObject(string name, Sprite sprite, Vector3 pos, Vector2 size, Color color, Transform parent = null, int order = 0)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    // ───────────────────────── 사운드 ─────────────────────────

    public enum Wave { Sine, Square, Saw, Noise }

    // 절차적 효과음 WAV 생성 (Assets/08_Audio/Generated/{name}.wav)
    // freqEnd 를 주면 주파수가 시작→끝으로 미끄러짐 (점프 "뿅", 레이저 등)
    public static AudioClip CreateToneClip(string name, float frequency, float duration, Wave wave = Wave.Sine, float volume = 0.5f,
        float freqEnd = -1f, float attack = 0.01f, float release = 0.1f)
    {
        const int sampleRate = 44100;
        int count = Mathf.CeilToInt(duration * sampleRate);
        var samples = new float[count];
        var rng = new System.Random(name.GetHashCode());
        double phase = 0;
        if (freqEnd < 0f) freqEnd = frequency;
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            float k = t / duration;
            float f = Mathf.Lerp(frequency, freqEnd, k);
            phase += f / sampleRate;
            float p = (float)(phase - Math.Floor(phase));
            float s = wave switch
            {
                Wave.Sine => Mathf.Sin(p * Mathf.PI * 2f),
                Wave.Square => p < 0.5f ? 1f : -1f,
                Wave.Saw => p * 2f - 1f,
                _ => (float)(rng.NextDouble() * 2.0 - 1.0),
            };
            float env = Mathf.Min(1f, t / Mathf.Max(attack, 1e-4f)) * Mathf.Min(1f, (duration - t) / Mathf.Max(release, 1e-4f));
            samples[i] = s * env * volume;
        }
        return SaveWav(name, samples, sampleRate);
    }

    // 여러 음을 이어 붙인 간단한 루프 음악 (BGM 데모용). notes: 주파수(Hz), 0 이면 쉼표
    public static AudioClip CreateMelodyClip(string name, float[] notes, float noteLength, Wave wave = Wave.Square, float volume = 0.25f)
    {
        const int sampleRate = 44100;
        int perNote = Mathf.CeilToInt(noteLength * sampleRate);
        var samples = new float[perNote * notes.Length];
        for (int n = 0; n < notes.Length; n++)
        {
            double phase = 0;
            for (int i = 0; i < perNote; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Min(1f, t / 0.01f) * Mathf.Clamp01((noteLength - t) / 0.05f);
                float s = 0f;
                if (notes[n] > 0f)
                {
                    phase += notes[n] / sampleRate;
                    float p = (float)(phase - Math.Floor(phase));
                    s = wave switch
                    {
                        Wave.Sine => Mathf.Sin(p * Mathf.PI * 2f),
                        Wave.Square => p < 0.5f ? 1f : -1f,
                        Wave.Saw => p * 2f - 1f,
                        _ => 0f,
                    };
                }
                samples[n * perNote + i] = s * env * volume;
            }
        }
        return SaveWav(name, samples, sampleRate);
    }

    private static AudioClip SaveWav(string name, float[] samples, int sampleRate)
    {
        EnsureFolder(AudioFolder);
        string path = $"{AudioFolder}/{name}.wav";
        using (var fs = new FileStream(path, FileMode.Create))
        using (var w = new BinaryWriter(fs))
        {
            int byteCount = samples.Length * 2;
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            w.Write(36 + byteCount);
            w.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            w.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            w.Write(16);
            w.Write((short)1);
            w.Write((short)1);
            w.Write(sampleRate);
            w.Write(sampleRate * 2);
            w.Write((short)2);
            w.Write((short)16);
            w.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            w.Write(byteCount);
            foreach (float s in samples) w.Write((short)(Mathf.Clamp(s, -1f, 1f) * short.MaxValue));
        }
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    // ───────────────────────── UI ─────────────────────────

    public static Canvas CreateCanvas(string name, int sortingOrder = 0)
    {
        var canvasGo = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.layer = LayerMask.NameToLayer("UI");
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    public static RectTransform CreateRect(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    public static Image CreateImage(string name, Transform parent, Vector2 pos, Vector2 size, Color color, Sprite sprite = null, bool sliced = false)
    {
        var rt = CreateRect(name, parent, pos, size);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.sprite = sprite;
        if (sliced) img.type = Image.Type.Sliced;
        return img;
    }

    public static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 pos, Vector2 size, string text, float fontSize, Color color,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        var rt = CreateRect(name, parent, pos, size);
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = Font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        return tmp;
    }

    public static Button CreateButton(string name, Transform parent, Vector2 pos, Vector2 size, string label, Color? color = null, float fontSize = 28f)
    {
        var img = CreateImage(name, parent, pos, size, color ?? AccentColor, RoundedSprite, true);
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        var txt = CreateText("Label", img.transform, Vector2.zero, Vector2.zero, label, fontSize, Color.white);
        Stretch(txt.rectTransform);
        txt.fontStyle = FontStyles.Bold;
        return btn;
    }

    // 화면 아래쪽 가운데에 버튼을 가로로 나열 (y 는 화면 아래 기준 높이)
    public static Button[] CreateButtonRow(Transform canvas, float y, params string[] labels)
    {
        const float width = 240f, spacing = 20f;
        float total = labels.Length * width + (labels.Length - 1) * spacing;
        var buttons = new Button[labels.Length];
        for (int i = 0; i < labels.Length; i++)
        {
            float x = -total * 0.5f + width * 0.5f + i * (width + spacing);
            buttons[i] = CreateButton($"Button_{i}", canvas, new Vector2(x, y), new Vector2(width, 70f), labels[i]);
            AnchorBottom(buttons[i].GetComponent<RectTransform>());
        }
        return buttons;
    }

    public static Slider CreateSlider(string name, Transform parent, Vector2 pos, float width, float min, float max, float value, string label = null)
    {
        var root = CreateRect(name, parent, pos, new Vector2(width, 30f));
        var bg = CreateImage("Background", root, Vector2.zero, Vector2.zero, new Color32(30, 33, 42, 255), RoundedSprite, true);
        Stretch(bg.rectTransform);
        var fillArea = CreateRect("Fill Area", root, Vector2.zero, Vector2.zero);
        Stretch(fillArea, 0f);
        fillArea.offsetMin = new Vector2(8f, 6f);
        fillArea.offsetMax = new Vector2(-8f, -6f);
        var fill = CreateImage("Fill", fillArea, Vector2.zero, Vector2.zero, AccentColor, RoundedSprite, true);
        Stretch(fill.rectTransform);
        var handleArea = CreateRect("Handle Slide Area", root, Vector2.zero, Vector2.zero);
        Stretch(handleArea);
        handleArea.offsetMin = new Vector2(12f, 0f);
        handleArea.offsetMax = new Vector2(-12f, 0f);
        var handle = CreateImage("Handle", handleArea, Vector2.zero, new Vector2(30f, 40f), Color.white, KnobSprite);

        var slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;

        if (label != null)
        {
            var t = CreateText("Label", root, new Vector2(0f, 34f), new Vector2(width, 30f), label, 22f, TextColor, TextAlignmentOptions.Left);
            t.rectTransform.anchoredPosition = new Vector2(0f, 36f);
        }
        return slider;
    }

    public static void Stretch(RectTransform rt, float inset = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
    }

    public static void AnchorTop(RectTransform rt) => SetAnchor(rt, new Vector2(0.5f, 1f));
    public static void AnchorBottom(RectTransform rt) => SetAnchor(rt, new Vector2(0.5f, 0f));
    public static void AnchorTopRight(RectTransform rt) => SetAnchor(rt, new Vector2(1f, 1f));
    public static void AnchorBottomRight(RectTransform rt) => SetAnchor(rt, new Vector2(1f, 0f));
    public static void AnchorBottomLeft(RectTransform rt) => SetAnchor(rt, new Vector2(0f, 0f));

    // 앵커만 옮기고 anchoredPosition 값은 유지 (새 앵커 기준 오프셋으로 해석됨)
    public static void SetAnchor(RectTransform rt, Vector2 anchor)
    {
        Vector2 pos = rt.anchoredPosition;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
    }

    // ───────────────────────── 직렬화 필드 / 이벤트 연결 ─────────────────────────

    private static SerializedProperty FindProperty(SerializedObject so, string field)
    {
        var p = so.FindProperty(field);
        if (p == null) throw new Exception($"{so.targetObject.GetType().Name}.{field} 필드를 찾을 수 없음");
        return p;
    }

    public static void Set(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        FindProperty(so, field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetArray(Object target, string field, params Object[] values)
    {
        var so = new SerializedObject(target);
        var p = FindProperty(so, field);
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetStrings(Object target, string field, params string[] values)
    {
        var so = new SerializedObject(target);
        var p = FindProperty(so, field);
        p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).stringValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetFloat(Object target, string field, float value) => Modify(target, field, p => p.floatValue = value);
    public static void SetInt(Object target, string field, int value) => Modify(target, field, p => p.intValue = value);
    public static void SetBool(Object target, string field, bool value) => Modify(target, field, p => p.boolValue = value);
    public static void SetString(Object target, string field, string value) => Modify(target, field, p => p.stringValue = value);
    public static void SetColor(Object target, string field, Color value) => Modify(target, field, p => p.colorValue = value);
    public static void SetVector3(Object target, string field, Vector3 value) => Modify(target, field, p => p.vector3Value = value);
    public static void SetLayerMask(Object target, string field, LayerMask value) => Modify(target, field, p => p.intValue = value.value);

    private static void Modify(Object target, string field, Action<SerializedProperty> apply)
    {
        var so = new SerializedObject(target);
        apply(FindProperty(so, field));
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // 버튼 OnClick 에 영구 리스너(인스펙터에 보이는 형태)로 연결
    public static void OnClick(Button button, UnityAction action) => UnityEventTools.AddPersistentListener(button.onClick, action);
    public static void OnClick(Button button, UnityAction<int> action, int arg) => UnityEventTools.AddIntPersistentListener(button.onClick, action, arg);
    public static void OnClick(Button button, UnityAction<float> action, float arg) => UnityEventTools.AddFloatPersistentListener(button.onClick, action, arg);
    public static void OnClick(Button button, UnityAction<string> action, string arg) => UnityEventTools.AddStringPersistentListener(button.onClick, action, arg);
    public static void OnValueChanged(Slider slider, UnityAction<float> action) => UnityEventTools.AddPersistentListener(slider.onValueChanged, action);

    // ───────────────────────── 에셋 ─────────────────────────

    // 생성 에셋 저장 (Assets/09_Generated/{sub}/{fileName}). 이미 있으면 지우고 새로 만듦
    public static T SaveAsset<T>(T asset, string sub, string fileName) where T : Object
    {
        string folder = $"{GeneratedFolder}/{sub}";
        EnsureFolder(folder);
        string path = $"{folder}/{fileName}";
        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    public static GameObject SavePrefab(GameObject go, string sub, string fileName, bool destroySceneObject = true)
    {
        string folder = $"{GeneratedFolder}/{sub}";
        EnsureFolder(folder);
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{folder}/{fileName}.prefab");
        if (destroySceneObject) Object.DestroyImmediate(go);
        return prefab;
    }

    public static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
