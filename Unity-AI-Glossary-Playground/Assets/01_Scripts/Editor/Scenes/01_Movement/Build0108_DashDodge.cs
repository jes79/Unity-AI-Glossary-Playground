using GlossaryDemo.Movement;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static MovementBuildUtil;

public static class Build0108_DashDodge
{
    [GlossaryScene("0108")]
    [MenuItem("Tools/Glossary/Build Scene/01 Movement/0108_DashDodge")]
    public static void Build()
    {
        var ctx = NewScene2D("0108",
            "WASD : 이동   Shift : 대시 (0.2초, 쿨타임 1초)\nI : 대시 무적(i-frame) ON/OFF\n총알이 오는 순간 대시로 몸을 통과시켜 보세요. 반투명 = 무적, 발밑 막대 = 쿨타임");
        ctx.Camera.backgroundColor = new Color32(30, 34, 44, 255);

        // 경기장 바닥 표시
        CreateSpriteObject("Arena", SquareSprite, new Vector3(0f, -0.2f, 1f), new Vector2(18.6f, 10.2f), new Color32(40, 45, 58, 255), order: -5);

        var player = CreateSpriteObject("Player", CircleSprite, new Vector3(2f, -1f), Vector2.one * 0.8f, PlayerColor, order: 1);
        player.tag = "Player";
        var dash = player.gameObject.AddComponent<DashPlayer>();
        Set(dash, "body", player);

        // 쿨타임 막대: 왼쪽 끝 기준으로 줄어들도록 루트를 왼쪽에 두고 막대를 오른쪽으로 0.5 옮김
        var barRoot = new GameObject("CooldownBar").transform;
        barRoot.SetParent(player.transform, false);
        barRoot.localPosition = new Vector3(-0.5f, -0.85f, 0f);
        barRoot.localScale = new Vector3(1f, 0.12f, 1f);
        CreateSpriteObject("BarFill", SquareSprite, new Vector3(0.5f, 0f), Vector2.one, new Color(0.6f, 1f, 0.6f), barRoot, 3);
        Set(dash, "cooldownBar", barRoot);

        // 포탑 4개 (가장자리에서 시간차로 발사)
        Vector3[] turrets = { new(-8.5f, -4.6f), new(8.5f, -4.6f), new(8.5f, 3.8f), new(0f, -4.6f) };
        for (int i = 0; i < turrets.Length; i++)
        {
            var t = CreateSpriteObject($"Turret_{i}", SquareSprite, turrets[i], Vector2.one * 0.7f, EnemyColor, order: 1);
            var turret = t.gameObject.AddComponent<DashTurret>();
            Set(turret, "target", dash);
            Set(turret, "bulletSprite", CircleSprite);
            SetFloat(turret, "interval", 1.4f + i * 0.2f);
            SetFloat(turret, "startDelay", 0.5f + i * 0.35f);
        }
        Save(ctx);
    }
}
