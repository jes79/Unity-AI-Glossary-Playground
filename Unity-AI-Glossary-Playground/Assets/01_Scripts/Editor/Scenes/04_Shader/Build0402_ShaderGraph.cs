using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static ShaderBuildUtil;

// 0402 Shader Graph: .shadergraph 를 코드로 만들지 않고, 노드 레시피 + 같은 계산의 HLSL 셰이더로 단계별 결과를 보여 줌
public static class Build0402_ShaderGraph
{
    [GlossaryScene("0402")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0402_ShaderGraph")]
    public static void Build()
    {
        var ctx = NewScene3D("0402",
            "← / → (B / N) : 레시피 단계 이동 (1~4)\n" +
            "관찰: 오른쪽 아래 노드 순서대로 연결할 때마다 구의 모습이 어떻게 바뀌는지\n" +
            "(구는 같은 계산을 손으로 쓴 HLSL 셰이더로 그림)");
        FrameCamera(ctx, new Vector3(0.6f, 2.2f, -5.5f), new Vector3(1.4f, 1.1f, 0f));
        DarkBackground(ctx);

        var mat = ShaderMat("Shader_0402_GraphEquivalent", "GraphEquivalent", m =>
        {
            m.SetTexture("_NoiseTex", NoiseTexture());
            m.SetTextureScale("_NoiseTex", new Vector2(2f, 1f));
            m.SetFloat("_Step", 1f);
        });

        var sphere = Part(PrimitiveType.Sphere, "PulseRimSphere", null, new Vector3(-0.2f, 1.2f, 0f), Vector3.one * 1.8f, mat);
        Spin(sphere, new Vector3(0f, 20f, 0f));

        var demo = new GameObject("ShaderGraphRecipeDemo").AddComponent<ShaderGraphRecipeDemo>();
        Set(demo, "target", sphere.GetComponent<Renderer>());

        var recipe = BottomRightText(ctx, "RecipePanel", new Vector2(860f, 600f), "", 20f);
        Set(demo, "recipeText", recipe);

        var panel = SidePanel(ctx, "Shader Graph 레시피", 360f);
        AddButton(panel, "◀ 이전 단계", demo.PrevStep);
        AddButton(panel, "다음 단계 ▶", demo.NextStep);
        AddText(panel, "<size=17>실제 그래프 파일은 에디터에서\n노드를 연결해 만드는 에셋입니다.</size>", 17f, 48f);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
