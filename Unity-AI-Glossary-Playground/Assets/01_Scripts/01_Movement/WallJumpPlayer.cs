using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // 벽 타기 + 벽 점프
    // - 공중에서 벽에 닿아 떨어지는 중이면 낙하 속도를 wallSlideSpeed 로 제한 (천천히 미끄러짐)
    // - 벽에 붙은 채 점프하면 벽 반대 방향 + 위로 튕겨 나감. 잠깐(wallJumpLockTime) 좌우 입력을 무시해야 같은 벽에 바로 다시 붙지 않음
    [RequireComponent(typeof(Rigidbody2D))]
    public class WallJumpPlayer : MonoBehaviour
    {
        [Header("이동")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = 12f;

        [Header("벽 타기 / 벽 점프")]
        [SerializeField] private bool useWallMechanics = true;
        [SerializeField] private float wallSlideSpeed = 2f;
        [SerializeField] private Vector2 wallJumpVelocity = new Vector2(8f, 13f);
        [SerializeField, Range(0f, 0.5f)] private float wallJumpLockTime = 0.15f;

        [Header("판정")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.7f, 0.1f);
        [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.8f);
        [SerializeField] private SpriteRenderer body;

        private Rigidbody2D rb;
        private int wallDir;         // -1 왼쪽 벽, 1 오른쪽 벽, 0 없음
        private bool grounded, sliding;
        private float lockTimer;     // 벽 점프 직후 입력 잠금
        private int wallJumps;
        private Vector3 spawnPos;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spawnPos = transform.position;
        }

        private void Update()
        {
            grounded = Check(Vector2.down * (transform.localScale.y * 0.5f), groundCheckSize);
            bool left = Check(Vector2.left * (transform.localScale.x * 0.5f), wallCheckSize);
            bool right = Check(Vector2.right * (transform.localScale.x * 0.5f), wallCheckSize);
            wallDir = right ? 1 : left ? -1 : 0;
            lockTimer -= Time.deltaTime;

            // 벽 슬라이드: 땅이 아니고, 벽에 닿았고, 떨어지는 중
            sliding = useWallMechanics && !grounded && wallDir != 0 && rb.linearVelocity.y < 0f;

            if (DemoInput.JumpPressed)
            {
                if (grounded)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
                }
                else if (useWallMechanics && wallDir != 0)
                {
                    // 벽 반대 방향(-wallDir)으로 튕겨 나감
                    rb.linearVelocity = new Vector2(-wallDir * wallJumpVelocity.x, wallJumpVelocity.y);
                    lockTimer = wallJumpLockTime;
                    wallJumps++;
                }
            }

            if (DemoInput.KeyDown(Key.W)) useWallMechanics = !useWallMechanics;
            if (transform.position.y < -8f) { transform.position = spawnPos; rb.linearVelocity = Vector2.zero; }

            if (body != null) body.color = sliding ? new Color(1f, 0.6f, 0.3f) : grounded ? new Color(0.3f, 0.65f, 1f) : new Color(0.6f, 0.6f, 0.7f);

            string wall = wallDir == 0 ? "없음" : wallDir > 0 ? "오른쪽" : "왼쪽";
            GlossaryInfoPanel.Status = $"벽 타기/벽 점프: {(useWallMechanics ? "ON" : "OFF")}\n" +
                                       $"닿은 벽: {wall}   슬라이드 중: {sliding} (주황색)\n" +
                                       $"낙하 속도: {rb.linearVelocity.y:0.0} (최대 -{wallSlideSpeed})   벽 점프 {wallJumps}회";
        }

        private void FixedUpdate()
        {
            // 벽 점프 직후에는 좌우 입력을 무시해 튕겨 나가는 속도를 유지
            if (lockTimer <= 0f)
                rb.linearVelocity = new Vector2(DemoInput.Move.x * moveSpeed, rb.linearVelocity.y);

            // 미끄러지는 중이면 낙하 속도 제한
            if (sliding && rb.linearVelocity.y < -wallSlideSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }

        public void ToggleWall() => useWallMechanics = !useWallMechanics;

        private bool Check(Vector2 offset, Vector2 size) =>
            Physics2D.OverlapBox((Vector2)transform.position + offset, size, 0f, groundMask) != null;
    }
}
