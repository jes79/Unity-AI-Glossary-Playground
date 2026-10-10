using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // 가변 점프: 올라가는 중에 버튼을 떼면 상승 속도에 jumpCutMultiplier 를 곱해 줄임
    // → 짧게 톡 누르면 낮게, 꾹 누르고 있으면 끝까지 높게 뜀
    [RequireComponent(typeof(Rigidbody2D))]
    public class VariableJumpPlayer : MonoBehaviour
    {
        [Header("이동")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = 15f;

        [Header("가변 점프")]
        [SerializeField] private bool useVariableJump = true;
        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f; // 뗐을 때 남길 상승 속도 비율

        [Header("Ground Check")]
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.7f, 0.1f);
        [SerializeField] private LayerMask groundMask;

        [Header("표시")]
        [SerializeField] private Transform apexMarker; // 마지막 점프 최고점 표시선
        [SerializeField] private SpriteRenderer body;

        private Rigidbody2D rb;
        private bool jumping;
        private float jumpStartY, apexY, lastHeight, pressTime, lastPressTime;
        private bool cut;

        private void Awake() => rb = GetComponent<Rigidbody2D>();

        private void Update()
        {
            bool grounded = IsGrounded();

            if (DemoInput.JumpPressed && grounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
                jumping = true;
                cut = false;
                jumpStartY = apexY = transform.position.y;
                pressTime = Time.time;
            }

            // 핵심: 올라가는 중(vy > 0)에 버튼을 떼면 상승 속도를 깎음 (한 번만)
            if (DemoInput.JumpReleased && jumping)
            {
                lastPressTime = Time.time - pressTime;
                if (useVariableJump && rb.linearVelocity.y > 0f && !cut)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
                    cut = true;
                }
            }

            if (jumping)
            {
                apexY = Mathf.Max(apexY, transform.position.y);
                if (rb.linearVelocity.y <= 0f && grounded && Time.time - pressTime > 0.1f)
                {
                    jumping = false;
                    lastHeight = apexY - jumpStartY;
                    if (apexMarker != null) apexMarker.position = new Vector3(apexMarker.position.x, apexY - transform.localScale.y * 0.5f, 0f);
                }
            }

            if (DemoInput.KeyDown(Key.V)) useVariableJump = !useVariableJump;
            if (body != null) body.color = cut ? new Color(1f, 0.6f, 0.3f) : new Color(0.3f, 0.65f, 1f);

            GlossaryInfoPanel.Status = $"가변 점프: {(useVariableJump ? $"ON (뗄 때 속도 × {jumpCutMultiplier:0.0})" : "OFF (항상 같은 높이)")}\n" +
                                       $"마지막 점프 높이: {lastHeight:0.00} m   누른 시간: {lastPressTime:0.00}초\n" +
                                       $"현재 vy: {rb.linearVelocity.y:0.0}";
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = new Vector2(DemoInput.Move.x * moveSpeed, rb.linearVelocity.y);
        }

        public void ToggleVariableJump() => useVariableJump = !useVariableJump;

        private bool IsGrounded()
        {
            Vector2 feet = (Vector2)transform.position + Vector2.down * (transform.localScale.y * 0.5f);
            return Physics2D.OverlapBox(feet, groundCheckSize, 0f, groundMask) != null;
        }
    }
}
