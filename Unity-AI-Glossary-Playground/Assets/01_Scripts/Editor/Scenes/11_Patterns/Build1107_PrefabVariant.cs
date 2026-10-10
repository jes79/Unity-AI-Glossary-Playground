using System;
using System.Text;
using GlossaryDemo.Patterns;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;
using Object = UnityEngine.Object;

public static class Build1107_PrefabVariant
{
    private const string Folder = GeneratedFolder + "/11_Patterns";
    private const string BasePath = Folder + "/Enemy_Base.prefab";
    private const string FastPath = Folder + "/Enemy_Fast (Variant).prefab";
    private const string StrongPath = Folder + "/Enemy_Strong (Variant).prefab";

    [GlossaryScene("1107")]
    [MenuItem("Tools/Glossary/Build Scene/11 Patterns/1107_PrefabVariant")]
    public static void Build()
    {
        var ctx = NewScene3D("1107",
            "1 / 2 / 3 : 기본 적 / 빠른 적(Variant) / 강한 적(Variant) 소환   T : 자동 소환 ON/OFF\n" +
            "아래 카드 = 각 Variant 가 기본 프리팹에서 '덮어쓴 값(Override)' 목록 (빌더가 실제로 읽어 온 값)\n" +
            "메뉴 Tools > Glossary > Patterns > 1107 Base 공격력 +5 → 덮어쓰지 않은 값만 Variant 에 반영", true, 30f);
        ctx.Camera.transform.SetPositionAndRotation(new Vector3(0f, 10f, -11f), Quaternion.Euler(42f, 0f, 0f));

        // ── 1) 기본 프리팹 ──
        var basePrefab = CreateBasePrefab();

        // ── 2) Variant: 기본 프리팹의 "인스턴스"를 고친 뒤 SaveAsPrefabAsset → 자동으로 Prefab Variant 가 됨 ──
        var fast = MakeVariant(basePrefab, FastPath, inst =>
        {
            var e = inst.GetComponent<VariantEnemy>();
            SetString(e, "displayName", "빠른 적");
            SetFloat(e, "moveSpeed", 5f);
            SetInt(e, "maxHp", 60);
            SetVector3(inst.transform, "m_LocalScale", new Vector3(0.75f, 0.75f, 0.75f));
            SetMaterial(inst, ColorMat(new Color(1f, 0.65f, 0.2f)));
        }, out string fastSummary);

        var strong = MakeVariant(basePrefab, StrongPath, inst =>
        {
            var e = inst.GetComponent<VariantEnemy>();
            SetString(e, "displayName", "강한 적");
            SetFloat(e, "moveSpeed", 1.2f);
            SetInt(e, "maxHp", 300);
            SetInt(e, "attackPower", 30);
            SetVector3(inst.transform, "m_LocalScale", new Vector3(1.5f, 1.5f, 1.5f));
            SetMaterial(inst, ColorMat(new Color(0.55f, 0.1f, 0.15f)));
        }, out string strongSummary);

        // ── 3) 씬: 줄(lane) 3개 + 스포너 ──
        var lanes = new Transform[3];
        float[] laneZ = { 3.5f, 0f, -3.5f };
        Color[] laneColors = { new Color(0.32f, 0.34f, 0.4f), new Color(0.4f, 0.33f, 0.25f), new Color(0.35f, 0.24f, 0.27f) };
        for (int i = 0; i < 3; i++)
        {
            var strip = Prim(PrimitiveType.Cube, $"Lane_{i}", new Vector3(0f, 0.01f, laneZ[i]), new Vector3(24f, 0.02f, 2.4f), laneColors[i]);
            Object.DestroyImmediate(strip.GetComponent<Collider>());
            lanes[i] = new GameObject($"SpawnPoint_{i}").transform;
            lanes[i].position = new Vector3(-11f, 1f, laneZ[i]);
        }

        var canvas = CreateCanvas("UI Canvas");
        var root = canvas.transform;
        var autoBtn = CreateButton("AutoToggle", root, Vector2.zero, new Vector2(260f, 64f), "자동 소환 ON (T)", new Color32(70, 80, 105, 255), 22f);
        Anchor((RectTransform)autoBtn.transform, Vector2.one, Vector2.one, new Vector2(-30f, -24f));

        string baseSummary = "원본 (Override 없음)\n• displayName = 기본 적\n• moveSpeed = 2\n• maxHp = 100\n• attackPower = 10\n• 크기 1, 회색 머티리얼";
        var spawnButtons = new Button[3];
        spawnButtons[0] = Card(root, new Vector2(-600f, -370f), "1  Enemy_Base (기본 프리팹)", baseSummary);
        spawnButtons[1] = Card(root, new Vector2(0f, -370f), "2  Enemy_Fast (Variant)", "기본에서 덮어쓴 값 <color=#9BE89B>(굵게·파란 줄로 표시됨)</color>\n" + fastSummary + "\n<color=#999>attackPower 는 덮어쓰지 않음 → Base 값 상속</color>");
        spawnButtons[2] = Card(root, new Vector2(600f, -370f), "3  Enemy_Strong (Variant)", "기본에서 덮어쓴 값\n" + strongSummary);

        var spawner = new GameObject("VariantSpawner").AddComponent<VariantSpawner>();
        SetArray(spawner, "prefabs", basePrefab, fast, strong);
        SetArray(spawner, "lanes", lanes);
        Set(spawner, "autoLabel", LabelOf(autoBtn));
        OnClick(autoBtn, spawner.ToggleAuto);
        for (int i = 0; i < 3; i++) OnClick(spawnButtons[i], spawner.Spawn, i);

        Save(ctx);
    }

    // 기본값이 들어 있는 원본 프리팹
    private static GameObject CreateBasePrefab()
    {
        var go = Prim(PrimitiveType.Capsule, "Enemy_Base", Vector3.zero, Vector3.one, new Color(0.6f, 0.6f, 0.65f));
        go.layer = LayerMask.NameToLayer("Enemy");
        var nose = Prim(PrimitiveType.Cube, "Nose", new Vector3(0f, 0.4f, 0.5f), new Vector3(0.3f, 0.2f, 0.3f), Color.black, go.transform);
        Object.DestroyImmediate(nose.GetComponent<Collider>());

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 1.9f, 0f);
        var tmp = labelGo.AddComponent<TextMeshPro>();
        tmp.font = GlossaryKit.Font;
        tmp.fontSize = 3.2f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(6f, 1.6f);
        tmp.richText = true;

        var e = go.AddComponent<VariantEnemy>();
        Set(e, "label", tmp);
        return SavePrefab(go, "11_Patterns", "Enemy_Base");
    }

    private static GameObject MakeVariant(GameObject basePrefab, string path, Action<GameObject> modify, out string summary)
    {
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        modify(inst);
        summary = Summarize(inst);
        var variant = PrefabUtility.SaveAsPrefabAsset(inst, path);
        Object.DestroyImmediate(inst);
        return variant;
    }

    private static void SetMaterial(GameObject inst, Material mat)
    {
        var so = new SerializedObject(inst.GetComponent<MeshRenderer>());
        so.FindProperty("m_Materials.Array.data[0]").objectReferenceValue = mat;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // 인스턴스에 기록된 Override(PropertyModification) 중 의미 있는 것만 글로 정리
    private static string Summarize(GameObject inst)
    {
        var sb = new StringBuilder();
        var mods = PrefabUtility.GetPropertyModifications(inst);
        if (mods == null) return "(없음)";
        foreach (var m in mods)
        {
            string p = m.propertyPath;
            if (p.StartsWith("m_LocalPosition") || p.StartsWith("m_LocalRotation") || p.StartsWith("m_LocalEulerAnglesHint") ||
                p == "m_Name" || p == "m_RootOrder") continue;
            if (p.StartsWith("m_LocalScale.") && !p.EndsWith(".x")) continue; // 크기는 x 하나만 표시
            string value = m.objectReference != null ? m.objectReference.name : m.value;
            string label = p switch
            {
                "m_LocalScale.x" => "크기(Scale)",
                "m_Materials.Array.data[0]" => "머티리얼",
                _ => p,
            };
            sb.Append("• <b>").Append(label).Append("</b> = ").Append(value).Append('\n');
        }
        return sb.ToString().TrimEnd();
    }

    private static Button Card(Transform root, Vector2 pos, string title, string body)
    {
        var box = InfoBox(root, $"Card_{title}", pos, new Vector2(580f, 280f), title, body, 19f);
        var btn = CreateButton("SpawnButton", box.transform.parent, Vector2.zero, new Vector2(120f, 46f), "소환", null, 22f);
        Anchor((RectTransform)btn.transform, Vector2.one, Vector2.one, new Vector2(-14f, -12f));
        return btn;
    }

    // Base 프리팹만 고치면, 덮어쓰지 않은 값은 Variant 에도 반영되는지 확인하는 메뉴
    [MenuItem("Tools/Glossary/Patterns/1107 Base 공격력 +5 (상속 확인)")]
    private static void IncreaseBaseAttack()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(BasePath) == null)
        {
            Debug.LogWarning("Enemy_Base 프리팹이 없음. 먼저 1107 씬을 빌드하세요.");
            return;
        }
        var contents = PrefabUtility.LoadPrefabContents(BasePath);
        var so = new SerializedObject(contents.GetComponent<VariantEnemy>());
        var atk = so.FindProperty("attackPower");
        atk.intValue += 5;
        so.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(contents, BasePath);
        PrefabUtility.UnloadPrefabContents(contents);

        Debug.Log($"[1107] Base attackPower = {ReadAttack(BasePath)}  →  Fast(상속) = {ReadAttack(FastPath)},  Strong(Override 30) = {ReadAttack(StrongPath)}");
    }

    private static int ReadAttack(string path)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (go == null) return -1;
        return new SerializedObject(go.GetComponent<VariantEnemy>()).FindProperty("attackPower").intValue;
    }
}
