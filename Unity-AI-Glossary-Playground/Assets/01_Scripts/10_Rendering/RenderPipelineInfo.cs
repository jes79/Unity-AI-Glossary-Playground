using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GlossaryDemo.Rendering
{
    // 렌더 파이프라인 확인 + "분홍색 머티리얼" 고치기 시연
    // - 지금 쓰는 파이프라인(GraphicsSettings.currentRenderPipeline)을 런타임에 표시
    // - Built-in Standard 셰이더 머티리얼은 URP 에서 셰이더가 맞지 않아 분홍(마젠타)으로 보임
    // - [고치기] = Render Pipeline Converter 가 하는 일과 같음: 셰이더를 URP/Lit 으로 바꾸고 _Color → _BaseColor 로 옮김
    public class RenderPipelineInfo : MonoBehaviour
    {
        [Header("분홍색으로 보이는 오브젝트들 (Built-in Standard 머티리얼)")]
        [SerializeField] private Renderer[] brokenRenderers;
        [SerializeField] private Material builtinMaterial;   // Shader "Standard" (Built-in 전용)
        [SerializeField] private Material urpLitMaterial;    // Shader "Universal Render Pipeline/Lit"

        private bool fixedNow;

        private void Start() => Apply();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) ToggleFix();

            var rp = GraphicsSettings.currentRenderPipeline; // 현재 활성 파이프라인 (Quality 설정이 우선, 없으면 Graphics 설정)
            string rpName = rp == null ? "없음 → Built-in Render Pipeline" : $"{rp.GetType().Name} ({rp.name})";
            string urpInfo = rp is UniversalRenderPipelineAsset urp
                ? $"URP 에셋: {urp.name}  /  HDR {(urp.supportsHDR ? "ON" : "OFF")}  /  SRP Batcher {(urp.useSRPBatcher ? "ON" : "OFF")}"
                : "URP 에셋 아님";
            string quality = QualitySettings.renderPipeline != null ? QualitySettings.renderPipeline.name : "(Graphics 기본값 사용)";
            string shaderName = brokenRenderers != null && brokenRenderers.Length > 0 && brokenRenderers[0] != null
                ? brokenRenderers[0].sharedMaterial.shader.name : "-";

            GlossaryInfoPanel.Status =
                $"현재 파이프라인: {rpName}\n{urpInfo}\n" +
                $"Quality 레벨 '{QualitySettings.names[QualitySettings.GetQualityLevel()]}' 의 RP: {quality}\n" +
                $"왼쪽 물체 셰이더: {shaderName}  →  {(fixedNow ? "<color=#8F8>고쳐짐</color>" : "<color=#F6F>분홍색(호환 안 됨)</color>")}";
        }

        // 버튼 / Space : 분홍 머티리얼 ↔ URP Lit 교체
        public void ToggleFix()
        {
            fixedNow = !fixedNow;
            Apply();
        }

        public void Fix() { fixedNow = true; Apply(); }
        public void Break() { fixedNow = false; Apply(); }

        private void Apply()
        {
            foreach (var r in brokenRenderers)
                if (r != null) r.sharedMaterial = fixedNow ? urpLitMaterial : builtinMaterial;
        }

        // 컨버터가 하는 일을 코드로 흉내: Built-in 머티리얼의 색을 URP Lit 의 _BaseColor 로 복사
        // (빌더가 이미 같은 색으로 만들어 두지만, 실제 프로젝트에서 직접 고칠 때 참고용)
        public static void ConvertStandardToUrpLit(Material m, Shader urpLit)
        {
            Color c = m.HasProperty("_Color") ? m.GetColor("_Color") : Color.white;
            Texture tex = m.HasProperty("_MainTex") ? m.GetTexture("_MainTex") : null;
            m.shader = urpLit;
            m.SetColor("_BaseColor", c);
            if (tex != null) m.SetTexture("_BaseMap", tex);
        }
    }
}
