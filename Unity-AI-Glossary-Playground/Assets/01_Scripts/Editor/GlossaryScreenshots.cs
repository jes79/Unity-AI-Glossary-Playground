using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// README 스크린샷용 Windows 플레이어 빌드 (Build Settings 의 전체 씬 포함)
// 실행 예: Builds/Screenshots/Glossary.exe -screen-width 1920 -screen-height 1080 -screen-fullscreen 0
//          -glossaryShots <폴더> -glossaryShotList 0000_Hub,0105_CoyoteTime
public static class GlossaryScreenshots
{
    public const string OutputPath = "../Builds/Screenshots/Glossary.exe";

    [MenuItem("Tools/Glossary/Build Screenshot Player")]
    public static void Build()
    {
        GlossaryKit.RefreshBuildSettings();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = OutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        });
        Debug.Log($"[GlossaryScreenshots] {report.summary.result} {report.summary.totalSize / (1024 * 1024)}MB {report.summary.totalTime}");
        if (report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
    }
}
