using GlossaryDemo.Effects;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0527_DepthOfField
{
    [GlossaryScene("0527")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0527_DepthOfField")]
    public static void Build()
    {
        var ctx = Stage("0527", "E / Space : 대화 시작 · 다음 줄\nM : DOF 모드 Gaussian ↔ Bokeh\n대화하는 동안만 NPC 뒤 배경이 부드럽게 흐려지는지 관찰",
            new Vector3(0f, 1.7f, -4f), new Vector3(4f, 0f, 0f), true, 80f);
        EnablePostProcessing(ctx.Camera);

        var npc = Prim(PrimitiveType.Capsule, "NPC", new Vector3(0.8f, 1f, 1f), Vector3.one, new Color32(230, 180, 90, 255));
        Prim(PrimitiveType.Sphere, "NPC_Head", new Vector3(0.8f, 2.2f, 1f), Vector3.one * 0.5f, new Color32(240, 200, 170, 255));

        // 멀리 있는 배경: 나무(원기둥+구)와 집들
        var rng = new System.Random(7);
        for (int i = 0; i < 24; i++)
        {
            float x = (float)(rng.NextDouble() * 30 - 15), z = (float)(8 + rng.NextDouble() * 25);
            Prim(PrimitiveType.Cylinder, $"Trunk_{i}", new Vector3(x, 1f, z), new Vector3(0.3f, 1f, 0.3f), new Color32(100, 70, 50, 255));
            Prim(PrimitiveType.Sphere, $"Leaves_{i}", new Vector3(x, 2.6f, z), Vector3.one * 2f, new Color32(60, 140, 70, 255));
        }
        for (int i = 0; i < 5; i++)
            Prim(PrimitiveType.Cube, $"House_{i}", new Vector3(-10f + i * 5f, 1.5f, 18f + (i % 2) * 6f), new Vector3(3f, 3f, 3f), new Color32(200, 120, 90, 255));
        ctx.Camera.backgroundColor = new Color32(120, 160, 210, 255);

        // 대화용 Volume: DOF 만 들어 있고 평소 weight 0
        var profile = Profile("Profile_0527_DialogueDOF");
        var dof = AddOverride<DepthOfField>(profile);
        dof.mode.Override(DepthOfFieldMode.Gaussian);
        dof.gaussianStart.Override(6f);
        dof.gaussianEnd.Override(15f);
        dof.gaussianMaxRadius.Override(1.5f);
        dof.highQualitySampling.Override(true);
        dof.focusDistance.Override(5f);
        dof.aperture.Override(2f);
        dof.focalLength.Override(120f);
        SaveProfile(profile);
        var volume = GlobalVolume("Dialogue Volume (DOF)", profile, 10f, 0f);

        // 대화창
        var canvas = EnsureCanvas(ctx).transform;
        var box = CreateImage("DialogueBox", canvas, new Vector2(0f, 200f), new Vector2(1100f, 150f), new Color(0.06f, 0.07f, 0.1f, 0.9f));
        AnchorBottom(box.rectTransform);
        var text = CreateText("Text", box.transform, Vector2.zero, Vector2.zero, "", 30f, Color.white, TextAlignmentOptions.MidlineLeft);
        Stretch(text.rectTransform, 30f);

        var demo = volume.gameObject.AddComponent<DialogueDofDemo>();
        Set(demo, "dialogueVolume", volume);
        Set(demo, "dialogueBox", box.gameObject);
        Set(demo, "dialogueText", text);
        Set(demo, "npc", npc.transform);
        var b = Buttons(ctx, "대화 / 다음", "Gaussian ↔ Bokeh");
        OnClick(b[0], demo.Advance);
        OnClick(b[1], demo.ToggleMode);
        Save(ctx);
    }
}
