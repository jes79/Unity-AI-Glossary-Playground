using GlossaryDemo.ShaderDemo;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static ShaderBuildUtil;

public static class Build0413_HitFlash
{
    [GlossaryScene("0413")]
    [MenuItem("Tools/Glossary/Build Scene/04 Shader/0413_HitFlash")]
    public static void Build()
    {
        var ctx = NewScene3D("0413",
            "클릭 : 적 때리기   Space : 아무 적이나 때리기\nF : 플래시 ON/OFF 비교   G : 딱 끊김 ↔ 서서히 빠짐   슬라이더 : 지속 시간\n" +
            "관찰: 머티리얼을 공유해도 맞은 한 마리만 0.1초 하얗게 번쩍임");
        FrameCamera(ctx, new Vector3(0f, 3.2f, -8f), new Vector3(0f, 1f, 0f));

        var mat = ShaderMat("Shader_0413_HitFlash", "HitFlash", m =>
        {
            m.SetColor("_BaseColor", new Color(0.85f, 0.35f, 0.3f));
            m.SetColor("_FlashColor", Color.white);
            m.SetFloat("_FlashAmount", 0f);
        });

        var enemies = new Transform[5];
        for (int i = 0; i < enemies.Length; i++)
            enemies[i] = Character($"Enemy_{i}", new Vector3((i - 2) * 1.9f, 0f, (i % 2) * 0.8f), mat).transform;

        var demo = new GameObject("HitFlashDemo").AddComponent<HitFlashDemo>();
        SetArray(demo, "enemies", enemies);
        var audio = demo.gameObject.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.clip = CreateToneClip("Shader_HitSnd", 520f, 0.09f, Wave.Square, 0.35f, 180f, 0.002f, 0.06f);
        Set(demo, "hitSound", audio);

        var panel = SidePanel(ctx, "피격 플래시 (Hit Flash)");
        AddButton(panel, "Space : 때리기", demo.HitRandom);
        AddButton(panel, "F : 플래시 ON/OFF", demo.ToggleFlash);
        AddSlider(panel, "플래시 지속 시간 (초)", 0.02f, 0.5f, 0.1f, demo.SetDuration);

        AssetDatabase.SaveAssets(); // 빌드 후 수정한 머티리얼 값까지 디스크에 저장
        Save(ctx);
    }
}
