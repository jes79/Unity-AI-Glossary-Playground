using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace GlossaryDemo.Effects
{
    // Color Grading: 장면마다 색감 프리셋을 Volume 하나씩으로 만들어 두고 weight 를 교차 페이드
    // 0 기본(낮)  1 밤: 푸른 톤(White Balance 차갑게 + Color Filter 파랑 + 어둡게)
    // 2 던전: 채도 낮게(Saturation -65) + 대비↑ + 어두운 그림자  3 LUT: Color Lookup 텍스처(세피아)
    public class ColorGradingDemo : MonoBehaviour
    {
        [SerializeField] private Volume[] presetVolumes; // 밤, 던전, LUT (기본은 아무 Volume 도 켜지지 않은 상태)
        [SerializeField] private Light sun;
        [SerializeField] private float fadeTime = 1f;
        [SerializeField] private int current;           // 0 = 기본

        private static readonly string[] Names = { "기본(낮)", "밤 — 푸른 톤", "던전 — 채도 낮게", "LUT — 세피아" };
        private float sunBase = -1f;

        private void Update()
        {
            for (int i = 0; i < 4; i++)
                if (DemoInput.KeyDown(Key.Digit1 + i)) Select(i);

            // 선택된 프리셋 Volume 은 weight → 1, 나머지는 → 0 (fadeTime 동안 교차)
            for (int i = 0; i < presetVolumes.Length; i++)
            {
                var v = presetVolumes[i];
                v.weight = Mathf.MoveTowards(v.weight, current == i + 1 ? 1f : 0f, Time.deltaTime / fadeTime);
            }

            // 색 보정은 화면 색만 바꾸므로, 밤·던전은 조명도 같이 낮춰야 자연스러움
            if (sun != null)
            {
                if (sunBase < 0f) sunBase = sun.intensity;
                float k = current == 1 ? 0.35f : current == 2 ? 0.6f : 1f;
                sun.intensity = Mathf.MoveTowards(sun.intensity, sunBase * k, Time.deltaTime * sunBase / fadeTime);
            }

            var sb = new System.Text.StringBuilder($"프리셋: {Names[current]}\n");
            for (int i = 0; i < presetVolumes.Length; i++) sb.Append($"{Names[i + 1].Split(' ')[0]} weight {presetVolumes[i].weight:0.00}   ");
            GlossaryInfoPanel.Status = sb.ToString();
        }

        public void Select(int preset) => current = Mathf.Clamp(preset, 0, presetVolumes.Length);
    }
}
