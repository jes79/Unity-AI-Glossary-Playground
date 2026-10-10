using System.Collections.Generic;
using UnityEngine;

namespace GlossaryDemo.EnemyAI
{
    // A* 길찾기 (격자용)
    //  f = g(시작부터 실제 비용) + h(목표까지 추정 비용)
    //  열린 목록(Open)에서 f 가 가장 작은 칸을 꺼내 이웃을 갱신하는 것을 목표에 닿을 때까지 반복
    //  h = 0 으로 두면 다익스트라(Dijkstra)와 같아짐 → 훨씬 많은 칸을 탐색
    public static class AStarPathfinder
    {
        private static readonly Vector2Int[] Dir4 = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };
        private static readonly Vector2Int[] Dir8 = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1), new(1, 1), new(1, -1), new(-1, 1), new(-1, -1) };

        public class Result
        {
            public List<Vector2Int> Path = new List<Vector2Int>();
            public List<Vector2Int> Visited = new List<Vector2Int>(); // 닫힌 목록에 들어간 순서
            public bool Found;
        }

        public static Result FindPath(bool[,] walls, Vector2Int start, Vector2Int goal, bool diagonal, bool useHeuristic)
        {
            int w = walls.GetLength(0), h = walls.GetLength(1);
            var result = new Result();
            if (!Inside(start, w, h) || !Inside(goal, w, h) || walls[goal.x, goal.y]) return result;

            var g = new float[w, h];
            var closed = new bool[w, h];
            var parent = new Vector2Int[w, h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++) g[x, y] = float.MaxValue;

            var open = new List<Vector2Int> { start };
            g[start.x, start.y] = 0f;
            parent[start.x, start.y] = start;
            var dirs = diagonal ? Dir8 : Dir4;

            while (open.Count > 0)
            {
                // f 가 가장 작은 칸 선택 (학습용이라 단순 선형 탐색. 큰 맵은 우선순위 큐 사용)
                int bestIdx = 0;
                float bestF = float.MaxValue;
                for (int i = 0; i < open.Count; i++)
                {
                    var c = open[i];
                    float f = g[c.x, c.y] + (useHeuristic ? Heuristic(c, goal, diagonal) : 0f);
                    if (f < bestF) { bestF = f; bestIdx = i; }
                }
                var cur = open[bestIdx];
                open.RemoveAt(bestIdx);
                if (closed[cur.x, cur.y]) continue;
                closed[cur.x, cur.y] = true;
                result.Visited.Add(cur);

                if (cur == goal)
                {
                    result.Found = true;
                    var p = goal;
                    while (p != start)
                    {
                        result.Path.Add(p);
                        p = parent[p.x, p.y];
                    }
                    result.Path.Add(start);
                    result.Path.Reverse();
                    return result;
                }

                foreach (var d in dirs)
                {
                    var n = cur + d;
                    if (!Inside(n, w, h) || walls[n.x, n.y] || closed[n.x, n.y]) continue;
                    // 대각선은 양옆이 모두 비어 있을 때만 (벽 모서리 뚫기 방지)
                    if (d.x != 0 && d.y != 0 && (walls[cur.x + d.x, cur.y] || walls[cur.x, cur.y + d.y])) continue;
                    float cost = g[cur.x, cur.y] + (d.x != 0 && d.y != 0 ? 1.4142f : 1f);
                    if (cost < g[n.x, n.y])
                    {
                        g[n.x, n.y] = cost;
                        parent[n.x, n.y] = cur;
                        open.Add(n);
                    }
                }
            }
            return result;
        }

        // 4방향: 맨해튼 거리, 8방향: 옥타일 거리
        private static float Heuristic(Vector2Int a, Vector2Int b, bool diagonal)
        {
            int dx = Mathf.Abs(a.x - b.x), dy = Mathf.Abs(a.y - b.y);
            if (!diagonal) return dx + dy;
            return (dx + dy) + (1.4142f - 2f) * Mathf.Min(dx, dy);
        }

        private static bool Inside(Vector2Int p, int w, int h) => p.x >= 0 && p.y >= 0 && p.x < w && p.y < h;
    }
}
