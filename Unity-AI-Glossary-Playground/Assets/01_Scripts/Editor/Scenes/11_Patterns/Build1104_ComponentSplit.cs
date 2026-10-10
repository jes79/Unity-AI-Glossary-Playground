using GlossaryDemo.Patterns;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build1104_ComponentSplit
{
    [GlossaryScene("1104")]
    [MenuItem("Tools/Glossary/Build Scene/11 Patterns/1104_ComponentSplit")]
    public static void Build()
    {
        var ctx = NewScene3D("1104",
            "WASD : 이동 (PlayerMovement)   J : 공격 (PlayerAttack)   빨간 바닥 : 피해 (PlayerHealth)\n" +
            "1 / 2 / 3 : 각 컴포넌트 끄기·켜기 → 하나를 꺼도 나머지는 그대로 동작\n" +
            "Player 를 선택하면 Inspector 에 스크립트 3개가 따로 보입니다", true, 20f);
        PatternsBuildUtil.LookAt(ctx.Camera, new Vector3(0f, 11f, -10f), new Vector3(0f, 0f, 0.5f));

        // ── 플레이어: 한 오브젝트에 역할별 컴포넌트 3개 ──
        var player = Prim(PrimitiveType.Capsule, "Player", new Vector3(0f, 1f, -3f), Vector3.one, PlayerColor);
        player.layer = LayerMask.NameToLayer("Player");
        player.tag = "Player";
        Object.DestroyImmediate(player.GetComponent<Collider>());
        var cc = player.AddComponent<CharacterController>();
        cc.center = Vector3.zero;
        var nose = Prim(PrimitiveType.Cube, "Nose", new Vector3(0f, 0.4f, 0.5f), new Vector3(0.3f, 0.2f, 0.3f), Color.white, player.transform);
        Object.DestroyImmediate(nose.GetComponent<Collider>());

        var slash = Prim(PrimitiveType.Cylinder, "SlashVisual", new Vector3(0f, 0f, 1.6f), Vector3.one, Color.white, player.transform);
        Object.DestroyImmediate(slash.GetComponent<Collider>());
        slash.GetComponent<Renderer>().sharedMaterial = UnlitMat("Unlit_1104_Slash", new Color(1f, 0.7f, 0.3f));
        slash.transform.localScale = new Vector3(1f, 0.02f, 1f);
        var slashRoot = new GameObject("Slash").transform;
        slashRoot.SetParent(player.transform, false);
        slashRoot.localPosition = new Vector3(0f, 0f, 1.6f);
        slash.transform.SetParent(slashRoot, false);
        slash.transform.localPosition = Vector3.zero;
        slash.transform.localScale = new Vector3(2f, 0.02f, 2f);

        var movement = player.AddComponent<PlayerMovement>();
        var attack = player.AddComponent<PlayerAttack>();
        Set(attack, "slashVisual", slashRoot);
        var health = player.AddComponent<PlayerHealth>();
        Set(health, "body", player.GetComponent<Renderer>());
        SetColor(health, "normalColor", PlayerColor);

        // ── 허수아비 ──
        var dummyMat = ColorMat(new Color(0.75f, 0.6f, 0.35f));
        for (int i = 0; i < 3; i++)
        {
            var root = new GameObject($"TrainingDummy_{i + 1}");
            root.transform.position = new Vector3(-4f + i * 4f, 0f, 2.5f);
            var pole = Prim(PrimitiveType.Cylinder, "Body", new Vector3(0f, 0.9f, 0f), new Vector3(0.6f, 0.9f, 0.6f), Color.white, root.transform);
            pole.GetComponent<Renderer>().sharedMaterial = dummyMat;
            pole.layer = LayerMask.NameToLayer("Enemy");
            var head = Prim(PrimitiveType.Sphere, "Head", new Vector3(0f, 2.1f, 0f), Vector3.one * 0.6f, Color.white, root.transform);
            head.GetComponent<Renderer>().sharedMaterial = dummyMat;
            var labelGo = new GameObject("HPLabel");
            labelGo.transform.SetParent(root.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 2.9f, 0f);
            var tmp = labelGo.AddComponent<TextMeshPro>();
            tmp.font = GlossaryKit.Font;
            tmp.text = "100";
            tmp.fontSize = 6f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.rectTransform.sizeDelta = new Vector2(4f, 1.2f);
            var dummy = root.AddComponent<TrainingDummy>();
            Set(dummy, "label", tmp);
        }

        // ── 피해 지대 ──
        var zone = Prim(PrimitiveType.Cube, "DamageZone", new Vector3(5f, 0.01f, -3f), new Vector3(3f, 0.02f, 3f), new Color(0.85f, 0.2f, 0.2f));
        Object.DestroyImmediate(zone.GetComponent<Collider>());
        var dz = zone.AddComponent<DamageZone>();
        Set(dz, "target", health);

        // ── UI ──
        var canvas = CreateCanvas("UI Canvas");
        var croot = canvas.transform;
        var hpBg = Panel(croot, "HPBar", Vector2.zero, new Vector2(500f, 40f), new Color(0.1f, 0.1f, 0.12f, 0.95f));
        AnchorBottom(hpBg.rectTransform);
        hpBg.rectTransform.anchoredPosition = new Vector2(0f, 140f);
        var hpFill = Filled(hpBg.transform, "Fill", Vector2.zero, Vector2.zero, new Color(0.9f, 0.3f, 0.3f), Image.FillMethod.Horizontal);
        Stretch(hpFill.rectTransform, 5f);

        var toggles = CreateButtonRow(croot, 60f, "1  이동 ON", "2  공격 ON", "3  체력 ON");

        InfoBox(croot, "SplitInfo", new Vector2(740f, 230f), new Vector2(400f, 440f), "분리 전 → 후",
            "<color=#FF8A65>전</color>: PlayerController 하나에\n이동 + 공격 + 체력 + 사운드…\n→ 한 줄 고치면 다 같이 흔들림\n\n" +
            "<color=#9BE89B>후</color>:\n• PlayerMovement  이동만\n• PlayerAttack    공격만\n• PlayerHealth    체력만\n\n" +
            "서로는 이벤트(Died)나\nGetComponent 로 필요한 만큼만 연결.\n적에게 PlayerHealth 만 재사용도 가능", 19f);

        var demo = new GameObject("ComponentSplitDemo").AddComponent<ComponentSplitDemo>();
        Set(demo, "movement", movement);
        Set(demo, "attack", attack);
        Set(demo, "health", health);
        Set(demo, "hpFill", hpFill);
        SetArray(demo, "toggleLabels", LabelOf(toggles[0]), LabelOf(toggles[1]), LabelOf(toggles[2]));
        OnClick(toggles[0], demo.ToggleMovement);
        OnClick(toggles[1], demo.ToggleAttack);
        OnClick(toggles[2], demo.ToggleHealth);

        Save(ctx);
    }
}
