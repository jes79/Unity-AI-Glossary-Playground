using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace GlossaryDemo.Systems
{
    // 타일맵 절차적 생성: 랜덤 방 10개 + L자 복도 + 벽 두르기
    //  1) 겹치지 않는 위치에 사각형 방을 무작위로 배치 (실패하면 다시 시도)
    //  2) 방 중심을 x 순서로 정렬해 이웃끼리 L자 복도로 연결 → 모든 방이 이어짐
    //  3) 바닥 옆의 빈 칸에 벽 타일
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("타일맵")]
        [SerializeField] private Tilemap floorMap;
        [SerializeField] private Tilemap wallMap;
        [SerializeField] private TileBase roomTile;
        [SerializeField] private TileBase corridorTile;
        [SerializeField] private TileBase wallTile;

        [Header("생성 설정")]
        [SerializeField] private int width = 80;
        [SerializeField] private int height = 45;
        [SerializeField] private int roomCount = 10;
        [SerializeField] private Vector2Int roomSizeMin = new Vector2Int(5, 4);
        [SerializeField] private Vector2Int roomSizeMax = new Vector2Int(13, 9);
        [SerializeField] private int maxAttempts = 300;
        [SerializeField] private int seed = 12345;
        [SerializeField] private bool showRoomNumbers = true;

        private readonly List<RectInt> rooms = new List<RectInt>();
        private readonly List<GameObject> labels = new List<GameObject>();
        private int attemptsUsed;
        private int floorCells;

        private void Start() => Generate(seed);

        private void Update()
        {
            if (DemoInput.KeyDown(Key.R) || DemoInput.KeyDown(Key.Space)) Generate(Random.Range(0, 1000000));
            if (DemoInput.KeyDown(Key.Equals) || DemoInput.KeyDown(Key.NumpadPlus)) { roomCount = Mathf.Min(25, roomCount + 1); Generate(seed); }
            if (DemoInput.KeyDown(Key.Minus) || DemoInput.KeyDown(Key.NumpadMinus)) { roomCount = Mathf.Max(2, roomCount - 1); Generate(seed); }
            if (DemoInput.KeyDown(Key.N)) { showRoomNumbers = !showRoomNumbers; foreach (var l in labels) l.SetActive(showRoomNumbers); }

            GlossaryInfoPanel.Status = $"시드 {seed}   방 {rooms.Count}/{roomCount}개 (시도 {attemptsUsed}회)\n" +
                                       $"맵 {width}×{height}   바닥 타일 {floorCells}칸";
        }

        public void Generate(int newSeed)
        {
            seed = newSeed;
            var rng = new System.Random(seed); // 같은 시드 → 같은 던전
            floorMap.ClearAllTiles();
            wallMap.ClearAllTiles();
            rooms.Clear();
            foreach (var l in labels) Destroy(l);
            labels.Clear();

            // 1) 방 배치
            attemptsUsed = 0;
            while (rooms.Count < roomCount && attemptsUsed < maxAttempts)
            {
                attemptsUsed++;
                int w = rng.Next(roomSizeMin.x, roomSizeMax.x + 1);
                int h = rng.Next(roomSizeMin.y, roomSizeMax.y + 1);
                var r = new RectInt(rng.Next(1, width - w - 1), rng.Next(1, height - h - 1), w, h);
                bool overlap = false;
                foreach (var o in rooms)
                {
                    // 방 사이에 최소 2칸 여유
                    var grown = new RectInt(o.xMin - 2, o.yMin - 2, o.width + 4, o.height + 4);
                    if (grown.Overlaps(r)) { overlap = true; break; }
                }
                if (!overlap) rooms.Add(r);
            }
            foreach (var r in rooms)
                for (int x = r.xMin; x < r.xMax; x++)
                    for (int y = r.yMin; y < r.yMax; y++)
                        floorMap.SetTile(new Vector3Int(x, y, 0), roomTile);

            // 2) 복도: x 순으로 정렬해 이웃 방끼리 L자 연결
            var ordered = new List<RectInt>(rooms);
            ordered.Sort((a, b) => a.center.x.CompareTo(b.center.x));
            for (int i = 0; i < ordered.Count - 1; i++)
            {
                var a = Vector2Int.FloorToInt(ordered[i].center);
                var b = Vector2Int.FloorToInt(ordered[i + 1].center);
                if (rng.Next(2) == 0) { HLine(a.x, b.x, a.y); VLine(a.y, b.y, b.x); }
                else { VLine(a.y, b.y, a.x); HLine(a.x, b.x, b.y); }
            }

            // 3) 벽: 바닥 주변 8칸 중 빈 칸
            floorCells = 0;
            var bounds = new BoundsInt(-1, -1, 0, width + 2, height + 2, 1);
            foreach (var p in bounds.allPositionsWithin)
            {
                if (floorMap.HasTile(p)) { floorCells++; continue; }
                if (HasFloorNeighbor(p)) wallMap.SetTile(p, wallTile);
            }

            // 방 번호 표시
            for (int i = 0; i < rooms.Count; i++)
            {
                var go = new GameObject($"RoomLabel_{i + 1}");
                go.transform.position = floorMap.CellToWorld(Vector3Int.FloorToInt(rooms[i].center)) + new Vector3(0.5f, 0.5f, -1f);
                var t = go.AddComponent<TextMeshPro>();
                t.font = GlossaryInfoPanel.Font;
                t.text = (i + 1).ToString();
                t.fontSize = 14f;
                t.alignment = TextAlignmentOptions.Center;
                t.color = new Color(1f, 1f, 1f, 0.85f);
                t.sortingOrder = 10;
                go.SetActive(showRoomNumbers);
                labels.Add(go);
            }
        }

        private void HLine(int x0, int x1, int y)
        {
            for (int x = Mathf.Min(x0, x1); x <= Mathf.Max(x0, x1); x++) Carve(x, y);
        }

        private void VLine(int y0, int y1, int x)
        {
            for (int y = Mathf.Min(y0, y1); y <= Mathf.Max(y0, y1); y++) Carve(x, y);
        }

        // 방 바닥은 그대로 두고 빈 곳만 복도 타일로
        private void Carve(int x, int y)
        {
            var p = new Vector3Int(x, y, 0);
            if (!floorMap.HasTile(p)) floorMap.SetTile(p, corridorTile);
        }

        private bool HasFloorNeighbor(Vector3Int p)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if ((dx != 0 || dy != 0) && floorMap.HasTile(p + new Vector3Int(dx, dy, 0))) return true;
            return false;
        }
    }
}
