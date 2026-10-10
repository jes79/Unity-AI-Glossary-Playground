using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Systems
{
    // 무적 시간(i-frame): 피격 직후 invincibleTime 초 동안 추가 피격 무시 + 스프라이트 깜빡임
    // OFF 로 두면 가시에 닿아 있는 동안 매 물리 프레임마다 맞아서 순식간에 HP 가 사라짐
    [RequireComponent(typeof(Rigidbody2D))]
    public class IFramePlayer : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private int maxHp = 10;

        [Header("무적 시간")]
        [SerializeField] private bool useIFrames = true;
        [SerializeField] private float invincibleTime = 1f;   // 피격 후 1초 무적
        [SerializeField] private float blinkInterval = 0.1f;  // 깜빡임 간격
        [SerializeField] private float knockback = 6f;

        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private UnityEngine.UI.Image hpFill;

        private Rigidbody2D rb;
        private int hp;
        private float invincibleTimer;
        private float knockTimer;
        private int hitCount, deaths;
        private Vector3 spawn;

        private bool Invincible => invincibleTimer > 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            hp = maxHp;
            spawn = transform.position;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.I)) useIFrames = !useIFrames;

            if (Invincible)
            {
                invincibleTimer -= Time.deltaTime;
                // 깜빡임: blinkInterval 마다 보였다 안 보였다
                sprite.enabled = Mathf.FloorToInt(invincibleTimer / blinkInterval) % 2 == 0;
                if (!Invincible) sprite.enabled = true;
            }

            if (hpFill != null) hpFill.fillAmount = (float)hp / maxHp;
            GlossaryInfoPanel.Status =
                $"무적 시간: {(useIFrames ? $"<color=#7CFC8A>ON ({invincibleTime:0.0}초)</color>" : "<color=#FF7A7A>OFF</color>")}   " +
                $"상태: {(Invincible ? $"무적 {invincibleTimer:0.00}초 남음" : "피격 가능")}\n" +
                $"HP {hp}/{maxHp}   피격 횟수 {hitCount}   사망 {deaths}";
        }

        private void FixedUpdate()
        {
            knockTimer -= Time.fixedDeltaTime;
            if (knockTimer <= 0f) rb.linearVelocity = DemoInput.Move * moveSpeed;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Enemy")) TryHit(other.transform.position);
        }

        private void TryHit(Vector3 from)
        {
            if (useIFrames && Invincible) return; // 핵심: 무적 중이면 무시

            hp--;
            hitCount++;
            if (useIFrames) invincibleTimer = invincibleTime;
            Vector2 dir = ((Vector2)(transform.position - from)).normalized;
            rb.linearVelocity = dir * knockback;
            knockTimer = 0.15f;

            if (hp <= 0)
            {
                deaths++;
                hp = maxHp;
                transform.position = spawn;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}
