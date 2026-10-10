using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Rendering
{
    // Update 에서 GC Alloc 이 생기는 "나쁜 코드"와 고친 "좋은 코드"를 나란히 실행해 비교
    // 두 코드는 같은 일을 함: 적 목록에서 살아있는 적을 골라 점수 합계를 구하고, 화면 글자를 갱신
    // 측정: ProfilerMarker 로 각 구간을 나누고, ProfilerRecorder("GC Allocated In Frame")로 프레임 전체 할당량을 읽음
    //       + 구간별 할당량은 GC.GetAllocatedBytesForCurrentThread() 차이로 계산
    public class GcAllocDemo : MonoBehaviour
    {
        public enum Mode { Bad, Good, Both }

        [SerializeField] private TextMeshProUGUI badText;
        [SerializeField] private TextMeshProUGUI goodText;
        [SerializeField] private int enemyCount = 200;
        [SerializeField] private int repeatPerFrame = 20;   // 차이를 크게 보이려고 같은 작업을 여러 번
        [SerializeField] private Mode mode = Mode.Both;

        private struct Enemy { public int Hp; public int Score; }
        private Enemy[] enemies;
        private int frame;

        // 좋은 코드용 캐시 (한 번만 만들고 재사용)
        private readonly List<int> cachedAlive = new List<int>(256);

        private ProfilerRecorder gcFrame;
        private static readonly ProfilerMarker BadMarker = new ProfilerMarker("GlossaryDemo.BadUpdate");
        private static readonly ProfilerMarker GoodMarker = new ProfilerMarker("GlossaryDemo.GoodUpdate");
        private long badBytes, goodBytes;
        private float badAvg, goodAvg;
        private int gcCountStart;

        private void Awake()
        {
            enemies = new Enemy[enemyCount];
            for (int i = 0; i < enemies.Length; i++) enemies[i] = new Enemy { Hp = i % 3, Score = i };
            gcCountStart = System.GC.CollectionCount(0);
        }

        private void OnEnable() => gcFrame = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        private void OnDisable() => gcFrame.Dispose();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) SetMode(0);
            if (DemoInput.KeyDown(Key.Digit2)) SetMode(1);
            if (DemoInput.KeyDown(Key.Digit3)) SetMode(2);
            frame++;
            for (int i = 0; i < enemies.Length; i++) enemies[i].Hp = (i + frame / 30) % 3; // 살아있는 적이 바뀜

            badBytes = goodBytes = 0;
            if (mode != Mode.Good)
            {
                long before = Allocated();
                using (BadMarker.Auto())
                    for (int r = 0; r < repeatPerFrame; r++) BadUpdate();
                badBytes = Allocated() - before;
            }
            if (mode != Mode.Bad)
            {
                long before = Allocated();
                using (GoodMarker.Auto())
                    for (int r = 0; r < repeatPerFrame; r++) GoodUpdate();
                goodBytes = Allocated() - before;
            }
            badAvg = Mathf.Lerp(badAvg, badBytes, 0.1f);
            goodAvg = Mathf.Lerp(goodAvg, goodBytes, 0.1f);

            GlossaryInfoPanel.Status =
                $"실행 중: <b>{(mode == Mode.Bad ? "나쁜 코드만" : mode == Mode.Good ? "좋은 코드만" : "둘 다")}</b>   (각 {repeatPerFrame}회/프레임)\n" +
                $"<color=#F99>나쁜 코드</color> {badAvg / 1024f:0.0} KB/프레임    <color=#9F9>좋은 코드</color> {goodAvg / 1024f:0.0} KB/프레임\n" +
                $"프레임 전체 GC Alloc {(gcFrame.Valid ? (gcFrame.LastValue / 1024f).ToString("0.0") + " KB" : "N/A")}   시작 후 GC 실행 횟수 {System.GC.CollectionCount(0) - gcCountStart}";
        }

        private static long Allocated()
        {
            // 현재 스레드가 지금까지 할당한 총 바이트 (지원 안 되면 GC 힙 크기로 대체)
            try { return System.GC.GetAllocatedBytesForCurrentThread(); }
            catch { return System.GC.GetTotalMemory(false); }
        }

        // ───── 나쁜 예: 매 프레임 쓰레기 생성 ─────
        private void BadUpdate()
        {
            var alive = enemies.Where(e => e.Hp > 0).ToList();   // LINQ + 람다 + 새 List
            var scores = new List<int>();                         // 매번 새 List
            foreach (var e in alive) scores.Add(e.Score);
            int total = scores.Sum();
            string label = "살아있는 적: " + alive.Count + "  점수 합: " + total + "  프레임: " + frame; // 문자열 + 박싱
            badText.text = label;                                 // 매번 새 string
        }

        // ───── 좋은 예: 캐시 재사용, for 루프, SetText ─────
        private void GoodUpdate()
        {
            cachedAlive.Clear();                                  // 새로 만들지 않고 비워서 재사용
            int total = 0;
            for (int i = 0; i < enemies.Length; i++)
                if (enemies[i].Hp > 0) { cachedAlive.Add(i); total += enemies[i].Score; }
            // TMP SetText 는 숫자 포맷을 할당 없이 처리
            goodText.SetText("살아있는 적: {0}  점수 합: {1}  프레임: {2}", cachedAlive.Count, total, frame);
        }

        public void SetMode(int m) => mode = (Mode)m;
    }
}
