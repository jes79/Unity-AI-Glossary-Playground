using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0819_Localization
{
    [GlossaryScene("0819")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0819_Localization")]
    public static void Build()
    {
        // 테이블 에셋은 새 씬을 만든 뒤에 불러온다: NewScene(Single) 이 참조 없는 에셋을 언로드해서
        // 먼저 만든 참조가 끊어지면 씬에 table: null 로 저장되어 빌드에서 NullReference 가 났음
        var ctx = NewSceneUI("0819", "[한국어] / [English] 버튼 또는 L : 언어 전환   G : 골드 +100\n왼쪽 메뉴의 글자는 모두 '키'만 가지고 있고, 문자열 테이블에서 현재 언어 문장을 가져옴\n이 프로젝트에는 Localization 패키지가 없어 같은 구조의 간단 버전으로 시연\n오른쪽 = 실제 Unity Localization 패키지로 하는 방법");
        var table = EnsureTable();
        var root = ctx.Canvas.transform;
        var demo = new GameObject("LocalizationDemo").AddComponent<LocalizationDemo>();
        Set(demo, "table", table);

        // ── 게임 메뉴 예시 (왼쪽) ──
        var menu = Panel(root, "GameMenu", new Vector2(CenterX - 300f, -20f), new Vector2(600f, 860f), new Color32(30, 36, 56, 255));
        Loc(Label(menu.transform, "Title", new Vector2(0f, 340f), new Vector2(560f, 80f), "", 48f, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.4f)), "menu_title").fontStyle = FontStyles.Bold;
        string[] keys = { "menu_start", "menu_options", "menu_quit" };
        for (int i = 0; i < keys.Length; i++)
        {
            var b = SmallButton(menu.transform, $"Btn_{keys[i]}", new Vector2(0f, 200f - i * 95f), new Vector2(380f, 76f), "", null, 30f);
            Loc(b.GetComponentInChildren<TextMeshProUGUI>(), keys[i]);
        }
        Loc(Label(menu.transform, "Greeting", new Vector2(0f, -110f), new Vector2(540f, 90f), "", 24f, TextAlignmentOptions.Center), "greeting");
        var gold = Label(menu.transform, "Gold", new Vector2(0f, -185f), new Vector2(540f, 40f), "", 26f, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.35f));
        var date = Label(menu.transform, "Date", new Vector2(0f, -230f), new Vector2(540f, 40f), "", 22f, TextAlignmentOptions.Center, SubTextColor);
        Loc(Label(menu.transform, "Fallback", new Vector2(0f, -290f), new Vector2(540f, 60f), "", 18f, TextAlignmentOptions.Center, new Color(0.7f, 0.75f, 0.85f)), "only_korean");

        var ko = SmallButton(menu.transform, "Korean", new Vector2(-110f, -375f), new Vector2(200f, 64f), "한국어");
        var en = SmallButton(menu.transform, "English", new Vector2(110f, -375f), new Vector2(200f, 64f), "English", new Color32(70, 160, 110, 255));
        OnClick(ko, demo.SetKorean);
        OnClick(en, demo.SetEnglish);
        Set(demo, "goldText", gold);
        Set(demo, "dateText", date);

        // ── 실제 패키지 사용법 (오른쪽) ──
        var help = Panel(root, "PackageGuide", new Vector2(CenterX + 330f, -20f), new Vector2(620f, 860f), new Color32(20, 22, 28, 240));
        Label(help.transform, "Guide", new Vector2(0f, 0f), new Vector2(580f, 820f),
            "<b><color=#7FB2FF>Unity Localization 패키지로 하는 법</color></b>\n\n" +
            "<b>1.</b> Package Manager → <b>Localization</b> 설치\n" +
            "<b>2.</b> Project Settings › Localization → Localization Settings 생성\n" +
            "    Locale Generator 로 <b>Korean (ko)</b>, <b>English (en)</b> 추가\n" +
            "<b>3.</b> Window › Asset Management › Localization Tables\n" +
            "    → <b>String Table Collection</b> 생성, 키와 번역 입력\n" +
            "    (이 데모의 LocalizationTable 에셋과 같은 역할)\n" +
            "<b>4.</b> 텍스트 오브젝트에 <b>Localize String Event</b> 추가\n" +
            "    → 테이블·키 선택 → Update String 을 TMP 텍스트에 연결\n" +
            "    (이 데모의 LocalizedText 와 같은 역할)\n" +
            "<b>5.</b> 언어 전환 코드\n" +
            "<color=#9BE89B><size=85%>LocalizationSettings.SelectedLocale =\n  LocalizationSettings.AvailableLocales.GetLocale(\"en\");</size></color>\n" +
            "<b>6.</b> 숫자 끼워 넣기: <b>Smart String</b> \"보유 골드: {0} G\"\n" +
            "<b>7.</b> 언어별 폰트·이미지: <b>Asset Table</b>\n" +
            "    (한글·일본어 등은 TMP 폰트에 글자가 있어야 함)\n\n" +
            "<color=#9AA3B5><size=85%>빠진 번역은 Fallback(기본 언어)으로 표시 · 선택한 언어는 PlayerPrefs 에 저장</size></color>",
            20f, TextAlignmentOptions.TopLeft);

        Save(ctx);
    }

    private static TextMeshProUGUI Loc(TextMeshProUGUI text, string key)
    {
        var lt = text.gameObject.AddComponent<LocalizedText>();
        SetString(lt, "key", key);
        return text;
    }

    private static LocalizationTable EnsureTable()
    {
        string folder = $"{GeneratedFolder}/{Sub}";
        EnsureFolder(folder);
        string path = $"{folder}/StringTable_0819.asset";
        var t = AssetDatabase.LoadAssetAtPath<LocalizationTable>(path);
        if (t == null)
        {
            t = ScriptableObject.CreateInstance<LocalizationTable>();
            AssetDatabase.CreateAsset(t, path);
        }
        t.entries.Clear();
        void Add(string key, string ko, string en) => t.entries.Add(new LocalizationTable.Entry { key = key, korean = ko, english = en });
        Add("menu_title", "용사의 모험", "Hero's Adventure");
        Add("menu_start", "게임 시작", "Start Game");
        Add("menu_options", "설정", "Options");
        Add("menu_quit", "종료", "Quit");
        Add("greeting", "어서 오세요, 모험가님!\n오늘도 좋은 하루 되세요.", "Welcome, adventurer!\nHave a great day.");
        Add("gold_amount", "보유 골드: {0} G", "Gold: {0} G");
        Add("today", "오늘은 {0} 입니다", "Today is {0}");
        Add("only_korean", "이 문장은 영어 번역이 비어 있어서\n영어에서도 한국어로 표시됩니다 (Fallback)", "");
        EditorUtility.SetDirty(t);
        AssetDatabase.SaveAssets();
        return t;
    }
}
