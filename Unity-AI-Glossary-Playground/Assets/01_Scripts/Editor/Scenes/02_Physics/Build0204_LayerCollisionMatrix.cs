using GlossaryDemo.PhysicsDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0204_LayerCollisionMatrix
{
    [GlossaryScene("0204")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0204_LayerCollisionMatrix")]
    public static void Build()
    {
        var ctx = NewScene3D("0204",
            "WASD : 이동   M : EnemyBullet ↔ Enemy 충돌 끄기/켜기\n오른쪽 표의 칸을 클릭해 다른 레이어 쌍도 바꿔 보세요\n켜져 있으면 뒤쪽 포탑의 총알이 앞의 적(아군)에게 막히고, 끄면 적 몸을 통과해 플레이어에게 날아옵니다\n실제 설정: Project Settings > Physics > Layer Collision Matrix",
            true, 26f);
        ctx.Camera.transform.position = new Vector3(0f, 14f, -12f);
        ctx.Camera.transform.rotation = Quaternion.Euler(52f, 0f, 0f);

        string[] names = { "Player", "Enemy", "PlayerBullet", "EnemyBullet" };

        var player = Player(new Vector3(0f, 1f, -5f));

        // 적 줄 (Enemy 레이어) — 포탑과 플레이어 사이에 서 있음
        for (int i = 0; i < 5; i++)
        {
            var e = Prim(PrimitiveType.Capsule, $"Enemy_{i + 1}", new Vector3(-6f + i * 3f, 1f, 2.5f), Vector3.one, EnemyColor);
            e.tag = "Enemy";
            e.layer = LayerMask.NameToLayer("Enemy");
        }
        Label("Enemy 레이어", new Vector3(-8.5f, 2.6f, 2.5f), 5f, new Color(1f, 0.7f, 0.7f));

        // 포탑 (적 뒤쪽)
        var turrets = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            var t = Prim(PrimitiveType.Cube, $"Turret_{i}", new Vector3(-5f + i * 5f, 1f, 7f), new Vector3(1f, 2f, 1f), new Color32(150, 60, 70, 255));
            turrets[i] = new GameObject("Muzzle").transform;
            turrets[i].SetParent(t.transform, false);
            turrets[i].localPosition = new Vector3(0f, 0f, -0.8f);
        }

        // 적 총알 프리팹 (EnemyBullet 레이어)
        var bullet = Prim(PrimitiveType.Sphere, "EnemyBullet", Vector3.zero, Vector3.one * 0.35f, new Color32(255, 140, 60, 255));
        bullet.layer = LayerMask.NameToLayer("EnemyBullet");
        bullet.tag = "Bullet";
        var rb = bullet.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        bullet.AddComponent<LayerBullet>();
        var prefab = SavePrefab(bullet, Sub, "0204_EnemyBullet");

        // 매트릭스 표 UI (Unity 설정 창처럼 아래 삼각형만)
        ctx.Canvas = CreateCanvas("DemoCanvas");
        const float cell = 104f, header = 150f;
        int n = names.Length;
        var panel = CreateImage("MatrixPanel", ctx.Canvas.transform, new Vector2(-24f, -24f), new Vector2(header + cell * n + 30f, 330f), new Color(0.07f, 0.08f, 0.1f, 0.85f));
        panel.rectTransform.pivot = new Vector2(1f, 1f);
        AnchorTopRight(panel.rectTransform);
        var title = CreateText("Title", panel.transform, new Vector2(0f, -22f), new Vector2(600f, 36f), "Layer Collision Matrix (클릭해서 전환)", 22f, TextColor);
        title.rectTransform.anchorMin = title.rectTransform.anchorMax = new Vector2(0.5f, 1f);

        var cells = new Image[n * n];
        var labels = new TextMeshProUGUI[n * n];
        Vector2 origin = new Vector2(15f + header, -50f - 40f); // 표 왼쪽 위 (패널 왼쪽 위 기준)
        for (int j = 0; j < n; j++)
        {
            var col = CreateText($"Col_{names[j]}", panel.transform, Vector2.zero, new Vector2(cell, 36f), names[j], 15f, SubTextColor);
            col.rectTransform.anchorMin = col.rectTransform.anchorMax = col.rectTransform.pivot = new Vector2(0f, 1f);
            col.rectTransform.anchoredPosition = new Vector2(origin.x + j * cell, origin.y + 36f);
        }
        for (int i = 0; i < n; i++)
        {
            var row = CreateText($"Row_{names[i]}", panel.transform, Vector2.zero, new Vector2(header - 10f, cell * 0.5f), names[i], 17f, SubTextColor, TextAlignmentOptions.Right);
            row.rectTransform.anchorMin = row.rectTransform.anchorMax = row.rectTransform.pivot = new Vector2(0f, 1f);
            row.rectTransform.anchoredPosition = new Vector2(15f, origin.y - i * cell * 0.5f);
            for (int j = 0; j <= i; j++)
            {
                var btn = CreateButton($"Cell_{names[i]}_{names[j]}", panel.transform, Vector2.zero, new Vector2(cell - 6f, cell * 0.5f - 6f), "충돌", new Color(0.3f, 0.75f, 0.4f), 18f);
                var rt = (RectTransform)btn.transform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(origin.x + j * cell, origin.y - i * cell * 0.5f);
                cells[i * n + j] = btn.GetComponent<Image>();
                labels[i * n + j] = btn.GetComponentInChildren<TextMeshProUGUI>();
                btn.name = $"Cell_{i * n + j}";
            }
        }

        var demo = new GameObject("LayerMatrixDemo").AddComponent<LayerMatrixDemo>();
        SetArray(demo, "cells", cells);
        SetArray(demo, "turrets", turrets);
        SetArray(demo, "cellLabels", labels);
        Set(demo, "bulletPrefab", prefab.GetComponent<Rigidbody>());
        Set(demo, "player", player.transform);
        // 버튼 → Toggle(index)
        for (int k = 0; k < cells.Length; k++)
            if (cells[k] != null) OnClick(cells[k].GetComponent<Button>(), demo.Toggle, k);
        Save(ctx);
    }
}
