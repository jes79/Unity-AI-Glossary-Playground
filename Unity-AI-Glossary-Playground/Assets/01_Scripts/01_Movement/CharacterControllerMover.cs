using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // CharacterController 이동: 물리(Rigidbody) 없이 Move() 로 움직이고, 벽·경사·계단은 컴포넌트가 처리
    // - WASD 이동(카메라 기준), Space 점프, 중력은 직접 계산
    // - slopeLimit(기본 45도) 보다 가파른 경사는 오르지 못함
    [RequireComponent(typeof(CharacterController))]
    public class CharacterControllerMover : MonoBehaviour
    {
        [Header("이동")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float turnSpeed = 720f;

        [Header("경사 / 계단")]
        [SerializeField, Range(0f, 90f)] private float slopeLimit = 45f;
        [SerializeField, Range(0f, 1f)] private float stepOffset = 0.3f;

        [Header("기타")]
        [SerializeField] private Transform cameraTransform; // 비우면 Camera.main 기준
        [SerializeField] private bool slopeKeys = true;     // 1/2/3 으로 slopeLimit 바꾸기 (0101 데모 전용)
        [SerializeField] private bool writeStatus = true;   // 다른 데모에서 재사용할 때는 끔

        private CharacterController cc;
        private float verticalVelocity;
        private float groundSlope;     // 발밑 경사 각도
        private Vector3 spawnPos;

        public bool IsGrounded => cc != null && cc.isGrounded;
        public Vector3 Velocity => cc != null ? cc.velocity : Vector3.zero;

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            cc.slopeLimit = slopeLimit;
            cc.stepOffset = stepOffset;
            spawnPos = transform.position;
        }

        private void Update()
        {
            if (slopeKeys)
            {
                if (DemoInput.KeyDown(Key.Digit1)) SetSlopeLimit(45f);
                if (DemoInput.KeyDown(Key.Digit2)) SetSlopeLimit(30f);
                if (DemoInput.KeyDown(Key.Digit3)) SetSlopeLimit(60f);
            }

            // 카메라가 보는 방향 기준으로 WASD 를 월드 방향으로 바꿈
            Vector2 input = DemoInput.Move;
            Transform camT = cameraTransform != null ? cameraTransform : Camera.main != null ? Camera.main.transform : null;
            Vector3 forward = camT != null ? Vector3.ProjectOnPlane(camT.forward, Vector3.up).normalized : Vector3.forward;
            Vector3 right = camT != null ? Vector3.ProjectOnPlane(camT.right, Vector3.up).normalized : Vector3.right;
            Vector3 move = forward * input.y + right * input.x;

            // 땅에 붙어 있으면 아래로 살짝 눌러 isGrounded 가 안정적으로 유지되게 함
            if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            if (DemoInput.JumpPressed && cc.isGrounded)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity); // v = √(2gh)
            verticalVelocity += gravity * Time.deltaTime;

            // CharacterController.Move: 충돌을 계산해서 벽에 막히고, slopeLimit / stepOffset 을 적용해 줌
            cc.Move((move * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);

            if (move.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(move), turnSpeed * Time.deltaTime);

            if (transform.position.y < -10f) Respawn();

            if (writeStatus)
                GlossaryInfoPanel.Status = $"slopeLimit: {cc.slopeLimit:0}°   stepOffset: {cc.stepOffset:0.00}\n" +
                                           $"isGrounded: {cc.isGrounded}   발밑 경사: {groundSlope:0}°\n" +
                                           $"속도: {cc.velocity.magnitude:0.0} m/s";
        }

        public void SetSlopeLimit(float degrees)
        {
            slopeLimit = degrees;
            cc.slopeLimit = degrees;
        }

        // Move() 중에 무언가에 닿으면 호출됨. 아래쪽 접촉의 법선으로 발밑 경사를 계산
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > 0.1f && hit.point.y < transform.position.y - cc.height * 0.25f)
                groundSlope = Vector3.Angle(hit.normal, Vector3.up);
        }

        private void Respawn()
        {
            cc.enabled = false; // CharacterController 는 켜진 상태에서 position 을 바꾸면 되돌아갈 수 있음
            transform.position = spawnPos;
            cc.enabled = true;
            verticalVelocity = 0f;
        }
    }
}
