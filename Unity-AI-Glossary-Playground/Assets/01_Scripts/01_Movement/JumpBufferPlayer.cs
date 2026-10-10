using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // 점프 버퍼링: 착지 직전(공중)에 누른 점프를 jumpBufferTime 초 동안 기억했다가 땅에 닿는 순간 실행
    // 버퍼가 없으면 "너무 일찍 누른" 점프는 그냥 무시됨
    [RequireComponent(typeof(Rigidbody2D))]
    public class JumpBufferPlayer : MonoBehaviour
    {
        [Header("이동")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = 12f;

        [Header("점프 버퍼링")]
        [SerializeField] private bool useJumpBuffer = true;
        [SerializeField, Range(0f, 0.5f)] private float jumpBufferTime = 0.15f;

        [Header("Ground Check")]
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.7f, 0.1f);
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private SpriteRenderer body;

        private Rigidbody2D rb;
        private float bufferTimer;     // 남은 기억 시간 (0 보다 크면 점프 예약 중)
        private int bufferedJumps;     // 버퍼 덕분에 성공한 점프 수
        private int ignoredPresses;    // 버퍼 OFF 에서 무시된 점프 수
        private Vector3 spawnPos;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spawnPos = transform.position;
        }

        private void Update()
        {
            bool grounded = IsGrounded();

            if (DemoInput.JumpPressed)
            {
                if (grounded) Jump();
                else if (useJumpBuffer) bufferTimer = jumpBufferTime; // 공중에서 눌렀으면 기억
                else ignoredPresses++;
            }
            else
            {
                bufferTimer -= Time.deltaTime;
                // 땅에 닿았는데 기억해 둔 점프가 아직 유효하면 바로 점프
                if (grounded && bufferTimer > 0f && rb.linearVelocity.y <= 0.01f)
                {
                    Jump();
                    bufferedJumps++;
                }
            }

            if (DemoInput.KeyDown(Key.B)) useJumpBuffer = !useJumpBuffer;
            if (transform.position.y < -8f) { transform.position = spawnPos; rb.linearVelocity = Vector2.zero; }

            if (body != null)
                body.color = bufferTimer > 0f ? new Color(1f, 0.85f, 0.3f) : grounded ? new Color(0.3f, 0.65f, 1f) : new Color(0.6f, 0.6f, 0.7f);

            GlossaryInfoPanel.Status = $"점프 버퍼링: {(useJumpBuffer ? $"ON ({jumpBufferTime:0.00}초)" : "OFF")}\n" +
                                       $"버퍼 남은 시간: {Mathf.Max(0f, bufferTimer):0.00}초 (노란색 = 예약 중)\n" +
                                       $"버퍼로 성공한 점프: {bufferedJumps}   무시된 입력(OFF): {ignoredPresses}";
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = new Vector2(DemoInput.Move.x * moveSpeed, rb.linearVelocity.y);
        }

        private void Jump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
            bufferTimer = 0f; // 사용한 예약은 지움
        }

        public void ToggleBuffer() => useJumpBuffer = !useJumpBuffer;

        private bool IsGrounded()
        {
            Vector2 feet = (Vector2)transform.position + Vector2.down * (transform.localScale.y * 0.5f);
            return Physics2D.OverlapBox(feet, groundCheckSize, 0f, groundMask) != null;
        }
    }
}
