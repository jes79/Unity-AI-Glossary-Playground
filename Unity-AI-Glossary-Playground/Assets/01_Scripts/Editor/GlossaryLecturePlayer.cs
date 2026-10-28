using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// 수업용 강의 플레이어 Windows 빌드 (Build Settings 의 전체 씬 포함, 스크린샷 플레이어와 별도 폴더)
// GLOSSARY_LECTURE_PLAYER 정의를 붙여 빌드 → 테두리 없는 창 전체화면으로 시작, 허브에서 Esc 로 종료 (LectureLaunch)
// 실행 예: Builds/LecturePlayer/Glossary.exe -glossaryStart 0105 -glossaryLecture   (그 용어부터 강의)
//          Builds/LecturePlayer/Glossary.exe -glossaryStart 0105                    (그 용어 씬만)
//          Builds/LecturePlayer/Glossary.exe                                        (허브)
// 명령줄: Unity -batchmode -projectPath <프로젝트> -executeMethod GlossaryBatch.BuildLecturePlayer -quit
public static class GlossaryLecturePlayer
{
    public const string OutputPath = "../Builds/LecturePlayer/Glossary.exe";
    public const string Define = "GLOSSARY_LECTURE_PLAYER";

    [MenuItem("Tools/Glossary/Build Lecture Player")]
    public static void BuildFromMenu()
    {
        if (Build()) EditorUtility.RevealInFinder(OutputPath);
    }

    public static bool Build()
    {
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
