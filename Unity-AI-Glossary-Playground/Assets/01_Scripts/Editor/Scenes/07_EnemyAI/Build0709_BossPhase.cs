using GlossaryDemo.EnemyAI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GlossaryKit;
using static EnemyAIBuildUtil;

public static class Build0709_BossPhase
{
    [GlossaryScene("0709")]
    [MenuItem("Tools/Glossary/Build Scene/07 EnemyAI/0709_BossPhase")]
    public static void Build()
    {
        var ctx = NewScene3D("0709", "WASD : 이동   Space(누르고 있기) : 보스에게 사격\nH : 보스 체력 -10% (빠르게 2페이즈 보기)   R : 다시 하기\n패턴 3개(부채꼴 탄막·돌진·점프 충격파)가 랜덤으로 반복\n체력 30% 이하 → 2페이즈 (보라색, 탄 수·속도 증가)");
        TopCamera(ctx, 22f, -16f, 55f, 0f);

        var player = Player(new Vector3(0f, 0f, -8f), 7f);

        var boss = Prim(PrimitiveType.Capsule, "Boss", new Vector3(0f, 2f, 4f), Vector3.one * 2f, EnemyColor);
        Object.DestroyImmediate(boss.GetComponent<Collider>());
        boss.layer = LayerMask.NameToLayer("Enemy");
        var eye = Prim(PrimitiveType.Cube, "Eye", new Vector3(0f, 0.5f, 0.45f), new Vector3(0.6f, 0.12f, 0.2f), Color.black, boss.transform);
        Object.DestroyImmediate(eye.GetComponent<Collider>());

        // 충격파 착지 지점 표시 (반지름 4m 원)
        var shock = new GameObject("ShockwaveRing").transform;
        Ring(shock, 4f, new Color(1f, 0.3f, 0.3f), "Ring", 64, 0.15f);

        ctx.Canvas = CreateCanvas("Canvas");
        var hpBg = CreateImage("BossHpBg", ctx.Canvas.transform, new Vector2(250f, -50f), new Vector2(900f, 34f), new Color(0f, 0f, 0f, 0.6f), WhiteSprite);
        AnchorTop(hpBg.rectTransform);
        var hpFill = CreateImage("Fill", hpBg.transform, Vector2.zero, Vector2.zero, new Color(0.9f, 0.25f, 0.25f), WhiteSprite);
        Stretch(hpFill.rectTransform, 3f);
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        // 30% 지점 표시선
        var mark = CreateImage("Phase2Mark", hpBg.transform, new Vector2(-450f + 900f * 0.3f, 0f), new Vector2(4f, 44f), Color.white, WhiteSprite);
        mark.raycastTarget = false;
        var hpText = CreateText("BossHpText", hpBg.transform, Vector2.zero, Vector2.zero, "BOSS", 22f, Color.white);
        Stretch(hpText.rectTransform);
        var banner = BigText(ctx, new Vector2(250f, -120f), "", 56f);

        var ctrl = boss.AddComponent<BossPhaseController>();
        SetFloat(ctrl, "maxHp", 1000f);
        SetFloat(ctrl, "phase2Threshold", 0.3f);
        Set(ctrl, "player", player);
        Set(ctrl, "body", boss.GetComponent<Renderer>());
        Set(ctrl, "bossBulletPrefab", BulletPrefab("BossBullet_0709", new Color(1f, 0.45f, 0.9f), 0.45f));
        Set(ctrl, "playerBulletPrefab", BulletPrefab("PlayerBullet_0709", new Color(0.5f, 0.9f, 1f), 0.25f));
        Set(ctrl, "hpFill", hpFill);
        Set(ctrl, "hpText", hpText);
        Set(ctrl, "bannerText", banner);
        Set(ctrl, "telegraph", Line("ChargeTelegraph", new Color(1f, 0.2f, 0.2f), 1.6f));
        Set(ctrl, "shockRing", shock);

        Save(ctx);
    }
}
