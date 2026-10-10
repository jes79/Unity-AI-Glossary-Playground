using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0401_ShaderMaterial
{
    [GlossaryScene("0401")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0401_ShaderMaterial")]
    public static void Build()
    {
        var ctx = NewScene3D("0401",
            "1 / 2 / 3 : 머티리얼 선택   슬라이더 : 선택한 머티리얼 색 변경\nS : 셰이더 교체 (SimpleLit ↔ SimpleUnlit)\n" +
            "관찰: 머티리얼 하나를 바꾸면 그걸 쓰는 2개가 같이 바뀌고, 셰이더를 바꾸면 3개 모두 그리는 방식이 바뀜");
        FrameCamera(ctx, new Vector3(0f, 3.2f, -8.5f), new Vector3(0f, 0.9f, 0f));

        // 셰이더 1개(Glossary/Shader/SimpleLit) → 색만 다른 머티리얼 3개
        Color[] colors = { new Color(0.9f, 0.25f, 0.25f), new Color(0.3f, 0.85f, 0.35f), new Color(0.3f, 0.5f, 1f) };
        string[] names = { "Shader_0401_Red", "Shader_0401_Green", "Shader_0401_Blue" };
        var mats = new Material[3];
        for (int i = 0; i < 3; i++)
        {
            Color c = colors[i];
            mats[i] = ShaderMat(names[i], "SimpleLit", m => m.SetColor("_BaseColor", c));
        }

        // 머티리얼마다 물체 2개 (구 + 큐브) → 공유 관계가 보이게
        var renderers = new Renderer[6];
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 3f;
            var sphere = Part(PrimitiveType.Sphere, $"Sphere_{names[i]}", null, new Vector3(x - 0.65f, 0.5f, 0f), Vector3.one, mats[i]);
            var cube = Part(PrimitiveType.Cube, $"Cube_{names[i]}", null, new Vector3(x + 0.65f, 0.5f, 0.3f), Vector3.one, mats[i]);
            renderers[i * 2] = sphere.GetComponent<Renderer>();
            renderers[i * 2 + 1] = cube.GetComponent<Renderer>();
            WorldLabel($"{i + 1}. {names[i].Replace("Shader_0401_", "Mat_")}", new Vector3(x, 1.6f, 0f), ctx.Camera, 2.6f);
        }

        var demo = new GameObject("ShaderMaterialDemo").AddComponent<ShaderMaterialDemo>();
        SetArray(demo, "materials", mats);
        SetArray(demo, "renderers", renderers);
        Set(demo, "litShader", Shader.Find(ShaderPrefix + "SimpleLit"));
        Set(demo, "unlitShader", Shader.Find(ShaderPrefix + "SimpleUnlit"));

        var panel = SidePanel(ctx, "Shader / Material");
        AddText(panel, "셰이더 = 그리는 규칙(코드)\n머티리얼 = 그 규칙에 넣은 값(색, 텍스처)", 19f, 56f);
        AddButton(panel, "1. Red 선택", null);
        AddButton(panel, "2. Green 선택", null);
        AddButton(panel, "3. Blue 선택", null);
        AddSlider(panel, "선택 머티리얼 Hue", 0f, 1f, 0f, demo.SetHue);
        AddButton(panel, "S : 셰이더 교체", demo.ToggleShader);
        // 정수 인자 버튼은 OnClick(btn, action, arg) 로 연결
        var buttons = panel.Root.GetComponentsInChildren<UnityEngine.UI.Button>();
        for (int i = 0; i < 3; i++) OnClick(buttons[i], demo.SelectMaterial, i);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
