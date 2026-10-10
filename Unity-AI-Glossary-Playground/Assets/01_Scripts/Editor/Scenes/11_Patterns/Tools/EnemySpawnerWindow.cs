using GlossaryDemo.Patterns;
using UnityEditor;
using UnityEngine;

// 나만의 툴 창 (EditorWindow): 씬의 EnemySpawnerTool 을 찾아 설정·생성·삭제
// 메뉴: Tools > Glossary > Patterns > Enemy Spawner Window
public class EnemySpawnerWindow : EditorWindow
{
    private Vector2 scroll;

    [MenuItem("Tools/Glossary/Patterns/Enemy Spawner Window")]
    public static void Open()
    {
        var w = GetWindow<EnemySpawnerWindow>("Enemy Spawner");
        w.minSize = new Vector2(320f, 220f);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("씬의 EnemySpawnerTool 목록", EditorStyles.boldLabel);
        var tools = FindObjectsByType<EnemySpawnerTool>(FindObjectsSortMode.None);
        if (tools.Length == 0)
        {
            EditorGUILayout.HelpBox("열린 씬에 EnemySpawnerTool 이 없습니다.\n1110_EditorExtension 씬을 열어 보세요.", MessageType.Warning);
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var tool in tools)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.ObjectField("대상", tool, typeof(EnemySpawnerTool), true);

                EditorGUI.BeginChangeCheck();
                int count = EditorGUILayout.IntSlider("생성 수", tool.SpawnCount, 1, 50);
                float radius = EditorGUILayout.Slider("반지름", tool.Radius, 1f, 15f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(tool, "Spawner 설정 변경");
                    tool.SpawnCount = count;
                    tool.Radius = radius;
                    EditorUtility.SetDirty(tool);
                }

                EditorGUILayout.LabelField("배치된 적", $"{tool.CountSpawned()}마리");
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button($"적 {tool.SpawnCount}마리 생성")) EnemySpawnerActions.Spawn(tool);
                    if (GUILayout.Button("모두 삭제")) EnemySpawnerActions.Clear(tool);
                    if (GUILayout.Button("선택")) Selection.activeGameObject = tool.gameObject;
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    // 씬이 바뀌면 다시 그림
    private void OnHierarchyChange() => Repaint();
}
