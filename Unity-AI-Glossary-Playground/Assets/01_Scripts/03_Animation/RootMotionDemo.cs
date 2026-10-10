using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AnimationDemo
{
    // Root Motion 데모: 평소 이동은 코드(transform 이동), 구르기만 애니메이션의 루트 이동(Root Motion)으로 이동
    // - 구르기 클립은 Hips 를 앞으로 2.5m 옮기는 애니메이션. Avatar 의 Root Node = Hips 라서
    //   그 이동이 deltaPosition 으로 뽑혀 나오고, OnAnimatorMove 에서 캐릭터 루트에 더해 줌
    // - Root Motion OFF 면 같은 클립을 재생해도 이동량을 버리므로 제자리에서 구름
    [RequireComponent(typeof(Animator))]
    public class RootMotionDemo : MonoBehaviour
    {
        [SerializeField] private bool useRootMotion = true;
        [SerializeField] private float walkSpeed = 2.5f;
        [SerializeField] private float limitX = 9f;
        [SerializeField] private Transform startMarker;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int RollHash = Animator.StringToHash("Roll");
        private static readonly int RollState = Animator.StringToHash("Roll");

        private Animator animator;
        private float rollStartX;
        private float lastRollDistance;
        private bool wasRolling;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.applyRootMotion = useRootMotion;
        }

        private bool IsRolling
        {
            get
            {
                if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == RollState) return true;
                return animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).shortNameHash == RollState;
            }
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.R))
            {
                useRootMotion = !useRootMotion;
                animator.applyRootMotion = useRootMotion;
            }

            bool rolling = IsRolling;
            float x = rolling ? 0f : DemoInput.Move.x;

            // ── 코드 이동 (걷기) ──
            if (Mathf.Abs(x) > 0.01f)
            {
                transform.rotation = Quaternion.Euler(0f, x > 0f ? 90f : -90f, 0f);
                transform.position += Vector3.right * (x * walkSpeed * Time.deltaTime);
            }
            animator.SetFloat(SpeedHash, Mathf.Abs(x));

            if (DemoInput.JumpPressed && !rolling)
            {
                animator.SetTrigger(RollHash);
                rollStartX = transform.position.x;
                if (startMarker != null) startMarker.position = new Vector3(rollStartX, 0.01f, 0f);
            }
            if (wasRolling && !rolling) lastRollDistance = Mathf.Abs(transform.position.x - rollStartX);
            wasRolling = rolling;

            // 화면 밖으로 나가면 반대쪽으로
            if (Mathf.Abs(transform.position.x) > limitX) transform.position = new Vector3(-Mathf.Sign(transform.position.x) * (limitX - 0.5f), 0f, 0f);

            GlossaryInfoPanel.Status = $"Root Motion: {(useRootMotion ? "ON (구르기가 캐릭터를 이동시킴)" : "OFF (제자리 구르기)")}\n" +
                                       $"현재: {(rolling ? "구르기 (애니메이션이 이동)" : "걷기 (코드가 이동)")}\n" +
                                       $"마지막 구르기 이동 거리: {lastRollDistance:0.00} m";
        }

        // Animator 가 루트 이동을 계산한 직후 호출. 이 함수가 있으면 루트 모션 적용을 스크립트가 직접 결정
        private void OnAnimatorMove()
        {
            if (!useRootMotion || !IsRolling) return;
            Vector3 delta = animator.deltaPosition;
            delta.y = 0f;
            transform.position += delta;
        }
    }
}
