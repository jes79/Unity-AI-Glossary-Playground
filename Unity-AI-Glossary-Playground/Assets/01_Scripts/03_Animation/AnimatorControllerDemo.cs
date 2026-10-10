using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AnimationDemo
{
    // Animator Controller 데모: Idle / Run / Jump 세 상태를 가진 컨트롤러를 코드가 파라미터로 조종
    // - 이동 입력 크기 → Speed(Float) → Idle ↔ Run 전환
    // - Space → Jump(Trigger) → Any State 에서 Jump 로, 끝나면 Idle 로 복귀
    [RequireComponent(typeof(Animator))]
    public class AnimatorControllerDemo : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float limitX = 7f;
        [SerializeField] private AnimatorStateDisplay display;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int JumpHash = Animator.StringToHash("Jump");

        private Animator animator;

        private void Awake() => animator = GetComponent<Animator>();

        private void Update()
        {
            float x = DemoInput.Move.x;
            if (Mathf.Abs(x) > 0.01f)
            {
                transform.rotation = Quaternion.Euler(0f, x > 0f ? 90f : -90f, 0f); // 진행 방향을 바라봄
                var p = transform.position + Vector3.right * (x * moveSpeed * Time.deltaTime);
                p.x = Mathf.Clamp(p.x, -limitX, limitX);
                transform.position = p;
            }

            animator.SetFloat(SpeedHash, Mathf.Abs(x));           // Speed > 0.1 이면 Run
            if (DemoInput.JumpPressed) animator.SetTrigger(JumpHash); // 트리거는 한 번 쓰면 자동으로 꺼짐

            if (DemoInput.KeyDown(Key.R)) animator.Play("Idle", 0, 0f);

            GlossaryInfoPanel.Status = $"현재 상태: {(display != null ? display.CurrentStateName : "?")}\n" +
                                       $"Speed = {Mathf.Abs(x):0.00}   (Idle→Run 조건: Speed > 0.1)";
        }
    }
}
