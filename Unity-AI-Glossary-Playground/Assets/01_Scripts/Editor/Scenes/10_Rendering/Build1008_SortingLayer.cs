using System.Collections.Generic;
using GlossaryDemo.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static GlossaryKit;
using static RenderingBuildUtil;

public static class Build1008_SortingLayer
{
    [GlossaryScene("1008")]
    [MenuItem("Tools/Glossary/Build Scene/10 Rendering/1008_SortingLayer")]
    public static void Build()
    {
        var ctx = NewScene2D("1008", "WASD : 이동   Y : 정렬 방식 바꾸기 (OFF / 카메라 Custom Axis / 코드)\n" +
                                     "나무 위쪽(뒤)으로 가면 가려지고, 아래쪽(앞)으로 오면 나무를 가리는지 관찰\n" +
                                     "Sorting Layer: Background(땅) < Characters(나무·캐릭터) < Foreground(덤불)");
        ctx.Canvas = CreateCanvas("Canvas");
        var circle = CircleSprite;
        var square = SquareSprite;

        // Background 레이어: 잔디 바닥
        var grass = CreateSpriteObject("Grass", square, Vector3.zero, new Vector2(30f, 14f), new Color(0.32f, 0.5f, 0.3f));
        grass.sortingLayerName = "Background";
        for (int i = 0; i < 40; i++)
        {
            var dot = CreateSpriteObject($"GrassDot_{i}", circle, new Vector3((i * 7.3f % 22f) - 11f, (i * 3.7f % 9f) - 4.5f), new Vector2(0.3f, 0.15f), new Color(0.38f, 0.6f, 0.34f));
            dot.sortingLayerName = "Background";
            dot.sortingOrder = 1;
        }

        var groups = new List<SortingGroup>();
        // Characters 레이어: 나무 (루트 = 줄기 밑동, SortingGroup)
        Vector2[] treePos = { new(-5f, 1f), new(-1.5f, -1.5f), new(2.5f, 1.5f), new(5.5f, -1f), new(-4f, -3f), new(1f, 3.2f) };
        foreach (var p in treePos)
        {
            var root = new GameObject("Tree");
            root.transform.position = p;
            var trunk = CreateSpriteObject("Trunk", square, new Vector3(0f, 0.6f), new Vector2(0.45f, 1.2f), new Color(0.45f, 0.3f, 0.18f), root.transform);
            trunk.sortingLayerName = "Characters";
            var leaves = CreateSpriteObject("Leaves", circle, new Vector3(0f, 1.9f), new Vector2(2.2f, 2.2f), new Color(0.2f, 0.55f, 0.25f), root.transform, 1);
            leaves.sortingLayerName = "Characters";
            var sg = root.AddComponent<SortingGroup>();
            sg.sortingLayerName = "Characters";
            groups.Add(sg);
        }

        // 플레이어 (루트 = 발)
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0f, -3.8f);
        var body = CreateSpriteObject("Body", square, new Vector3(0f, 0.45f), new Vector2(0.6f, 0.9f), PlayerColor, player.transform);
        body.sortingLayerName = "Characters";
        var head = CreateSpriteObject("Head", circle, new Vector3(0f, 1.1f), new Vector2(0.55f, 0.55f), new Color(1f, 0.85f, 0.7f), player.transform, 1);
        head.sortingLayerName = "Characters";
        var pg = player.AddComponent<SortingGroup>();
        pg.sortingLayerName = "Characters";
        groups.Add(pg);

        // Foreground 레이어: 화면 맨 아래 덤불 (항상 모든 것 위)
        for (int i = 0; i < 7; i++)
        {
            var bush = CreateSpriteObject($"Bush_{i}", circle, new Vector3(-9f + i * 3f, -5.2f), new Vector2(2.6f, 1.6f), new Color(0.15f, 0.4f, 0.2f, 0.95f));
            bush.sortingLayerName = "Foreground";
        }

        var demo = new GameObject("YSortDemo").AddComponent<YSortDemo>();
        Set(demo, "cam", ctx.Camera);
        Set(demo, "player", player.transform);
        SetArray(demo, "sortables", groups.ToArray());
        Set(demo, "playerGroup", pg);

        var b = CreateButtonRow(ctx.Canvas.transform, 60f, "Y 정렬 방식");
        OnClick(b[0], demo.NextMode);

        HowToPanel(ctx.Canvas.transform, "Y축 정렬 설정 (에디터)",
            "<b>URP 2D Renderer</b> 사용 시\n  Renderer2D 에셋 > Transparency Sort Mode = <b>Custom Axis</b>\n  Axis = (0, 1, 0)\n" +
            "<b>Universal(3D) Renderer / Built-in</b>\n  Project Settings > Graphics > Transparency Sort Mode\n  또는 camera.transparencySortMode (이 데모)\n\n" +
            "· 여러 조각 캐릭터는 루트에 <b>Sorting Group</b>\n" +
            "· 스프라이트 Sprite Sort Point = <b>Pivot</b>, 피벗을 발에\n" +
            "· Sorting Layer 로 큰 층(바닥/캐릭터/전경)을 나누고\n  같은 층 안에서 Y 정렬",
            540f, 380f);

        Save(ctx);
    }
}
