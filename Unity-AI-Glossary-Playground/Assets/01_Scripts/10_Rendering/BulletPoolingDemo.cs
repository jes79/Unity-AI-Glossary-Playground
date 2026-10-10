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

        private ObjectPool<PoolBullet> pool;
        private float leftTimer, rightTimer;
        private int instantiateCount, destroyCount, poolCreateCount, poolGetCount, poolReleaseCount;
        private ProfilerRecorder gcAlloc;

        private void Awake()
        {
            // ObjectPool<T>(생성, 꺼낼 때, 반납할 때, 버릴 때, 중복반납 검사, 기본 용량, 최대 크기)
            pool = new ObjectPool<PoolBullet>(
                createFunc: () => { poolCreateCount++; return Instantiate(bulletPrefab, transform); },
                actionOnGet: b => { poolGetCount++; b.gameObject.SetActive(true); },
                actionOnRelease: b => { poolReleaseCount++; b.gameObject.SetActive(false); },
                actionOnDestroy: b => Destroy(b.gameObject),   // maxSize 를 넘어 반납되면 진짜로 파괴
                collectionCheck: true,                         // 같은 걸 두 번 반납하면 에러 (개발 중 실수 방지)
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
        }

        private void OnEnable() => gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        private void OnDisable() => gcAlloc.Dispose();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) ToggleFire();
            if (DemoInput.KeyDown(Key.UpArrow)) ChangeRate(10f);
            if (DemoInput.KeyDown(Key.DownArrow)) ChangeRate(-10f);

            if (firing)
            {
                float interval = 1f / shotsPerSecond;
                leftTimer += Time.deltaTime;
                while (leftTimer >= interval) { leftTimer -= interval; FireInstantiate(); }
                rightTimer += Time.deltaTime;
                while (rightTimer >= interval) { rightTimer -= interval; FirePooled(); }
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
            b.Launch(leftMuzzle.position, Spread(leftMuzzle.forward), x => { destroyCount++; Destroy(x.gameObject); });
        }

        // 좋은 예: 풀에서 꺼내고, 끝나면 풀에 반납
        private void FirePooled()
        {
            var b = pool.Get();
            b.Launch(rightMuzzle.position, Spread(rightMuzzle.forward), pool.Release);
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
