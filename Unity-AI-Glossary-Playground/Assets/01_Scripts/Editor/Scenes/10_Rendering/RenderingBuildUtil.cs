using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;

// 10_Rendering 빌더 전용 헬퍼 (공통 GlossaryKit 은 수정 금지라 분야 안에 따로 둠)
public static class RenderingBuildUtil
{
    public const string Sub = "Rendering"; // 생성 에셋: Assets/09_Generated/Rendering
    public const string Folder = GeneratedFolder + "/" + Sub;

    // 3D 씬은 데모용 Canvas 가 없으므로 필요할 때 만들어 ctx 에 넣음
    public static Canvas EnsureCanvas(SceneContext ctx)
    {
        if (ctx.Canvas == null) ctx.Canvas = CreateCanvas("Canvas");
        return ctx.Canvas;
    }

    // 화면 오른쪽 위 "방법 설명" 패널 (정보 패널은 왼쪽 위라서 겹치지 않음)
    public static TextMeshProUGUI HowToPanel(Transform canvas, string title, string body, float width = 560f, float height = 520f, float top = 16f)
    {
        var bg = CreateImage("HowToPanel", canvas, Vector2.zero, new Vector2(width, height), new Color(0.07f, 0.08f, 0.1f, 0.86f));
        var rt = bg.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-16f, -top);
        bg.raycastTarget = false;

        var t = CreateText("Text", bg.transform, Vector2.zero, Vector2.zero,
            $"<color=#FFD866><b>{title}</b></color>\n{body}", 19f, TextColor, TextAlignmentOptions.TopLeft);
        Stretch(t.rectTransform, 14f);
        t.richText = true;
        return t;
    }

    // 화면 아래 가운데 위쪽에 놓는 일반 텍스트 (라벨)
    public static TextMeshProUGUI BottomLabel(Transform canvas, string text, float y, float width = 1100f)
    {
        var t = CreateText("Label", canvas, new Vector2(0f, y), new Vector2(width, 40f), text, 22f, SubTextColor);
        AnchorBottom(t.rectTransform);
        return t;
    }

    // 월드 공간 TextMeshPro 라벨
    public static TextMeshPro WorldLabel(string text, Vector3 pos, float size = 4f, Transform parent = null, Color? color = null)
    {
        var go = new GameObject("Label");
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.font = GlossaryKit.Font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color ?? Color.white;
        tmp.rectTransform.sizeDelta = new Vector2(8f, 2f);
        return tmp;
    }

    // Built-in 파이프라인의 Standard 셰이더 머티리얼 (URP 에서는 분홍색으로 보임 = 데모용 "깨진" 머티리얼)
    public static Material BuiltinStandardMat(string name, Color color)
    {
        var shader = Shader.Find("Standard") ?? Shader.Find("Hidden/InternalErrorShader");
        EnsureFolder(MaterialFolder);
        string path = $"{MaterialFolder}/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else mat.shader = shader;
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // GPU Instancing 이 켜진 URP Lit 머티리얼
    public static Material InstancedLit(string name, Color color)
    {
        var m = LitMat(name, color);
        m.enableInstancing = true;
        EditorUtility.SetDirty(m);
        return m;
    }

    public static Mesh BuiltinMesh(PrimitiveType type)
    {
        var go = GameObject.CreatePrimitive(type);
        var mesh = go.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(go);
        return mesh;
    }

    // 위치 지정한 버튼 (CreateButtonRow 대신 화면 오른쪽 아래 등에 둘 때)
    public static Button Button(Transform canvas, string label, Vector2 pos, Vector2 size, Vector2 anchor)
    {
        var b = CreateButton("Button_" + label, canvas, pos, size, label, null, 24f);
        SetAnchor(b.GetComponent<RectTransform>(), anchor);
        return b;
    }

    public static void SetStatic(GameObject go, StaticEditorFlags flags)
    {
        GameObjectUtility.SetStaticEditorFlags(go, flags);
    }

    // Gradient 직렬화 필드 설정 (GlossaryKit 에 없어서 추가)
    public static void SetGradient(Object target, string field, params (float t, Color c)[] keys)
    {
        var g = new Gradient();
        var ck = new GradientColorKey[keys.Length];
        for (int i = 0; i < keys.Length; i++) ck[i] = new GradientColorKey(keys[i].c, keys[i].t);
        g.SetKeys(ck, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        var so = new SerializedObject(target);
        so.FindProperty(field).gradientValue = g;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
