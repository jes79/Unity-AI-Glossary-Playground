using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace GlossaryDemo.Effects
{
    // Bloom Threshold: 이 밝기(선형, HDR)보다 밝은 픽셀만 번짐
    // 일반 머티리얼은 조명을 받아도 대략 1 이하 → Threshold 1 이상이면 HDR Emission(강도 4) 오브젝트만 빛남
    // Threshold 를 0.5 로 내리면 흰 큐브·밝은 바닥까지 뿌옇게 번지는 것을 비교
    public class BloomThresholdDemo : MonoBehaviour
    {
        [SerializeField] private Volume volume;
        [SerializeField] private Slider thresholdSlider;
        [SerializeField, Range(0f, 3f)] private float threshold = 1f;
        [SerializeField, Range(0f, 5f)] private float intensity = 1.5f;

        private Bloom bloom;

        private void Start()
        {
            volume.profile.TryGet(out bloom);
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.UpArrow)) SetThreshold(threshold + 0.1f);
            if (DemoInput.KeyDown(Key.DownArrow)) SetThreshold(threshold - 0.1f);
            if (DemoInput.KeyDown(Key.B)) ToggleBloom();

            string note = threshold >= 0.95f ? "Emission 오브젝트만 빛남 (원하는 상태)" : "일반 물체까지 번져 화면이 뿌옇게 됨";
            GlossaryInfoPanel.Status = $"Bloom {FxUtil.OnOff(bloom.active)}   Threshold {threshold:0.00}   Intensity {intensity:0.0}\n{note}";
        }

        public void SetThreshold(float v)
        {
            threshold = Mathf.Clamp(v, 0f, 3f);
            if (thresholdSlider != null) thresholdSlider.SetValueWithoutNotify(threshold);
            Apply();
        }

        public void SetIntensity(float v) { intensity = v; Apply(); }
        public void ToggleBloom() => bloom.active = !bloom.active;
        public void PresetLow() => SetThreshold(0.5f);
        public void PresetEmissionOnly() => SetThreshold(1.1f);

        private void Apply()
        {
            if (bloom == null) return;
            bloom.threshold.value = threshold;
            bloom.intensity.value = intensity;
        }
    }
}
