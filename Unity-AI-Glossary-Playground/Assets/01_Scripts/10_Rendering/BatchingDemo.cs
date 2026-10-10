using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GlossaryDemo.Rendering
{
    // Draw Call / Batching 비교
    // - 같은 모양의 상자 400개 그리드 두 벌: 하나는 그대로(동적), 하나는 시작할 때 StaticBatchingUtility.Combine
    // - SRP Batcher ON/OFF: URP 의 기본 배칭. 같은 셰이더(변형)끼리 SetPass 를 줄임
    // - MaterialPropertyBlock 으로 색을 바꾸면 SRP Batcher 호환이 깨지는 것도 확인
    // 수치는 ProfilerRecorder("Batches Count" 등)로 읽음 (Editor / Development Build 에서 유효)
    public class BatchingDemo : MonoBehaviour
    {
        [SerializeField] private GameObject dynamicGrid;
        [SerializeField] private GameObject staticGrid;   // Start 에서 Combine
        [SerializeField] private bool showStatic;

        private UniversalRenderPipelineAsset urp;
        private bool originalSrpBatcher;
        private bool mpbOn;
        private ProfilerRecorder batches, setPass, drawCalls, srpBatcherBatches;
        private float timer;
        private string stats = "";

        private void Awake()
        {
            urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null) originalSrpBatcher = urp.useSRPBatcher;
        }

        private void Start()
        {
            // 런타임 정적 배칭: 자식들의 메시를 머티리얼별로 큰 메시로 합침 (이후 개별 이동 불가)
            StaticBatchingUtility.Combine(staticGrid);
            Apply();
        }

        private void OnEnable()
        {
            batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            srpBatcherBatches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SRP Batcher Batches Count");
        }

        private void OnDisable()
        {
            batches.Dispose();
            setPass.Dispose();
            drawCalls.Dispose();
            srpBatcherBatches.Dispose();
        }

        // Play 중 에셋 값을 바꾸면 파일에 남으므로 원래대로
        private void OnDestroy()
        {
            if (urp != null) urp.useSRPBatcher = originalSrpBatcher;
            GraphicsSettings.useScriptableRenderPipelineBatching = originalSrpBatcher;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.B)) ToggleStatic();
            if (DemoInput.KeyDown(Key.S)) ToggleSrpBatcher();
            if (DemoInput.KeyDown(Key.M)) ToggleMpb();

            if ((timer -= Time.unscaledDeltaTime) <= 0f)
            {
                timer = 0.25f;
                stats = $"Draw Calls {Read(drawCalls)}   Batches {Read(batches)}   SetPass {Read(setPass)}   SRP Batcher Batches {Read(srpBatcherBatches)}";
            }
            bool srp = GraphicsSettings.useScriptableRenderPipelineBatching;
            GlossaryInfoPanel.Status =
                $"보이는 그리드: <b>{(showStatic ? "Static Batching (Combine 됨)" : "일반(동적) 오브젝트")}</b>\n" +
                $"SRP Batcher: {(srp ? "<color=#8F8>ON</color>" : "<color=#F88>OFF</color>")}   MaterialPropertyBlock: {(mpbOn ? "<color=#FC6>사용 (SRP Batcher 깨짐)</color>" : "없음")}\n" +
                stats;
        }

        private static string Read(ProfilerRecorder r) => r.Valid ? r.LastValue.ToString() : "N/A";

        public void ToggleStatic()
        {
            showStatic = !showStatic;
            Apply();
        }

        public void ToggleSrpBatcher()
        {
            bool on = !GraphicsSettings.useScriptableRenderPipelineBatching;
            if (urp != null) urp.useSRPBatcher = on;
            GraphicsSettings.useScriptableRenderPipelineBatching = on;
        }

        // 렌더러마다 MaterialPropertyBlock 으로 색을 넣으면 SRP Batcher 대상에서 빠짐 → Batches 증가
        public void ToggleMpb()
        {
            mpbOn = !mpbOn;
            var block = new MaterialPropertyBlock();
            foreach (var r in dynamicGrid.GetComponentsInChildren<Renderer>())
            {
                if (mpbOn)
                {
                    block.SetColor("_BaseColor", Color.HSVToRGB(Random.value, 0.5f, 1f));
                    r.SetPropertyBlock(block);
                }
                else r.SetPropertyBlock(null);
            }
        }

        private void Apply()
        {
            dynamicGrid.SetActive(!showStatic);
            staticGrid.SetActive(showStatic);
        }
    }
}
