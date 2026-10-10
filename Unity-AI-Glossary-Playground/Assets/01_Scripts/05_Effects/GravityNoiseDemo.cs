using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // Gravity Modifier: 중력 배율 (음수면 위로 떠오름)
    // Noise 모듈: 입자 위치에 흐르는 노이즈를 더해 하늘하늘 흔들리게 함
    public class GravityNoiseDemo : MonoBehaviour
    {
        [SerializeField] private ParticleSystem embers;
        [SerializeField, Range(-1f, 1f)] private float gravityModifier = -0.15f;
        [SerializeField] private bool useNoise = true;
        [SerializeField, Range(0f, 3f)] private float noiseStrength = 0.8f;
        [SerializeField, Range(0.05f, 3f)] private float noiseFrequency = 0.6f;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.N)) ToggleNoise();
            if (DemoInput.KeyDown(Key.G)) CycleGravity();
            GlossaryInfoPanel.Status = $"Gravity Modifier {gravityModifier:+0.00;-0.00;0}  ({(gravityModifier < 0f ? "위로 떠오름" : gravityModifier > 0f ? "아래로 떨어짐" : "무중력")})\n" +
                                       $"Noise {FxUtil.OnOff(useNoise)}  Strength {noiseStrength:0.00}  Frequency {noiseFrequency:0.00}";
        }

        public void ToggleNoise() { useNoise = !useNoise; Apply(); }

        public void CycleGravity()
        {
            gravityModifier = gravityModifier < -0.01f ? 0f : gravityModifier < 0.01f ? 0.5f : -0.15f;
            Apply();
        }

        public void SetStrength(float v) { noiseStrength = v; Apply(); }
        public void SetFrequency(float v) { noiseFrequency = v; Apply(); }

        private void Apply()
        {
            var main = embers.main;
            main.gravityModifier = gravityModifier;
            var noise = embers.noise;
            noise.enabled = useNoise;
            noise.strength = noiseStrength;
            noise.frequency = noiseFrequency;
            noise.scrollSpeed = 0.4f; // 노이즈 필드가 흘러가며 계속 다른 흔들림
        }
    }
}
