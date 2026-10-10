using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0604_TextMeshPro
{
    private const string LatinFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    [GlossaryScene("0604")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0604_TextMeshPro")]
    public static void Build()
    {
        var ctx = NewSceneUI("0604",
            "입력창에 한글을 입력 → 오른쪽 아틀라스에 새 글자가 그려짐 (동적 폰트)\n" +
            "F : 한글 폰트 ↔ 영문 전용 폰트(한글 □)   Z : 아틀라스 확대\n← / → : 폰트 에셋 만드는 법 페이지 넘기기");
        var root = ctx.Canvas.transform;
        var font = EnsureKoreanFont();
        var latin = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LatinFontPath);

        // ── 샘플 텍스트 (폰트 전환 대상) ──
        var fontLabel = Body(root, "FontLabel", new Vector2(10f, 490f), new Vector2(700f, 36f), "", 22f, TextAlignmentOptions.Left, TitleColor);
        var sample = CreateText("SampleText", root, new Vector2(10f, 445f), new Vector2(700f, 50f), "안녕하세요, TextMeshPro! 가나다 ABC 123", 34f, Color.white, TextAlignmentOptions.Left);
        sample.enableAutoSizing = true;   // 폰트를 바꿔도 한 줄에 맞춤
        sample.fontSizeMin = 20f;
        sample.fontSizeMax = 34f;

        // ── 크기별 선명도 (SDF 는 확대해도 깨지지 않음) ──
        CreateText("Size18", root, new Vector2(10f, 400f), new Vector2(700f, 30f), "18pt  SDF 폰트는 작게 써도", 18f, TextColor, TextAlignmentOptions.Left);
        CreateText("Size32", root, new Vector2(10f, 360f), new Vector2(700f, 46f), "32pt  크게 써도 선명함", 32f, TextColor, TextAlignmentOptions.Left);
        CreateText("Size64", root, new Vector2(10f, 285f), new Vector2(700f, 80f), "64pt  큰 제목", 64f, TextColor, TextAlignmentOptions.Left).fontStyle = FontStyles.Bold;

        // ── 머티리얼 프리셋: 같은 폰트 아틀라스에 외곽선·그림자만 다른 머티리얼 ──
        var outlineMat = new Material(font.material);
        outlineMat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.25f);
        outlineMat.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.1f, 0.15f, 0.4f));
        outlineMat.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        outlineMat.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.7f));
        outlineMat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.6f);
        outlineMat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.6f);
        outlineMat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.3f);
        outlineMat = SaveAsset(outlineMat, "06_UI", "GlossaryFont_OutlineShadow.mat");
        var fx = CreateText("OutlineText", root, new Vector2(10f, 205f), new Vector2(700f, 60f), "외곽선 + 그림자 (Material Preset)", 36f, new Color(1f, 0.9f, 0.5f), TextAlignmentOptions.Left);
        fx.fontSharedMaterial = outlineMat;
        fx.fontStyle = FontStyles.Bold;
        fx.enableAutoSizing = true;   // 한 줄(700px)에 맞춤 → 위아래 텍스트와 겹치지 않게
        fx.fontSizeMin = 20f;
        fx.fontSizeMax = 36f;

        // ── 입력창 (TMP_InputField) ──
        var inputBg = Panel(root, "InputField", new Vector2(10f, 130f), new Vector2(700f, 64f), new Color32(30, 33, 42, 255));
        var area = CreateRect("Text Area", inputBg.transform, Vector2.zero, Vector2.zero);
        Stretch(area, 0f);
        area.offsetMin = new Vector2(16f, 8f);
        area.offsetMax = new Vector2(-16f, -8f);
        area.gameObject.AddComponent<RectMask2D>();
        var placeholder = CreateText("Placeholder", area, Vector2.zero, Vector2.zero, "여기에 한글을 입력해 보세요 (예: 뷁 똠 햏)", 28f, SubTextColor, TextAlignmentOptions.Left);
        Stretch(placeholder.rectTransform);
        placeholder.fontStyle = FontStyles.Italic;
        var inputText = CreateText("Text", area, Vector2.zero, Vector2.zero, "", 28f, Color.white, TextAlignmentOptions.Left);
        Stretch(inputText.rectTransform);
        var input = inputBg.gameObject.AddComponent<TMP_InputField>();
        input.targetGraphic = inputBg;
        input.textViewport = area;
        input.textComponent = inputText;
        input.placeholder = placeholder;
        input.fontAsset = font;
        input.pointSize = 28f;

        var preview = CreateText("Preview", root, new Vector2(10f, 30f), new Vector2(700f, 110f), "", 56f, Color.white, TextAlignmentOptions.Left);
        preview.richText = true;
        // 입력 길이와 상관없이 칸(700x110) 안에 들어오게: 자동 크기 + 넘치면 잘라냄 (아래 가이드 패널과 겹치지 않게)
        preview.enableAutoSizing = true;
        preview.fontSizeMin = 22f;
        preview.fontSizeMax = 56f;
        preview.overflowMode = TextOverflowModes.Truncate;

        // ── 아틀라스 텍스처 보기 ──
        var atlasTitle = Body(root, "AtlasLabel", new Vector2(650f, 470f), new Vector2(460f, 36f), "", 22f, TextAlignmentOptions.Left, TitleColor);
        var atlasBg = CreateImage("AtlasBackground", root, new Vector2(650f, 230f), new Vector2(440f, 440f), Color.white);
        var atlas = atlasBg.gameObject.AddComponent<RectMask2D>();
        var atlasRaw = CreateRect("AtlasView", atlasBg.transform, Vector2.zero, Vector2.zero);
        Stretch(atlasRaw);
        var raw = atlasRaw.gameObject.AddComponent<RawImage>();
        raw.color = Color.black; // Alpha8 아틀라스를 흰 바탕 위 검은 글자로 표시
        raw.texture = font.atlasTexture;
        raw.raycastTarget = false;

        // ── 폰트 에셋 만드는 법 (단계별) ──
        PatternsBuildUtil.CreateGuide(root, "FontAssetGuide", new Vector2(420f, -310f), new Vector2(920f, 400f),
            ("1. 폰트 파일 준비",
             "• 한글이 들어 있는 .ttf / .otf 파일을 Assets 아래로 복사합니다.\n" +
             "• <color=#FFB74D>라이선스 확인</color>: 게임에 넣어 배포해도 되는 폰트인지 확인 (예: 본고딕·나눔·Pretendard 는 OFL).\n" +
             "• 시스템 폰트(맑은 고딕 등)는 재배포가 막혀 있는 경우가 많습니다."),
            ("2. Font Asset Creator 열기",
             "• 메뉴 <b>Window > TextMeshPro > Font Asset Creator</b>\n" +
             "• Source Font File: 1단계의 폰트\n" +
             "• Sampling Point Size: Auto Sizing (또는 64~90)\n" +
             "• Padding: 5~9 (외곽선·그림자를 쓸 거면 넉넉히)\n" +
             "• Packing Method: Optimum,  Atlas Resolution: 4096 x 4096\n" +
             "• Render Mode: SDFAA"),
            ("3. 한글 글자 범위 지정",
             "• Character Set: <b>Custom Range</b>\n" +
             "  <color=#9BE89B>32-126,44032-55203,12593-12643,8200-9900</color>\n" +
             "  (영문·숫자 + 완성형 한글 11,172자 + 자모 + 기호)\n" +
             "• 용량이 부담되면 <b>Characters from File</b> 로 자주 쓰는 2,350자(KS X 1001) 목록만 넣습니다.\n" +
             "• Generate Font Atlas → Save 로 TMP_FontAsset(.asset) 저장"),
            ("4. 동적 폰트 (이 씬의 방식)",
             "• Project 창에서 폰트 파일 우클릭 > <b>Create > TextMeshPro > Font Asset</b>\n" +
             "• 인스펙터에서 Atlas Population Mode = <b>Dynamic</b>\n" +
             "• 미리 굽지 않고, 화면에 처음 나오는 글자를 실행 중에 아틀라스에 추가합니다.\n" +
             "  → 오른쪽 아틀라스 그림이 입력할 때마다 채워지는 것이 이 동작입니다.\n" +
             "• 원본 폰트 파일이 빌드에 포함되므로 용량이 늘어나는 점에 주의"),
            ("5. 기본 폰트 / 폴백 지정",
             "• <b>Project Settings > TextMesh Pro > Settings</b>\n" +
             "  - Default Font Asset: 한글 폰트 에셋 → 새로 만드는 모든 TMP 텍스트에 적용\n" +
             "  - Fallback Font Assets: 영문 폰트를 메인으로 쓰고 한글을 폴백으로 붙여도 됨\n" +
             "• 폰트 에셋의 Fallback Font Assets 목록에 넣으면 그 폰트에 없는 글자를 대신 찾아 줍니다."),
            ("6. 이 프로젝트에서는 (코드로 자동 생성)",
             "• <b>GlossaryKit.EnsureKoreanFont()</b>  (메뉴 Tools > Glossary > Setup Korean Font)\n" +
             "  1) Windows 의 맑은 고딕(malgun.ttf)을 Assets/06_Fonts 로 복사\n" +
             "  2) <color=#9BE89B>TMP_FontAsset.CreateFontAsset(font, 64, 6, SDFAA, 2048, 2048, Dynamic)</color>\n" +
             "  3) Assets/Resources/Fonts/GlossaryFont.asset 로 저장 (아틀라스·머티리얼을 하위 에셋으로)\n" +
             "  4) TMP Settings 의 Default Font Asset 으로 지정\n" +
             "• 런타임에 만드는 텍스트는 GlossaryInfoPanel.Font (Resources.Load) 를 넣습니다.\n" +
             "• malgun.ttf 는 재배포 불가라 .gitignore 처리, 없으면 메뉴가 다시 복사합니다."));

        var buttons = ButtonRow(root, new Vector2(650f, -60f), 210f, 56f, 22f, "폰트 전환 (F)", "아틀라스 확대 (Z)");

        var demo = new GameObject("TmpFontDemo").AddComponent<TmpFontDemo>();
        Set(demo, "koreanFont", font);
        Set(demo, "latinOnlyFont", latin);
        Set(demo, "sampleText", sample);
        Set(demo, "fontNameLabel", fontLabel);
        Set(demo, "input", input);
        Set(demo, "previewText", preview);
        Set(demo, "atlasView", raw);
        Set(demo, "atlasZoomLabel", atlasTitle);
        OnClick(buttons[0], demo.ToggleFont);
        OnClick(buttons[1], demo.ToggleAtlasZoom);

        Save(ctx);
    }
}
