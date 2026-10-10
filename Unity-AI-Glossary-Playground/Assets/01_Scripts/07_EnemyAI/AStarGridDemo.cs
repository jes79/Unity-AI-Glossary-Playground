using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace GlossaryDemo.EnemyAI
{
    // 2D 타일맵 + A* 적 길찾기
    // 벽 타일 정보를 bool[,] 격자로 읽어 A* 로 경로를 구하고, 적이 경로를 따라 플레이어를 쫓아감
    public class AStarGridDemo : MonoBehaviour
    {
        [Header("타일맵")]
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap ground;
        [SerializeField] private Tilemap overlay;
        [SerializeField] private TileBase floorTile;
        [SerializeField] private TileBase wallTile;
        [SerializeField] private TileBase markTile;
        [SerializeField] private int width = 26;
        [SerializeField] private int height = 16;

        [Header("유닛")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform enemy;
        [SerializeField] private Vector2Int playerStart = new Vector2Int(22, 12);
        [SerializeField] private Vector2Int enemyStart = new Vector2Int(2, 2);
        [SerializeField] private float enemySpeed = 3.5f;   // 초당 칸 수
        [SerializeField] private float repathInterval = 0.25f;
        [SerializeField] private LineRenderer pathLine;

        [Header("옵션")]
        [SerializeField] private bool diagonal = false;
        [SerializeField] private bool useHeuristic = true;   // false = 다익스트라
        [SerializeField] private bool showVisited = true;

        private bool[,] walls;
        private Vector2Int playerCell;
        private AStarPathfinder.Result result;
        private float repathTimer;
        private float moveRepeat;
        private bool paintValue;
        private bool enemyPaused;
        private int caught;
        private double lastMs;
        private Vector2Int enemyCell;    // 적이 서 있는(출발한) 칸
        private Vector2Int enemyTarget;  // 지금 향하는 다음 칸
        private bool enemyMoving;

        private void Start()
        {
            walls = new bool[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    walls[x, y] = ground.GetTile(new Vector3Int(x, y, 0)) == wallTile;
            playerCell = playerStart;
            player.position = CellCenter(playerCell);
            enemyCell = enemyTarget = enemyStart;
            enemy.position = CellCenter(enemyStart);
            Repath();
        }

        private void Update()
        {
            HandleInput();

            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f) Repath();

            if (!enemyPaused) MoveEnemy();

            string mode = useHeuristic ? "A* (f = g + h)" : "다익스트라 (h = 0)";
            GlossaryInfoPanel.Status =
                $"알고리즘: <b>{mode}</b>   이동: {(diagonal ? "8방향" : "4방향")}\n" +
                $"경로 길이: {(result != null && result.Found ? result.Path.Count - 1 + "칸" : "<color=#FF7A7A>경로 없음</color>")}   " +
                $"탐색한 칸: {(result != null ? result.Visited.Count : 0)}개   계산 {lastMs:0.00}ms\n" +
                $"잡힌 횟수: {caught}{(enemyPaused ? "   (적 정지)" : "")}";
        }

        private void HandleInput()
        {
            if (DemoInput.KeyDown(Key.H)) { useHeuristic = !useHeuristic; Repath(); }
            if (DemoInput.KeyDown(Key.G)) { diagonal = !diagonal; Repath(); }
            if (DemoInput.KeyDown(Key.V)) { showVisited = !showVisited; Repath(); }
            if (DemoInput.KeyDown(Key.Space)) enemyPaused = !enemyPaused;

            // 플레이어: 방향키/WASD 로 한 칸씩 (누르고 있으면 반복)
            Vector2 m = DemoInput.Move;
            Vector2Int step = Vector2Int.zero;
            if (Mathf.Abs(m.x) > 0.5f) step.x = (int)Mathf.Sign(m.x);
            else if (Mathf.Abs(m.y) > 0.5f) step.y = (int)Mathf.Sign(m.y);
            moveRepeat -= Time.deltaTime;
            if (step == Vector2Int.zero) moveRepeat = 0f;
            else if (moveRepeat <= 0f)
            {
                var next = playerCell + step;
                if (Inside(next) && !walls[next.x, next.y])
                {
                    playerCell = next;
                    player.position = CellCenter(playerCell);
                    Repath();
                }
                moveRepeat = 0.13f;
            }

            // 마우스: 누른 칸의 반대 상태로 드래그하며 벽 칠하기/지우기
            if (DemoInput.LeftHeld)
            {
                Vector3Int c = grid.WorldToCell(DemoInput.MouseWorld2D());
                var cell = new Vector2Int(c.x, c.y);
                if (Inside(cell) && cell != playerCell && cell != enemyCell && cell != enemyTarget)
                {
                    if (DemoInput.LeftClick) paintValue = !walls[cell.x, cell.y];
                    if (walls[cell.x, cell.y] != paintValue)
                    {
                        walls[cell.x, cell.y] = paintValue;
                        ground.SetTile(c, paintValue ? wallTile : floorTile);
                        Repath();
                    }
                }
            }
        }

        private void Repath()
        {
            repathTimer = repathInterval;
            var sw = Stopwatch.StartNew();
            result = AStarPathfinder.FindPath(walls, enemyCell, playerCell, diagonal, useHeuristic);
            sw.Stop();
            lastMs = sw.Elapsed.TotalMilliseconds;
            DrawOverlay();
        }

        private void DrawOverlay()
        {
            overlay.ClearAllTiles();
            if (showVisited)
                foreach (var v in result.Visited) Mark(v, new Color(0.3f, 0.6f, 1f, 0.28f));
            if (pathLine != null)
            {
                pathLine.positionCount = result.Path.Count;
                for (int i = 0; i < result.Path.Count; i++)
                {
                    Mark(result.Path[i], new Color(1f, 0.85f, 0.2f, 0.45f));
                    pathLine.SetPosition(i, CellCenter(result.Path[i]) + Vector3.back * 0.1f);
                }
            }
        }

        private void Mark(Vector2Int cell, Color color)
        {
            var c = new Vector3Int(cell.x, cell.y, 0);
            overlay.SetTile(c, markTile);
            overlay.SetTileFlags(c, TileFlags.None);
            overlay.SetColor(c, color);
        }

        // 칸 중심 → 다음 칸 중심으로만 이동하고, 도착할 때마다 경로를 다시 계산
        private void MoveEnemy()
        {
            if (!enemyMoving)
            {
                if (result == null || !result.Found || result.Path.Count < 2) return;
                enemyTarget = result.Path[1];
                enemyMoving = true;
            }

            Vector3 target = CellCenter(enemyTarget);
            enemy.position = Vector3.MoveTowards(enemy.position, target, enemySpeed * grid.cellSize.x * grid.transform.lossyScale.x * Time.deltaTime);
            if ((enemy.position - target).sqrMagnitude > 0.0001f) return;

            enemyCell = enemyTarget;
            enemyMoving = false;
            if (enemyCell == playerCell)
            {
                caught++;
                enemyCell = enemyTarget = enemyStart;
                enemy.position = CellCenter(enemyStart);
            }
            Repath();
        }

        private Vector3 CellCenter(Vector2Int cell)
        {
            Vector3 p = grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
            p.z = -0.2f;
            return p;
        }

        private bool Inside(Vector2Int p) => p.x >= 0 && p.y >= 0 && p.x < width && p.y < height;
    }
}
