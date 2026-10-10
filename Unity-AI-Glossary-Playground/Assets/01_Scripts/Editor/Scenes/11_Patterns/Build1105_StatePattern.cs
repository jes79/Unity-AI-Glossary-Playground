using GlossaryDemo.Patterns;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build1105_StatePattern
{
    [GlossaryScene("1105")]
    [MenuItem("Tools/Glossary/Build Scene/11 Patterns/1105_StatePattern")]
    public static void Build()
    {
        var ctx = NewScene2D("1105",
            "A/D : 달리기 (Run)   Space : 점프 (Jump)   J : 공격 (Attack, 0.35초 동안 이동 불가)\n" +
            "오른쪽 상태 그림에서 현재 상태가 노랗게 켜짐. 색·찌그러짐도 상태마다 다름");
        ctx.Camera.transform.position = new Vector3(1.5f, 0f, -10f);

        int ground = LayerMask.NameToLayer("Ground");
        void Platform(string name, Vector3 pos, Vector2 size)
        {
            var p = CreateSpriteObject(name, SquareSprite, pos, size, GroundColor);
            p.gameObject.layer = ground;
            p.gameObject.AddComponent<BoxCollider2D>();
        }
        Platform("Ground", new Vector3(0f, -2f, 0f), new Vector2(12f, 0.6f));
        Platform("Ledge", new Vector3(2.5f, 0f, 0f), new Vector2(3f, 0.4f));

        var player = CreateSpriteObject("Player", SquareSprite, new Vector3(-2f, -1f, 0f), new Vector2(0.8f, 1f), PlayerColor, order: 2);
        player.gameObject.layer = LayerMask.NameToLayer("Player");
        var rb = player.gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        player.gameObject.AddComponent<BoxCollider2D>();
        var slash = CreateSpriteObject("Slash", SquareSprite, new Vector3(0.9f, 0f, 0f), new Vector2(1f, 1.2f), new Color(1f, 0.55f, 0.3f, 0.75f), player.transform, 3);

        // ── 상태 그림 (UI) ──
        var canvas = CreateCanvas("UI Canvas");
        var root = canvas.transform;
        Body(root, "DiagramTitle", new Vector2(700f, 420f), new Vector2(480f, 40f), "상태 (클래스)", 24f, TextAlignmentOptions.Center, TitleColor);
        string[] names = { "IdleState", "RunState", "JumpState", "AttackState" };
        Vector2[] pos = { new(580f, 350f), new(820f, 350f), new(580f, 250f), new(820f, 250f) };
        var nodes = new Image[4];
        for (int i = 0; i < 4; i++)
        {
            nodes[i] = Panel(root, $"Node_{names[i]}", pos[i], new Vector2(220f, 80f), new Color32(52, 58, 76, 255));
            CreateText("Label", nodes[i].transform, Vector2.zero, new Vector2(210f, 80f), names[i], 24f, Color.white).fontStyle = FontStyles.Bold;
        }
        var historyPanel = Panel(root, "History", new Vector2(700f, 20f), new Vector2(460f, 320f), DarkPanel);
        var history = CreateText("Text", historyPanel.transform, Vector2.zero, Vector2.zero, "", 21f, TextColor, TextAlignmentOptions.TopLeft);
        Stretch(history.rectTransform, 16f);
        history.richText = true;

        InfoBox(root, "CodeInfo", new Vector2(250f, -380f), new Vector2(1100f, 220f), "switch 문 → 상태 클래스",
            "<color=#FF8A65>전</color>: switch (state) { case Idle: … case Run: … case Jump: … case Attack: … }  — 상태가 늘수록 한 함수가 거대해짐\n" +
            "<color=#9BE89B>후</color>: interface IPlayerState { Enter(); Tick(); Exit(); }   class RunState : IPlayerState { … }\n" +
            "      전환은 <color=#9BE89B>ChangeState(next)</color> 한 곳에서: current.Exit() → current = next → next.Enter()\n" +
            "      '공격 중엔 이동 불가' 같은 규칙이 AttackState 안에만 있어 찾기·고치기 쉬움", 19f);

        var ctrl = player.gameObject.AddComponent<StatePatternPlayer>();
        Set(ctrl, "body", player);
        Set(ctrl, "slash", slash.gameObject);
        SetLayerMask(ctrl, "groundMask", LayerMask.GetMask("Ground"));
        SetArray(ctrl, "stateNodes", nodes);
        Set(ctrl, "historyText", history);

        Save(ctx);
    }
}
