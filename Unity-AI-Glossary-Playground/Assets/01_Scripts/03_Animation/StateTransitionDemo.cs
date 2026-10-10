using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AnimationDemo
{
    // State / Transition 데모: 같은 Run → Jump 전환을 Has Exit Time ON / OFF 두 캐릭터로 비교
    // - Has Exit Time ON  : 조건(Jump 트리거)이 켜져도 Run 클립이 Exit Time 지점까지 재생돼야 넘어감 → 반응이 늦음
    // - Has Exit Time OFF : 조건이 켜지는 즉시 전환 → 버튼을 누르자마자 점프
    public class StateTransitionDemo : MonoBehaviour
    {
        [SerializeField] private Animator exitTimeOn;
        [SerializeField] private Animator exitTimeOff;

        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int JumpState = Animator.StringToHash("Jump");

        private float pressTime = -1f;
        private float delayOn = -1f, delayOff = -1f;

        private void Update()
        {
            if (DemoInput.JumpPressed)
            {
                pressTime = Time.time;
                delayOn = delayOff = -1f;
                exitTimeOn.SetTrigger(JumpHash);
                exitTimeOff.SetTrigger(JumpHash);
            }
            if (DemoInput.KeyDown(Key.R))
            {
                exitTimeOn.Play("Run", 0, 0f);
                exitTimeOff.Play("Run", 0, 0f);
            }

            // 버튼을 누른 뒤 실제로 Jump 상태(또는 Jump 로 가는 전환)가 시작될 때까지 걸린 시간 측정
            if (pressTime >= 0f)
            {
                if (delayOn < 0f && EnteredJump(exitTimeOn)) delayOn = Time.time - pressTime;
                if (delayOff < 0f && EnteredJump(exitTimeOff)) delayOff = Time.time - pressTime;
            }

            float runT = exitTimeOn.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
            GlossaryInfoPanel.Status =
                $"왼쪽  Has Exit Time ON  (Exit Time 0.95) → 반응 {Fmt(delayOn)}\n" +
                $"오른쪽 Has Exit Time OFF                → 반응 {Fmt(delayOff)}\n" +
                $"왼쪽 Run 진행도 {runT:0.00} (0.95 에 도달해야 전환)";
        }

        private static bool EnteredJump(Animator a)
        {
            return a.GetCurrentAnimatorStateInfo(0).shortNameHash == JumpState ||
                   (a.IsInTransition(0) && a.GetNextAnimatorStateInfo(0).shortNameHash == JumpState);
        }

        private static string Fmt(float d) => d < 0f ? "대기 중…" : $"{d * 1000f:0} ms";
    }
}
