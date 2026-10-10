using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// 1113 빌드 설정: 현재 빌드 설정을 읽어 출력하고, 관련 창을 여는 에디터 메뉴
// (설정을 바꾸지는 않음 — 읽기 전용)
public static class BuildSettingsTools
{
    // 현재 설정 요약 (rich text 없이)
    public static string Summary()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"활성 빌드 대상: {EditorUserBuildSettings.activeBuildTarget}");
        sb.AppendLine($"제품 이름: {PlayerSettings.productName}   회사: {PlayerSettings.companyName}   버전: {PlayerSettings.bundleVersion}");
        var scenes = EditorBuildSettings.scenes;
        int enabled = scenes.Count(s => s.enabled);
        sb.AppendLine($"Scene List: {enabled}/{scenes.Length}개 포함" + (scenes.Length > 0 ? $"  (0번: {System.IO.Path.GetFileNameWithoutExtension(scenes[0].path)})" : ""));
        sb.AppendLine($"WebGL 기본 해상도: {PlayerSettings.defaultWebScreenWidth}x{PlayerSettings.defaultWebScreenHeight}   템플릿: {PlayerSettings.WebGL.template}");
        sb.AppendLine($"WebGL 압축: {PlayerSettings.WebGL.compressionFormat}   Decompression Fallback: {PlayerSettings.WebGL.decompressionFallback}");
        sb.Append($"Android 패키지 이름: {PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android)}");
        return sb.ToString();
    }

    [MenuItem("Tools/Glossary/Patterns/Build/Print Build Settings")]
    public static void Print()
    {
        Debug.Log("[1113] 현재 빌드 설정\n" + Summary());
        var scenes = EditorBuildSettings.scenes;
        var sb = new StringBuilder("[1113] Scene List (Build Profiles 의 순서)\n");
        for (int i = 0; i < scenes.Length; i++)
            sb.AppendLine($"{i,3} {(scenes[i].enabled ? "[x]" : "[ ]")} {scenes[i].path}");
        Debug.Log(sb.ToString());
    }

    [MenuItem("Tools/Glossary/Patterns/Build/Open Build Profiles Window")]
    public static void OpenBuildWindow()
    {
        // Unity 6: File > Build Profiles (이전 버전: File > Build Settings...)
        if (!EditorApplication.ExecuteMenuItem("File/Build Profiles"))
            EditorApplication.ExecuteMenuItem("File/Build Settings...");
    }

    [MenuItem("Tools/Glossary/Patterns/Build/Open Player Settings")]
    public static void OpenPlayerSettings() => SettingsService.OpenProjectSettings("Project/Player");
}
