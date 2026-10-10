using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0410_Dissolve
{
    [GlossaryScene("0410")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0410_Dissolve")]
    public static void Build()
    {
        var ctx = NewScene3D("0410",
            "Space : 죽기 (1초 디졸브)   R : 되살리기   H : 아래→위 모드\n슬라이더 : 디졸브 시간, 경계 두께\n" +
            "관찰: 왼쪽은 한 프레임에 사라지고, 오른쪽은 노이즈 모양으로 타들어가며 주황 경계가 빛남");
        FrameCamera(ctx, new Vector3(0f, 2.2f, -6.5f), new Vector3(0f, 1.2f, 0f));
        DarkBackground(ctx, new Color(0.06f, 0.06f, 0.09f));
        AddBloomVolume(ctx, "0410", 1f, 1f);

        var plainMat = UrpLit("Shader_0410_Plain", new Color(0.55f, 0.6f, 0.75f), 0.4f);
        var dissolveMat = ShaderMat("Shader_0410_Dissolve", "Dissolve", m =>
        {
            m.SetColor("_BaseColor", new Color(0.55f, 0.6f, 0.75f));
            m.SetTexture("_NoiseTex", NoiseTexture());
            m.SetFloat("_NoiseScale", 1.2f);
            m.SetFloat("_Dissolve", 0f);
            m.SetFloat("_EdgeWidth", 0.08f);
            m.SetColor("_EdgeColor", new Color(1f, 0.35f, 0.05f) * 4f); // 주황 HDR (강도 2)
        });

        var instant = Character("Character_SetActive", new Vector3(-1.8f, 0f, 0f), plainMat);
        var dissolve = Character("Character_Dissolve", new Vector3(1.8f, 0f, 0f), dissolveMat);
        // 이름표는 발 앞 바닥 쪽에 둔다: 머리 위(y 2.9)에 두면 화면 왼쪽 위 정보 패널(600x460) 영역에 가려짐
        WorldLabel("SetActive(false)", new Vector3(-1.8f, 0.25f, -1.3f), ctx.Camera, 2.6f);
        WorldLabel("Dissolve 1초", new Vector3(1.8f, 0.25f, -1.3f), ctx.Camera, 2.6f);

        var demo = new GameObject("DissolveDemo").AddComponent<DissolveDemo>();
        Set(demo, "instantCharacter", instant);
        SetArray(demo, "dissolveRenderers", dissolve.GetComponentsInChildren<Renderer>());

        var panel = SidePanel(ctx, "디졸브 (Dissolve)");
        AddButton(panel, "Space : 죽기", demo.Die);
        AddButton(panel, "R : 되살리기", demo.Revive);
        AddButton(panel, "H : 아래→위 모드", demo.ToggleBottomToTop);
        AddSlider(panel, "디졸브 시간 (초)", 0.2f, 3f, 1f, demo.SetDuration);
        AddSlider(panel, "경계 두께", 0.01f, 0.3f, 0.08f, demo.SetEdgeWidth);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
