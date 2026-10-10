using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace GlossaryDemo.Effects
{
    // VFX Graph 대체 구현: 입자 50,000개 소용돌이 포탈
    // 시작할 때 한 번만 "입자 = 사각형 4정점" 메시를 만들고(씨앗값 저장), 이후 움직임은 전부 셰이더가 GPU 에서 계산
    // → CPU 는 매 프레임 아무것도 안 함. VFX Graph 도 같은 원리(GPU 시뮬레이션)로 수십만 개를 처리함
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class VortexPortal : MonoBehaviour
    {
        [SerializeField] private int particleCount = 50000;
        [SerializeField, Range(0f, 3f)] private float spinSpeed = 0.8f;
        [SerializeField, Range(0f, 0.6f)] private float pullSpeed = 0.12f;

        private static readonly int VisibleCountId = Shader.PropertyToID("_VisibleCount");
        private static readonly int SpinId = Shader.PropertyToID("_SpinPhase");
        private static readonly int PullId = Shader.PropertyToID("_PullPhase");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

        private Material mat;
        private int visible;
        private float fps, burst, spinPhase, pullPhase;

        private void Awake()
        {
            visible = particleCount;
            mat = GetComponent<MeshRenderer>().material; // 인스턴스 (에셋 수정 방지)
            float t0 = Time.realtimeSinceStartup;
            GetComponent<MeshFilter>().sharedMesh = BuildMesh(particleCount);
            Debug.Log($"[VortexPortal] {particleCount}개 입자 메시 생성 {(Time.realtimeSinceStartup - t0) * 1000f:0}ms (이후 CPU 비용 0)");
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.UpArrow)) SetCount(visible + 5000);
            if (DemoInput.KeyDown(Key.DownArrow)) SetCount(visible - 5000);
            if (DemoInput.KeyDown(Key.RightArrow)) spinSpeed = Mathf.Min(3f, spinSpeed + 0.2f);
            if (DemoInput.KeyDown(Key.LeftArrow)) spinSpeed = Mathf.Max(0f, spinSpeed - 0.2f);
            if (DemoInput.KeyDown(Key.Space)) Pulse();

            burst = Mathf.MoveTowards(burst, 0f, Time.deltaTime * 1.5f);
            // 속도를 바로 넘기면 속도가 바뀌는 순간 위치가 튐 → 위상(속도×시간)을 누적해서 넘김
            spinPhase += spinSpeed * (1f + burst * 2f) * Time.deltaTime;
            pullPhase += pullSpeed * (1f + burst * 3f) * Time.deltaTime;
            mat.SetFloat(VisibleCountId, visible);
            mat.SetFloat(SpinId, spinPhase);
            mat.SetFloat(PullId, pullPhase);
            mat.SetFloat(IntensityId, 1.2f + burst * 1.5f);

            fps = Mathf.Lerp(fps, 1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime), 0.05f);
            GlossaryInfoPanel.Status = $"입자 {visible:N0} / {particleCount:N0}개   회전 {spinSpeed:0.0}   {fps:0} FPS\n" +
                                       "움직임은 정점 셰이더가 위상값으로 계산 → CPU 업데이트 0";
        }

        public void SetCount(int n) => visible = Mathf.Clamp(n, 0, particleCount);
        public void MoreParticles() => SetCount(visible + 10000);
        public void FewerParticles() => SetCount(visible - 10000);
        public void Pulse() => burst = 1f;

        private static Mesh BuildMesh(int count)
        {
            var verts = new Vector3[count * 4];
            var corners = new Vector2[count * 4];
            var seeds = new Vector4[count * 4];
            var indices = new int[count * 6];
            var rng = new System.Random(1234);
            Vector2[] quad = { new Vector2(-1f, -1f), new Vector2(1f, -1f), new Vector2(1f, 1f), new Vector2(-1f, 1f) };

            for (int i = 0; i < count; i++)
            {
                var seed = new Vector4((float)rng.NextDouble(), (float)(rng.NextDouble() * Mathf.PI * 2.0), (float)rng.NextDouble(), i);
                for (int k = 0; k < 4; k++)
                {
                    corners[i * 4 + k] = quad[k];
                    seeds[i * 4 + k] = seed; // 같은 입자의 4정점은 같은 씨앗 → 셰이더에서 같은 위치로 이동
                }
                int v = i * 4, t = i * 6;
                indices[t] = v; indices[t + 1] = v + 1; indices[t + 2] = v + 2;
                indices[t + 3] = v; indices[t + 4] = v + 2; indices[t + 5] = v + 3;
            }

            var mesh = new Mesh { name = "VortexParticles", indexFormat = IndexFormat.UInt32 }; // 65,535 정점 초과
            mesh.vertices = verts;
            mesh.uv = corners;
            mesh.SetUVs(1, seeds);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0, false);
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 12f); // 정점이 모두 원점이라 직접 지정(컬링 방지)
            return mesh;
        }
    }
}
