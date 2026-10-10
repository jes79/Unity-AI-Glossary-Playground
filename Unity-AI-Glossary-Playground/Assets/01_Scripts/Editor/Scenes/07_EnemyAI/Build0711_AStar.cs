using GlossaryDemo.EnemyAI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static GlossaryKit;
using static EnemyAIBuildUtil;

public static class Build0711_AStar
{
    const int W = 26, H = 16;

    [GlossaryScene("0711")]
    [MenuItem("Tools/Glossary/Build Scene/07 EnemyAI/0711_AStar")]
    public static void Build()
    {
        var ctx = NewScene2D("0711", "WASD/방향키 : 플레이어(파랑) 한 칸씩 이동\n마우스 왼쪽 드래그 : 벽 칠하기/지우기\nH : A* ↔ 다익스트라 비교   G : 대각선 이동   V : 탐색 칸 표시   Space : 적 정지\n노란 칸 = 최단 경로, 하늘색 칸 = 알고리즘이 탐색한 칸");

        // 타일 에셋 생성 (스프라이트 1칸 = 1유닛)
        var floorSprite = CreateSprite("AStar_Floor", 32, 32, (u, v) =>
        {
            bool edge = u < 0.04f || v < 0.04f || u > 0.96f || v > 0.96f;
            return edge ? new Color32(48, 52, 64, 255) : new Color32(62, 67, 82, 255);
        }, 32f, default, FilterMode.Point);
        var wallSprite = CreateSprite("AStar_Wall", 32, 32, (u, v) =>
        {
            bool edge = u < 0.08f || v < 0.08f || u > 0.92f || v > 0.92f;
            return edge ? new Color32(120, 110, 100, 255) : new Color32(150, 138, 120, 255);
        }, 32f, default, FilterMode.Point);
        var floorTile = MakeTile("AStar_FloorTile", floorSprite);
        var wallTile = MakeTile("AStar_WallTile", wallSprite);
        var markTile = MakeTile("AStar_MarkTile", SquareSprite);

        // Grid (스케일 0.6 → 한 칸 0.6 유닛) + 바닥/오버레이 타일맵
        var gridGo = new GameObject("Grid");
        gridGo.transform.position = new Vector3(-5.9f, -4.9f, 0f);
        gridGo.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        var grid = gridGo.AddComponent<Grid>();
        grid.cellSize = new Vector3(1f, 1f, 0f);

        var ground = NewTilemap(gridGo.transform, "Ground", 0);
        var overlay = NewTilemap(gridGo.transform, "Overlay (탐색 표시)", 1);

        // 초기 지도: 벽 몇 줄
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                bool wall = (x == 7 && y < 11) || (x == 15 && y > 4) || (y == 8 && x > 9 && x < 14) || (x == 20 && y > 2 && y < 12 && y != 7) || (y == 4 && x > 15 && x < 20);
                ground.SetTile(new Vector3Int(x, y, 0), wall ? wallTile : floorTile);
            }

        var player = CreateSpriteObject("Player", CircleSprite, Vector3.zero, new Vector2(0.45f, 0.45f), PlayerColor, null, 5);
        var enemy = CreateSpriteObject("Enemy (A*)", CircleSprite, Vector3.zero, new Vector2(0.45f, 0.45f), EnemyColor, null, 5);

        var lineGo = new GameObject("PathLine");
        var line = lineGo.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.startWidth = line.endWidth = 0.08f;
        line.sharedMaterial = LineMat(new Color(1f, 0.85f, 0.2f));
        line.sortingOrder = 4;

        var demo = new GameObject("AStarGridDemo").AddComponent<AStarGridDemo>();
        Set(demo, "grid", grid);
        Set(demo, "ground", ground);
        Set(demo, "overlay", overlay);
        Set(demo, "floorTile", floorTile);
        Set(demo, "wallTile", wallTile);
        Set(demo, "markTile", markTile);
        SetInt(demo, "width", W);
        SetInt(demo, "height", H);
        Set(demo, "player", player.transform);
        Set(demo, "enemy", enemy.transform);
        Set(demo, "pathLine", line);

        Save(ctx);
    }

    private static Tile MakeTile(string name, Sprite sprite)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.color = Color.white;
        tile.flags = TileFlags.None; // 타일맵에서 색 변경(SetColor) 허용
        return SaveAsset(tile, $"{Sub}/Tiles", $"{name}.asset");
    }

    private static Tilemap NewTilemap(Transform grid, string name, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(grid, false);
        var tm = go.AddComponent<Tilemap>();
        var r = go.AddComponent<TilemapRenderer>();
        r.sortingOrder = order;
        return tm;
    }
}
