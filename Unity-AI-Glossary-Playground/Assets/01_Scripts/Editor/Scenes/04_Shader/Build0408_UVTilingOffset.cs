using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0408_UVTilingOffset
{
    [GlossaryScene("0408")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0408_UVTilingOffset")]
    public static void Build()
    {
        var ctx = NewScene3D("0408",
            "Space : 흐름 ON/OFF   슬라이더 : Tiling X / Y, 흐르는 속도\n" +
            "관찰: Offset 값이 매 프레임 바뀌며 물 무늬가 흘러감. 왼쪽 UV 큐브(빨강=U, 초록=V)로 Tiling 효과 확인");
        FrameCamera(ctx, new Vector3(-1.5f, 5.5f, -11f), new Vector3(0f, 1f, 2f));

        // 이음새 없이 반복되는 물 무늬 (노이즈의 0.5 근처를 밝은 선으로)
        var waterTex = CreateTexture("Shader_WaterStreaks", 256, 256, (u, v) =>
        {
            float n = TileNoise(u, v, 4, 4, 13);
            float line = Mathf.Pow(1f - Mathf.Abs(n * 2f - 1f), 10f);
            float n2 = TileNoise(u, v, 8, 2, 17);
            var deep = new Color(0.05f, 0.3f, 0.55f);
            var light = new Color(0.6f, 0.95f, 1f);
            var c = Color.Lerp(deep, light, Mathf.Clamp01(line * 1.2f + n2 * 0.25f));
            c.a = 1f;
            return c;
        });

        // UV 확인용: 빨강 = U(가로 0→1), 초록 = V(세로 0→1), 0.125 간격 격자
        var uvTex = CreateTexture("Shader_UVChecker", 256, 256, (u, v) =>
        {
            bool grid = Mathf.Repeat(u * 8f, 1f) < 0.06f || Mathf.Repeat(v * 8f, 1f) < 0.06f;
            var c = new Color(u, v, 0.25f);
            if (grid) c = Color.Lerp(c, Color.white, 0.7f);
            if (u < 0.12f && v < 0.12f) c = Color.white; // UV (0,0) 모서리 표시
            return c;
        });

        var riverMat = UrpUnlit("Shader_0408_River", Color.white, waterTex);
        riverMat.SetTextureScale("_BaseMap", new Vector2(2f, 4f));
        var fallMat = UrpUnlit("Shader_0408_Waterfall", new Color(0.85f, 0.95f, 1f), waterTex);
        var uvMat = UrpLit("Shader_0408_UVChecker", Color.white, 0.2f, 0f, m => m.SetTexture("_BaseMap", uvTex));

        // 강: 바닥 위 길쭉한 Plane (4 x 16m)
        var river = Part(PrimitiveType.Plane, "River", null, new Vector3(0f, 0.02f, 1f), new Vector3(0.4f, 1f, 1.6f), riverMat);
        Object.DestroyImmediate(river.GetComponent<Collider>());
        var bankMat = UrpLit("Shader_0408_Bank", new Color(0.3f, 0.55f, 0.25f), 0.1f);
        Part(PrimitiveType.Cube, "Bank_L", null, new Vector3(-2.6f, 0.25f, 1f), new Vector3(1.2f, 0.5f, 16f), bankMat);
        Part(PrimitiveType.Cube, "Bank_R", null, new Vector3(2.6f, 0.25f, 1f), new Vector3(1.2f, 0.5f, 16f), bankMat);

        // 폭포: 강 끝의 세로 Quad + 뒤 절벽
        var fall = Part(PrimitiveType.Quad, "Waterfall", null, new Vector3(0f, 2.5f, 8.9f), new Vector3(4f, 5f, 1f), fallMat);
        Object.DestroyImmediate(fall.GetComponent<Collider>());
        Part(PrimitiveType.Cube, "Cliff", null, new Vector3(0f, 2.5f, 9.6f), new Vector3(8f, 5f, 1f), UrpLit("Shader_0408_Cliff", new Color(0.45f, 0.42f, 0.4f), 0.1f));

        var cube = Part(PrimitiveType.Cube, "UVCheckerCube", null, new Vector3(-5f, 1.2f, -1f), Vector3.one * 2f, uvMat);
        Spin(cube, new Vector3(0f, 20f, 0f));
        WorldLabel("UV 확인 큐브", new Vector3(-5f, 2.9f, -1f), ctx.Camera, 2.6f);

        var demo = new GameObject("UVScrollDemo").AddComponent<UVScrollDemo>();
        Set(demo, "river", river.GetComponent<Renderer>());
        Set(demo, "waterfall", fall.GetComponent<Renderer>());
        Set(demo, "checkerCube", cube.GetComponent<Renderer>());

        var panel = SidePanel(ctx, "UV / Tiling / Offset");
        AddSlider(panel, "Tiling X", 0.5f, 8f, 2f, demo.SetTilingX);
        AddSlider(panel, "Tiling Y", 0.5f, 8f, 4f, demo.SetTilingY);
        AddSlider(panel, "흐르는 속도 (UV/초)", -1.5f, 1.5f, 0.4f, demo.SetFlowSpeed);
        AddButton(panel, "Space : 흐름 ON/OFF", demo.ToggleFlow);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
