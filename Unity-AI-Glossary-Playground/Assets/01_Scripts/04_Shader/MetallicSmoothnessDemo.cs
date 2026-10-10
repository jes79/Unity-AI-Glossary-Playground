using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.ShaderDemo
{
    // 0405 Metallic / Smoothness
    // Metallic  : 0 = 플라스틱·나무처럼 자기 색으로 빛을 퍼뜨림, 1 = 금속처럼 주변을 자기 색으로 반사
    // Smoothness: 0 = 거칠어서 반사가 뿌옇게 퍼짐, 1 = 매끈해서 거울처럼 또렷한 반사
    // 왼쪽 5x5 구: 가로 Smoothness, 세로 Metallic 표. 오른쪽 갑옷: 요청 예문 값(0.9 / 0.7)
    public class MetallicSmoothnessDemo : MonoBehaviour
    {
        [SerializeField] private Renderer[] armorRenderers;
        [SerializeField] private ReflectionProbe probe;

        [Header("갑옷 (요청 예문 값)")]
        [SerializeField, Range(0f, 1f)] private float metallic = 0.9f;
        [SerializeField, Range(0f, 1f)] private float smoothness = 0.7f;

        private Material armorMat;

        private void Start()
        {
            ShaderDemoUtil.RefreshEnvironment();
            if (probe != null) probe.RenderProbe(); // 주변(하늘, 색 기둥)을 반사 텍스처로 찍어 둠
            armorMat = ShaderDemoUtil.CloneShared(armorRenderers[0].sharedMaterial, armorRenderers);
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.R)) ResetToPrompt();
            if (DemoInput.KeyDown(Key.Digit0)) { metallic = 0f; smoothness = 0.2f; Apply(); }
        }

        public void SetMetallic(float value) { metallic = value; Apply(); }
        public void SetSmoothness(float value) { smoothness = value; Apply(); }
        public void ResetToPrompt() { metallic = 0.9f; smoothness = 0.7f; Apply(); }

        private void Apply()
        {
            if (armorMat == null) return;
            armorMat.SetFloat("_Metallic", metallic);
            armorMat.SetFloat("_Smoothness", smoothness);
            GlossaryInfoPanel.Status = $"갑옷  Metallic <b>{metallic:0.00}</b>   Smoothness <b>{smoothness:0.00}</b>\n" +
                                       "(R: 요청 값 0.9 / 0.7 로 복귀,  0: 천 갑옷 느낌 0 / 0.2)";
        }
    }
}
