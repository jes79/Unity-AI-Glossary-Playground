using GlossaryDemo.AudioDemo;
using UnityEditor;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0901_AudioSourceClip
{
    [GlossaryScene("0901")]
    [MenuItem("Tools/Glossary/Build Scene/09 Audio/0901_AudioSourceClip")]
    public static void Build()
    {
        var ctx = NewScene2D("0901", "Space : 점프 (효과음 재생, 공중에서도 가능)\nT : PlayOneShot ↔ Play 전환\nSpace 를 빠르게 연타해서 소리가 겹치는지(PlayOneShot) 끊기는지(Play) 들어 보세요");

        var jump = CreateToneClip("0901_Jump", 300f, 0.45f, Wave.Square, 0.35f, 900f, 0.005f, 0.15f);
        var land = CreateToneClip("0901_Land", 120f, 0.12f, Wave.Noise, 0.4f, attack: 0.002f, release: 0.1f);

        CreateSpriteObject("Ground", SquareSprite, new Vector3(0f, -3f), new Vector2(20f, 1f), GroundColor);
        // 루트(발밑 피벗) 아래 몸통을 0.5 올려 두면 찌그러짐(스케일)이 발을 기준으로 일어남
        var root = new GameObject("Player");
        root.transform.position = new Vector3(0f, -2.5f, 0f);
        CreateSpriteObject("Body", SquareSprite, new Vector3(0f, 0.5f), Vector2.one, PlayerColor, root.transform, 1);

        // AudioSource = 스피커. 2D 효과음이므로 spatialBlend 0
        var source = root.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;

        var demo = root.AddComponent<JumpSoundDemo>();
        Set(demo, "source", source);
        Set(demo, "jumpClip", jump);
        Set(demo, "landClip", land);

        var canvas = CreateCanvas("Canvas");
        ExplainPanel(canvas, "AudioSource 와 AudioClip",
            "• <b>AudioClip</b> : 소리 파일 (wav/mp3/ogg 를 Project 에 넣으면 생김)\n" +
            "• <b>AudioSource</b> : 소리를 내는 스피커 컴포넌트\n" +
            "• <b>AudioListener</b> : 귀 (보통 Main Camera 에 1개)\n\n" +
            "<color=#9FD0FF>[SerializeField] AudioSource source;\n[SerializeField] AudioClip jumpClip;\n\n" +
            "void Jump() {\n  source.PlayOneShot(jumpClip, 0.8f);\n}</color>\n\n" +
            "효과음은 PlayOneShot, BGM 은 clip 지정 + Play() + loop", 520f, 380f);

        Save(ctx);
    }
}
