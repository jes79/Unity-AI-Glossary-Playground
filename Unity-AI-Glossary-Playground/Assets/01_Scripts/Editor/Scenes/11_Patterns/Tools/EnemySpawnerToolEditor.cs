using GlossaryDemo.Patterns;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// EnemySpawnerTool 의 Inspector 를 바꾸는 Custom Editor
// 기본 Inspector 아래에 "적 10마리 생성" / "모두 삭제" 버튼을 추가하고, Scene 뷰에 반지름 핸들을 그림
[CustomEditor(typeof(EnemySpawnerTool))]
public class EnemySpawnerToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // 원래 필드들 (Header, Range 그대로)

        var tool = (EnemySpawnerTool)target;
        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox("Custom Editor 로 추가한 버튼입니다. Ctrl+Z 로 되돌릴 수 있어요.", MessageType.Info);

        using (new EditorGUI.DisabledScope(tool.EnemyPrefab == null))
        {
            GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);
            if (GUILayout.Button($"적 {tool.SpawnCount}마리 생성", GUILayout.Height(32)))
                EnemySpawnerActions.Spawn(tool);
            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("모두 삭제", GUILayout.Height(24)))
                EnemySpawnerActions.Clear(tool);
            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.LabelField("현재 배치된 적", $"{tool.CountSpawned()}마리");
        if (GUILayout.Button("Enemy Spawner 창 열기 (EditorWindow)"))
            EnemySpawnerWindow.Open();
    }

    // Scene 뷰: 반지름을 마우스로 끌어 조절하는 핸들
    private void OnSceneGUI()
    {
        var tool = (EnemySpawnerTool)target;
        Handles.color = new Color(1f, 0.6f, 0.3f);
        EditorGUI.BeginChangeCheck();
        float r = Handles.RadiusHandle(Quaternion.identity, tool.transform.position, tool.Radius);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(tool, "반지름 변경");
            tool.Radius = r;
        }
        Handles.Label(tool.transform.position + Vector3.up * 2f, $"생성 반지름 {tool.Radius:0.0}");
    }
}

// Inspector 버튼과 EditorWindow 가 같이 쓰는 실제 동작 (Undo 지원)
public static class EnemySpawnerActions
{
    public static void Spawn(EnemySpawnerTool tool)
    {
        if (Application.isPlaying)
        {
            tool.SpawnRuntime();
            return;
        }

        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName($"적 {tool.SpawnCount}마리 생성");
        int salt = tool.CountSpawned();
        foreach (var p in tool.GetSpawnPositions(salt))
        {
            // 프리팹 연결을 유지한 채 생성 (Instantiate 대신 PrefabUtility.InstantiatePrefab)
            var go = (GameObject)PrefabUtility.InstantiatePrefab(tool.EnemyPrefab, tool.Container);
            go.transform.position = p;
            Vector3 look = tool.transform.position - new Vector3(p.x, tool.transform.position.y, p.z);
            if (look.sqrMagnitude > 0.001f) go.transform.rotation = Quaternion.LookRotation(look);
            Undo.RegisterCreatedObjectUndo(go, "적 생성");
        }
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(tool.gameObject.scene);
    }

    public static void Clear(EnemySpawnerTool tool)
    {
        if (Application.isPlaying)
        {
            tool.ClearRuntime();
            return;
        }

        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("적 모두 삭제");
        var parent = tool.Container;
        for (int i = parent.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(tool.gameObject.scene);
    }
}
