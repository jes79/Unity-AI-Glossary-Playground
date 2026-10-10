using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace GlossaryDemo.Rendering
{
    // 나무 treeCount(1000)그루를 GPU Instancing 으로 그리기
    // Graphics.RenderMeshInstanced: GameObject 없이 "같은 메시 + 같은 머티리얼"을 위치 배열(행렬)만 바꿔 한 번에 그림
    // 비교: GameObject 방식 = 나무마다 GameObject 2개(줄기+잎) → 2000개 오브젝트 관리 비용
    // 머티리얼에 Enable GPU Instancing 이 켜져 있어야 함 (빌더가 enableInstancing = true 로 만듦)
    public class InstancedForest : MonoBehaviour
    {
        [SerializeField] private Mesh trunkMesh;
        [SerializeField] private Mesh leavesMesh;
        [SerializeField] private Material trunkMaterial;
        [SerializeField] private Material leavesMaterial;
        [SerializeField] private int treeCount = 1000;
        [SerializeField] private float areaSize = 90f;
        [SerializeField] private bool useInstancing = true;

        private const int MaxPerCall = 1023; // 한 번 호출에 넣을 최대 개수 (안전하게 1023 단위로 나눔)
        private Matrix4x4[] trunkMatrices;
        private Matrix4x4[] leavesMatrices;
        private GameObject objectsRoot;       // 비교용 GameObject 방식
        private RenderParams trunkParams, leavesParams;
        private ProfilerRecorder batches, drawCalls;
        private float smoothMs;

        private void OnEnable()
        {
            batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        }

        private void OnDisable()
        {
            batches.Dispose();
            drawCalls.Dispose();
        }

        private void Start()
        {
            trunkParams = new RenderParams(trunkMaterial) { shadowCastingMode = ShadowCastingMode.On, receiveShadows = true };
            leavesParams = new RenderParams(leavesMaterial) { shadowCastingMode = ShadowCastingMode.On, receiveShadows = true };
            Generate();
        }

        private void Generate()
        {
            trunkMatrices = new Matrix4x4[treeCount];
            leavesMatrices = new Matrix4x4[treeCount];
            var rng = new System.Random(1013);
            for (int i = 0; i < treeCount; i++)
            {
                float x = ((float)rng.NextDouble() - 0.5f) * areaSize;
                float z = ((float)rng.NextDouble() - 0.5f) * areaSize + areaSize * 0.4f;
                float s = 0.7f + (float)rng.NextDouble() * 0.8f;
                var rot = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                // 줄기(실린더 높이 2) / 잎(구)
                trunkMatrices[i] = Matrix4x4.TRS(new Vector3(x, s, z), rot, new Vector3(0.3f * s, s, 0.3f * s));
                leavesMatrices[i] = Matrix4x4.TRS(new Vector3(x, 2.4f * s, z), rot, new Vector3(1.8f * s, 2.2f * s, 1.8f * s));
            }
            if (objectsRoot != null) Destroy(objectsRoot);
            objectsRoot = null;
            if (!useInstancing) BuildObjects();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.I)) ToggleMode();
            if (DemoInput.KeyDown(Key.UpArrow)) ChangeCount(1000);
            if (DemoInput.KeyDown(Key.DownArrow)) ChangeCount(-1000);

            if (useInstancing)
            {
                // 매 프레임 호출해야 그려짐 (GameObject 가 없으니 그리라고 직접 요청)
                for (int start = 0; start < treeCount; start += MaxPerCall)
                {
                    int n = Mathf.Min(MaxPerCall, treeCount - start);
                    Graphics.RenderMeshInstanced(trunkParams, trunkMesh, 0, trunkMatrices, n, start);
                    Graphics.RenderMeshInstanced(leavesParams, leavesMesh, 0, leavesMatrices, n, start);
                }
            }

            smoothMs = Mathf.Lerp(smoothMs, Time.unscaledDeltaTime * 1000f, 0.05f);
            GlossaryInfoPanel.Status =
                $"방식: <b>{(useInstancing ? "GPU Instancing (RenderMeshInstanced)" : "GameObject 2개 x 나무")}</b>\n" +
                $"나무 {treeCount}그루   GameObject {(useInstancing ? 0 : treeCount * 2)}개\n" +
                $"Draw Calls {(drawCalls.Valid ? drawCalls.LastValue.ToString() : "N/A")}   Batches {(batches.Valid ? batches.LastValue.ToString() : "N/A")}   프레임 {smoothMs:0.0} ms";
        }

        private void BuildObjects()
        {
            objectsRoot = new GameObject("Trees (GameObjects)");
            for (int i = 0; i < treeCount; i++)
            {
                Spawn("Trunk", trunkMesh, trunkMaterial, trunkMatrices[i]);
                Spawn("Leaves", leavesMesh, leavesMaterial, leavesMatrices[i]);
            }
        }

        private void Spawn(string name, Mesh mesh, Material mat, Matrix4x4 m)
        {
            var go = new GameObject(name);
            go.transform.SetParent(objectsRoot.transform, false);
            go.transform.SetPositionAndRotation(m.GetPosition(), m.rotation);
            go.transform.localScale = m.lossyScale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }

        public void ToggleMode()
        {
            useInstancing = !useInstancing;
            Generate();
        }

        public void ChangeCount(int delta)
        {
            treeCount = Mathf.Clamp(treeCount + delta, 1000, 10000);
            Generate();
        }
    }
}
