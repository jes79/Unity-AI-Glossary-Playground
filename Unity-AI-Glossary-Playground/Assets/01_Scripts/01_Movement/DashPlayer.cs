using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // 대시 / 회피: Shift 로 dashDuration(0.2초) 동안 빠르게 이동
    // - 대시 중에는 무적(i-frame, invincibility frame): 총알이 몸을 통과해도 피해 없음
    // - 쿨타임(dashCooldown 1초)이 지나야 다시 대시 가능
    // 탑다운 2D, Transform 직접 이동 (총알 판정은 DashBullet 이 거리로 검사)
    public class DashPlayer : MonoBehaviour
    {
        [Header("이동")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private Rect arena = new Rect(-9f, -5f, 18f, 9.6f);

        [Header("대시")]
        [SerializeField] private float dashSpeed = 22f;
        [SerializeField, Range(0.05f, 1f)] private float dashDuration = 0.2f;
        [SerializeField, Range(0f, 3f)] private float dashCooldown = 1f;
        [SerializeField] private bool useIFrames = true;

        [Header("표시")]
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private Transform cooldownBar; // 발밑 쿨타임 막대 (scale.x 0~1)

        private Vector2 lastDir = Vector2.right;
        private float dashTimer;      // 남은 대시 시간
        private float cooldownTimer;  // 남은 쿨타임
        private float hurtTimer;      // 피격 직후 잠깐 무적 (연속 피격 방지)
        private int hits, dodges;
        private float afterImageTimer;

        public bool IsDashing => dashTimer > 0f;
        // 대시 중 + i-frame ON 이면 무적
        public bool IsInvincible => (IsDashing && useIFrames) || hurtTimer > 0f;

        private void Update()
        {
            Vector2 input = DemoInput.Move;
            if (input.sqrMagnitude > 0.01f) lastDir = input.normalized;

            cooldownTimer -= Time.deltaTime;
            hurtTimer -= Time.deltaTime;

            bool shift = DemoInput.KeyDown(Key.LeftShift) || DemoInput.KeyDown(Key.RightShift);
            if (shift && cooldownTimer <= 0f && !IsDashing)
            {
                dashTimer = dashDuration;
                cooldownTimer = dashCooldown;
            }
            if (DemoInput.KeyDown(Key.I)) useIFrames = !useIFrames;

            Vector2 velocity;
            if (IsDashing)
            {
                dashTimer -= Time.deltaTime;
                velocity = lastDir * dashSpeed; // 대시 중에는 방향 고정
                afterImageTimer -= Time.deltaTime;
                if (afterImageTimer <= 0f) { afterImageTimer = 0.03f; SpawnAfterImage(); }
            }
            else
            {
                velocity = input * moveSpeed;
            }

            Vector3 p = transform.position + (Vector3)(velocity * Time.deltaTime);
            p.x = Mathf.Clamp(p.x, arena.xMin, arena.xMax);
            p.y = Mathf.Clamp(p.y, arena.yMin, arena.yMax);
            transform.position = p;

            if (body != null)
            {
                Color c = new Color(0.3f, 0.65f, 1f);
                if (hurtTimer > 0f) c = Color.Lerp(new Color(1f, 0.3f, 0.3f), c, 1f - hurtTimer / 0.5f);
                if (IsDashing) c = useIFrames ? new Color(1f, 1f, 1f, 0.45f) : new Color(1f, 0.85f, 0.3f);
                body.color = c;
            }
            if (cooldownBar != null)
                cooldownBar.localScale = new Vector3(1f - Mathf.Clamp01(cooldownTimer / Mathf.Max(0.01f, dashCooldown)), 0.12f, 1f);

            GlossaryInfoPanel.Status = $"i-frame(대시 중 무적): {(useIFrames ? "ON" : "OFF")}\n" +
                                       $"대시 {(IsDashing ? "중!" : "")}  쿨타임 남음: {Mathf.Max(0f, cooldownTimer):0.00}초\n" +
                                       $"피격 {hits}회   회피 성공 {dodges}회";
        }

        // 총알이 닿았을 때 호출. 무적이면 회피로 처리
        public void OnBulletTouch(out bool damaged)
        {
            damaged = false;
            if (IsInvincible)
            {
                if (IsDashing) dodges++;
                return;
            }
            hits++;
            hurtTimer = 0.5f;
            damaged = true;
        }

        public void ToggleIFrames() => useIFrames = !useIFrames;

        private void SpawnAfterImage()
        {
            if (body == null) return;
            var go = new GameObject("AfterImage");
            go.transform.SetPositionAndRotation(transform.position, transform.rotation);
            go.transform.localScale = transform.localScale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = body.sprite;
            sr.sortingOrder = body.sortingOrder - 1;
            StartCoroutine(FadeAndDestroy(sr));
        }

        private static IEnumerator FadeAndDestroy(SpriteRenderer sr)
        {
            Color c = new Color(0.5f, 0.8f, 1f, 0.5f);
            yield return Ease.Run(0.25f, Ease.Linear, t => { if (sr != null) sr.color = new Color(c.r, c.g, c.b, c.a * (1f - t)); });
            if (sr != null) Destroy(sr.gameObject);
        }
    }
}
