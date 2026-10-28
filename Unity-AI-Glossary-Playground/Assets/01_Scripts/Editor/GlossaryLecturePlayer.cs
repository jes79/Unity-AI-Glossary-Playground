using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// 수업용 강의 플레이어 Windows 빌드 (Builds/ 는 git 에 올리지 않음 → 빌드한 PC가 커맨드센터 [떠나기] 때 GitHub Releases 에 올린다)
// GLOSSARY_LECTURE_PLAYER 정의를 붙여 빌드 → 테두리 없는 창 전체화면으로 시작, F10 으로 종료 (LectureLaunch)
// 실행 예: Builds/LecturePlayer/Glossary.exe -glossaryStart 0105 -glossaryLecture   (그 용어부터 강의)
//          Builds/LecturePlayer/Glossary.exe -glossaryStart 0105                    (강의 없이 그 용어 씬)
//          Builds/LecturePlayer/Glossary.exe                                        (허브부터)
// 명령줄: Unity -batchmode -projectPath Unity-AI-Glossary-Playground -executeMethod GlossaryLecturePlayer.BuildFromCommandLine -quit -logFile <로그>
public static class GlossaryLecturePlayer
{
    public const string OutputPath = "../Builds/LecturePlayer/Glossary.exe"; // 저장소 루트의 Builds (OMP Studio 앱이 찾는 자리)
    public const string Define = "GLOSSARY_LECTURE_PLAYER";

    [MenuItem("Tools/Glossary/Build Lecture Player")]
    private static void BuildFromMenu()
    {
        if (Build()) EditorUtility.RevealInFinder(OutputPath);
    }

    // 배치 모드용: 실패하면 종료 코드 1
    public static void BuildFromCommandLine()
    {
        if (!Build()) EditorApplication.Exit(1);
    }

    public static bool Build()
    {
        // 원본 한글 ttf 는 git 에 없어서(라이선스) 새로 받은 PC에서는 폰트 연결이 끊겨 있다 → 빌드 전에 이어 붙인다 (없으면 강의 글자가 안 보임)
        GlossaryKit.EnsureKoreanFont();
        GlossaryKit.RefreshBuildSettings();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = OutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
            extraScriptingDefines = new[] { Define },
        });
        Debug.Log($"[GlossaryLecturePlayer] {report.summary.result} {report.summary.totalSize / (1024 * 1024)}MB {report.summary.totalTime} → {System.IO.Path.GetFullPath(OutputPath)}");
        return report.summary.result == BuildResult.Succeeded;
    }
}
