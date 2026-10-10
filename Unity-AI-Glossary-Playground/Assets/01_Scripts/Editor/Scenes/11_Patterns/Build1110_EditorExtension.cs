using GlossaryDemo.Patterns;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build1110_EditorExtension
{
    [GlossaryScene("1110")]
    [MenuItem("Tools/Glossary/Build Scene/11 Patterns/1110_EditorExtension")]
    public static void Build()
    {
        var ctx = NewScene3D("1110",
            "<b>에디터에서(플레이 전)</b>: Hierarchy 의 EnemySpawner 선택 → Inspector 의 '적 10마리 생성' 버튼 (Ctrl+Z 가능)\n" +
            "메뉴 Tools > Glossary > Patterns > Enemy Spawner Window : 같은 기능의 EditorWindow\n" +
            "플레이 중: G = 생성, X = 모두 삭제 (같은 위치 계산을 런타임 코드로)", true, 30f);
        PatternsBuildUtil.LookAt(ctx.Camera, new Vector3(0f, 14f, -13f), new Vector3(0f, 0f, 0f));

        // 생성할 적 프리팹
        var enemy = Prim(PrimitiveType.Capsule, "Enemy1110", Vector3.zero, Vector3.one, EnemyColor);
        enemy.layer = LayerMask.NameToLayer("Enemy");
        enemy.tag = "Enemy";
        var nose = Prim(PrimitiveType.Cube, "Nose", new Vector3(0f, 0.4f, 0.5f), new Vector3(0.3f, 0.2f, 0.3f), Color.black, enemy.transform);
        Object.DestroyImmediate(nose.GetComponent<Collider>());
        var prefab = SavePrefab(enemy, "11_Patterns", "Enemy1110");

        var spawnerGo = new GameObject("EnemySpawner (선택 → Inspector 버튼)");
        var container = new GameObject("Enemies").transform;
        container.SetParent(spawnerGo.transform, false);
        var tool = spawnerGo.AddComponent<EnemySpawnerTool>();
        Set(tool, "enemyPrefab", prefab);
        Set(tool, "container", container);
        var center = Prim(PrimitiveType.Cylinder, "SpawnCenter", new Vector3(0f, 0.02f, 0f), new Vector3(0.6f, 0.02f, 0.6f), new Color(1f, 0.6f, 0.3f));
        Object.DestroyImmediate(center.GetComponent<Collider>());

        var canvas = CreateCanvas("UI Canvas");
        var buttons = CreateButtonRow(canvas.transform, 60f, "적 10마리 생성 (G)", "모두 삭제 (X)");
        OnClick(buttons[0], tool.SpawnRuntime);
        OnClick(buttons[1], tool.ClearRuntime);

        PatternsBuildUtil.CreateGuide(canvas.transform, "EditorGuide", new Vector2(700f, 150f), new Vector2(460f, 560f),
            ("1. Custom Editor",
             "<color=#9BE89B>[CustomEditor(typeof(EnemySpawnerTool))]\nclass EnemySpawnerToolEditor : Editor\n{\n  public override void OnInspectorGUI()\n  {\n    DrawDefaultInspector();\n    if (GUILayout.Button(\"적 10마리 생성\"))\n      Spawn();\n  }\n}</color>\n\n" +
             "• 파일은 반드시 <b>Editor</b> 폴더에\n  (빌드에서 빠짐)"),
            ("2. Undo 지원",
             "에디터에서 만든 오브젝트는\n<color=#9BE89B>Undo.RegisterCreatedObjectUndo(go, \"적 생성\");</color>\n\n지울 때는\n<color=#9BE89B>Undo.DestroyObjectImmediate(go);</color>\n\n" +
             "프리팹 연결 유지:\n<color=#9BE89B>PrefabUtility.InstantiatePrefab(prefab)</color>\n\n씬 저장 표시:\n<color=#9BE89B>EditorSceneManager.MarkSceneDirty()</color>"),
            ("3. EditorWindow",
             "<color=#9BE89B>class EnemySpawnerWindow : EditorWindow\n{\n  [MenuItem(\"Tools/.../Enemy Spawner Window\")]\n  static void Open() =>\n    GetWindow<EnemySpawnerWindow>();\n\n  void OnGUI() { 슬라이더·버튼 }\n}</color>\n\n" +
             "• 메뉴에서 여는 독립 창\n• 여러 오브젝트를 한 번에 다루는 툴에 적합"),
            ("4. Scene 뷰 핸들",
             "Custom Editor 의 <color=#9BE89B>OnSceneGUI()</color> 에서\n<color=#9BE89B>Handles.RadiusHandle(...)</color>\n로 반지름을 Scene 뷰에서 마우스로 조절.\n\n" +
             "이 씬의 EnemySpawner 를 선택하고 Scene 뷰의 주황 원 테두리 점을 끌어 보세요."));

        Selection.activeGameObject = spawnerGo; // 빌드 직후 Inspector 에 버튼이 바로 보이도록
        Save(ctx);
    }
}
