using GlossaryDemo.PhysicsDemo;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static PhysicsBuildUtil;

public static class Build0202_Trigger
{
    [GlossaryScene("0202")]
    [MenuItem("Tools/Glossary/Build Scene/02 Physics/0202_Trigger")]
    public static void Build()
    {
        var ctx = NewScene3D("0202",
            "WASD : 이동\nT : 코인 Is Trigger ON/OFF\nON: 코인을 통과하며 OnTriggerEnter → 점수 +10\nOFF: 코인이 벽처럼 막히고 OnCollisionEnter 만 호출 (점수 없음)",
            true, 24f);

        var coinMat = LitMat("Coin", new Color32(255, 205, 60, 255), 0.8f, 0.6f);
        for (int i = 0; i < 10; i++)
        {
            float a = i * 36f * Mathf.Deg2Rad;
            var coin = Prim(PrimitiveType.Cylinder, $"Coin_{i}", new Vector3(Mathf.Cos(a) * 5f, 0.8f, Mathf.Sin(a) * 5f + 1f), new Vector3(0.8f, 0.08f, 0.8f), Color.white);
            coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            coin.GetComponent<Renderer>().sharedMaterial = coinMat;
            coin.tag = "Coin";
            Object.DestroyImmediate(coin.GetComponent<Collider>());
            var col = coin.AddComponent<BoxCollider>();
            col.size = new Vector3(1f, 6f, 1f); // 얇은 원판이라 두께 방향으로 넉넉하게
            col.isTrigger = true; // ★ 핵심: 통과 가능, 닿았다는 것만 감지
            coin.AddComponent<Spinner>();
        }

        var player = Player(new Vector3(0f, 1f, 1f));
        var collector = player.gameObject.AddComponent<CoinCollector>();

        ctx.Canvas = CreateCanvas("DemoCanvas");
        var score = CreateText("Score", ctx.Canvas.transform, new Vector2(-40f, -30f), new Vector2(400f, 80f), "점수 0", 56f, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Right);
        score.rectTransform.pivot = new Vector2(1f, 1f);
        AnchorTopRight(score.rectTransform);
        score.fontStyle = FontStyles.Bold;
        Set(collector, "scoreText", score);
        Save(ctx);
    }
}
