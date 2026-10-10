using UnityEngine;

namespace GlossaryDemo.Movement
{
    // 코요테 타임: 발판 끝에서 떨어진 직후 coyoteTime 초 동안은 아직 땅에 있는 것처럼 점프를 허용
    // Rigidbody2D 기반 횡스크롤 이동 (A/D 이동, Space 점프)
    [RequireComponent(typeof(Rigidbody2D))]
    public class CoyoteTimePlayer : MonoBehaviour
    {
        [Header("이동")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = 11f;

        [Header("코요테 타임")]
        [SerializeField] private bool useCoyoteTime = true;
        [SerializeField, Range(0f, 0.5f)] private float coyoteTime = 0.1f;

        [Header("Ground Check")]
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
        [SerializeField] private LayerMask groundMask;

        [SerializeField] private SpriteRenderer body;

        private Rigidbody2D rb;
        private float coyoteTimer;   // 땅을 떠난 뒤 남은 점프 허용 시간
        private Vector3 spawnPos;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spawnPos = transform.position;
        }

        private void Update()
        {
            bool grounded = IsGrounded();

            // 땅에 있으면 타이머를 가득 채우고, 공중이면 줄여 나감
            if (grounded) coyoteTimer = coyoteTime;
            else coyoteTimer -= Time.deltaTime;

            bool canJump = useCoyoteTime ? coyoteTimer > 0f : grounded;
            if (DemoInput.JumpPressed && canJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
                coyoteTimer = 0f; // 한 번 쓰면 끝 (공중 2단 점프 방지)
            }

            if (DemoInput.KeyDown(UnityEngine.InputSystem.Key.C)) ToggleCoyote();
            if (transform.position.y < -8f) Respawn();

            // 코요테 타임이 남아 있는 공중 상태를 노란색으로 표시
            if (body != null)
                body.color = grounded ? new Color(0.3f, 0.65f, 1f) : coyoteTimer > 0f && useCoyoteTime ? new Color(1f, 0.85f, 0.3f) : new Color(0.6f, 0.6f, 0.7f);

            GlossaryInfoPanel.Status = $"코요테 타임: {(useCoyoteTime ? $"ON ({coyoteTime:0.00}초)" : "OFF")}\n" +
                                       $"땅 위: {grounded}   남은 허용 시간: {Mathf.Max(0f, coyoteTimer):0.00}초";
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = new Vector2(DemoInput.Move.x * moveSpeed, rb.linearVelocity.y);
        }

        public void ToggleCoyote() => useCoyoteTime = !useCoyoteTime;

        private void Respawn()
        {
            transform.position = spawnPos;
            rb.linearVelocity = Vector2.zero;
        }

        private bool IsGrounded()
        {
            Vector2 feet = (Vector2)transform.position + Vector2.down * (transform.localScale.y * 0.5f);
            return Physics2D.OverlapBox(feet, groundCheckSize, 0f, groundMask) != null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector2 feet = (Vector2)transform.position + Vector2.down * (transform.localScale.y * 0.5f);
            Gizmos.DrawWireCube(feet, groundCheckSize);
        }
    }
}
