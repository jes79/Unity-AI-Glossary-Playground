using UnityEngine;

namespace GlossaryDemo.AnimationDemo
{
    // Parameter 데모: 스크립트가 세 종류의 Animator 파라미터를 제어
    // - Speed (Float)     : 이동 속도. Locomotion 의 Idle ↔ Run 전환 기준
    // - IsGrounded (Bool)  : 땅에 있는지. false 면 Air 상태 유지 (계속 켜져 있는 스위치)
    // - Attack (Trigger)   : 한 번 쏘고 자동으로 꺼지는 스위치. 공격 1회
    [RequireComponent(typeof(Animator))]
    public class AnimatorParameterDemo : MonoBehaviour
    {
        [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float acceleration = 12f;
        [SerializeField] private float jumpVelocity = 7f;
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float limitX = 7f;
        [SerializeField] private AnimatorStateDisplay display;

        // 문자열 대신 해시를 미리 구해 두면 매 프레임 문자열 비교를 피함
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int GroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        private Animator animator;
        private float speed;      // 현재 수평 속도 (부호 = 방향)
        private float vy;
        private bool grounded = true;
        private int attackCount;

        private void Awake() => animator = GetComponent<Animator>();

        private void Update()
        {
            // 이동: 가속해서 목표 속도로
            float target = DemoInput.Move.x * maxSpeed;
            speed = Mathf.MoveTowards(speed, target, acceleration * Time.deltaTime);
            if (Mathf.Abs(speed) > 0.05f) transform.rotation = Quaternion.Euler(0f, speed > 0f ? 90f : -90f, 0f);

            // 간단한 점프 물리 (바닥 y = 0)
            if (DemoInput.JumpPressed && grounded)
            {
                vy = jumpVelocity;
                grounded = false;
            }
            if (!grounded)
            {
                vy -= gravity * Time.deltaTime;
            }

            var p = transform.position + new Vector3(speed, vy, 0f) * Time.deltaTime;
            p.x = Mathf.Clamp(p.x, -limitX, limitX);
            if (p.y <= 0f && !grounded)
            {
                p.y = 0f;
                vy = 0f;
                grounded = true;
            }
            transform.position = p;

            // ── 파라미터 제어 ──
            animator.SetFloat(SpeedHash, Mathf.Abs(speed));
            animator.SetBool(GroundedHash, grounded);
            if (DemoInput.LeftClick || DemoInput.KeyDown(UnityEngine.InputSystem.Key.J))
            {
                animator.SetTrigger(AttackHash);
                attackCount++;
            }

            GlossaryInfoPanel.Status =
                $"Speed (Float)      = {animator.GetFloat(SpeedHash):0.00}\n" +
                $"IsGrounded (Bool)  = {animator.GetBool(GroundedHash)}\n" +
                $"Attack (Trigger)   = 지금까지 {attackCount}회 발동\n" +
                $"현재 상태: {(display != null ? display.CurrentStateName : "")}";
        }
    }
}
