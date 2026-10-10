using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.PhysicsDemo
{
    // 터널링: 빠른 물체가 한 물리 스텝(0.02초) 사이에 얇은 벽을 건너뛰어 충돌을 놓치는 현상
    //   예) 속도 150 m/s → 한 스텝에 3m 이동, 벽 두께 0.1m → 벽 "앞"에 있다가 다음 스텝엔 벽 "뒤"
    // 해결: Rigidbody.collisionDetectionMode = Continuous (CCD, 스텝 사이 경로까지 검사)
    // 왼쪽 레인은 항상 Discrete, 오른쪽 레인은 1~4 키로 모드 선택해 비교
    public class TunnelingDemo : MonoBehaviour
    {
        [SerializeField] private Rigidbody bulletPrefab;
        [SerializeField] private Transform leftMuzzle, rightMuzzle;
        [SerializeField] private float wallZ = 8f;
        [SerializeField] private float bulletSpeed = 150f;
        [SerializeField] private float fireInterval = 0.25f;

        private static readonly CollisionDetectionMode[] Modes =
        {
            CollisionDetectionMode.Discrete, CollisionDetectionMode.Continuous,
            CollisionDetectionMode.ContinuousDynamic, CollisionDetectionMode.ContinuousSpeculative
        };
        private CollisionDetectionMode rightMode = CollisionDetectionMode.Continuous;
        private float timer;

        // [0] = 왼쪽 레인, [1] = 오른쪽 레인
        public static readonly int[] Blocked = new int[2], Passed = new int[2];

        private void Start()
        {
            Blocked[0] = Blocked[1] = Passed[0] = Passed[1] = 0;
        }

        private void Update()
        {
            for (int i = 0; i < Modes.Length; i++)
                if (DemoInput.KeyDown(Key.Digit1 + i)) { rightMode = Modes[i]; ResetCounts(); }
            if (DemoInput.KeyDown(Key.UpArrow)) { bulletSpeed = Mathf.Min(400f, bulletSpeed + 25f); ResetCounts(); }
            if (DemoInput.KeyDown(Key.DownArrow)) { bulletSpeed = Mathf.Max(25f, bulletSpeed - 25f); ResetCounts(); }

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = fireInterval;
                Fire(leftMuzzle, CollisionDetectionMode.Discrete, 0);
                Fire(rightMuzzle, rightMode, 1);
            }

            float stepDist = bulletSpeed * Time.fixedDeltaTime;
            GlossaryInfoPanel.Status = $"총알 속도 {bulletSpeed:0} m/s → 물리 한 스텝({Time.fixedDeltaTime:0.00}초)에 {stepDist:0.0} m 이동 (벽 두께 0.1 m)\n" +
                                       $"왼쪽 Discrete       : 막힘 {Blocked[0]}  뚫음 {Passed[0]}\n" +
                                       $"오른쪽 {rightMode,-20}: 막힘 {Blocked[1]}  뚫음 {Passed[1]}";
        }

        private void ResetCounts() => Blocked[0] = Blocked[1] = Passed[0] = Passed[1] = 0;

        private void Fire(Transform muzzle, CollisionDetectionMode mode, int lane)
        {
            var rb = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
            rb.collisionDetectionMode = mode; // ★ 핵심 설정
            rb.linearVelocity = Vector3.forward * bulletSpeed;
            var b = rb.GetComponent<TunnelingBullet>();
            b.Init(lane, wallZ);
        }
    }
}
