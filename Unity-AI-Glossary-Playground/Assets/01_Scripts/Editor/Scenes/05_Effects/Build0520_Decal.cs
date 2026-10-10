using GlossaryDemo.Effects;
using UnityEditor;
using UnityEngine;
using static EffectsBuildUtil;
using static GlossaryKit;

public static class Build0520_Decal
{
    [GlossaryScene("0520")]
    [MenuItem("Tools/Glossary/Build Scene/05 Effects/0520_Decal")]
    public static void Build()
    {
        var ctx = Stage("0520", "클릭 : 그 자리에 총알 자국   Space : 랜덤 사격   C : 모두 지우기\nM : URP Decal Projector ↔ Quad 대체 구현\n자국은 5초 뒤 서서히 사라짐 — 기둥(곡면)에 쏴서 두 방식 차이를 비교",
            new Vector3(0f, 2f, -6f), new Vector3(6f, 0f, 0f));

        // 방: 뒷벽, 옆벽, 둥근 기둥
        var wall = new Color32(150, 145, 135, 255);
        Prim(PrimitiveType.Cube, "BackWall", new Vector3(0f, 2.5f, 4f), new Vector3(14f, 5f, 0.4f), wall);
        Prim(PrimitiveType.Cube, "SideWall", new Vector3(5f, 2.5f, 1f), new Vector3(0.4f, 5f, 6f), wall);
        Prim(PrimitiveType.Cylinder, "Pillar", new Vector3(-2f, 2f, 2f), new Vector3(1.2f, 2f, 1.2f), new Color32(170, 160, 150, 255));

        var tex = BulletHoleTex;
        var quadMat = UnlitTex("BulletHole_Quad", tex, Color.white, false);

        // URP Decal 셰이더(Shader Graphs/Decal) 머티리얼. 없으면 Quad 만 사용
        Material decalMat = null;
        try
        {
            decalMat = CreateMaterial("FX05_BulletHole_Decal", "Shader Graphs/Decal", m => m.SetTexture("Base_Map", tex));
        }
        catch (System.Exception e) { Debug.LogWarning($"[0520] Decal 셰이더를 찾지 못해 Quad 대체 구현만 사용: {e.Message}"); }

        // 공유 렌더러는 그대로 두고 Decal Renderer Feature 를 넣은 복제 렌더러 + 복제 파이프라인 생성
        var pipeline = decalMat != null ? CreateDecalPipeline() : null;

        var demo = new GameObject("DecalDemo").AddComponent<DecalDemo>();
        Set(demo, "decalPipeline", pipeline);
        Set(demo, "decalMaterial", decalMat);
        Set(demo, "quadMaterial", quadMat);

        var b = Buttons(ctx, "랜덤 사격", "방식 전환", "지우기");
        OnClick(b[0], demo.ShootRandom);
        OnClick(b[1], demo.ToggleMode);
        OnClick(b[2], demo.ClearAll);

        InfoCard(ctx, "실제 프로젝트에서 URP Decal 쓰는 법",
            "1. Project 창에서 사용 중인 <b>Universal Renderer</b> 에셋 선택\n" +
            "   (이 프로젝트: Assets/Settings/PC_Renderer)\n" +
            "2. Inspector 맨 아래 <b>Add Renderer Feature → Decal</b>\n" +
            "   Technique: Automatic (DBuffer / Screen Space)\n" +
            "3. 머티리얼 셰이더를 <b>Shader Graphs/Decal</b> 로, Base Map 에 자국 텍스처\n" +
            "4. 빈 오브젝트에 <b>Decal Projector</b> 추가, Material 지정\n" +
            "   Size(x,y = 크기, z = 투영 깊이), 표면을 향하도록 회전\n" +
            "5. 사라지게: <b>fadeFactor</b> 를 1→0 으로 줄인 뒤 Destroy\n\n" +
            "<color=#9AA3B5>이 데모는 공유 렌더러를 고치지 않으려고 09_Generated/Effects 에\n" +
            "복제 렌더러+파이프라인을 만들고 이 씬에서만 교체해서 씀.\n" +
            "Quad 방식은 곡면(기둥)에서 자국이 떠 보이는 한계가 있음</color>", 600f, 470f);
        Save(ctx);
    }
}
