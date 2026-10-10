using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0522_VFXGraph
{
    [GlossaryScene("0522")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0522_VFXGraph")]
    public static void Build()
    {
        var ctx = Stage("0522", "↑ / ↓ : 보이는 입자 수 ±5,000 (최대 50,000)\n← / → : 회전 속도   Space : 포탈 펄스\n.vfx 를 코드로 만들 수 없어 같은 원리(GPU 계산)의 대체 구현 — 오른쪽에 VFX Graph 레시피",
            new Vector3(-0.3f, 2.5f, -10f), new Vector3(0f, 0f, 0f), ground: false);
        ctx.Camera.backgroundColor = new Color32(6, 6, 12, 255);
        EnablePostProcessing(ctx.Camera);

        var portal = new GameObject("VortexPortal");
        portal.transform.position = new Vector3(0f, 2.5f, 4f);
        portal.AddComponent<MeshFilter>();
        var mr = portal.AddComponent<MeshRenderer>();
        mr.sharedMaterial = CreateMaterial("FX05_VortexPortal", "Glossary/05_Effects/VortexPortal");
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        portal.AddComponent<VortexPortal>();

        // 포탈 테두리 링 (가산 Quad)
        var ring = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ring.name = "PortalRing";
        NoCollider(ring);
        ring.transform.position = portal.transform.position + new Vector3(0f, 0f, -0.05f);
        ring.transform.localScale = Vector3.one * 9.6f;
        ring.GetComponent<Renderer>().sharedMaterial = UnlitTex("PortalRing", RingTex, new Color(0.5f, 0.4f, 1f, 0.8f), true);

        // 은은한 Bloom 으로 입자 빛 번짐
        var profile = Profile("Profile_0522_Portal");
        var bloom = AddOverride<Bloom>(profile);
        bloom.intensity.Override(1.2f);
        bloom.threshold.Override(0.8f);
        bloom.scatter.Override(0.7f);
        SaveProfile(profile);
        GlobalVolume("Global Volume", profile);

        var b = Buttons(ctx, "입자 -10,000", "입자 +10,000", "펄스");
        var demo = portal.GetComponent<VortexPortal>();
        OnClick(b[0], demo.FewerParticles);
        OnClick(b[1], demo.MoreParticles);
        OnClick(b[2], demo.Pulse);

        InfoCard(ctx, "VFX Graph 로 만들 때의 노드 레시피",
            "0. Create ▸ Visual Effects ▸ Visual Effect Graph (Simple Loop)\n" +
            "1. <b>Spawn</b>: Constant Spawn Rate 10000/s (또는 Single Burst 50000)\n" +
            "2. <b>Initialize Particle</b>: Capacity 50000\n" +
            "   Set Position (Shape: Arc Circle, Radius 4, Surface)\n" +
            "   Set Lifetime Random 4~6, Set Size 0.03~0.06\n" +
            "3. <b>Update Particle</b>\n" +
            "   Add Velocity = Cross(Position, (0,0,1)) × Spin  ← 공전\n" +
            "   Add Velocity = -Normalize(Position) × Pull   ← 빨려듦\n" +
            "   Linear Drag 1,  Kill (Sphere) 반지름 0.4 안쪽은 제거\n" +
            "4. <b>Output Particle Quad</b>: Blend Additive\n" +
            "   Color over Life (분홍→파랑), Size over Life\n" +
            "5. Blackboard 에 Spin / Pull 노출 → 스크립트에서\n" +
            "   <b>visualEffect.SetFloat(\"Spin\", 1.5f)</b>\n\n" +
            "<color=#9AA3B5>대체 구현: 입자마다 사각형+씨앗값만 담은 메시를 한 번 만들고,\n" +
            "위치는 셰이더(Glossary/05_Effects/VortexPortal)가 GPU 에서 계산.\n" +
            "VFX Graph 도 같은 이유(GPU 시뮬레이션)로 수십만 개가 가능함</color>", 620f, 560f);
        Save(ctx);
    }
}
