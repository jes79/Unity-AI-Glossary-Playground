using GlossaryDemo.EnemyAI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static EnemyAIBuildUtil;

public static class Build0707_Aggro
{
    [GlossaryScene("0707")]
    [MenuItem("Tools/Glossary/Build Scene/07 EnemyAI/0707_Aggro")]
    public static void Build()
    {
        var ctx = NewScene3D("0707", "1 / 2 / 3 : 전사·마법사·궁수가 강하게 공격 (위협 +)\nT : 전사 도발(Taunt)   A : 자동 공격 ON/OFF   R : 위협 초기화\nM : 대상 규칙 전환 (위협 최고 ↔ 가장 가까운 대상)\n오른쪽 막대 = 위협 수치. 빨간 선 = 적이 노리는 대상");
        TopCamera(ctx);

        string[] names = { "전사", "마법사", "궁수" };
        Color[] colors = { new Color32(90, 150, 255, 255), new Color32(190, 110, 255, 255), new Color32(90, 220, 140, 255) };
        Vector3[] pos = { new(-6f, 1f, -5f), new(9f, 1f, -6f), new(3f, 1f, 9f) };
        float[] hit = { 6f, 18f, 12f };        // 1회 공격 데미지 (마법사가 가장 셈)
        float[] mult = { 1f, 1f, 1f };         // 위협 = 데미지 그대로 (탱커 배율은 Inspector 에서 조절)
        float[] dps = { 4f, 9f, 6f };
        var allies = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            var a = Prim(PrimitiveType.Capsule, $"Ally_{names[i]}", pos[i], Vector3.one, colors[i]);
            Object.DestroyImmediate(a.GetComponent<Collider>());
            Label(a.transform, $"{i + 1}. {names[i]}", 1.6f, 4f, colors[i]);
            allies[i] = a.transform;
        }

        var enemy = Prim(PrimitiveType.Capsule, "Enemy_Aggro", new Vector3(2f, 1.5f, 0f), Vector3.one * 1.5f, EnemyColor);
        Object.DestroyImmediate(enemy.GetComponent<Collider>());
        enemy.layer = LayerMask.NameToLayer("Enemy");
        var eye = Prim(PrimitiveType.Cube, "Eye", new Vector3(0f, 0.5f, 0.45f), new Vector3(0.6f, 0.12f, 0.2f), Color.black, enemy.transform);
        Object.DestroyImmediate(eye.GetComponent<Collider>());
        Label(enemy.transform, "몬스터", 1.4f, 4f);

        // 위협 막대 UI (오른쪽 위)
        ctx.Canvas = CreateCanvas("Canvas");
        var bars = new Image[3];
        var texts = new TextMeshProUGUI[3];
        var title = CreateText("ThreatTitle", ctx.Canvas.transform, new Vector2(-380f, -40f), new Vector2(700f, 40f), "위협 수치 (Threat Table)", 28f, TextColor, TextAlignmentOptions.Left);
        AnchorTopRight(title.rectTransform);
        for (int i = 0; i < 3; i++)
        {
            float y = -100f - i * 80f;
            var bg = CreateImage($"ThreatBg_{i}", ctx.Canvas.transform, new Vector2(-380f, y), new Vector2(700f, 30f), new Color(0f, 0f, 0f, 0.5f), WhiteSprite);
            AnchorTopRight(bg.rectTransform);
            var fill = CreateImage("Fill", bg.transform, Vector2.zero, Vector2.zero, colors[i], WhiteSprite);
            Stretch(fill.rectTransform);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0f;
            bars[i] = fill;
            var t = CreateText($"ThreatText_{i}", ctx.Canvas.transform, new Vector2(-380f, y + 30f), new Vector2(700f, 30f), names[i], 22f, TextColor, TextAlignmentOptions.Left);
            AnchorTopRight(t.rectTransform);
            texts[i] = t;
        }

        var aggro = enemy.AddComponent<AggroEnemy>();
        var so = new SerializedObject(aggro);
        var arr = so.FindProperty("attackers");
        arr.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            var e = arr.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("name").stringValue = names[i];
            e.FindPropertyRelative("transform").objectReferenceValue = allies[i];
            e.FindPropertyRelative("damagePerHit").floatValue = hit[i];
            e.FindPropertyRelative("threatMultiplier").floatValue = mult[i];
            e.FindPropertyRelative("autoDps").floatValue = dps[i];
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        SetArray(aggro, "threatBars", bars);
        SetArray(aggro, "threatTexts", texts);
        Set(aggro, "targetLine", Line("TargetLine", new Color(1f, 0.25f, 0.25f), 0.12f));

        Save(ctx);
    }
}
