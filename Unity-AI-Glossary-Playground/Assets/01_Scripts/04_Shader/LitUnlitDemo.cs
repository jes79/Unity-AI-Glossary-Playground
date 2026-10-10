using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0403 Lit / Unlit
    // 왼쪽 줄: URP Lit (조명 방향·세기에 따라 밝기·그림자가 변함)
    // 오른쪽 줄: URP Unlit (조명을 무시하고 항상 같은 색 → UI·아이콘·발광 표지판에 적합)
    // 가운데 표지판은 U 키로 Lit ↔ Unlit 을 바꿔서 "UI처럼 항상 밝게" 바뀌는 순간을 확인
    public class LitUnlitDemo : MonoBehaviour
    {
        [SerializeField] private Light mainLight;
        [SerializeField] private Renderer[] signRenderers;
        [SerializeField] private Material signLit;
        [SerializeField] private Material signUnlit;

        [Header("조명")]
        [SerializeField, Range(0f, 360f)] private float lightYaw = 330f;
        [SerializeField, Range(0f, 2f)] private float lightIntensity = 1.2f;
        [SerializeField] private bool autoRotate = true;

        private bool signIsUnlit;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.U)) ToggleSign();
            if (DemoInput.KeyDown(Key.Space)) autoRotate = !autoRotate;
            if (DemoInput.KeyDown(Key.L)) SetLightIntensity(lightIntensity > 0.05f ? 0f : 1.2f);
            if (autoRotate)
            {
                lightYaw = Mathf.Repeat(lightYaw + 40f * Time.deltaTime, 360f);
                Apply();
            }
        }

        public void SetLightYaw(float yaw) { lightYaw = yaw; autoRotate = false; Apply(); }
        public void SetLightIntensity(float value) { lightIntensity = value; Apply(); }

        // 표지판의 머티리얼만 Lit ↔ Unlit 교체
        public void ToggleSign()
        {
            signIsUnlit = !signIsUnlit;
            foreach (var r in signRenderers) r.sharedMaterial = signIsUnlit ? signUnlit : signLit;
            Apply();
        }

        private void Apply()
        {
            if (mainLight != null)
            {
                mainLight.transform.rotation = Quaternion.Euler(35f, lightYaw, 0f);
                mainLight.intensity = lightIntensity;
            }
            GlossaryInfoPanel.Status =
                $"조명 각도 {lightYaw:0}°  세기 {lightIntensity:0.00}  (자동 회전 {ShaderDemoUtil.OnOff(autoRotate)})\n" +
                $"가운데 표지판: <b>{(signIsUnlit ? "Unlit (항상 밝음)" : "Lit (조명 영향)")}</b>";
        }
    }
}
