using GlossaryDemo.Systems;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static GlossaryKit;
using static SystemsBuildUtil;

public static class Build0818_TilemapProcedural
{
    [GlossaryScene("0818")]
    [MenuItem("Tools/Glossary/Build Scene/08 Systems/0818_TilemapProcedural")]
    public static void Build()
    {
        var ctx = NewScene2D("0818", "R 또는 Space : 새 시드로 다시 생성\n+ / - : 방 개수 조절 (기본 10)   N : 방 번호 표시\n밝은 칸 = 방, 어두운 칸 = 복도, 갈색 = 벽\n같은 시드는 항상 같은 던전 (System.Random(seed))");
        ctx.Camera.orthographicSize = 27f;
        ctx.Camera.transform.position = new Vector3(36f, 21f, -10f);
        ctx.Camera.backgroundColor = new Color32(14, 15, 20, 255);

        // 타일 에셋 (스프라이트 1칸 = 1유닛, 테두리 있는 사각형)
        var room = MakeTile("Dungeon_Room", new Color32(170, 175, 190, 255), new Color32(140, 145, 160, 255));
        var corridor = MakeTile("Dungeon_Corridor", new Color32(105, 110, 125, 255), new Color32(85, 90, 105, 255));
        var wall = MakeTile("Dungeon_Wall", new Color32(120, 85, 60, 255), new Color32(85, 60, 40, 255));

        var gridGo = new GameObject("Grid");
        gridGo.AddComponent<Grid>().cellSize = new Vector3(1f, 1f, 0f);
        var floorMap = NewTilemap(gridGo.transform, "Floor", 0);
        var wallMap = NewTilemap(gridGo.transform, "Walls", 1);

        var gen = new GameObject("DungeonGenerator").AddComponent<DungeonGenerator>();
        Set(gen, "floorMap", floorMap);
        Set(gen, "wallMap", wallMap);
        Set(gen, "roomTile", room);
        Set(gen, "corridorTile", corridor);
        Set(gen, "wallTile", wall);
        SetInt(gen, "roomCount", 10);

        Save(ctx);
    }

    private static Tile MakeTile(string name, Color fill, Color edge)
    {
        var sprite = CreateSprite(name, 16, 16, (u, v) =>
        {
            bool border = u < 0.07f || v < 0.07f || u > 0.93f || v > 0.93f;
            return border ? edge : fill;
        }, 16f, default, FilterMode.Point);
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.colliderType = Tile.ColliderType.None;
        return SaveAsset(tile, $"{Sub}/Tiles", $"{name}.asset");
    }

    private static Tilemap NewTilemap(Transform grid, string name, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(grid, false);
        var tm = go.AddComponent<Tilemap>();
        go.AddComponent<TilemapRenderer>().sortingOrder = order;
        return tm;
    }
}
