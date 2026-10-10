using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.UIDemo
{
    // 가상 조이스틱 값으로 움직이고, 점프 버튼으로 점프하는 캐릭터 (Rigidbody)
    // 조이스틱의 Vector2 를 (x, 0, y) 로 바꿔 이동 방향으로 씀. 키보드(WASD/Space)도 같이 받음
    [RequireComponent(typeof(Rigidbody))]
    public class JoystickPlayer : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick joystick;
        [SerializeField] private JumpButton jumpButton;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 9f, -9f);
        [SerializeField] private TextMeshProUGUI modeLabel;

        [Header("이동")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = 7f;
        [SerializeField] private LayerMask groundMask;

        private Rigidbody rb;
        private bool jumpQueued;

        private void Awake() => rb = GetComponent<Rigidbody>();

        private void Start() => RefreshLabel();

        private void Update()
        {
            if (jumpButton.Consume() || DemoInput.JumpPressed) jumpQueued = true;
            if (DemoInput.KeyDown(Key.F)) ToggleJoystickMode();

            Vector2 v = joystick.Value;
            GlossaryInfoPanel.Status = $"조이스틱 값 ({v.x:0.00}, {v.y:0.00})  크기 {v.magnitude:0.00}   눌림 {joystick.Pressed}\n" +
                                       $"모드 {joystick.CurrentMode}   점프 버튼 {(jumpButton.Held ? "누르는 중" : "-")}   땅 위 {IsGrounded()}";
        }

        private void FixedUpdate()
        {
            Vector2 input = joystick.Value;
            if (input == Vector2.zero) input = DemoInput.Move; // 키보드 보조 입력
            Vector3 move = new Vector3(input.x, 0f, input.y) * moveSpeed; // 기울인 만큼 속도 (아날로그)
            rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
            if (move.sqrMagnitude > 0.01f) rb.MoveRotation(Quaternion.LookRotation(new Vector3(move.x, 0f, move.z)));

            if (jumpQueued)
            {
                if (IsGrounded()) rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
                jumpQueued = false;
            }
        }

        private void LateUpdate()
        {
            cameraTransform.position = transform.position + cameraOffset;
            cameraTransform.LookAt(transform.position);
        }

        public void ToggleJoystickMode()
        {
            joystick.ToggleMode();
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (modeLabel != null) modeLabel.text = joystick.CurrentMode == VirtualJoystick.Mode.Fixed ? "조이스틱: 고정 (F)" : "조이스틱: 플로팅 (F)";
        }

        private bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, 1.1f, groundMask);
    }
}
