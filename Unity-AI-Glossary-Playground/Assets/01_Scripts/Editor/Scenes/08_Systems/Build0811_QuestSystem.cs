using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0811_QuestSystem
{
    [GlossaryScene("0811")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0811_QuestSystem")]
    public static void Build()
    {
        var ctx = NewScene2D("0811", "오른쪽 [수락] → 필드의 몬스터/버섯을 마우스로 클릭\n목표를 채우면 [보상 받기]\n퀘스트는 QuestData 에셋(Assets/09_Generated/Systems/Quests) — 에셋을 추가해\nQuestManager 목록에 넣으면 코드 수정 없이 새 퀘스트가 생김");

        Random.InitState(811); // 빌드할 때마다 같은 배치
        // 퀘스트 데이터 에셋
        var quests = new[]
        {
            Quest("q_slime", "슬라임 10마리 처치", "마을 앞 초원의 슬라임을 처치하자.", "slime", 10, 100, 50),
            Quest("q_bat", "박쥐 5마리 처치", "밤마다 날아드는 박쥐를 쫓아내자.", "bat", 5, 150, 80),
            Quest("q_mushroom", "버섯 3개 채집", "약초상에게 줄 빨간 버섯을 모으자.", "mushroom", 3, 60, 30),
        };

        // 필드
        var field = new Rect(-3f, -4.4f, 6.5f, 5.2f);
        var bg = CreateSpriteObject("Field", SquareSprite, new Vector3(field.center.x, field.center.y), new Vector2(field.width + 1.2f, field.height + 1.2f), new Color32(52, 78, 58, 255), null, -5);
        bg.name = "Field (Grass)";
        for (int i = 0; i < 4; i++) Monster($"Slime_{i}", "slime", "슬라임", CircleSprite, new Vector2(0.7f, 0.55f), new Color(0.4f, 0.9f, 0.45f), field, 0.8f);
        for (int i = 0; i < 2; i++) Monster($"Bat_{i}", "bat", "박쥐", KnobSprite, new Vector2(0.6f, 0.4f), new Color(0.6f, 0.4f, 0.85f), field, 2.2f);
        for (int i = 0; i < 2; i++) Monster($"Mushroom_{i}", "mushroom", "버섯", CircleSprite, new Vector2(0.45f, 0.45f), new Color(0.95f, 0.3f, 0.3f), field, 0f);

        // 퀘스트 UI
        ctx.Canvas = CreateCanvas("Canvas");
        var root = ctx.Canvas.transform;
        var panel = Panel(root, "QuestPanel", new Vector2(680f, 70f), new Vector2(540f, 760f), new Color32(25, 28, 36, 235));
        Label(panel.transform, "Title", new Vector2(0f, 345f), new Vector2(500f, 44f), "<b>퀘스트 목록</b>", 30f, TextAlignmentOptions.Center);
        var wallet = Label(panel.transform, "Wallet", new Vector2(0f, 300f), new Vector2(500f, 36f), "", 22f, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.35f));

        // 줄 템플릿 (비활성) → QuestManager 가 퀘스트 수만큼 복제
        var row = Panel(panel.transform, "QuestRowTemplate", new Vector2(0f, 190f), new Vector2(510f, 140f));
        var title = Label(row.transform, "Title", new Vector2(-60f, 45f), new Vector2(370f, 36f), "", 24f);
        title.fontStyle = FontStyles.Bold;
        var desc = Label(row.transform, "Desc", new Vector2(-60f, 2f), new Vector2(370f, 50f), "", 16f, TextAlignmentOptions.TopLeft, SubTextColor);
        var fill = Bar(row.transform, "Progress", new Vector2(-60f, -45f), new Vector2(370f, 24f), new Color(0.4f, 0.8f, 0.45f), 0f);
        var progText = Label(fill.transform.parent, "ProgressText", Vector2.zero, new Vector2(370f, 24f), "", 18f, TextAlignmentOptions.Center);
        var btn = SmallButton(row.transform, "Button", new Vector2(185f, 0f), new Vector2(120f, 110f), "수락", null, 22f);
        var view = row.gameObject.AddComponent<QuestRowView>();
        Set(view, "titleText", title);
        Set(view, "descText", desc);
        Set(view, "progressFill", fill);
        Set(view, "progressText", progText);
        Set(view, "button", btn);
        Set(view, "buttonLabel", btn.GetComponentInChildren<TextMeshProUGUI>());
        Set(view, "background", row);
        row.gameObject.SetActive(false);

        var log = Label(panel.transform, "Log", new Vector2(0f, -330f), new Vector2(500f, 60f), "", 20f, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.4f));

        var mgr = new GameObject("QuestManager").AddComponent<QuestManager>();
        SetArray(mgr, "quests", quests);
        Set(mgr, "rowTemplate", view);
        Set(mgr, "walletText", wallet);
        Set(mgr, "logText", log);

        Save(ctx);
    }

    private static QuestData Quest(string id, string title, string desc, string target, int count, int gold, int xp)
    {
        string folder = $"{GeneratedFolder}/{Sub}/Quests";
        EnsureFolder(folder);
        string path = $"{folder}/{id}.asset";
        var q = AssetDatabase.LoadAssetAtPath<QuestData>(path);
        if (q == null)
        {
            q = ScriptableObject.CreateInstance<QuestData>();
            AssetDatabase.CreateAsset(q, path);
        }
        q.questId = id;
        q.title = title;
        q.description = desc;
        q.targetId = target;
        q.requiredCount = count;
        q.rewardGold = gold;
        q.rewardXp = xp;
        EditorUtility.SetDirty(q);
        return q;
    }

    private static void Monster(string name, string id, string display, Sprite sprite, Vector2 size, Color color, Rect area, float speed)
    {
        var pos = new Vector3(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));
        var sr = CreateSpriteObject(name, sprite, pos, size, color, null, 2);
        sr.gameObject.AddComponent<CircleCollider2D>().radius = 0.6f;
        var m = sr.gameObject.AddComponent<QuestMonster>();
        SetString(m, "targetId", id);
        SetString(m, "displayName", display);
        var so = new SerializedObject(m);
        so.FindProperty("area").rectValue = area;
        so.ApplyModifiedPropertiesWithoutUndo();
        SetFloat(m, "wanderSpeed", speed);
    }
}
