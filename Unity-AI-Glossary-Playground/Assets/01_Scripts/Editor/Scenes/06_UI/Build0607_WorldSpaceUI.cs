using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build0607_WorldSpaceUI
{
    [GlossaryScene("0607")]
    [MenuItem("Tools/Glossary/Build Scene/06 UI/0607_WorldSpaceUI")]
    public static void Build()
    {
        var ctx = NewScene3D("0607",
            "카메라가 천천히 돕니다 (Q/E : 직접 회전, R : 자동 회전 ON/OFF)\n적 클릭 : 피해 20\n" +
            "B : 빌보드 OFF → 체력바가 적의 몸 방향을 따라 돌아서 옆에서 보면 납작해짐", true, 24f);

        var enemyMat = ColorMat(EnemyColor);
        var noseMat = ColorMat(new Color(0.2f, 0.2f, 0.25f));
        string[] names = { "슬라임", "고블린", "오크" };
        Vector3[] centers = { new(-3.5f, 1f, 1f), new(3.5f, 1f, 1.5f), new(0f, 1f, -2.5f) };
        float[] radii = { 2.2f, 2.6f, 1.8f };
        float[] hps = { 60f, 100f, 200f };

        for (int i = 0; i < names.Length; i++)
        {
            var enemy = Prim(PrimitiveType.Capsule, $"Enemy_{names[i]}", centers[i] + new Vector3(radii[i], 0f, 0f), Vector3.one, EnemyColor);
            enemy.GetComponent<Renderer>().sharedMaterial = enemyMat;
            enemy.layer = LayerMask.NameToLayer("Enemy");
            var nose = Prim(PrimitiveType.Cube, "Nose", new Vector3(0f, 0.45f, 0.5f), new Vector3(0.3f, 0.2f, 0.3f), Color.black, enemy.transform);
            nose.GetComponent<Renderer>().sharedMaterial = noseMat;
            Object.DestroyImmediate(nose.GetComponent<Collider>());

            var bar = CreateWorldBar(enemy.transform, names[i]);

            var e = enemy.AddComponent<WorldUIEnemy>();
            SetString(e, "displayName", names[i]);
            SetFloat(e, "maxHp", hps[i]);
            SetFloat(e, "walkRadius", radii[i]);
            SetFloat(e, "walkSpeed", 0.5f + i * 0.2f);
            SetVector3(e, "center", centers[i]);
            Set(e, "healthBar", bar);
        }

        // 화면 UI (버튼)
        var canvas = CreateCanvas("UI Canvas");
        var buttons = CreateButtonRow(canvas.transform, 60f, "빌보드 ON (B)", "카메라 자동 회전 (R)");

        var demo = new GameObject("WorldSpaceUIDemo").AddComponent<WorldSpaceUIDemo>();
        Set(demo, "cam", ctx.Camera);
        Set(demo, "billboardLabel", LabelOf(buttons[0]));
        OnClick(buttons[0], demo.ToggleBillboard);
        OnClick(buttons[1], demo.ToggleAutoOrbit);

        Save(ctx);
    }

    // 적 머리 위 World Space Canvas 체력바
    private static WorldHealthBar CreateWorldBar(Transform enemy, string label)
    {
        var go = new GameObject("HPCanvas (World Space)", typeof(RectTransform), typeof(Canvas));
        var rt = (RectTransform)go.transform;
        rt.SetParent(enemy, false);
        rt.localPosition = new Vector3(0f, 1.55f, 0f);
        rt.sizeDelta = new Vector2(160f, 52f);
        rt.localScale = Vector3.one * 0.01f; // 160px → 1.6 유닛
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var name = CreateText("Name", rt, Vector2.zero, new Vector2(240f, 30f), label, 22f, Color.white);
        Anchor(name.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero);
        name.fontStyle = FontStyles.Bold;

        var bg = CreateImage("Background", rt, Vector2.zero, new Vector2(160f, 20f), new Color(0.05f, 0.05f, 0.05f, 0.9f));
        Anchor(bg.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero);
        var fill = Filled(bg.transform, "Fill", Vector2.zero, Vector2.zero, new Color(0.9f, 0.25f, 0.25f), Image.FillMethod.Horizontal);
        Stretch(fill.rectTransform, 3f);

        var bar = go.AddComponent<WorldHealthBar>();
        Set(bar, "fill", fill);
        Set(bar, "label", name);
        return bar;
    }
}
