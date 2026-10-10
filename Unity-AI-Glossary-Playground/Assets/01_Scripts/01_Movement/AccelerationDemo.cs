using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // 0104 데모 관리: 위쪽 러너(가속/감속)의 시간 프리셋 전환 + 상태 표시
    public class AccelerationDemo : MonoBehaviour
    {
        [SerializeField] private AccelerationRunner smooth;  // 가속/감속 적용
        [SerializeField] private AccelerationRunner instant; // 즉시 반응

        private static readonly Vector2[] Presets = { new Vector2(0.2f, 0.1f), new Vector2(0.5f, 0.8f), new Vector2(0.05f, 0.05f) };
        private static readonly string[] PresetNames = { "요청값 (가속 0.2 / 감속 0.1)", "빙판 (0.5 / 0.8)", "아주 빠름 (0.05 / 0.05)" };
        private int preset;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) SetPreset(0);
            if (DemoInput.KeyDown(Key.Digit2)) SetPreset(1);
            if (DemoInput.KeyDown(Key.Digit3)) SetPreset(2);

            GlossaryInfoPanel.Status = $"프리셋: {PresetNames[preset]}\n" +
                                       $"가속/감속 속도: {smooth.Speed,5:0.0}   즉시 반응 속도: {instant.Speed,5:0.0}";
        }

        public void SetPreset(int index)
        {
            preset = index;
            smooth.SetTimes(Presets[index].x, Presets[index].y);
        }
    }
}
