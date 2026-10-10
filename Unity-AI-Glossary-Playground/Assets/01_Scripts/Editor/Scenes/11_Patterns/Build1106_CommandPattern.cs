using GlossaryDemo.Patterns;
using TMPro;
using UnityEditor;
using UnityEngine;
using static GlossaryKit;
using static UIDemoBuildUtil;

public static class Build1106_CommandPattern
{
    [GlossaryScene("1106")]
    [MenuItem("Tools/Glossary/Build Scene/11 Patterns/1106_CommandPattern")]
    public static void Build()
    {
        var ctx = NewScene2D("1106",
            "WASD/방향키 : 이동 (상자를 밀어 초록 목표에)   Z : Undo   Y : Redo   R : 처음부터\n" +
            "C : 명령 기록 OFF → 이동은 되지만 되돌리기 불가 (비교)\n오른쪽에 쌓이는 명령 객체 목록을 보세요");

        var puzzleGo = new GameObject("CommandPuzzle");
        puzzleGo.transform.position = new Vector3(1.5f, -0.3f, 0f);
        var puzzle = puzzleGo.AddComponent<CommandPuzzle>();
        Set(puzzle, "tileSprite", SquareSprite);
        SetFloat(puzzle, "cellSize", 0.8f);

        var canvas = CreateCanvas("UI Canvas");
        var root = canvas.transform;
        var historyPanel = Panel(root, "History", new Vector2(770f, 140f), new Vector2(320f, 520f), DarkPanel);
        var history = CreateText("Text", historyPanel.transform, Vector2.zero, Vector2.zero, "", 20f, TextColor, TextAlignmentOptions.TopLeft);
        Stretch(history.rectTransform, 16f);
        history.richText = true;
        Set(puzzle, "historyText", history);

        InfoBox(root, "CodeInfo", new Vector2(770f, -270f), new Vector2(320f, 280f), "커맨드 코드",
            "<color=#9BE89B>interface ICommand\n{ void Execute(); void Undo(); }</color>\n" +
            "<color=#9BE89B>cmd.Execute();\nundo.Push(cmd);</color>\n" +
            "Z: <color=#9BE89B>undo.Pop().Undo();</color>", 18f);

        var buttons = CreateButtonRow(root, 60f, "Undo (Z)", "Redo (Y)", "리셋 (R)", "기록 ON/OFF (C)");
        OnClick(buttons[0], puzzle.Undo);
        OnClick(buttons[1], puzzle.Redo);
        OnClick(buttons[2], puzzle.ResetLevel);
        OnClick(buttons[3], puzzle.ToggleRecording);

        Save(ctx);
    }
}
