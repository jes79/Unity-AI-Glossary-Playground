using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0414 툰 셰이딩 (Cel Shading)
    // URP 메인 라이트(Directional Light)를 돌리면서 명암이 3단계로 "뚝뚝" 끊기는 것을 확인
    // T: 같은 물체를 URP Lit(부드러운 명암)으로 바꿔 비교,  B: 셰이더 안에서 끊기만 끔(_Banding 0)
    public class ToonShadingDemo : MonoBehaviour
    {
        [SerializeField] private Light mainLight;
        [SerializeField] private Renderer[] targets;
        [SerializeField] private Material[] toonMaterials;   // targets 와 같은 순서
        [SerializeField] private Material[] litMaterials;

        [Header("툰")]
        [SerializeField, Range(2, 6)] private int steps = 3;
        [SerializeField] private bool banding = true;
        [SerializeField] private bool useToon = true;
        [SerializeField] private bool rotateLight = true;
        [SerializeField] private float lightYaw = 200f;

        private Material[] runtimeToon;

        private void Start()
        {
            runtimeToon = new Material[toonMaterials.Length];
            for (int i = 0; i < toonMaterials.Length; i++)
                runtimeToon[i] = new Material(toonMaterials[i]) { name = toonMaterials[i].name + " (Runtime)" };
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.T)) ToggleToon();
            if (DemoInput.KeyDown(Key.B)) ToggleBanding();
            if (DemoInput.KeyDown(Key.Space)) rotateLight = !rotateLight;
            if (rotateLight)
            {
                lightYaw = Mathf.Repeat(lightYaw + 35f * Time.deltaTime, 360f);
                mainLight.transform.rotation = Quaternion.Euler(40f, lightYaw, 0f);
            }
            GlossaryInfoPanel.Status = $"셰이더: <b>{(useToon ? "Glossary/Shader/Toon" : "URP Lit (비교)")}</b>   단계 수 {steps}   " +
                                       $"끊기 {ShaderDemoUtil.OnOff(banding)}   조명 회전 {ShaderDemoUtil.OnOff(rotateLight)}";
        }

        public void SetSteps(float value) { steps = Mathf.RoundToInt(value); Apply(); }
        public void ToggleToon() { useToon = !useToon; Apply(); }
        public void ToggleBanding() { banding = !banding; Apply(); }
        public void SetLightYaw(float yaw)
        {
            rotateLight = false;
            lightYaw = yaw;
            mainLight.transform.rotation = Quaternion.Euler(40f, lightYaw, 0f);
        }

        private void Apply()
        {
            if (runtimeToon == null) return;
            for (int i = 0; i < targets.Length; i++)
            {
                runtimeToon[i].SetFloat("_Steps", steps);
                runtimeToon[i].SetFloat("_Banding", banding ? 1f : 0f);
                targets[i].sharedMaterial = useToon ? runtimeToon[i] : litMaterials[i];
            }
        }
    }
}
