using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Movement
{
    // Ground Check: 발밑으로 SphereCast 를 쏴서 groundMask(Ground 레이어)에 닿으면 "땅 위"
    // - SphereCast 는 굵은 레이저라 발판 가장자리에 걸쳐 서 있어도 인식됨 (R 키로 가는 Raycast 와 비교)
    // - G 키: 마스크를 Ground 만 ↔ 모든 레이어로 바꿔서, Obstacle 레이어 상자 위에서 결과가 어떻게 달라지는지 확인
    [RequireComponent(typeof(Rigidbody))]
    public class GroundCheckPlayer : MonoBehaviour
    {
        [Header("이동")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpVelocity = 6f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundMask;            // 기본: Ground 레이어만
        [SerializeField] private float castRadius = 0.4f;
        [SerializeField] private float castDistance = 0.7f;       // 캡슐 중심 → 발밑 + 여유 0.1
        [SerializeField] private bool useSphereCast = true;

        [Header("표시")]
        [SerializeField] private Transform castVisual;            // 캐스트 끝 위치의 반투명 구
        [SerializeField] private LineRenderer castLine;
        [SerializeField] private Renderer body;

        private Rigidbody rb;
        private Vector3 spawnPos;
        private bool grounded;
        private RaycastHit hit;
        private bool jumpRequested;
        private bool allLayers;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            spawnPos = transform.position;
        }

        private void Update()
        {
            if (DemoInput.JumpPressed) jumpRequested = true;
            if (DemoInput.KeyDown(Key.R)) useSphereCast = !useSphereCast;
            if (DemoInput.KeyDown(Key.G)) allLayers = !allLayers;
            if (transform.position.y < -10f) { rb.position = spawnPos; transform.position = spawnPos; rb.linearVelocity = Vector3.zero; }

            grounded = CheckGround(out hit);
            UpdateVisual();

            string what = grounded ? $"{hit.collider.name} (레이어: {LayerMask.LayerToName(hit.collider.gameObject.layer)}, 거리 {hit.distance:0.00})" : "없음";
            GlossaryInfoPanel.Status = $"방식: {(useSphereCast ? $"SphereCast (반지름 {castRadius})" : "Raycast (가는 선)")}   마스크: {(allLayers ? "모든 레이어" : "Ground 만")}\n" +
                                       $"isGrounded: {grounded}\n밟은 것: {what}";
        }

        private bool CheckGround(out RaycastHit result)
        {
            int mask = allLayers ? ~LayerMask.GetMask("Player") : groundMask.value;
            Vector3 origin = transform.position;
            // 트리거는 무시(QueryTriggerInteraction.Ignore). 시작점이 캡슐 중심이라 자기 콜라이더는 Player 레이어 제외로 거름
            if (useSphereCast)
                return Physics.SphereCast(origin, castRadius, Vector3.down, out result, castDistance, mask, QueryTriggerInteraction.Ignore);
            return Physics.Raycast(origin, Vector3.down, out result, castDistance + castRadius, mask, QueryTriggerInteraction.Ignore);
        }

        private void FixedUpdate()
        {
            Vector2 input = DemoInput.Move;
            rb.linearVelocity = new Vector3(input.x * moveSpeed, rb.linearVelocity.y, input.y * moveSpeed);
            rb.angularVelocity = Vector3.zero;
            if (jumpRequested)
            {
                jumpRequested = false;
                if (grounded) rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z); // Ground Check 통과해야 점프
            }
        }

        private void UpdateVisual()
        {
            Color c = grounded ? new Color(0.3f, 1f, 0.4f, 0.45f) : new Color(1f, 0.3f, 0.3f, 0.45f);
            float dist = grounded ? hit.distance : useSphereCast ? castDistance : castDistance + castRadius;
            Vector3 end = transform.position + Vector3.down * dist;
            if (castVisual != null)
            {
                castVisual.gameObject.SetActive(useSphereCast);
                castVisual.position = end;
                castVisual.localScale = Vector3.one * castRadius * 2f;
                castVisual.GetComponent<Renderer>().material.SetColor("_BaseColor", c);
            }
            if (castLine != null)
            {
                castLine.SetPosition(0, transform.position);
                castLine.SetPosition(1, useSphereCast ? end : transform.position + Vector3.down * dist);
                castLine.startColor = castLine.endColor = new Color(c.r, c.g, c.b, 1f);
            }
            if (body != null) body.material.SetColor("_BaseColor", grounded ? new Color(0.3f, 0.65f, 1f) : new Color(0.6f, 0.6f, 0.7f));
        }
    }
}
