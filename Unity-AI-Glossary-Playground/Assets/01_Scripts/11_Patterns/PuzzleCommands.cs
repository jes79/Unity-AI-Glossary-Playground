using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // 커맨드 패턴: "무엇을 할지"를 객체로 만들어 실행(Execute)과 되돌리기(Undo)를 한 쌍으로 가짐
    // 실행한 명령을 스택에 쌓아 두면, 꺼내서 Undo 만 호출하면 되돌리기 완성
    public interface ICommand
    {
        string Name { get; }
        void Execute();
        void Undo();
    }

    // 플레이어 한 칸 이동 (+ 상자를 밀었으면 상자 이동도 함께 기억)
    public class MoveCommand : ICommand
    {
        private readonly CommandPuzzle puzzle;
        private readonly Vector2Int dir;
        private readonly Vector2Int playerFrom;
        private readonly int boxIndex; // 밀린 상자 (-1 = 없음)

        public string Name { get; }

        public MoveCommand(CommandPuzzle puzzle, Vector2Int dir, int boxIndex)
        {
            this.puzzle = puzzle;
            this.dir = dir;
            this.boxIndex = boxIndex;
            playerFrom = puzzle.PlayerCell;
            string arrow = dir == Vector2Int.up ? "↑" : dir == Vector2Int.down ? "↓" : dir == Vector2Int.left ? "←" : "→";
            Name = boxIndex >= 0 ? $"이동 {arrow} + 상자 밀기" : $"이동 {arrow}";
        }

        public void Execute()
        {
            if (boxIndex >= 0) puzzle.SetBoxCell(boxIndex, puzzle.GetBoxCell(boxIndex) + dir);
            puzzle.SetPlayerCell(playerFrom + dir);
        }

        // 실행의 정확한 반대: 위치를 원래대로
        public void Undo()
        {
            puzzle.SetPlayerCell(playerFrom);
            if (boxIndex >= 0) puzzle.SetBoxCell(boxIndex, puzzle.GetBoxCell(boxIndex) - dir);
        }
    }
}
