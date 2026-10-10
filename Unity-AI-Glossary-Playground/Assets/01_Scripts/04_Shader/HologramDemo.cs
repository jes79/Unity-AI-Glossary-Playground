using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0415 홀로그램 - 스캔라인 속도/밀도, 깜빡임, 글리치를 실시간으로 조절
    public class HologramDemo : MonoBehaviour
    {
        [SerializeField] private Renderer[] holoRenderers;
        [SerializeField] private float scanSpeed = 1.2f;
        [SerializeField] private float scanDensity = 18f;
        [SerializeField] private bool flicker = true;
        [SerializeField] private bool glitch = true;
        [SerializeField] private float glitchStrength = 0.04f;

        private Material mat;

        private void Start()
        {
            mat = ShaderDemoUtil.CloneShared(holoRenderers[0].sharedMaterial, holoRenderers);
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.F)) ToggleFlicker();
            if (DemoInput.KeyDown(Key.G)) ToggleGlitch();
        }

        public void SetScanSpeed(float value) { scanSpeed = value; Apply(); }
        public void SetScanDensity(float value) { scanDensity = value; Apply(); }
        public void ToggleFlicker() { flicker = !flicker; Apply(); }
        public void ToggleGlitch() { glitch = !glitch; Apply(); }

        private void Apply()
        {
            if (mat == null) return;
            mat.SetFloat("_ScanSpeed", scanSpeed);
            mat.SetFloat("_ScanDensity", scanDensity);
            mat.SetFloat("_Flicker", flicker ? 1f : 0f);
            mat.SetFloat("_GlitchStrength", glitch ? glitchStrength : 0f);
            GlossaryInfoPanel.Status = $"스캔라인 속도 {scanSpeed:0.0} (양수 = 위로 흐름)   밀도 {scanDensity:0}줄/m\n" +
                                       $"깜빡임 {ShaderDemoUtil.OnOff(flicker)}   글리치 {ShaderDemoUtil.OnOff(glitch)}";
        }
    }
}
