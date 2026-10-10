using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0411 림 라이트 (Fresnel)
    // 오른쪽 캐릭터만 림 라이트를 켜고 끄며, 같은 셰이더지만 림을 끈 왼쪽과 비교
    // Power 를 올리면 테두리가 얇아지고, Intensity 를 올리면 더 밝아짐
    public class RimLightDemo : MonoBehaviour
    {
        [SerializeField] private Renderer[] rimRenderers;
        [SerializeField] private Color rimColor = new Color(0.45f, 0.85f, 1.6f);
        [SerializeField, Range(0.5f, 10f)] private float rimPower = 3f;
        [SerializeField, Range(0f, 4f)] private float rimIntensity = 1.5f;
        [SerializeField] private bool rimOn = true;

        private Material mat;

        private void Start()
        {
            mat = ShaderDemoUtil.CloneShared(rimRenderers[0].sharedMaterial, rimRenderers);
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.R)) ToggleRim();
        }

        public void SetPower(float value) { rimPower = value; Apply(); }
        public void SetIntensity(float value) { rimIntensity = value; Apply(); }
        public void ToggleRim() { rimOn = !rimOn; Apply(); }

        private void Apply()
        {
            if (mat == null) return;
            mat.SetColor("_RimColor", rimColor);
            mat.SetFloat("_RimPower", rimPower);
            mat.SetFloat("_RimIntensity", rimOn ? rimIntensity : 0f);
            GlossaryInfoPanel.Status = $"림 라이트 {ShaderDemoUtil.OnOff(rimOn)}   Power {rimPower:0.0} (클수록 얇게)   Intensity {rimIntensity:0.0}\n" +
                                       "공식: pow(1 - dot(N, V), Power) × Intensity × 하늘색";
        }
    }
}
