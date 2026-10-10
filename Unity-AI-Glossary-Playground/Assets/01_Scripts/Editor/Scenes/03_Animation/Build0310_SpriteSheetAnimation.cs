using System.IO;
using System.Linq;
using GlossaryDemo.AnimationDemo;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static AnimationBuildUtil;
using static GlossaryKit;

public static class Build0310_SpriteSheetAnimation
{
    private const int Frames = 8;
    private const int Cell = 64;
    private const string SheetPath = SpriteFolder + "/0310_WalkSheet.png";

    [GlossaryScene("0310")]
    [MenuItem("Tools/Glossary/Build Scene/03 Animation/0310_SpriteSheetAnimation")]
    public static void Build()
    {
        var ctx = NewScene2D("0310", "A/D : 걷기 (이동 중에만 재생, 왼쪽은 flipX)\n1/2/3 : 6 / 12 / 24 fps   Space : 제자리 재생 ON/OFF\n위쪽 줄이 잘라낸 8프레임, 노란 칸이 지금 보이는 프레임입니다");

        var sprites = CreateSlicedSheet();

        // 클립: SpriteRenderer.m_Sprite 를 1/12초마다 교체 (ObjectReferenceKeyframe)
        var clip = new AnimationClip { name = "0310_Walk", frameRate = 12f };
        var keys = new ObjectReferenceKeyframe[Frames + 1];
        for (int i = 0; i < Frames; i++) keys[i] = new ObjectReferenceKeyframe { time = i / 12f, value = sprites[i] };
        keys[Frames] = new ObjectReferenceKeyframe { time = Frames / 12f, value = sprites[Frames - 1] }; // 마지막 프레임도 1/12초 유지
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
        var s = AnimationUtility.GetAnimationClipSettings(clip);
        s.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, s);
        SaveAsset(clip, Sub, "0310_Walk.anim");

        var ctrl = NewController("0310_SpriteWalk");
        var sm = ctrl.layers[0].stateMachine;
        sm.defaultState = AddState(sm, "Walk", clip, new Vector3(300, 0));

        // 바닥
        CreateSpriteObject("Ground", SquareSprite, new Vector3(0f, -2.6f), new Vector2(20f, 0.4f), GroundColor);

        // 캐릭터 (픽셀 아트 → 3배 크게)
        var player = CreateSpriteObject("Walker", sprites[0], new Vector3(0f, -2.4f), new Vector2(3f, 3f), Color.white, order: 2);
        var animator = player.gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = ctrl;

        // 잘린 프레임 목록
        var slots = new Transform[Frames];
        float startX = -(Frames - 1) * 0.75f;
        for (int i = 0; i < Frames; i++)
        {
            var bg = CreateSpriteObject($"Slot_{i}", SquareSprite, new Vector3(startX + i * 1.5f, 2.6f), new Vector2(1.3f, 1.3f), PanelColor, order: 0);
            CreateSpriteObject("Frame", sprites[i], new Vector3(0f, -0.5f, 0f), Vector2.one, Color.white, bg.transform, 2); // 피벗이 발밑이라 칸 아래쪽에 맞춤
            slots[i] = bg.transform;
        }
        var highlight = CreateSpriteObject("Highlight", SquareSprite, slots[0].position, new Vector2(1.45f, 1.45f), new Color(1f, 0.85f, 0.2f), order: -1);

        var demo = player.gameObject.AddComponent<SpriteSheetDemo>();
        SetArray(demo, "frames", sprites);
        SetArray(demo, "frameSlots", slots);
        Set(demo, "highlight", highlight.transform);

        var canvas = CreateCanvas("Canvas");
        ExplainPanel(canvas, "스프라이트 시트 자르기",
            "1. 시트 PNG 선택 → Texture Type: Sprite (2D and UI)\n" +
            "2. Sprite Mode: <b>Multiple</b>, Filter: Point (픽셀 아트)\n" +
            "3. Sprite Editor → Slice → Grid By Cell Size 64×64 → Apply\n" +
            "4. 잘린 8장을 모두 선택해 씬(Hierarchy)으로 드래그\n   → 클립과 Animator 가 자동 생성\n" +
            "5. 클립 Samples(fps) 조절\n\n" +
            "코드로: TextureImporter.spriteImportMode = Multiple\n+ 스프라이트 사각형 8개 지정 (이 빌더 참고)", 520f, 330f);

        AssetDatabase.SaveAssets();
        Save(ctx);
    }

    // 8프레임 걷기 시트를 그리고 64×64 격자로 잘라 스프라이트 8개를 만듦
    private static Sprite[] CreateSlicedSheet()
    {
        EnsureFolder(SpriteFolder);
        int w = Cell * Frames, h = Cell;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var px = new Color[w * h];
        for (int f = 0; f < Frames; f++) DrawFrame(px, w, f);
        tex.SetPixels(px);
        tex.Apply();
        File.WriteAllBytes(SheetPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(SheetPath);

        var importer = (TextureImporter)AssetImporter.GetAtPath(SheetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = Cell;
        // 격자 자르기 (Sprite Editor 의 Grid By Cell Size 와 같은 결과)
#pragma warning disable CS0618
        importer.spritesheet = Enumerable.Range(0, Frames).Select(i => new SpriteMetaData
        {
            name = $"0310_Walk_{i}",
            rect = new Rect(i * Cell, 0, Cell, Cell),
            alignment = (int)SpriteAlignment.BottomCenter,
            pivot = new Vector2(0.5f, 0f),
        }).ToArray();
#pragma warning restore CS0618
        importer.SaveAndReimport();

        var sprites = AssetDatabase.LoadAllAssetsAtPath(SheetPath).OfType<Sprite>()
            .Where(sp => sp.name.StartsWith("0310_Walk_")).OrderBy(sp => sp.name).ToArray();
        if (sprites.Length == Frames) return sprites;

        // 예비 경로: 임포터 슬라이스가 동작하지 않으면 Sprite.Create 로 잘라 서브 에셋으로 저장
        Debug.LogWarning("[0310] spritesheet 슬라이스 실패 → Sprite.Create 예비 경로 사용");
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.SaveAndReimport();
        var texAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(SheetPath);
        var result = new Sprite[Frames];
        for (int i = 0; i < Frames; i++)
        {
            result[i] = Sprite.Create(texAsset, new Rect(i * Cell, 0, Cell, Cell), new Vector2(0.5f, 0f), Cell);
            result[i].name = $"0310_Walk_{i}";
        }
        SaveAsset(result[0], Sub, "0310_WalkFrames.asset");
        for (int i = 1; i < Frames; i++) AssetDatabase.AddObjectToAsset(result[i], result[0]);
        AssetDatabase.SaveAssets();
        return result;
    }

    // 프레임 f 의 걷는 사람 그리기: 다리·팔이 sin 곡선으로 앞뒤로 흔들림
    private static void DrawFrame(Color[] px, int w, int f)
    {
        float phase = f / (float)Frames * Mathf.PI * 2f;
        float swing = Mathf.Sin(phase) * 32f * Mathf.Deg2Rad;
        float bob = Mathf.Abs(Mathf.Cos(phase)) * 2f;
        int ox = f * Cell;
        Color body = new Color32(80, 170, 255, 255), dark = new Color32(40, 70, 130, 255), skin = new Color32(255, 214, 170, 255), outline = new Color32(20, 22, 30, 255);

        Vector2 hip = new Vector2(32f, 22f + bob), shoulder = new Vector2(32f, 38f + bob), headC = new Vector2(32f, 47f + bob);
        Vector2 Limb(Vector2 from, float len, float a) => from + new Vector2(Mathf.Sin(a), -Mathf.Cos(a)) * len;

        for (int y = 0; y < Cell; y++)
        for (int x = 0; x < Cell; x++)
        {
            var p = new Vector2(x + 0.5f, y + 0.5f);
            Color c = Color.clear;
            // 뒤쪽 팔·다리 (어둡게) → 몸 → 앞쪽 팔·다리 순서로 덮어 그림
            if (Seg(p, hip, Limb(hip, 20f, -swing), 3.2f)) c = dark;
            if (Seg(p, shoulder, Limb(shoulder, 14f, swing), 2.6f)) c = dark;
            if (Seg(p, hip, shoulder, 5.5f)) c = body;
            if ((p - headC).magnitude < 7.5f) c = skin;
            if ((p - (headC + new Vector2(3.5f, 1f))).magnitude < 1.4f) c = outline; // 눈 (오른쪽을 봄)
            if (Seg(p, hip, Limb(hip, 20f, swing), 3.2f)) c = body * 0.85f + Color.black * 0.15f;
            if (Seg(p, shoulder, Limb(shoulder, 14f, -swing), 2.6f)) c = skin;
            if (c.a > 0f) c.a = 1f;
            px[y * w + ox + x] = c;
        }
    }

    private static bool Seg(Vector2 p, Vector2 a, Vector2 b, float r)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return (p - (a + ab * t)).magnitude < r;
    }
}
