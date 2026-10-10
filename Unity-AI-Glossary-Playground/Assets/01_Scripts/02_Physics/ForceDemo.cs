using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.PhysicsDemo
{
    // AddForce / Impulse
    // - 왼쪽 클릭: 클릭 지점에서 AddExplosionForce → 주변 Rigidbody 가 사방으로 날아감 (요청 예문)
    // - Space: 4개 레인의 상자에 같은 값(pushAmount)으로 AddForce 를 "한 번" 줌. ForceMode 만 다름
    //     Force          : 힘 × 시간(한 물리 스텝 0.02초) ÷ 질량 → 한 번 주면 거의 안 움직임 (계속 줘야 하는 힘)
    //     Acceleration   : 질량 무시, 힘 × 시간
    //     Impulse        : 순간 충격 ÷ 질량 → 무거우면 덜 날아감 (점프·타격에 사용)
    //     VelocityChange : 질량 무시, 속도를 바로 바꿈
    // - H 를 누르고 있으면 매 물리 스텝 계속 줌 (Force 는 이렇게 써야 함)
    // - R: 모두 제자리로
    public class ForceDemo : MonoBehaviour
    {
        [System.Serializable]
        public struct Lane
        {
            public ForceMode mode;
            public Rigidbody[] bodies;
        }

        [SerializeField] private Lane[] lanes;
        [SerializeField] private float pushAmount = 10f;

        [Header("폭발")]
        [SerializeField] private float explosionForce = 12f;
        [SerializeField] private float explosionRadius = 6f;
        [SerializeField] private float upwardsModifier = 1f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private Transform explosionVisual;

        private readonly List<(Rigidbody rb, Vector3 pos, Quaternion rot)> initial = new List<(Rigidbody, Vector3, Quaternion)>();
        private bool pushOnce;
        private float visualTimer;

        private void Start()
        {
            foreach (var rb in FindObjectsByType<Rigidbody>(FindObjectsSortMode.None))
                initial.Add((rb, rb.position, rb.rotation));
        }

        private void Update()
        {
            if (DemoInput.JumpPressed) pushOnce = true;
            if (DemoInput.KeyDown(Key.R)) ResetAll();
            if (PhysicsClick.LeftWorld && Physics.Raycast(DemoInput.MouseRay(), out var hit, 200f, groundMask)) Explode(hit.point);

            visualTimer -= Time.deltaTime;
            if (explosionVisual != null)
            {
                explosionVisual.gameObject.SetActive(visualTimer > 0f);
                explosionVisual.localScale = Vector3.one * explosionRadius * 2f * (1f - Mathf.Max(0f, visualTimer) / 0.3f * 0.5f);
            }

            string speeds = "";
            foreach (var lane in lanes)
            {
                speeds += $"{lane.mode,-14}";
                foreach (var rb in lane.bodies) speeds += $" m{rb.mass:0}:{rb.linearVelocity.magnitude,5:0.0}";
                speeds += "\n";
            }
            GlossaryInfoPanel.Status = $"AddForce(앞쪽 × {pushAmount}) 결과 속도 (m/s)\n{speeds.TrimEnd()}";
        }

        private void FixedUpdate()
        {
            bool hold = DemoInput.KeyHeld(Key.H);
            if (!pushOnce && !hold) return;
            pushOnce = false;
            foreach (var lane in lanes)
                foreach (var rb in lane.bodies)
                    rb.AddForce(Vector3.forward * pushAmount, lane.mode); // ★ 같은 값, 다른 ForceMode
        }

        private void Explode(Vector3 center)
        {
            // 반경 안의 Collider 를 찾아 Rigidbody 가 있으면 폭발력 적용
            foreach (var col in Physics.OverlapSphere(center, explosionRadius))
            {
                var rb = col.attachedRigidbody;
                if (rb == null) continue;
                // 중심에서 멀수록 약해짐, upwardsModifier 로 위로 띄우는 성분 추가
                rb.AddExplosionForce(explosionForce, center, explosionRadius, upwardsModifier, ForceMode.Impulse);
            }
            if (explosionVisual != null) { explosionVisual.position = center; visualTimer = 0.3f; }
        }

        public void ResetAll()
        {
            foreach (var (rb, pos, rot) in initial)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = pos;
                rb.rotation = rot;
                rb.transform.SetPositionAndRotation(pos, rot);
            }
        }
    }
}
