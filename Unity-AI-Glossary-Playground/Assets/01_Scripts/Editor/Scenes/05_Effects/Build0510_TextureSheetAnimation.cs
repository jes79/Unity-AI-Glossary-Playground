using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0510_TextureSheetAnimation
{
    [GlossaryScene("0510")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0510_TextureSheetAnimation")]
    public static void Build()
    {
        var ctx = Stage("0510", "Space / 클릭 : 폭발 1개 추가 (1.5초마다 자동 재생)\nA : Texture Sheet Animation ON/OFF\n↑ / ↓ : Cycles (수명 동안 시트를 몇 번 돌릴지)",
            new Vector3(0f, 2.2f, -8f), new Vector3(6f, 0f, 0f));

        var sheet = ExplosionSheetTex;

        // 왼쪽: 원본 4x4 시트 (참고용)
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "SheetPreview";
        NoCollider(quad);
        quad.transform.position = new Vector3(-1.6f, 1.7f, 0f);
        quad.transform.localScale = Vector3.one * 2.4f;
        quad.GetComponent<Renderer>().sharedMaterial = UnlitTex("ExplosionSheetPreview", sheet, Color.white, false);
        var bg = Prim(PrimitiveType.Quad, "SheetBg", new Vector3(-1.6f, 1.7f, 0.02f), Vector3.one * 2.5f, new Color32(60, 64, 78, 255));
        NoCollider(bg);
        Label3D("스프라이트 시트 4x4 (0→15)", new Vector3(-1.6f, 0.25f, 0f), 2.5f);

        // 오른쪽: 시트를 재생하는 파티클
        var ps = PS("Explosion", null, new Vector3(2.4f, 1.7f, 0f), ParticleMat("FX05_ExplosionSheet", Color.white, false, sheet));
        var m = ps.main;
        m.duration = 1.5f;
        m.loop = true;
        m.startLifetime = 1.2f;
        m.startSpeed = 0f;
        m.startSize = 3f;
        m.startRotation = Range(0f, Mathf.PI * 2f);
        Burst(ps, 1);
        var shape = ps.shape;
        shape.enabled = false;
        var tsa = ps.textureSheetAnimation;
        tsa.enabled = true;
        tsa.numTilesX = 4;
        tsa.numTilesY = 4;
        tsa.frameOverTime = CurveMM(1f, 0f, 0f, 1f, 1f); // 수명 0→1 동안 프레임 0→끝
        Label3D("Texture Sheet Animation", new Vector3(2.4f, 0.25f, 0f), 2.5f);

        var demo = ps.gameObject.AddComponent<TextureSheetDemo>();
        Set(demo, "explosion", ps);
        var b = Buttons(ctx, "폭발", "Sheet ON/OFF");
        OnClick(b[0], demo.Explode);
        OnClick(b[1], demo.ToggleSheet);
        Save(ctx);
    }
}
