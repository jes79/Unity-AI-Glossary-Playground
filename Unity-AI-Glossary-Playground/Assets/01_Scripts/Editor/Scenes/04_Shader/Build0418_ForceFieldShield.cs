using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0418_ForceFieldShield
{
    [GlossaryScene("0418")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0418_ForceFieldShield")]
    public static void Build()
    {
        var ctx = NewScene3D("0418",
            "클릭 : 쉴드 때리기(파문)   I : Intersection ON/OFF 비교\n슬라이더 : 경계 빛 폭\n" +
            "관찰: 쉴드가 바닥·기둥·지나가는 상자와 닿는 선만 밝게 빛남", groundSize: 40f);
        FrameCamera(ctx, new Vector3(0f, 5f, -10f), new Vector3(0f, 1f, 0f));
        DarkBackground(ctx, new Color(0.03f, 0.04f, 0.07f));
        ctx.Light.intensity = 0.7f;
        RequireDepthTexture(ctx.Camera); // Depth Intersection 은 카메라 Depth Texture 가 필요
        AddBloomVolume(ctx, "0418", 0.8f, 1f);

        var shieldMat = ShaderMat("Shader_0418_Shield", "Shield", m =>
        {
            m.SetColor("_ShieldColor", new Color(0.3f, 0.9f, 2.5f));
            m.SetFloat("_IntersectWidth", 0.35f);
        });

        // 쉴드: 지름 7m 구, 중심이 바닥 높이 → 아래 절반은 땅에 묻혀 바닥과 닿는 원이 생김
        var shield = Part(PrimitiveType.Sphere, "Shield", null, new Vector3(0f, 0f, 0f), Vector3.one * 7f, shieldMat);
        shield.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

        // 안에 있는 캐릭터 + 쉴드를 관통하는 기둥 + 들락날락하는 상자
        Character("Player", new Vector3(0f, 0f, 0f), UrpLit("Shader_0418_Player", PlayerColor, 0.4f));
        var pillarMat = UrpLit("Shader_0418_Pillar", new Color(0.5f, 0.5f, 0.55f), 0.3f);
        Part(PrimitiveType.Cylinder, "Pillar", null, new Vector3(2.6f, 1.5f, 1.5f), new Vector3(0.7f, 1.5f, 0.7f), pillarMat);
        var box = Part(PrimitiveType.Cube, "MovingBox", null, new Vector3(4.5f, 1f, -0.5f), Vector3.one * 1.1f, UrpLit("Shader_0418_Box", EnemyColor, 0.3f));
        Object.DestroyImmediate(box.GetComponent<Collider>());

        var demo = new GameObject("ShieldDemo").AddComponent<ShieldDemo>();
        Set(demo, "shield", shield.GetComponent<Renderer>());
        Set(demo, "shieldCollider", shield.GetComponent<Collider>());
        Set(demo, "movingBox", box.transform);

        var panel = SidePanel(ctx, "쉴드 (Depth Intersection)");
        AddButton(panel, "I : Intersection ON/OFF", demo.ToggleIntersection);
        AddSlider(panel, "경계 빛 폭 (m)", 0.05f, 1.5f, 0.35f, demo.SetWidth);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
