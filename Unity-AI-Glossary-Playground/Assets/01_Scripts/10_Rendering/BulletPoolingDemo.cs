using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

namespace GlossaryDemo.Rendering
{
    // 왼쪽 포탑: Instantiate / Destroy (매번 새로 만들고 지움 → GC·생성 비용)
    // 오른쪽 포탑: ObjectPool<PoolBullet> (꺼내 쓰고 반납 → 일정 개수만 재사용)
    // 같은 연사 속도로 쏘면서 생성/파괴 횟수와 풀 상태를 비교
    public class BulletPoolingDemo : MonoBehaviour
    {
        [SerializeField] private PoolBullet bulletPrefab;
        [SerializeField] private Transform leftMuzzle;   // Instantiate/Destroy
        [SerializeField] private Transform rightMuzzle;  // ObjectPool
        [SerializeField] private float shotsPerSecond = 30f;
        [SerializeField] private int defaultCapacity = 20;
        [SerializeField] private int maxSize = 100;
        [SerializeField] private bool firing = true;

        // 한 프레임에 쏠 수 있는 최대 발수 (렉으로 deltaTime 이 커져도 while 이 길게 돌지 않게)
        private const int MaxShotsPerFrame = 16;

        private ObjectPool<PoolBullet> pool;
        private bool tearingDown;   // 씬 언로드/파괴 중이면 풀 반납·파괴 콜백을 무시
        private float leftTimer, rightTimer;
        private int instantiateCount, destroyCount, poolCreateCount, poolGetCount, poolReleaseCount;
        private ProfilerRecorder gcAlloc;

        private void Awake()
        {
            // ObjectPool<T>(생성, 꺼낼 때, 반납할 때, 버릴 때, 중복반납 검사, 기본 용량, 최대 크기)
            pool = new ObjectPool<PoolBullet>(
                createFunc: () => { poolCreateCount++; return Instantiate(bulletPrefab, transform); },
                actionOnGet: b => { poolGetCount++; b.gameObject.SetActive(true); },
                actionOnRelease: b => { poolReleaseCount++; if (b != null) b.gameObject.SetActive(false); },
                actionOnDestroy: b => { if (!tearingDown && b != null) Destroy(b.gameObject); },   // maxSize 를 넘어 반납되면 진짜로 파괴
                collectionCheck: true,                         // 같은 걸 두 번 반납하면 에러 (개발 중 실수 방지)
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
        }

        private void OnEnable() => gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        private void OnDisable()
        {
            if (gcAlloc.Valid) gcAlloc.Dispose();
            gcAlloc = default;
        }

        // 씬을 떠날 때: 총알들은 이 오브젝트의 자식이라 씬과 함께 파괴됨.
        // 풀 콜백(Release/Destroy)이 파괴 중인 오브젝트를 다시 건드리지 않도록 플래그를 세우고 풀만 비움
        private void OnDestroy()
        {
            tearingDown = true;
            firing = false;
            var p = pool;
            pool = null;
            p?.Dispose();   // tearingDown 이므로 actionOnDestroy 는 아무것도 하지 않음
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) ToggleFire();
            if (DemoInput.KeyDown(Key.UpArrow)) ChangeRate(10f);
            if (DemoInput.KeyDown(Key.DownArrow)) ChangeRate(-10f);

            if (pool == null) return;

            if (firing)
            {
                float interval = 1f / Mathf.Max(1f, shotsPerSecond);
                float dt = Mathf.Max(0f, Time.deltaTime);
                leftTimer += dt;
                for (int n = 0; leftTimer >= interval && n < MaxShotsPerFrame; n++) { leftTimer -= interval; FireInstantiate(); }
                if (leftTimer >= interval) leftTimer = 0f;   // 못 쏜 몫은 버림 (따라잡기 폭주 방지)
                rightTimer += dt;
                for (int n = 0; rightTimer >= interval && n < MaxShotsPerFrame; n++) { rightTimer -= interval; FirePooled(); }
                if (rightTimer >= interval) rightTimer = 0f;
            }

            GlossaryInfoPanel.Status =
                $"연사 {shotsPerSecond:0}/초   {(firing ? "발사 중" : "정지")}\n" +
                $"<color=#F99>왼쪽 Instantiate/Destroy</color>  생성 {instantiateCount}  파괴 {destroyCount}\n" +
                $"<color=#9F9>오른쪽 ObjectPool</color>  실제 생성 {poolCreateCount}  Get {poolGetCount}  Release {poolReleaseCount}\n" +
                $"  풀 상태: 사용 중 {pool.CountActive} / 대기 {pool.CountInactive} / 전체 {pool.CountAll}\n" +
                $"GC Alloc(프레임) {(gcAlloc.Valid ? gcAlloc.LastValue / 1024f : 0f):0.0} KB";
        }

        // 나쁜 예: 쏠 때마다 새로 만들고, 끝나면 Destroy
        private void FireInstantiate()
        {
            var b = Instantiate(bulletPrefab, transform);
            instantiateCount++;
            b.Launch(leftMuzzle.position, Spread(leftMuzzle.forward), x => { destroyCount++; if (x != null) Destroy(x.gameObject); });
        }

        // 좋은 예: 풀에서 꺼내고, 끝나면 풀에 반납
        private void FirePooled()
        {
            var b = pool.Get();
            b.Launch(rightMuzzle.position, Spread(rightMuzzle.forward), ReleasePooled);
        }

        // 반납: 파괴 중이거나 이미 풀에 있는(비활성) 총알은 다시 반납하지 않음
        private void ReleasePooled(PoolBullet b)
        {
            if (tearingDown || pool == null || b == null || !b.gameObject.activeSelf) return;
            pool.Release(b);
        }

        private static Vector3 Spread(Vector3 fwd) =>
            Quaternion.Euler(Random.Range(-4f, 4f), Random.Range(-12f, 12f), 0f) * fwd;

        public void ToggleFire() => firing = !firing;
        public void ChangeRate(float delta) => shotsPerSecond = Mathf.Clamp(shotsPerSecond + delta, 10f, 200f);

        public void ResetCounters()
        {
            instantiateCount = destroyCount = poolCreateCount = poolGetCount = poolReleaseCount = 0;
        }
    }
}
