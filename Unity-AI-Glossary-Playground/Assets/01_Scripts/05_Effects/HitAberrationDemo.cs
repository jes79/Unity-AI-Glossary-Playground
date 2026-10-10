using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GlossaryDemo.Effects
{
    // Chromatic Aberration: 화면 가장자리로 갈수록 R/G/B 가 어긋나 보이는 렌즈 왜곡
    // 평소엔 0, 피격 순간 1 로 올렸다가 duration(0.2초) 동안 0 으로 빠르게 되돌림 → "충격" 연출
    public class HitAberrationDemo : MonoBehaviour
    {
        [SerializeField] private Volume volume;
        [SerializeField] private EffectsCameraShake cameraShake;
        [SerializeField, Range(0.05f, 1f)] private float duration = 0.2f;
        [SerializeField, Range(0f, 1f)] private float peakIntensity = 1f;
        [SerializeField] private bool useShake = true;

        private ChromaticAberration ca;
        private float timer;

        private void Start() => volume.profile.TryGet(out ca);

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space) || (DemoInput.LeftClick && !FxUtil.PointerOverUI)) Hit();
            if (DemoInput.KeyDown(Key.S)) useShake = !useShake;

            // 피격 직후 최대 → 남은 비율의 제곱(k²) 곡선으로 처음엔 빠르게, 끝은 부드럽게 감소
            timer = Mathf.Max(0f, timer - Time.deltaTime);
            float k = duration > 0f ? timer / duration : 0f;
            ca.intensity.value = peakIntensity * Ease.InQuad(k);

            GlossaryInfoPanel.Status = $"Chromatic Aberration Intensity {ca.intensity.value:0.00}   지속 {duration:0.00}초   쉐이크 {FxUtil.OnOff(useShake)}\n" +
                                       "화면 가장자리(기둥 윤곽)에서 색 번짐이 가장 잘 보임";
        }

        public void Hit()
        {
            timer = duration;
            if (useShake && cameraShake != null) cameraShake.Shake(0.15f, 0.08f);
        }

        public void SetDuration(float v) => duration = v;
        public void ToggleShake() => useShake = !useShake;
    }
}
