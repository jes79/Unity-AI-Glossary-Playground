using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GlossaryDemo.ShaderDemo
{
    // 0406 Emission
    // Emission = 조명과 상관없이 스스로 내는 색. HDR 강도 3 = 색 × 2^3 (유니티 색상 창의 Intensity 와 같은 계산)
    // 1 을 넘는 밝기는 Bloom(포스트 프로세싱)이 주변으로 번지게 해서 "빛나 보이게" 만듦
    public class EmissionDemo : MonoBehaviour
    {
        [SerializeField] private Renderer[] edgeRenderers;
        [SerializeField] private Volume volume;

        [Header("Emission")]
        [SerializeField] private Color emissionColor = new Color(0.2f, 0.55f, 1f);
        [SerializeField, Range(-2f, 6f)] private float hdrIntensity = 3f;
        [SerializeField] private bool emissionOn = true;
        [SerializeField] private bool pulse;

        private Material edgeMat;
        private Bloom bloom;

        private void Start()
        {
            edgeMat = ShaderDemoUtil.CloneShared(edgeRenderers[0].sharedMaterial, edgeRenderers);
            // volume.profile 은 실행용 복사본을 돌려줘서 에셋을 건드리지 않음
            if (volume != null) volume.profile.TryGet(out bloom);
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.E)) ToggleEmission();
            if (DemoInput.KeyDown(Key.B)) ToggleBloom();
            if (DemoInput.KeyDown(Key.P)) pulse = !pulse;
            if (pulse) Apply();
        }

        public void SetIntensity(float value) { hdrIntensity = value; Apply(); }
        public void ToggleEmission() { emissionOn = !emissionOn; Apply(); }
        public void ToggleBloom()
        {
            if (bloom != null) bloom.active = !bloom.active;
            Apply();
        }

        private void Apply()
        {
            if (edgeMat == null) return;
            float intensity = hdrIntensity + (pulse ? Mathf.Sin(Time.time * 4f) * 1f : 0f);
            float multiplier = Mathf.Pow(2f, intensity); // HDR 강도 3 → ×8
            // 핵심: _EmissionColor 에 1 보다 큰 값을 넣음 (HDR). 끄면 검정 = 발광 없음
            edgeMat.SetColor("_EmissionColor", emissionOn ? emissionColor * multiplier : Color.black);

            GlossaryInfoPanel.Status =
                $"Emission {ShaderDemoUtil.OnOff(emissionOn)}  HDR 강도 <b>{hdrIntensity:0.0}</b> (색 × {Mathf.Pow(2f, hdrIntensity):0.##})   " +
                $"Bloom {ShaderDemoUtil.OnOff(bloom == null || bloom.active)}   Pulse {ShaderDemoUtil.OnOff(pulse)}";
        }
    }
}
