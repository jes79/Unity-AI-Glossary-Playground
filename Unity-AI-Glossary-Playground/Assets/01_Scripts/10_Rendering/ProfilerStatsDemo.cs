using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.Rendering
{
    // Profiler 체험: 게임 안에서 FPS / 프레임 시간 / GC Alloc 을 ProfilerRecorder 로 읽어 표시
    // [스파이크] 를 누르면 일부러 큰 쓰레기(GC Alloc) + 무거운 계산을 한 프레임에 몰아서 함
    // → Profiler 창에서 튀는 막대를 클릭하면 "GlossaryDemo.Spike" 마커와 GC Alloc 열로 원인을 찾을 수 있음
    public class ProfilerStatsDemo : MonoBehaviour
    {
        [SerializeField] private RawImage graph;               // 프레임 시간 그래프 (Texture2D 로 직접 그림)
        [SerializeField] private int spikeAllocMB = 30;        // 스파이크 1회 쓰레기 크기
        [SerializeField] private int spikeLoop = 3_000_000;    // 스파이크 1회 계산량
        [SerializeField] private bool autoSpike;               // 3초마다 자동 스파이크

        // Profiler 마커: Profiler 창 Hierarchy 에 이 이름으로 나타남
        private static readonly ProfilerMarker SpikeMarker = new ProfilerMarker("GlossaryDemo.Spike");

        private ProfilerRecorder mainThreadTime;
        private ProfilerRecorder gcAllocInFrame;
        private ProfilerRecorder gcUsedMemory;
        private ProfilerRecorder batches;

        private Texture2D tex;
        private Color32[] pixels;
        private const int W = 300, H = 100;
        private readonly Queue<float> history = new Queue<float>();
        private float worstMs, smoothMs, autoTimer;
        private long lastSpikeAlloc;
        private readonly StringBuilder sb = new StringBuilder(512);
        private object keepAlive; // 최적화로 할당이 사라지지 않게 참조

        private void OnEnable()
        {
            // 이름은 Profiler 의 카운터 이름 그대로 (Editor·Development Build 에서 유효)
            mainThreadTime = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
            gcAllocInFrame = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            gcUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
            batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        }

        private void OnDisable()
        {
            mainThreadTime.Dispose();
            gcAllocInFrame.Dispose();
            gcUsedMemory.Dispose();
            batches.Dispose();
        }

        private void Start()
        {
            tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            pixels = new Color32[W * H];
            graph.texture = tex;
        }

        private void OnDestroy()
        {
            if (tex != null) Destroy(tex);
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) Spike();
            if (DemoInput.KeyDown(Key.G)) ForceGC();
            if (DemoInput.KeyDown(Key.A)) ToggleAuto();
            if (autoSpike && (autoTimer -= Time.unscaledDeltaTime) <= 0f) { autoTimer = 3f; Spike(); }

            float ms = Time.unscaledDeltaTime * 1000f;
            smoothMs = Mathf.Lerp(smoothMs, ms, 0.1f);
            history.Enqueue(ms);
            if (history.Count > W) history.Dequeue();
            worstMs = 0f;
            foreach (float h in history) worstMs = Mathf.Max(worstMs, h);
            DrawGraph();

            sb.Clear();
            sb.Append("FPS ").Append((1000f / Mathf.Max(0.01f, smoothMs)).ToString("0")).Append("   프레임 ").Append(smoothMs.ToString("0.0")).Append(" ms");
            sb.Append("   최근 최악 ").Append(worstMs.ToString("0.0")).Append(" ms\n");
            sb.Append("Main Thread ").Append(mainThreadTime.Valid ? (mainThreadTime.LastValue * 1e-6).ToString("0.00") + " ms" : "N/A");
            sb.Append("   Batches ").Append(batches.Valid ? batches.LastValue.ToString() : "N/A").Append('\n');
            sb.Append("GC Alloc (이번 프레임) ").Append(gcAllocInFrame.Valid ? FormatBytes(gcAllocInFrame.LastValue) : "N/A");
            sb.Append("   GC 힙 사용 ").Append(gcUsedMemory.Valid ? FormatBytes(gcUsedMemory.LastValue) : "N/A").Append('\n');
            sb.Append("마지막 스파이크 할당 ").Append(FormatBytes(lastSpikeAlloc)).Append("   자동 스파이크 ").Append(autoSpike ? "ON" : "OFF");
            GlossaryInfoPanel.Status = sb.ToString();
        }

        // 일부러 나쁜 프레임 만들기: 큰 배열 + 문자열 쓰레기 + 무거운 계산
        public void Spike()
        {
            using (SpikeMarker.Auto())
            {
                long before = System.GC.GetTotalMemory(false);
                var junk = new List<byte[]>();
                for (int i = 0; i < spikeAllocMB; i++) junk.Add(new byte[1024 * 1024]); // 1MB 씩
                string s = "";
                for (int i = 0; i < 2000; i++) s += i;                                    // 문자열 이어붙이기 = 쓰레기 대량
                double acc = 0;
                for (int i = 0; i < spikeLoop; i++) acc += Mathf.Sqrt(i) * Mathf.Sin(i);   // CPU 부하
                keepAlive = (junk.Count, s.Length, acc);
                lastSpikeAlloc = System.Math.Max(0, System.GC.GetTotalMemory(false) - before);
            }
        }

        // GC.Collect 도 스파이크를 만든다 (로딩 화면 같은 곳에서만 호출)
        public void ForceGC() => System.GC.Collect();
        public void ToggleAuto() { autoSpike = !autoSpike; autoTimer = 0f; }

        private void DrawGraph()
        {
            var bg = new Color32(20, 22, 28, 220);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = bg;
            // 기준선: 16.7ms(60fps), 33.3ms(30fps)
            const float maxMs = 50f;
            int y60 = (int)(16.7f / maxMs * (H - 1)), y30 = (int)(33.3f / maxMs * (H - 1));
            for (int x = 0; x < W; x++)
            {
                pixels[y60 * W + x] = new Color32(80, 200, 120, 255);
                pixels[y30 * W + x] = new Color32(230, 180, 60, 255);
            }
            int col = W - history.Count;
            foreach (float ms in history)
            {
                int h = Mathf.Clamp((int)(ms / maxMs * (H - 1)), 1, H - 1);
                var c = ms > 33.3f ? new Color32(255, 80, 80, 255) : ms > 16.7f ? new Color32(255, 200, 80, 255) : new Color32(110, 160, 255, 255);
                for (int y = 0; y < h; y++) pixels[y * W + col] = c;
                col++;
            }
            tex.SetPixels32(pixels);
            tex.Apply(false);
        }

        private static string FormatBytes(long b) =>
            b >= 1 << 20 ? (b / 1048576f).ToString("0.0") + " MB" : b >= 1024 ? (b / 1024f).ToString("0.0") + " KB" : b + " B";
    }
}
