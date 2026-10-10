using GlossaryDemo.AnimationDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0312_Timeline
{
    [GlossaryScene("0312")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0312_Timeline")]
    public static void Build()
    {
        var ctx = NewScene3D("0312", "시작하면 보스 등장 컷신 자동 재생\nSpace : 처음부터 다시   P : 일시정지/계속   S : 건너뛰기\n카메라 이동 · 보스 애니메이션 · 대사 · 사운드가 한 타임라인에서 맞춰 재생됩니다");
        var canvas = EnsureCanvas(ctx);

        // ── 보스 (BossRoot 의 Animator 를 Timeline 이 구동, 자식 Body 를 애니메이션) ──
        var bossRoot = new GameObject("BossRoot");
        var bossAnimator = bossRoot.AddComponent<Animator>();
        var boss = CreateRig("Body", new Vector3(0f, -4f, 6f), EnemyColor, bossRoot.transform, yaw: 180f);
        Object.DestroyImmediate(boss.Animator);
        boss.Root.transform.localScale = Vector3.one * 2f;
        // 뿔
        Visual(PrimitiveType.Cube, boss.Head, new Vector3(-0.15f, 0.42f, 0f), new Vector3(0.06f, 0.25f, 0.06f), Color.black, "HornL").transform.localRotation = Quaternion.Euler(0, 0, 20);
        Visual(PrimitiveType.Cube, boss.Head, new Vector3(0.15f, 0.42f, 0f), new Vector3(0.06f, 0.25f, 0.06f), Color.black, "HornR").transform.localRotation = Quaternion.Euler(0, 0, -20);

        // 플레이어 (가만히 서서 보스를 봄)
        CreateRig("Player", new Vector3(0f, 0f, -2f), PlayerColor);

        // ── 카메라 리그 (CameraRig 의 Animator 를 Timeline 이 구동, 자식 Main Camera 를 애니메이션) ──
        var camRig = new GameObject("CameraRig");
        var camAnimator = camRig.AddComponent<Animator>();
        ctx.Camera.transform.SetParent(camRig.transform, false);
        Vector3 c0 = new Vector3(2.5f, 1.6f, -5f), c1 = new Vector3(0f, 3.2f, 1.2f), c2 = new Vector3(0f, 4.5f, -9f);
        Vector3 l0 = new Vector3(0f, 1.5f, 4f), l1 = new Vector3(0f, 3.8f, 6f), l2 = new Vector3(0f, 2.5f, 3f);
        ctx.Camera.transform.localPosition = c0;
        ctx.Camera.transform.localRotation = Look(c0, l0);

        // ── 클립 ──
        var bossClip = new ClipBuilder("0312_BossEnter")
            .Pos("Body", T(0f, 2.4f, 2.6f, 6f), V(0, -4f, 6f), V(0, 0.2f, 6f), V(0, 0f, 6f), V(0, 0f, 6f))
            .Rot("Body", T(0f, 6f), V(0, 180, 0), V(0, 180, 0))
            .Rot("Body/Hips/Chest", T(0f, 2.6f, 3.0f, 4.2f, 4.6f, 6f), V(20, 0, 0), V(20, 0, 0), V(-15, 0, 0), V(-15, 0, 0), V(5, 0, 0), V(5, 0, 0))
            .Rot("Body/Hips/Chest/ArmL", T(0f, 2.6f, 3.0f, 4.2f, 4.6f, 6f), V(0, 0, -6), V(0, 0, -6), V(0, 0, -150), V(0, 0, -150), V(-30, 0, -30), V(-30, 0, -30))
            .Rot("Body/Hips/Chest/ArmR", T(0f, 2.6f, 3.0f, 4.2f, 4.6f, 6f), V(0, 0, 6), V(0, 0, 6), V(0, 0, 150), V(0, 0, 150), V(-30, 0, 30), V(-30, 0, 30))
            .Rot("Body/Hips/Chest/Head", T(0f, 2.6f, 3.0f, 4.2f, 6f), V(30, 0, 0), V(30, 0, 0), V(-25, 0, 0), V(-25, 0, 0), V(0, 0, 0))
            .Save();

        var camClip = new ClipBuilder("0312_CameraMove")
            .Pos("Main Camera", T(0f, 2.4f, 4.5f, 6f), c0, c1, c1 + new Vector3(0f, 0f, -0.6f), c2)
            .Rot("Main Camera", T(0f, 2.4f, 4.5f, 6f), Euler(c0, l0), Euler(c1, l1), Euler(c1 + new Vector3(0f, 0f, -0.6f), l1), Euler(c2, l2))
            .Save();

        var rumble = CreateToneClip("0312_Rumble", 55f, 2.5f, Wave.Noise, 0.35f, attack: 0.5f, release: 0.6f);
        var roar = CreateToneClip("0312_Roar", 320f, 1.3f, Wave.Saw, 0.4f, 70f, 0.05f, 0.4f);
        var sting = CreateMelodyClip("0312_Sting", new[] { 220f, 0f, 207.65f, 196f, 185f, 174.61f }, 0.22f, Wave.Square, 0.25f);

        // ── 대사 UI (Activation Track 이 켜고 끔) ──
        var dialogue = CreateImage("DialogueBox", canvas.transform, new Vector2(0f, 120f), new Vector2(1100f, 140f), new Color(0f, 0f, 0f, 0.8f));
        AnchorBottom(dialogue.rectTransform);
        CreateText("Name", dialogue.transform, new Vector2(-420f, 40f), new Vector2(240f, 40f), "어둠의 군주", 28f, new Color(1f, 0.5f, 0.45f), TextAlignmentOptions.Left).fontStyle = FontStyles.Bold;
        CreateText("Line", dialogue.transform, new Vector2(0f, -15f), new Vector2(1040f, 60f), "감히 내 영역에 발을 들이다니… 각오는 되었겠지!", 34f, Color.white);
        dialogue.gameObject.SetActive(false);

        // 진행 막대
        var bar = CreateImage("TimelineProgress", canvas.transform, new Vector2(0f, 30f), new Vector2(900f, 14f), PanelColor, WhiteSprite);
        AnchorBottom(bar.rectTransform);
        var fill = CreateImage("Fill", bar.transform, Vector2.zero, Vector2.zero, AccentColor, WhiteSprite);
        Stretch(fill.rectTransform);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;

        // ── TimelineAsset ──
        var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
        timeline.name = "0312_BossIntro";
        SaveAsset(timeline, Sub, "0312_BossIntro.playable");

        var bossTrack = timeline.CreateTrack<AnimationTrack>(null, "Boss Animation");
        var bc = bossTrack.CreateClip(bossClip);
        bc.start = 0;
        bc.displayName = "보스 등장";

        var camTrack = timeline.CreateTrack<AnimationTrack>(null, "Camera Move");
        var cc = camTrack.CreateClip(camClip);
        cc.start = 0;
        cc.displayName = "카메라 이동";

        var dialogueTrack = timeline.CreateTrack<ActivationTrack>(null, "Dialogue");
        dialogueTrack.postPlaybackState = ActivationTrack.PostPlaybackState.Inactive;
        var dc = dialogueTrack.CreateDefaultClip();
        dc.start = 3.0;
        dc.duration = 3.0;
        dc.displayName = "대사";

        var audioTrack = timeline.CreateTrack<AudioTrack>(null, "Sound");
        audioTrack.CreateClip(rumble).start = 0;
        audioTrack.CreateClip(roar).start = 2.6;
        audioTrack.CreateClip(sting).start = 4.2;

        EditorUtility.SetDirty(timeline);
        AssetDatabase.SaveAssets();

        // ── PlayableDirector + 바인딩 (트랙 ↔ 씬 오브젝트) ──
        var cutscene = new GameObject("Cutscene");
        var audioSource = cutscene.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        var director = cutscene.AddComponent<PlayableDirector>();
        director.playableAsset = timeline;
        director.playOnAwake = true;
        director.extrapolationMode = DirectorWrapMode.Hold;
        director.SetGenericBinding(bossTrack, bossAnimator);
        director.SetGenericBinding(camTrack, camAnimator);
        director.SetGenericBinding(dialogueTrack, dialogue.gameObject);
        director.SetGenericBinding(audioTrack, audioSource);

        var demo = cutscene.AddComponent<TimelineCutsceneDemo>();
        Set(demo, "director", director);
        Set(demo, "progressFill", fill);

        ExplainPanel(canvas, "Timeline 컷신 만들기",
            "1. Window → Sequencing → <b>Timeline</b>\n" +
            "2. 빈 오브젝트 선택 → Create (PlayableDirector + .playable)\n" +
            "3. 트랙 추가 (+ 버튼)\n" +
            "   • Animation Track ← 보스 Animator (등장 동작)\n" +
            "   • Animation Track ← 카메라 (또는 Cinemachine Track)\n" +
            "   • Activation Track ← 대사 UI (켜졌다 꺼짐)\n" +
            "   • Audio Track ← AudioSource (굉음, 효과음)\n" +
            "4. 클립을 시간 위치에 배치, 빨간 녹화 버튼으로 키 기록\n" +
            "5. 코드: director.Play() / Pause() / time\n\n" +
            "생성 에셋: 09_Generated/Animation/0312_BossIntro.playable", 540f, 400f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }

    private static Quaternion Look(Vector3 from, Vector3 to) => Quaternion.LookRotation(to - from);

    // 오일러 커브는 -180~180 범위로 맞춰 보간이 반대 방향으로 돌지 않게 함
    private static Vector3 Euler(Vector3 from, Vector3 to)
    {
        var e = Look(from, to).eulerAngles;
        return new Vector3(Mathf.DeltaAngle(0f, e.x), Mathf.DeltaAngle(0f, e.y), Mathf.DeltaAngle(0f, e.z));
    }
}
