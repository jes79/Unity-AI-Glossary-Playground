using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // Particle System Renderer 의 Render Mode
    // Billboard: 항상 카메라를 정면으로 봄(둥근 점)   Stretched Billboard: 속도 방향으로 길게 늘어남(스파크, 빗줄기)
    // Horizontal / Vertical Billboard: 바닥과 평행 / 세로로 선 판
    public class BillboardDemo : MonoBehaviour
    {
        [SerializeField] private ParticleSystemRenderer sparks;
        [SerializeField] private ParticleSystemRenderMode mode = ParticleSystemRenderMode.Stretch;
        [SerializeField, Range(0f, 0.3f)] private float velocityScale = 0.08f;
        [SerializeField, Range(0.5f, 5f)] private float lengthScale = 2f;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) SetMode(0);
            if (DemoInput.KeyDown(Key.Digit2)) SetMode(1);
            if (DemoInput.KeyDown(Key.Digit3)) SetMode(2);
            if (DemoInput.KeyDown(Key.Digit4)) SetMode(3);

            string extra = mode == ParticleSystemRenderMode.Stretch ? $"  Speed Scale {velocityScale:0.00}  Length Scale {lengthScale:0.0}  (길이 = 크기×Length + 속도×Speed)" : "";
            GlossaryInfoPanel.Status = $"Render Mode = {mode}{extra}";
        }

        // 0 Billboard, 1 Stretched, 2 Horizontal, 3 Vertical
        public void SetMode(int i)
        {
            mode = i switch
            {
                0 => ParticleSystemRenderMode.Billboard,
                1 => ParticleSystemRenderMode.Stretch,
                2 => ParticleSystemRenderMode.HorizontalBillboard,
                _ => ParticleSystemRenderMode.VerticalBillboard,
            };
            Apply();
        }

        public void SetVelocityScale(float v) { velocityScale = v; Apply(); }

        private void Apply()
        {
            sparks.renderMode = mode;
            sparks.velocityScale = velocityScale;
            sparks.lengthScale = lengthScale;
        }
    }
}
