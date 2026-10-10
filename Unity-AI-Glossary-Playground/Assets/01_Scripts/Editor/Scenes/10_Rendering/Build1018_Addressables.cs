using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1018_Addressables
{
    [GlossaryScene("1018")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1018_Addressables")]
    public static void Build()
    {
        var ctx = NewScene3D("1018", "1 / 2 / 3 : 스테이지 1(숲) / 2(사막) / 3(얼음) 불러오기   U : 언로드\n" +
                                     "씬에는 스테이지가 없고, 버튼을 누를 때 비동기로 불러와 생성됨\n" +
                                     "다른 스테이지를 부르면 이전 것은 내려놓음 (메모리 수치 관찰)", false);
        ctx.Camera.transform.position = new Vector3(0f, 12f, -14f);
        ctx.Camera.transform.rotation = Quaternion.Euler(40f, 0f, 0f);
        var canvas = EnsureCanvas(ctx);

        // 스테이지 프리팹 3개 → Resources 폴더 (Addressables 였다면 Addressable 체크 + 그룹 분리)
        const string sub = Sub + "/Resources/Glossary1018";
        MakeStage(sub, "Stage1_Forest", new Color(0.3f, 0.55f, 0.3f), new Color(0.2f, 0.45f, 0.2f), PrimitiveType.Sphere, 1);
        MakeStage(sub, "Stage2_Desert", new Color(0.85f, 0.72f, 0.45f), new Color(0.55f, 0.75f, 0.35f), PrimitiveType.Cylinder, 2);
        MakeStage(sub, "Stage3_Ice", new Color(0.8f, 0.9f, 1f), new Color(0.6f, 0.8f, 1f), PrimitiveType.Cube, 3);

        var loader = new GameObject("StageResourceLoader").AddComponent<StageResourceLoader>();

        // 로딩 진행 막대
        var barBg = CreateImage("ProgressBar", canvas.transform, new Vector2(0f, 150f), new Vector2(700f, 26f), new Color32(30, 33, 42, 255), RoundedSprite, true);
        AnchorBottom(barBg.rectTransform);
        var fill = CreateImage("Fill", barBg.transform, Vector2.zero, new Vector2(690f, 18f), AccentColor, WhiteSprite);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 0f;
        Set(loader, "progressFill", fill);
        BottomLabel(canvas.transform, "로딩 진행률 (Resources.LoadAsync → ResourceRequest.progress)", 190f);

        var b = CreateButtonRow(canvas.transform, 60f, "1 숲", "2 사막", "3 얼음", "U 언로드");
        OnClick(b[0], loader.LoadStage, 0);
        OnClick(b[1], loader.LoadStage, 1);
        OnClick(b[2], loader.LoadStage, 2);
        OnClick(b[3], loader.Unload);

        HowToPanel(canvas.transform, "Addressables 로 하는 법 (패키지 미설치)",
            "1. Package Manager > <b>Addressables</b> 설치\n" +
            "2. 스테이지 프리팹 Inspector > <b>Addressable</b> 체크, 주소 \"stage1\"\n" +
            "3. Window > Asset Management > Addressables > Groups\n   스테이지별 그룹(Stage1, Stage2…)으로 나누기\n" +
            "4. 불러오기\n" +
            "<noparse>   var h = Addressables.LoadAssetAsync<GameObject>(\"stage1\");\n" +
            "   await h.Task;  Instantiate(h.Result);\n" +
            "   // 또는 Addressables.InstantiateAsync(\"stage1\")</noparse>\n" +
            "5. 내려놓기: <b>Addressables.Release(h)</b> / ReleaseInstance\n" +
            "6. Groups 창 Build > New Build > Default Build Script\n\n" +
            "<color=#9BE89B>이 씬 대체</color>: Resources.LoadAsync + UnloadUnusedAssets\n" +
            "Resources 는 빌드에 전부 포함·패치 불가 → 실제 프로젝트는 Addressables 권장",
            600f, 520f);

        Save(ctx);
    }

    private static void MakeStage(string sub, string name, Color groundColor, Color propColor, PrimitiveType propType, int seed)
    {
        var root = new GameObject(name);
        // 스테이지 전용 텍스처 (메모리 차이가 보이도록 512x512)
        var tex = CreateTexture($"Stage1018_{name}", 512, 512, (u, v) =>
        {
            float n = Mathf.PerlinNoise(u * 12f + seed * 10f, v * 12f);
            return Color.Lerp(groundColor * 0.75f, groundColor, n);
        });
        var groundMat = LitMat($"Stage1018_{name}_Ground", Color.white);
        groundMat.SetTexture("_BaseMap", tex);
        var ground = Prim(PrimitiveType.Cube, "Ground", new Vector3(0f, -0.5f, 0f), new Vector3(24f, 1f, 16f), Color.white, root.transform);
        ground.GetComponent<Renderer>().sharedMaterial = groundMat;

        var rng = new System.Random(seed);
        for (int i = 0; i < 25; i++)
        {
            float x = (float)rng.NextDouble() * 22f - 11f, z = (float)rng.NextDouble() * 14f - 7f;
            float s = 0.6f + (float)rng.NextDouble() * 1.4f;
            Prim(propType, $"Prop_{i}", new Vector3(x, s * 0.5f, z), Vector3.one * s, propColor, root.transform);
        }
        var label = RenderingBuildUtil.WorldLabel(name, new Vector3(0f, 0.05f, -6.5f), 10f, root.transform);
        label.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        SavePrefab(root, sub, name);
    }
}
