using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.Effects
{
    // Main 모듈의 Start Lifetime / Start Speed / Start Size
    // 이동 거리 ≈ 속도 × 수명 이므로 둘을 같이 바꾸면 퍼지는 반경이 달라짐
    public class LifetimeSpeedSizeDemo : MonoBehaviour
    {
        [SerializeField] private ParticleSystem ps;
        [SerializeField] private Slider[] sliders; // lifetime, speed, sizeMin, sizeMax

        [Header("Main 모듈 (요청값)")]
        [SerializeField, Range(0.1f, 3f)] private float startLifetime = 0.5f;
        [SerializeField, Range(0f, 15f)] private float startSpeed = 5f;
        [SerializeField, Range(0.01f, 1f)] private float sizeMin = 0.1f;
        [SerializeField, Range(0.01f, 1f)] private float sizeMax = 0.3f;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) PresetRequest();
            if (DemoInput.KeyDown(Key.Digit2)) PresetSlow();
            if (DemoInput.KeyDown(Key.Digit3)) PresetFast();

            GlossaryInfoPanel.Status = $"Start Lifetime {startLifetime:0.00}s   Start Speed {startSpeed:0.0}   Start Size {sizeMin:0.00}~{sizeMax:0.00} (Random Between Two Constants)\n" +
                                       $"퍼지는 반경 ≈ 속도×수명 = {startSpeed * startLifetime:0.0}m   살아 있는 입자 {ps.particleCount}개";
        }

        public void SetLifetime(float v) { startLifetime = v; Apply(); }
        public void SetSpeed(float v) { startSpeed = v; Apply(); }
        public void SetSizeMin(float v) { sizeMin = v; Apply(); }
        public void SetSizeMax(float v) { sizeMax = v; Apply(); }

        public void PresetRequest() => SetAll(0.5f, 5f, 0.1f, 0.3f);
        public void PresetSlow() => SetAll(2.5f, 1f, 0.3f, 0.6f);
        public void PresetFast() => SetAll(0.2f, 14f, 0.05f, 0.1f);

        private void SetAll(float life, float speed, float min, float max)
        {
            startLifetime = life; startSpeed = speed; sizeMin = min; sizeMax = max;
            float[] values = { life, speed, min, max };
            for (int i = 0; i < sliders.Length && i < values.Length; i++)
                if (sliders[i] != null) sliders[i].SetValueWithoutNotify(values[i]);
            Apply();
        }

        private void Apply()
        {
            var main = ps.main;
            main.startLifetime = startLifetime;
            main.startSpeed = startSpeed;
            main.startSize = new ParticleSystem.MinMaxCurve(Mathf.Min(sizeMin, sizeMax), Mathf.Max(sizeMin, sizeMax)); // 두 값 사이 랜덤
        }
    }
}
