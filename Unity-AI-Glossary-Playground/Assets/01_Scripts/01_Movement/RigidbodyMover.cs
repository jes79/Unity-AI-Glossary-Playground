using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // Rigidbody 기반 이동: 입력은 Update 에서 읽고, 물리(velocity) 적용은 FixedUpdate 에서
    // 비교용으로 transform.position 을 직접 바꾸는 방식도 넣어 둠 (M 키)
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyMover : MonoBehaviour
    {
        public enum MoveMode { Velocity, TransformPosition }

        [SerializeField] private MoveMode mode = MoveMode.Velocity;
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = 6f;
        [SerializeField] private LayerMask groundMask = ~0;

        private Rigidbody rb;
        private Vector3 moveInput;   // Update 에서 읽은 입력 (월드 방향)
        private bool jumpRequested;  // Update 에서 누른 점프를 FixedUpdate 로 넘기는 깃발
        private Vector3 spawnPos;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            spawnPos = transform.position;
        }

        private void Update()
        {
            // 입력은 매 프레임(Update) 읽어야 키 눌림을 놓치지 않음
            Vector2 input = DemoInput.Move;
            moveInput = new Vector3(input.x, 0f, input.y);
            if (DemoInput.JumpPressed) jumpRequested = true;
            if (DemoInput.KeyDown(Key.M)) ToggleMode();

            if (mode == MoveMode.TransformPosition)
            {
                // 나쁜 예: 물리 엔진을 무시하고 위치를 직접 바꿈 → 벽에 파고들었다 튕겨 나오며 떨리고, 상자를 제대로 밀지 못함
                transform.position += moveInput * moveSpeed * Time.deltaTime;
            }

            if (transform.position.y < -10f) Respawn();

            GlossaryInfoPanel.Status = $"이동 방식: {(mode == MoveMode.Velocity ? "Rigidbody.linearVelocity (FixedUpdate)" : "transform.position 직접 (Update)")}\n" +
                                       $"velocity: {rb.linearVelocity.x:0.0}, {rb.linearVelocity.y:0.0}, {rb.linearVelocity.z:0.0}";
        }

        private void FixedUpdate()
        {
            if (mode == MoveMode.Velocity)
            {
                // 좋은 예: 물리 스텝마다 속도를 정함. y(중력/점프)는 물리에 맡기고 수평만 덮어씀
                Vector3 v = moveInput * moveSpeed;
                rb.linearVelocity = new Vector3(v.x, rb.linearVelocity.y, v.z);
            }

            if (jumpRequested)
            {
                jumpRequested = false;
                if (IsGrounded()) rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
            }

            rb.angularVelocity = Vector3.zero; // 부딪혀서 빙글빙글 도는 것 방지
            if (moveInput.sqrMagnitude > 0.01f)
                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(moveInput), 720f * Time.fixedDeltaTime));
        }

        public void ToggleMode() => mode = mode == MoveMode.Velocity ? MoveMode.TransformPosition : MoveMode.Velocity;

        private bool IsGrounded() =>
            Physics.SphereCast(transform.position, 0.4f, Vector3.down, out _, 0.7f, groundMask, QueryTriggerInteraction.Ignore);

        private void Respawn()
        {
            rb.position = spawnPos;
            transform.position = spawnPos;
            rb.linearVelocity = Vector3.zero;
        }
    }
}
