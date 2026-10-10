using GlossaryDemo.Systems;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0812_SaveLoad
{
    [GlossaryScene("0812")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0812_SaveLoad")]
    public static void Build()
    {
        var items = EnsureItems();
        var ctx = NewScene3D("0812", "WASD : 이동   H / J : HP -10 / +10   1 / 2 / 3 : 포션·젤리·보석 획득\nF5 : 저장   F9 : 불러오기   Delete : 저장 파일 삭제 (버튼도 있음)\n저장 후 위치·HP·인벤토리를 바꾼 다음 F9 → 저장 시점으로 돌아감\n오른쪽 = 실제 JSON 파일 내용 (플레이를 껐다 켜도 남아 있음)");
        EnemyAIBuildUtil.TopCamera(ctx, 18f, -13f, 55f, -3f);

        // 위치 확인용 기둥들
        Color[] c = { new Color32(255, 120, 90, 255), new Color32(90, 200, 255, 255), new Color32(255, 220, 90, 255), new Color32(140, 230, 120, 255) };
        Vector3[] p = { new(-10f, 1f, 8f), new(6f, 1f, 8f), new(-10f, 1f, -8f), new(6f, 1f, -8f) };
        for (int i = 0; i < 4; i++) Prim(PrimitiveType.Cylinder, $"Pillar_{i}", p[i], new Vector3(1.5f, 1f, 1.5f), c[i]);

        var player = EnemyAIBuildUtil.Player(new Vector3(-2f, 0f, 0f));

        ctx.Canvas = CreateCanvas("Canvas");
        var root = ctx.Canvas.transform;
        var panel = Panel(root, "SavePanel", new Vector2(-260f, 0f), new Vector2(480f, 1000f), new Color32(20, 22, 28, 225));
        AnchorTopRight(panel.rectTransform);
        panel.rectTransform.anchoredPosition = new Vector2(-260f, -520f);
        var state = Label(panel.transform, "State", new Vector2(0f, 380f), new Vector2(440f, 200f), "", 20f, TextAlignmentOptions.TopLeft);
        Label(panel.transform, "JsonTitle", new Vector2(0f, 255f), new Vector2(440f, 30f), "<b>저장 파일 (JSON)</b>", 20f, TextAlignmentOptions.Left, new Color(0.6f, 0.85f, 1f));
        var json = Label(panel.transform, "Json", new Vector2(0f, -40f), new Vector2(440f, 560f), "", 15f, TextAlignmentOptions.TopLeft, new Color(0.8f, 0.9f, 0.8f));
        var path = Label(panel.transform, "Path", new Vector2(0f, -360f), new Vector2(440f, 70f), "", 13f, TextAlignmentOptions.TopLeft, SubTextColor);
        path.overflowMode = TextOverflowModes.Ellipsis;

        var demo = new GameObject("SaveLoadDemo").AddComponent<SaveLoadDemo>();
        OnClick(SmallButton(panel.transform, "Save", new Vector2(-150f, -440f), new Vector2(130f, 60f), "저장 F5"), demo.Save);
        OnClick(SmallButton(panel.transform, "Load", new Vector2(0f, -440f), new Vector2(130f, 60f), "불러오기 F9", new Color32(70, 160, 110, 255), 20f), demo.Load);
        OnClick(SmallButton(panel.transform, "Delete", new Vector2(150f, -440f), new Vector2(130f, 60f), "삭제", EnemyColor), demo.DeleteSave);

        Set(demo, "player", player);
        SetArray(demo, "itemDatabase", items["potion_hp"], items["slime_jelly"], items["gem"], items["potion_mp"], items["sword_steel"]);
        Set(demo, "stateText", state);
        Set(demo, "jsonText", json);
        Set(demo, "pathText", path);

        Save(ctx);
    }
}
