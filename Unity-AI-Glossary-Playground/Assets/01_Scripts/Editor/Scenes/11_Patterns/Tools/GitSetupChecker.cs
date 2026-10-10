using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// 1112 버전 관리: 이 프로젝트의 Git 관련 설정을 검사하는 에디터 도구
// 메뉴 Tools > Glossary > Patterns > Git > ...
public static class GitSetupChecker
{
    // .gitignore 에서 찾아 설명할 핵심 규칙 (패턴 조각, 설명)
    private static readonly (string pattern, string why)[] Highlights =
    {
        ("[Ll]ibrary/", "Unity 가 다시 만드는 캐시(수 GB) — 절대 올리지 않음"),
        ("[Tt]emp/", "실행 중 임시 파일"),
        ("[Oo]bj/", "컴파일 중간 파일"),
        ("[Bb]uild/", "빌드 결과물"),
        ("[Bb]uilds/", "빌드 결과물"),
        ("[Ll]ogs/", "에디터 로그"),
        ("[Uu]ser[Ss]ettings/", "개인 에디터 설정 (사람마다 다름)"),
        ("*.csproj", "IDE 가 자동 생성하는 프로젝트 파일"),
        ("*.sln", "IDE 솔루션 파일 (자동 생성)"),
        (".vs/", "Visual Studio 캐시"),
        ("*.apk", "안드로이드 빌드 결과"),
        ("malgun.ttf", "이 프로젝트 전용: 재배포 불가 폰트 제외"),
    };

    public static string FindRepoFile(string fileName)
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); // Unity 프로젝트 폴더
        for (int i = 0; i < 4 && dir != null; i++, dir = dir.Parent)
        {
            string p = Path.Combine(dir.FullName, fileName);
            if (File.Exists(p)) return p;
        }
        return null;
    }

    // .gitignore 실제 내용에서 핵심 줄을 뽑아 설명을 붙임 (rich text)
    public static string GitignoreHighlights()
    {
        string path = FindRepoFile(".gitignore");
        if (path == null) return "<color=#FF8A65>.gitignore 를 찾지 못했습니다.</color>\nGitHub 의 Unity.gitignore 템플릿을 저장소 최상위에 두세요.";

        var lines = File.ReadAllLines(path).Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith("#")).ToList();
        var sb = new StringBuilder();
        sb.Append($"<color=#999>{path.Replace('\\', '/')}  ({lines.Count}개 규칙)</color>\n");
        foreach (var (pattern, why) in Highlights)
        {
            string line = lines.FirstOrDefault(l => l.Contains(pattern));
            if (line == null) continue;
            sb.Append($"<color=#9BE89B>{Escape(line)}</color>   ← {why}\n");
        }
        return sb.ToString().TrimEnd();
    }

    // 체크리스트 (● / × + 설명)
    public static List<string> Checklist()
    {
        var list = new List<string>();
        string gi = FindRepoFile(".gitignore");
        list.Add(Mark(gi != null, ".gitignore 있음"));
        bool ignoresLibrary = gi != null && File.ReadAllText(gi).Contains("ibrary/");
        list.Add(Mark(ignoresLibrary, "Library/ 무시 규칙"));

        string ga = FindRepoFile(".gitattributes");
        bool lfs = ga != null && File.ReadAllText(ga).Contains("filter=lfs");
        list.Add(lfs ? Mark(true, "Git LFS 규칙 (.gitattributes)")
                     : $"<color=#FFD54F>△</color> Git LFS 미설정 ({(ga != null ? ".gitattributes 는 있음" : ".gitattributes 없음")}) — 큰 이미지·사운드가 많으면 권장");

        bool forceText = EditorSettings.serializationMode == SerializationMode.ForceText;
        list.Add(Mark(forceText, $"Asset Serialization = {EditorSettings.serializationMode} (Force Text 권장)"));
        string meta = VersionControlSettings.mode;
        list.Add(Mark(meta == "Visible Meta Files", $"Version Control Mode = {meta} (.meta 파일도 함께 커밋)"));
        return list;
    }

    private static string Mark(bool ok, string text) => ok ? $"<color=#9BE89B>●</color> {text}" : $"<color=#FF6B6B>×</color> {text}";

    // TMP 리치 텍스트에서 < > 가 태그로 해석되지 않도록
    private static string Escape(string s) => s.Replace("<", "<noparse><</noparse>");

    [MenuItem("Tools/Glossary/Patterns/Git/Check Version Control Setup")]
    public static void LogCheck()
    {
        var sb = new StringBuilder("[1112] 버전 관리 설정 검사\n");
        foreach (var line in Checklist()) sb.Append(StripTags(line)).Append('\n');
        Debug.Log(sb.ToString());
    }

    [MenuItem("Tools/Glossary/Patterns/Git/Open .gitignore")]
    public static void OpenGitignore()
    {
        string path = FindRepoFile(".gitignore");
        if (path != null) EditorUtility.OpenWithDefaultApp(path);
        else Debug.LogWarning(".gitignore 를 찾지 못함");
    }

    private static string StripTags(string s) => System.Text.RegularExpressions.Regex.Replace(s, "<.*?>", "");
}
