using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Patterns
{
    // 상자 밀기 퍼즐 + Undo/Redo (커맨드 패턴)
    // 입력 → MoveCommand 생성 → Execute → undoStack 에 Push.  Z → Pop 해서 Undo → redoStack 에 Push
    public class CommandPuzzle : MonoBehaviour
    {
        [Header("맵 ( # 벽, . 목표, B 상자, P 플레이어 )")]
        [SerializeField] private string[] level =
        {
            "##########",
            "#        #",
            "#  B  .  #",
            "#  P  #  #",
            "# .  B   #",
            "#        #",
            "##########",
        };

        [Header("표시")]
        [SerializeField] private Sprite tileSprite;
        [SerializeField] private Color wallColor = new Color32(70, 76, 90, 255);
        [SerializeField] private Color floorColor = new Color32(36, 40, 52, 255);
        [SerializeField] private Color goalColor = new Color32(90, 200, 120, 255);
        [SerializeField] private Color boxColor = new Color32(200, 140, 70, 255);
        [SerializeField] private Color playerColor = new Color32(80, 170, 255, 255);
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private float moveLerp = 18f;
        [SerializeField] private TextMeshProUGUI historyText;

        [Header("비교")]
        [Tooltip("끄면 명령을 기록하지 않음 → Undo 불가 (직접 위치를 바꾸는 방식)")]
        [SerializeField] private bool recordCommands = true;

        private readonly Stack<ICommand> undoStack = new();
        private readonly Stack<ICommand> redoStack = new();
        private readonly HashSet<Vector2Int> walls = new();
        private readonly List<Vector2Int> goals = new();
        private readonly List<Vector2Int> boxes = new();
        private readonly List<Transform> boxViews = new();
        private Transform playerView;
        private Vector2Int playerCell;
        private Vector2Int startPlayer;
        private readonly List<Vector2Int> startBoxes = new();
        private int width, height;

        public Vector2Int PlayerCell => playerCell;
        public Vector2Int GetBoxCell(int i) => boxes[i];
        public void SetPlayerCell(Vector2Int c) => playerCell = c;
        public void SetBoxCell(int i, Vector2Int c) => boxes[i] = c;

        private void Start()
        {
            Parse();
            RefreshHistory();
        }

        private void Update()
        {
            Vector2Int dir = Vector2Int.zero;
            if (DemoInput.KeyDown(Key.W) || DemoInput.KeyDown(Key.UpArrow)) dir = Vector2Int.up;
            else if (DemoInput.KeyDown(Key.S) || DemoInput.KeyDown(Key.DownArrow)) dir = Vector2Int.down;
            else if (DemoInput.KeyDown(Key.A) || DemoInput.KeyDown(Key.LeftArrow)) dir = Vector2Int.left;
            else if (DemoInput.KeyDown(Key.D) || DemoInput.KeyDown(Key.RightArrow)) dir = Vector2Int.right;
            if (dir != Vector2Int.zero) TryMove(dir);

            if (DemoInput.KeyDown(Key.Z)) Undo();
            if (DemoInput.KeyDown(Key.Y)) Redo();
            if (DemoInput.KeyDown(Key.R)) ResetLevel();
            if (DemoInput.KeyDown(Key.C)) ToggleRecording();

            // 보이는 위치는 칸 좌표를 부드럽게 따라감
            playerView.position = Vector3.Lerp(playerView.position, CellToWorld(playerCell), moveLerp * Time.deltaTime);
            for (int i = 0; i < boxes.Count; i++)
            {
                boxViews[i].position = Vector3.Lerp(boxViews[i].position, CellToWorld(boxes[i]), moveLerp * Time.deltaTime);
                boxViews[i].GetComponent<SpriteRenderer>().color = goals.Contains(boxes[i]) ? Color.Lerp(boxColor, goalColor, 0.6f) : boxColor;
            }

            int onGoal = 0;
            foreach (var b in boxes) if (goals.Contains(b)) onGoal++;
            GlossaryInfoPanel.Status = $"Undo 스택 {undoStack.Count}   Redo 스택 {redoStack.Count}   명령 기록 {(recordCommands ? "ON" : "<color=#FF6060>OFF (Undo 불가)</color>")}\n" +
                                       $"목표 위 상자 {onGoal}/{goals.Count}{(onGoal == goals.Count ? "   <color=#9BE89B>클리어!</color>" : "")}";
        }

        public void TryMove(Vector2Int dir)
        {
            Vector2Int next = playerCell + dir;
            if (walls.Contains(next)) return;
            int box = boxes.IndexOf(next);
            if (box >= 0)
            {
                Vector2Int boxNext = next + dir;
                if (walls.Contains(boxNext) || boxes.Contains(boxNext)) return; // 밀 수 없음
            }

            var cmd = new MoveCommand(this, dir, box);
            cmd.Execute();
            if (recordCommands)
            {
                undoStack.Push(cmd);
                redoStack.Clear(); // 새 명령을 하면 Redo 기록은 무효
            }
            RefreshHistory();
        }

        public void Undo()
        {
            if (undoStack.Count == 0) return;
            var cmd = undoStack.Pop();
            cmd.Undo();
            redoStack.Push(cmd);
            RefreshHistory();
        }

        public void Redo()
        {
            if (redoStack.Count == 0) return;
            var cmd = redoStack.Pop();
            cmd.Execute();
            undoStack.Push(cmd);
            RefreshHistory();
        }

        public void ResetLevel()
        {
            playerCell = startPlayer;
            for (int i = 0; i < boxes.Count; i++) boxes[i] = startBoxes[i];
            undoStack.Clear();
            redoStack.Clear();
            RefreshHistory();
        }

        public void ToggleRecording()
        {
            recordCommands = !recordCommands;
            if (!recordCommands)
            {
                undoStack.Clear();
                redoStack.Clear();
            }
            RefreshHistory();
        }

        private void RefreshHistory()
        {
            if (historyText == null) return;
            var sb = new System.Text.StringBuilder("명령 기록 (위가 최근)\n");
            int n = 0;
            foreach (var c in undoStack)
            {
                sb.Append(n == 0 ? "<color=#FFD54F>▶ " : "   ").Append(c.Name).Append(n == 0 ? "</color>\n" : "\n");
                if (++n >= 12) { sb.Append("   …\n"); break; }
            }
            if (undoStack.Count == 0) sb.Append(recordCommands ? "   (없음)\n" : "   <color=#FF6060>기록 안 함</color>\n");
            sb.Append($"\nRedo 대기 {redoStack.Count}개");
            historyText.text = sb.ToString();
        }

        private Vector3 CellToWorld(Vector2Int c) =>
            transform.position + new Vector3((c.x - (width - 1) * 0.5f) * cellSize, (c.y - (height - 1) * 0.5f) * cellSize, 0f);

        // 문자열 맵 → 칸 데이터 + 스프라이트 생성. 맨 윗줄이 가장 큰 y
        private void Parse()
        {
            height = level.Length;
            width = 0;
            foreach (var row in level) width = Mathf.Max(width, row.Length);

            for (int r = 0; r < height; r++)
            {
                for (int x = 0; x < level[r].Length; x++)
                {
                    var cell = new Vector2Int(x, height - 1 - r);
                    char ch = level[r][x];
                    if (ch == '#') { walls.Add(cell); Spawn("Wall", cell, wallColor, 0, 0.98f); continue; }
                    Spawn("Floor", cell, floorColor, 0, 0.94f);
                    if (ch == '.') { goals.Add(cell); Spawn("Goal", cell, goalColor, 1, 0.4f); }
                    if (ch == 'B') { boxes.Add(cell); startBoxes.Add(cell); boxViews.Add(Spawn("Box", cell, boxColor, 3, 0.8f)); }
                    if (ch == 'P') { playerCell = startPlayer = cell; playerView = Spawn("Player", cell, playerColor, 4, 0.7f); }
                }
            }
        }

        private Transform Spawn(string name, Vector2Int cell, Color color, int order, float scale)
        {
            var go = new GameObject($"{name}_{cell.x}_{cell.y}");
            go.transform.SetParent(transform, true);
            go.transform.position = CellToWorld(cell);
            go.transform.localScale = Vector3.one * cellSize * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = tileSprite;
            sr.color = color;
            sr.sortingOrder = order;
            return go.transform;
        }
    }
}
