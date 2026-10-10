using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // Console 창 대신 화면에 로그를 보여 주는 미니 콘솔 (빌드에서도 Debug.Log 확인 가능)
    // Application.logMessageReceived 이벤트로 모든 로그를 받아 색을 구분해 표시
    public class OnScreenConsole : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI output;
        [SerializeField] private int maxLines = 9;

        private readonly Queue<string> lines = new();

        private void OnEnable() => Application.logMessageReceived += OnLog;
        private void OnDisable() => Application.logMessageReceived -= OnLog;

        private void OnLog(string message, string stackTrace, LogType type)
        {
            string color = type switch
            {
                LogType.Warning => "#FFD54F",
                LogType.Error or LogType.Exception or LogType.Assert => "#FF6B6B",
                _ => "#E0E0E0",
            };
            string first = message.Split('\n')[0];
            lines.Enqueue($"<color={color}>[{Time.frameCount}] {first}</color>");
            while (lines.Count > maxLines) lines.Dequeue();
            output.text = string.Join("\n", lines);
        }

        public void Clear()
        {
            lines.Clear();
            output.text = "";
        }
    }
}
