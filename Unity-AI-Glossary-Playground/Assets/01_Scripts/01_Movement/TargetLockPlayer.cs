using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // 록온(Target Lock)
    // - Tab: lockRange 안에서 가장 가까운 적(Enemy 태그)에 록온 / 다시 Tab 이면 해제
    // - Q / E: 록온 중 다른 적으로 전환 (가까운 순서)
    // - 록온 중에는 몸이 항상 적을 향하고, 이동은 적 기준(앞뒤 = 다가가기/멀어지기, 좌우 = 원을 그리며 돌기)
    // 카메라 쪽 처리는 LockOnCamera 가 CurrentTarget 을 보고 함
    [RequireComponent(typeof(CharacterController))]
    public class TargetLockPlayer : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float lockRange = 20f;
        [SerializeField] private Transform reticle;   // 록온 표시 (적 머리 위)
        [SerializeField] private Transform cameraTransform;

        private CharacterController cc;
        private float verticalVelocity;

        public Transform CurrentTarget { get; private set; }

        private void Awake() => cc = GetComponent<CharacterController>();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Tab))
                CurrentTarget = CurrentTarget == null ? FindNearest(null) : null;
            if (CurrentTarget != null && (DemoInput.KeyDown(Key.Q) || DemoInput.KeyDown(Key.E)))
                CurrentTarget = FindNearest(CurrentTarget) ?? CurrentTarget;

            // 너무 멀어지면 록온 해제
            if (CurrentTarget != null && Vector3.Distance(transform.position, CurrentTarget.position) > lockRange * 1.3f)
                CurrentTarget = null;

            Move();
            UpdateReticle();

            GlossaryInfoPanel.Status = CurrentTarget != null
                ? $"록온: {CurrentTarget.name}   거리 {Vector3.Distance(transform.position, CurrentTarget.position):0.0} m\nA/D 로 적 주위를 돌아 보세요"
                : $"록온 없음 (Tab 으로 {lockRange:0}m 안의 가장 가까운 적 선택)";
        }

        private void Move()
        {
            Vector2 input = DemoInput.Move;
            Vector3 forward, right;
            if (CurrentTarget != null)
            {
                // 록온 중: 적 방향 기준으로 이동 축을 잡음 → 좌우 입력이 적 주위 원운동이 됨
                forward = Vector3.ProjectOnPlane(CurrentTarget.position - transform.position, Vector3.up).normalized;
                right = Vector3.Cross(Vector3.up, forward);
            }
            else
            {
                Transform cam = cameraTransform != null ? cameraTransform : Camera.main.transform;
                forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            }

            Vector3 move = (forward * input.y + right * input.x) * moveSpeed;
            verticalVelocity = cc.isGrounded ? -2f : verticalVelocity - 20f * Time.deltaTime;
            cc.Move((move + Vector3.up * verticalVelocity) * Time.deltaTime);

            // 회전: 록온 중이면 적을 바라봄, 아니면 이동 방향
            Vector3 face = CurrentTarget != null ? forward : move;
            if (face.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(face), 720f * Time.deltaTime);
        }

        // except 가 null 이면 가장 가까운 적, 아니면 except 다음으로 가까운 적
        private Transform FindNearest(Transform except)
        {
            var candidates = new List<Transform>();
            foreach (var go in GameObject.FindGameObjectsWithTag("Enemy"))
                if (Vector3.Distance(transform.position, go.transform.position) <= lockRange) candidates.Add(go.transform);
            if (candidates.Count == 0) return null;
            candidates.Sort((a, b) => Vector3.Distance(transform.position, a.position).CompareTo(Vector3.Distance(transform.position, b.position)));
            if (except == null) return candidates[0];
            int i = candidates.IndexOf(except);
            return candidates[(i + 1) % candidates.Count];
        }

        private void UpdateReticle()
        {
            if (reticle == null) return;
            reticle.gameObject.SetActive(CurrentTarget != null);
            if (CurrentTarget == null) return;
            reticle.position = CurrentTarget.position + Vector3.up * 1.8f;
            float s = 1f + Mathf.Sin(Time.time * 6f) * 0.1f;
            reticle.localScale = Vector3.one * s;
        }
    }
}
