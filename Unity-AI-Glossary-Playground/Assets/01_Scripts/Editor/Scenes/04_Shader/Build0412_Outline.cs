using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0412_Outline
{
    [GlossaryScene("0412")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0412_Outline")]
    public static void Build()
    {
        var ctx = NewScene3D("0412",
            "마우스 이동 : 아이템 위에 올리면 노란 아웃라인   A : 전체 표시\n슬라이더 : 아웃라인 두께\n" +
            "관찰: 부풀린 뒷면 껍질(Inverted Hull)이 외곽선이 됨. 각진 상자는 모서리에 틈이 생김");
        FrameCamera(ctx, new Vector3(0f, 4f, -6.5f), new Vector3(0f, 0.8f, 0.3f));

        // 테이블
        Prim(PrimitiveType.Cube, "Table", new Vector3(0f, 0.4f, 0.5f), new Vector3(7f, 0.8f, 3f), new Color(0.45f, 0.32f, 0.22f));

        (string name, PrimitiveType type, Vector3 pos, Vector3 scale, Color color)[] defs =
        {
            ("Potion", PrimitiveType.Sphere, new Vector3(-2.6f, 1.25f, 0.4f), Vector3.one * 0.9f, new Color(0.85f, 0.2f, 0.35f)),
            ("Coin", PrimitiveType.Cylinder, new Vector3(-1.3f, 0.9f, 0.6f), new Vector3(0.8f, 0.08f, 0.8f), new Color(1f, 0.8f, 0.25f)),
            ("Scroll", PrimitiveType.Capsule, new Vector3(0f, 1.05f, 0.5f), new Vector3(0.45f, 0.6f, 0.45f), new Color(0.9f, 0.85f, 0.7f)),
            ("Gem", PrimitiveType.Sphere, new Vector3(1.3f, 1.2f, 0.5f), new Vector3(0.55f, 0.8f, 0.55f), new Color(0.3f, 0.8f, 1f)),
            ("Crate", PrimitiveType.Cube, new Vector3(2.6f, 1.25f, 0.5f), Vector3.one * 0.9f, new Color(0.6f, 0.45f, 0.25f)),
        };

        var items = new Renderer[defs.Length];
        for (int i = 0; i < defs.Length; i++)
        {
            var d = defs[i];
            var color = d.color;
            var mat = ShaderMat($"Shader_0412_{d.name}", "Outline", m =>
            {
                m.SetColor("_BaseColor", color);
                m.SetColor("_OutlineColor", new Color(1f, 0.85f, 0.1f));
                m.SetFloat("_OutlineWidth", 0f); // 평소엔 껍질 없음
            });
            var go = Part(d.type, d.name, null, d.pos, d.scale, mat);
            if (d.type == PrimitiveType.Capsule) go.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            items[i] = go.GetComponent<Renderer>();
        }

        var demo = new GameObject("OutlineHoverDemo").AddComponent<OutlineHoverDemo>();
        SetArray(demo, "items", items);

        var panel = SidePanel(ctx, "아웃라인 (Outline)");
        AddSlider(panel, "아웃라인 두께 (m)", 0.005f, 0.15f, 0.04f, demo.SetWidth);
        AddButton(panel, "A : 전체 표시", demo.ToggleShowAll);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
