using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0414_CelShading
{
    [GlossaryScene("0414")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0414_CelShading")]
    public static void Build()
    {
        var ctx = NewScene3D("0414",
            "T : 툰 ↔ URP Lit 비교   B : 단계 끊기 ON/OFF   Space : 조명 자동 회전\n슬라이더 : 단계 수(기본 3), 조명 각도\n" +
            "관찰: 조명이 돌 때 명암이 그라데이션 없이 3단계로 뚝뚝 바뀜 (그림자도 단계에 포함)");
        FrameCamera(ctx, new Vector3(0f, 3.4f, -7.5f), new Vector3(0f, 1f, 0f));
        ctx.Light.transform.rotation = Quaternion.Euler(40f, 200f, 0f);

        (string name, PrimitiveType type, Vector3 pos, Vector3 scale, Color color)[] defs =
        {
            ("Sphere", PrimitiveType.Sphere, new Vector3(-3f, 0.9f, 0f), Vector3.one * 1.8f, new Color(1f, 0.55f, 0.35f)),
            ("Capsule", PrimitiveType.Capsule, new Vector3(3f, 1.1f, 0f), new Vector3(1f, 1.1f, 1f), new Color(0.4f, 0.75f, 1f)),
            ("Cube", PrimitiveType.Cube, new Vector3(1.2f, 0.5f, 1.8f), Vector3.one, new Color(0.6f, 0.9f, 0.4f)),
        };

        var targets = new System.Collections.Generic.List<Renderer>();
        var toonMats = new System.Collections.Generic.List<Material>();
        var litMats = new System.Collections.Generic.List<Material>();

        void Add(Renderer r, string key, Color color)
        {
            targets.Add(r);
            toonMats.Add(ShaderMat($"Shader_0414_Toon_{key}", "Toon", m =>
            {
                m.SetColor("_BaseColor", color);
                m.SetColor("_ShadowColor", new Color(0.35f, 0.3f, 0.5f));
                m.SetFloat("_Steps", 3f);
            }));
            litMats.Add(UrpLit($"Shader_0414_Lit_{key}", color, 0.3f));
            r.sharedMaterial = toonMats[toonMats.Count - 1];
        }

        foreach (var d in defs)
        {
            var go = Part(d.type, d.name, null, d.pos, d.scale, null);
            Add(go.GetComponent<Renderer>(), d.name, d.color);
        }

        // 캐릭터 (파츠마다 같은 색)
        var hero = Character("ToonCharacter", new Vector3(-0.6f, 0f, 0.2f), null);
        foreach (var r in hero.GetComponentsInChildren<Renderer>())
            Add(r, "Hero", new Color(1f, 0.85f, 0.55f));
        Spin(hero, new Vector3(0f, 25f, 0f));

        var demo = new GameObject("ToonShadingDemo").AddComponent<ToonShadingDemo>();
        Set(demo, "mainLight", ctx.Light);
        SetArray(demo, "targets", targets.ToArray());
        SetArray(demo, "toonMaterials", toonMats.ToArray());
        SetArray(demo, "litMaterials", litMats.ToArray());

        var panel = SidePanel(ctx, "툰 셰이딩 (Cel Shading)");
        AddButton(panel, "T : 툰 ↔ URP Lit", demo.ToggleToon);
        AddButton(panel, "B : 단계 끊기 ON/OFF", demo.ToggleBanding);
        AddSlider(panel, "명암 단계 수", 2f, 6f, 3f, demo.SetSteps).wholeNumbers = true;
        AddSlider(panel, "조명 각도 (Yaw)", 0f, 360f, 200f, demo.SetLightYaw);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
