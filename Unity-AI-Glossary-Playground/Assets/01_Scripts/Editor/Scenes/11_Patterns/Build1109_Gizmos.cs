using GlossaryDemo.Patterns;
using GlossaryDemo.UIDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build1109_Gizmos
{
    [GlossaryScene("1109")]
    [MenuItem("Tools/Glossary/Build Scene/11 Patterns/1109_Gizmos")]
    public static void Build()
    {
        var ctx = NewScene3D("1109",
            "WASD : 이동 — 노란 부채꼴(시야)에 들어가면 적이 주황, 빨간 원(공격 범위)이면 빨강\n" +
            "Scene 뷰에는 OnDrawGizmos 선이 항상 보이고, 적을 선택하면 OnDrawGizmosSelected 가 추가로 그려짐\n" +
            "G : 같은 모양의 LineRenderer 끄기 → Game 뷰 'Gizmos' 버튼을 켜야 보임", true, 30f);
        PatternsBuildUtil.LookAt(ctx.Camera, new Vector3(0f, 16f, -11f), new Vector3(0f, 0f, 2f));

        var idle = ColorMat(new Color(0.55f, 0.45f, 0.5f));
        var seen = ColorMat(new Color(1f, 0.6f, 0.2f));
        var attack = ColorMat(new Color(1f, 0.15f, 0.15f));
        var yellow = UnlitMat("Unlit_1109_View", new Color(1f, 0.9f, 0.2f));
        var red = UnlitMat("Unlit_1109_Attack", new Color(1f, 0.25f, 0.25f));
        var green = UnlitMat("Unlit_1109_Target", new Color(0.3f, 1f, 0.4f));

        // 플레이어 (0615 의 간단 이동 스크립트 재사용)
        var player = Prim(PrimitiveType.Capsule, "Player", new Vector3(0f, 1f, -3f), Vector3.one, PlayerColor);
        player.layer = LayerMask.NameToLayer("Player");
        player.tag = "Player";
        Object.DestroyImmediate(player.GetComponent<Collider>());
        var cc = player.AddComponent<CharacterController>();
        cc.center = Vector3.zero;
        player.AddComponent<MinimapPlayerMover>();

        (Vector3 pos, float yaw, float radius, float angle, float range)[] cfg =
        {
            (new Vector3(-6f, 1f, 5f), 160f, 6f, 90f, 2f),
            (new Vector3(6f, 1f, 5f), 200f, 8f, 50f, 3f),
            (new Vector3(0f, 1f, 9f), 180f, 4.5f, 160f, 1.5f),
        };
        var enemies = new EnemyVisionGizmos[cfg.Length];
        for (int i = 0; i < cfg.Length; i++)
        {
            var e = Prim(PrimitiveType.Capsule, $"Enemy_{i + 1} (선택해 보세요)", cfg[i].pos, Vector3.one, EnemyColor);
            e.transform.rotation = Quaternion.Euler(0f, cfg[i].yaw, 0f);
            e.layer = LayerMask.NameToLayer("Enemy");
            var nose = Prim(PrimitiveType.Cube, "Nose", new Vector3(0f, 0.4f, 0.5f), new Vector3(0.3f, 0.2f, 0.3f), Color.black, e.transform);
            Object.DestroyImmediate(nose.GetComponent<Collider>());

            var v = e.AddComponent<EnemyVisionGizmos>();
            SetFloat(v, "viewRadius", cfg[i].radius);
            SetFloat(v, "viewAngle", cfg[i].angle);
            SetFloat(v, "attackRange", cfg[i].range);
            SetFloat(v, "scanSpeed", 30f + i * 15f);
            Set(v, "player", player.transform);
            Set(v, "body", e.GetComponent<Renderer>());
            Set(v, "idleMat", idle);
            Set(v, "seenMat", seen);
            Set(v, "attackMat", attack);
            Set(v, "viewLine", Line(e.transform, "ViewLine", yellow, 0.06f));
            Set(v, "attackLine", Line(e.transform, "AttackLine", red, 0.06f));
            Set(v, "targetLine", Line(e.transform, "TargetLine", green, 0.05f));
            enemies[i] = v;
        }

        var canvas = CreateCanvas("UI Canvas");
        var toggle = CreateButtonRow(canvas.transform, 60f, "LineRenderer 선 ON (G)")[0];
        InfoBox(canvas.transform, "CodeInfo", new Vector2(740f, 250f), new Vector2(400f, 420f), "Gizmos 코드",
            "<color=#9BE89B>void OnDrawGizmos()\n{\n  Gizmos.color = Color.yellow;\n  Gizmos.DrawWireSphere(\n    transform.position, viewRadius);\n  Gizmos.DrawLine(a, b);\n}</color>\n\n" +
            "<color=#9BE89B>OnDrawGizmosSelected()</color>\n  → 선택했을 때만\n\n" +
            "• 에디터 전용 (빌드엔 안 그려짐)\n• Game 뷰는 'Gizmos' 버튼 ON", 18f);

        var demo = new GameObject("GizmosDemo").AddComponent<GizmosDemo>();
        SetArray(demo, "enemies", enemies);
        Set(demo, "toggleLabel", LabelOf(toggle));
        OnClick(toggle, demo.ToggleRuntimeLines);

        Save(ctx);
    }

    private static LineRenderer Line(Transform parent, string name, Material mat, float width)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.sharedMaterial = mat;
        lr.widthMultiplier = width;
        lr.useWorldSpace = true;
        lr.positionCount = 0;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        return lr;
    }
}
