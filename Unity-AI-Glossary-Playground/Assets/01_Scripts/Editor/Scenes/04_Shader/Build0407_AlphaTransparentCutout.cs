using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0407_AlphaTransparentCutout
{
    [GlossaryScene("0407")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0407_AlphaTransparentCutout")]
    public static void Build()
    {
        var ctx = NewScene3D("0407",
            "M : 나무 잎 방식 순환 (Opaque → Transparent → Cutout)   C : 바로 Cutout\n슬라이더 : Alpha Cutoff (Threshold)\n" +
            "관찰: 같은 텍스처인데 Opaque 는 사각형, Cutout 은 잎 모양만 딱 남음");
        FrameCamera(ctx, new Vector3(-0.5f, 2.4f, -8.5f), new Vector3(-0.5f, 1.8f, 0f));

        var leafTex = CreateTexture("Shader_Leaf", 256, 256, LeafPixel, wrap: TextureWrapMode.Clamp);
        var checker = CreateTexture("Shader_Checker", 64, 64, (u, v) =>
            ((Mathf.FloorToInt(u * 8f) + Mathf.FloorToInt(v * 8f)) % 2 == 0) ? new Color(0.85f, 0.85f, 0.9f) : new Color(0.25f, 0.3f, 0.45f));

        var opaque = UrpLit("Shader_0407_LeafOpaque", Color.white, 0.2f, 0f, m =>
        {
            m.SetTexture("_BaseMap", leafTex);
            MakeDoubleSided(m);
        });
        var transparent = UrpLit("Shader_0407_LeafTransparent", Color.white, 0.2f, 0f, m =>
        {
            m.SetTexture("_BaseMap", leafTex);
            MakeTransparent(m);
            MakeDoubleSided(m);
        });
        var cutout = UrpLit("Shader_0407_LeafCutout", Color.white, 0.2f, 0f, m =>
        {
            m.SetTexture("_BaseMap", leafTex);
            MakeCutout(m, 0.5f); // 요청 예문: Alpha Clipping(Cutout)
            MakeDoubleSided(m);
        });

        // 뒤 배경: 체크무늬 벽 (투명/구멍이 잘 보이게)
        var wallMat = UrpLit("Shader_0407_CheckerWall", Color.white, 0.1f, 0f, m =>
        {
            m.SetTexture("_BaseMap", checker);
            m.SetTextureScale("_BaseMap", new Vector2(4f, 1.5f));
        });
        Part(PrimitiveType.Cube, "CheckerWall", null, new Vector3(-2.5f, 2f, 1.2f), new Vector3(8f, 4f, 0.2f), wallMat);

        // 같은 잎 텍스처, 세 가지 방식
        string[] labels = { "Opaque", "Transparent", "Cutout" };
        Material[] mats = { opaque, transparent, cutout };
        for (int i = 0; i < 3; i++)
        {
            var q = Part(PrimitiveType.Quad, "Leaf_" + labels[i], null, new Vector3(-5f + i * 2.4f, 1.9f, 0.5f), Vector3.one * 2f, mats[i]);
            Object.DestroyImmediate(q.GetComponent<Collider>());
            WorldLabel(labels[i], new Vector3(-5f + i * 2.4f, 3.25f, 0.4f), ctx.Camera, 3f);
        }

        // 나무: 줄기 + 잎 쿼드 여러 장
        var tree = new GameObject("Tree");
        tree.transform.position = new Vector3(3.2f, 0f, 0f);
        Part(PrimitiveType.Cylinder, "Trunk", tree.transform, new Vector3(0f, 1.1f, 0f), new Vector3(0.25f, 1.1f, 0.25f), UrpLit("Shader_0407_Bark", new Color(0.4f, 0.27f, 0.17f), 0.1f));
        var rng = new System.Random(4);
        var leaves = new Renderer[18];
        for (int i = 0; i < leaves.Length; i++)
        {
            float a = (float)rng.NextDouble() * Mathf.PI * 2f;
            float r = 0.2f + (float)rng.NextDouble() * 0.9f;
            var pos = new Vector3(Mathf.Cos(a) * r, 2.2f + (float)rng.NextDouble() * 1.4f, Mathf.Sin(a) * r);
            var q = Part(PrimitiveType.Quad, $"TreeLeaf_{i}", tree.transform, pos, Vector3.one * (0.8f + (float)rng.NextDouble() * 0.5f), opaque);
            q.transform.localRotation = Quaternion.Euler((float)rng.NextDouble() * 70f - 35f, (float)rng.NextDouble() * 360f, (float)rng.NextDouble() * 360f);
            Object.DestroyImmediate(q.GetComponent<Collider>());
            leaves[i] = q.GetComponent<Renderer>();
        }
        WorldLabel("나무 잎 (M 으로 방식 변경)", new Vector3(3.2f, 4.3f, 0f), ctx.Camera, 2.6f);

        var demo = new GameObject("AlphaModeDemo").AddComponent<AlphaModeDemo>();
        SetArray(demo, "treeLeaves", leaves);
        Set(demo, "opaqueMat", opaque);
        Set(demo, "transparentMat", transparent);
        Set(demo, "cutoutMat", cutout);

        var panel = SidePanel(ctx, "Alpha / Transparent / Cutout");
        AddButton(panel, "M : 나무 잎 방식 바꾸기", demo.NextMode);
        AddSlider(panel, "Alpha Cutoff (Threshold)", 0f, 1f, 0.5f, demo.SetCutoff);
        AddText(panel, "<size=17>잎 안쪽 알파는 0.55~1 로 얼룩져 있어서\nCutoff 를 0.55 이상 올리면 구멍이 생김</size>", 17f, 50f);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }

    // 나뭇잎: 알파 = 잎 모양(가장자리는 부드럽게) × 얼룩, 바깥 RGB 는 밝은 종이색 (Opaque 에서 사각형이 보이게)
    private static Color LeafPixel(float u, float v)
    {
        float x = (u - 0.5f) * 2f;        // -1 ~ 1
        float y = v * 2.2f - 1.1f;        // 아래 → 위
        float width = 0.62f * Mathf.Sqrt(Mathf.Max(0f, 1f - y * y)) * (1f - 0.25f * y);
        float edge = width - Mathf.Abs(x);
        float shape = Mathf.Clamp01(edge / 0.08f);
        // 잎자루
        if (y < -0.85f && Mathf.Abs(x) < 0.04f && y > -1.1f) shape = 1f;

        float noise = TileNoise(u, v, 6, 3, 21);
        float alpha = shape * (0.55f + 0.45f * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.35f, 0.65f, noise)));

        var green = Color.Lerp(new Color(0.18f, 0.5f, 0.15f), new Color(0.45f, 0.75f, 0.2f), noise);
        if (Mathf.Abs(x) < 0.025f) green *= 0.7f; // 가운데 잎맥
        var outside = new Color(0.86f, 0.82f, 0.7f);
        var c = shape > 0f ? green : outside;
        c.a = alpha;
        return c;
    }
}
