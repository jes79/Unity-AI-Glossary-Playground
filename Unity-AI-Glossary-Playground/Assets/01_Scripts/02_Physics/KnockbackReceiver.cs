using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // 넉백: 맞으면 "공격자 → 나" 방향(= 공격자 반대 방향)으로 knockbackDistance(3m) 만큼 밀려남
    // knockbackDuration 동안 속도를 직접 정해 줌 (처음 빠르고 끝에 느려지는 감속 곡선) + 그동안 조작 불가
    // ※ AddForce(dir * 힘, ForceMode.Impulse) 한 번으로도 되지만, 마찰·질량에 따라 거리가 들쭉날쭉해서
    //    "정확히 3m" 같은 기획 수치를 맞추려면 이렇게 거리 기준으로 처리하는 경우가 많음
    [RequireComponent(typeof(Rigidbody))]
    public class KnockbackReceiver : MonoBehaviour
    {
        [SerializeField] private float knockbackDistance = 3f;
        [SerializeField, Range(0.05f, 1f)] private float knockbackDuration = 0.25f;
        [SerializeField] private bool knockFromEnemyContact; // 플레이어용: Enemy 태그와 부딪히면 넉백
        [SerializeField] private Renderer body;

        public static bool Enabled = true; // 데모 전체 ON/OFF (K 키)

        private Rigidbody rb;
        private Vector3 dir;
        private float timer;
        private Color baseColor;

        public bool IsKnockedBack => timer > 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (body != null) baseColor = body.sharedMaterial.GetColor("_BaseColor");
        }

        public void ApplyKnockback(Vector3 attackerPosition)
        {
            if (body != null) body.material.SetColor("_BaseColor", new Color(1f, 0.3f, 0.25f));
            FloatingText.Spawn(transform.position + Vector3.up * 1.4f, Enabled ? "넉백!" : "피격", new Color(1f, 0.8f, 0.4f), 4f);
            if (!Enabled) { timer = 0.15f; dir = Vector3.zero; return; }

            // ★ 공격자 반대 방향 = (내 위치 - 공격자 위치), 수평만
            dir = transform.position - attackerPosition;
            dir.y = 0f;
            dir = dir.sqrMagnitude > 0.001f ? dir.normalized : -transform.forward;
            timer = knockbackDuration;

            // 조작/추적을 잠깐 멈춤
            if (TryGetComponent(out PhysicsDemoPlayer player)) player.Stun(knockbackDuration);
            if (TryGetComponent(out ChaserEnemy chaser)) chaser.Pause(knockbackDuration + 0.4f);
        }

        private void FixedUpdate()
        {
            if (timer <= 0f) return;
            float t = 1f - timer / knockbackDuration;               // 0 → 1
            // 거리 d 를 시간 T 동안 OutQuad 로 이동할 때 속도 = d × 2(1 - t) / T  (적분하면 정확히 d)
            float speed = knockbackDistance * 2f * (1f - t) / knockbackDuration;
            rb.linearVelocity = new Vector3(dir.x * speed, rb.linearVelocity.y, dir.z * speed);
            timer -= Time.fixedDeltaTime;
            if (timer <= 0f && body != null) body.material.SetColor("_BaseColor", baseColor);
        }

        private void OnCollisionEnter(Collision c)
        {
            if (knockFromEnemyContact && c.collider.CompareTag("Enemy") && !IsKnockedBack)
                ApplyKnockback(c.transform.position); // 부딪힌 적의 위치 = 공격자 위치
        }
    }
}
