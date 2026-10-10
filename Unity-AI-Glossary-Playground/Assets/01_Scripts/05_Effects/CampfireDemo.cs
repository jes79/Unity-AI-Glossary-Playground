using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Effects
{
    // Particle System 기본: 모닥불 = 불꽃 + 불씨 + 연기 3개의 파티클 시스템을 겹친 것
    // 각 레이어를 켜고 끄며 "이펙트는 여러 파티클 시스템의 합"이라는 걸 확인
    public class CampfireDemo : MonoBehaviour
    {
        [SerializeField] private ParticleSystem flame;
        [SerializeField] private ParticleSystem embers;
        [SerializeField] private ParticleSystem smoke;
        [SerializeField] private Light fireLight;

        [Header("불꽃 Emission (Rate over Time)")]
        [SerializeField, Range(0f, 120f)] private float flameRate = 40f;

        private float baseLight;

        private void Start()
        {
            if (fireLight != null) baseLight = fireLight.intensity;
            ApplyRate();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) ToggleFlame();
            if (DemoInput.KeyDown(Key.Digit2)) ToggleEmbers();
            if (DemoInput.KeyDown(Key.Digit3)) ToggleSmoke();
            if (DemoInput.KeyDown(Key.UpArrow)) { flameRate = Mathf.Min(120f, flameRate + 10f); ApplyRate(); }
            if (DemoInput.KeyDown(Key.DownArrow)) { flameRate = Mathf.Max(0f, flameRate - 10f); ApplyRate(); }

            // 불빛 깜빡임: Perlin 노이즈로 밝기를 흔들어 불꽃처럼 보이게
            if (fireLight != null)
            {
                float flicker = Mathf.PerlinNoise(Time.time * 6f, 0.3f);
                fireLight.intensity = flame.isEmitting ? baseLight * (0.75f + 0.5f * flicker) : Mathf.MoveTowards(fireLight.intensity, 0f, Time.deltaTime * 4f);
            }

            var m = flame.main;
            GlossaryInfoPanel.Status =
                $"1 불꽃 {FxUtil.OnOff(flame.isEmitting)} ({flame.particleCount}개)   2 불씨 {FxUtil.OnOff(embers.isEmitting)} ({embers.particleCount}개)   3 연기 {FxUtil.OnOff(smoke.isEmitting)} ({smoke.particleCount}개)\n" +
                $"불꽃: Rate {flameRate:0}/s, Lifetime {m.startLifetime.constantMin:0.0}~{m.startLifetime.constantMax:0.0}s, " +
                $"Speed {m.startSpeed.constantMin:0.0}~{m.startSpeed.constantMax:0.0}, Size {m.startSize.constantMin:0.0}~{m.startSize.constantMax:0.0}";
        }

        public void ToggleFlame() => FxUtil.ToggleEmit(flame);
        public void ToggleEmbers() => FxUtil.ToggleEmit(embers);
        public void ToggleSmoke() => FxUtil.ToggleEmit(smoke);

        private void ApplyRate()
        {
            var em = flame.emission;
            em.rateOverTime = flameRate;
        }
    }
}
