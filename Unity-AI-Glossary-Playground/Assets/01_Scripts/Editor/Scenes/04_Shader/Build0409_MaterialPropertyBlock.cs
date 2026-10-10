using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0409_MaterialPropertyBlock
{
    [GlossaryScene("0409")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0409_MaterialPropertyBlock")]
    public static void Build()
    {
        var ctx = NewScene3D("0409",
            "M : 방식 전환 (PropertyBlock ↔ renderer.material 복제)   R : 색 다시 뽑기\nSpace : 색 물결 ON/OFF\n" +
            "관찰: 색은 100마리 모두 다르지만 PropertyBlock 방식은 머티리얼이 1개, 복제 방식은 100개");
        FrameCamera(ctx, new Vector3(0f, 11f, -12f), new Vector3(0f, 0f, 0.5f));

        // 100마리가 공유하는 머티리얼 하나 (흰색)
        var shared = UrpLit("Shader_0409_EnemyShared", Color.white, 0.45f);

        var enemies = new Renderer[100];
        var root = new GameObject("Enemies");
        for (int i = 0; i < 100; i++)
        {
            int x = i % 10, z = i / 10;
            var e = Part(PrimitiveType.Capsule, $"Enemy_{i:000}", root.transform,
                new Vector3((x - 4.5f) * 1.2f, 0.6f, (z - 4.5f) * 1.2f), new Vector3(0.6f, 0.6f, 0.6f), shared);
            Object.DestroyImmediate(e.GetComponent<Collider>());
            enemies[i] = e.GetComponent<Renderer>();
        }

        var demo = new GameObject("PropertyBlockDemo").AddComponent<PropertyBlockDemo>();
        SetArray(demo, "enemies", enemies);
        Set(demo, "sharedMaterial", shared);

        var panel = SidePanel(ctx, "MaterialPropertyBlock");
        AddButton(panel, "M : 방식 전환", demo.ToggleMode);
        AddButton(panel, "R : 색 다시 뽑기", demo.Randomize);
        AddButton(panel, "Space : 색 물결 ON/OFF", demo.ToggleAnimate);
        AddText(panel, "<size=17>코드 핵심\nr.GetPropertyBlock(block);\nblock.SetColor(\"_BaseColor\", c);\nr.SetPropertyBlock(block);</size>", 17f, 100f);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
