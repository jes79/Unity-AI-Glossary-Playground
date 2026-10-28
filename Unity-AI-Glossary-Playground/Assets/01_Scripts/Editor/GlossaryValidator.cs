using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlossaryDemo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// 생성된 데모 씬 일괄 점검
// - 프로젝트 셰이더 컴파일 오류
// - 씬마다: 빠진 스크립트(Missing Script), 셰이더가 없는(분홍) 머티리얼, 비어 있는 머티리얼 슬롯
// 명령줄: Unity -batchmode -executeMethod GlossaryValidator.BatchValidate -quit
public static class GlossaryValidator
{
    [MenuItem("Tools/Glossary/Validate All Scenes")]
    public static void ValidateAll()
    {
        var problems = new List<string>();

        foreach (string guid in AssetDatabase.FindAssets("t:Shader", new[] { "Assets/02_Shaders" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader == null) continue;
            foreach (var msg in ShaderUtil.GetShaderMessages(shader))
                if (msg.severity == UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error)
                    problems.Add($"[Shader] {path}:{msg.line} {msg.message}");
        }

        // 새 용어를 넣고 GlossaryCatalog.Guide.cs 를 빼먹으면 정보 패널에서 그 줄만 조용히 사라진다.
        // 빠진 것은 여기서 잡는다 (docs/CONVENTIONS.md 참고)
        foreach (var entry in GlossaryCatalog.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Concept))
                problems.Add($"[{entry.Id}] GlossaryCatalog.Guide.cs 에 '개념' 없음");
            if (string.IsNullOrWhiteSpace(entry.Check))
                problems.Add($"[{entry.Id}] GlossaryCatalog.Guide.cs 에 '확인 포인트' 없음");
        }
        foreach (string id in GlossaryCatalog.Guide.Keys)
            if (!GlossaryCatalog.TryFind(id, out _))
                problems.Add($"[{id}] Guide 에는 있는데 카탈로그에 없는 id");

        foreach (var entry in GlossaryCatalog.Entries)
        {
            if (!File.Exists(entry.ScenePath))
            {
                problems.Add($"[{entry.Id}] 씬 없음");
                continue;
            }

            var scene = EditorSceneManager.OpenScene(entry.ScenePath, OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                    if (missing > 0) problems.Add($"[{entry.Id}] Missing Script: {t.name}");

                    var r = t.GetComponent<Renderer>();
                    if (r == null || r is ParticleSystemRenderer psr && !psr.enabled) continue;
                    foreach (var m in r.sharedMaterials)
                    {
                        if (m == null)
                        {
                            if (!(r is ParticleSystemRenderer)) problems.Add($"[{entry.Id}] 빈 머티리얼 슬롯: {t.name}");
                            continue;
                        }
                        if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader" || !m.shader.isSupported)
                            problems.Add($"[{entry.Id}] 분홍 머티리얼: {t.name} ({m.name}, {m.shader?.name})");
                    }
                }
            }
        }

        foreach (string p in problems) Debug.LogWarning("[GlossaryValidator] " + p);
        Debug.Log($"[GlossaryValidator] 완료: 문제 {problems.Count}건");
    }

    public static void BatchValidate() => ValidateAll();
}

public static class GlossaryBatch
{
    // 전체 재생성 + 점검 (명령줄용)
    public static void BuildAndValidate()
    {
        GlossaryKit.BatchBuildAll();
        GlossaryValidator.ValidateAll();
    }

    // 한글 폰트만 복구 (명령줄용)
    public static void SetupFont()
    {
        var fa = GlossaryKit.EnsureKoreanFont();
        Debug.Log($"[GlossaryBatch] 폰트: {fa?.name}, 원본: {(fa != null && fa.sourceFontFile != null ? fa.sourceFontFile.name : "없음")}");
    }

    // 강의 플레이어 빌드 → Builds/LecturePlayer/Glossary.exe (명령줄용, 실패하면 종료 코드 1)
    public static void BuildLecturePlayer()
    {
        if (!GlossaryLecturePlayer.Build()) EditorApplication.Exit(1);
    }
}
