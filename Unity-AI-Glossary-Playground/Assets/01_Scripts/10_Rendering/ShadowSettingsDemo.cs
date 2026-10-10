using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GlossaryDemo.Rendering
{
    // 그림자 거리 / Soft Shadow 를 런타임에 바꿔 보는 데모
    // URP 에서는 QualitySettings.shadowDistance 가 아니라 URP 에셋의 shadowDistance 가 실제 값
    // 에디터에서 Play 중 에셋 값을 바꾸면 에셋 파일에 남으므로, 시작 값을 기억했다가 씬을 나갈 때 되돌림
    public class ShadowSettingsDemo : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [Header("모바일 프리셋 (요청 예문 수치)")]
        [SerializeField] private float mobileShadowDistance = 30f;
        [SerializeField] private bool mobileSoftShadows = false;
        [SerializeField] private float[] distances = { 10f, 30f, 50f, 100f, 150f };

        private UniversalRenderPipelineAsset urp;
        private float originalDistance;
        private int originalCascades;
        private float originalQualityDistance;
        private int index = 2;

        private void Awake()
        {
            urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            originalQualityDistance = QualitySettings.shadowDistance;
            if (urp != null)
            {
                originalDistance = urp.shadowDistance;
                originalCascades = urp.shadowCascadeCount;
            }
        }

        // 씬을 나가거나 Play 를 멈추면 URP 에셋 값을 원래대로
        private void OnDestroy()
        {
            QualitySettings.shadowDistance = originalQualityDistance;
            if (urp != null)
            {
                urp.shadowDistance = originalDistance;
                urp.shadowCascadeCount = originalCascades;
            }
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.D)) NextDistance();
            if (DemoInput.KeyDown(Key.S)) ToggleSoft();
            if (DemoInput.KeyDown(Key.M)) MobilePreset();
            if (DemoInput.KeyDown(Key.P)) PcPreset();

            float dist = urp != null ? urp.shadowDistance : QualitySettings.shadowDistance;
            GlossaryInfoPanel.Status =
                $"그림자 거리(Max Distance): <b>{dist:0}m</b>   캐스케이드: {(urp != null ? urp.shadowCascadeCount : QualitySettings.shadowCascades)}\n" +
                $"태양 그림자: <b>{sun.shadows}</b> {(sun.shadows == LightShadows.Soft ? "(가장자리 부드러움, 비용↑)" : "(계단 현상, 비용↓)")}\n" +
                $"URP 에셋: {(urp != null ? urp.name : "없음")}   Soft 지원: {(urp != null && urp.supportsSoftShadows ? "예" : "아니오")}\n" +
                "기둥은 5m 간격 → 거리 밖 기둥은 그림자가 사라짐";
        }

        public void NextDistance()
        {
            index = (index + 1) % distances.Length;
            SetDistance(distances[index]);
        }

        public void SetDistance(float d)
        {
            QualitySettings.shadowDistance = d;       // Built-in 파이프라인용 (URP 에서는 무시됨)
            if (urp != null) urp.shadowDistance = d;  // URP 는 이 값이 실제로 쓰임
        }

        public void ToggleSoft() => sun.shadows = sun.shadows == LightShadows.Soft ? LightShadows.Hard : LightShadows.Soft;

        // 요청 예문: 모바일용 그림자 거리 30m + Soft Shadow 끄기
        public void MobilePreset()
        {
            SetDistance(mobileShadowDistance);
            sun.shadows = mobileSoftShadows ? LightShadows.Soft : LightShadows.Hard;
            if (urp != null) urp.shadowCascadeCount = 1; // 모바일은 캐스케이드도 1~2 로 줄이는 경우가 많음
        }

        public void PcPreset()
        {
            SetDistance(150f);
            sun.shadows = LightShadows.Soft;
            if (urp != null) urp.shadowCascadeCount = 4;
        }
    }
}
