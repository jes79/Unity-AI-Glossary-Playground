using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.AnimationDemo
{
    // Foot IK 대체 구현: 2본(허벅지-정강이) IK 를 코드로 풀어 계단 위에 발을 붙임
    // - 각 발 아래로 Raycast → 바닥 높이를 찾아 발 목표(IK Target)로 사용
    // - 엉덩이 높이는 더 낮은 발에 맞춰 내림 (안 그러면 낮은 쪽 다리가 닿지 않음)
    // - 코사인 법칙으로 무릎 각도를 구해 허벅지·정강이 회전 (Humanoid 의 OnAnimatorIK / Animation Rigging 의 Two Bone IK 와 같은 계산)
    // IK OFF 면 다리는 곧게 뻗은 그대로 → 계단에서 발이 공중에 뜨거나 계단을 뚫음
    public class FootIKDemo : MonoBehaviour
    {
        [SerializeField] private bool useIK = true;

        [Header("본")]
        [SerializeField] private Transform hips;
        [SerializeField] private Transform thighL, shinL, footL;
        [SerializeField] private Transform thighR, shinR, footR;
        [SerializeField] private Transform targetMarkerL, targetMarkerR;

        [Header("설정")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float legLength = 0.95f;
        [SerializeField] private float footHeight = 0.04f;   // 발목 높이 (발바닥과 바닥 사이 여유)
        [SerializeField] private float moveSpeed = 1.6f;
        [SerializeField] private float stepLength = 0.5f;
        [SerializeField] private float stepTime = 0.18f;
        [SerializeField] private float stepHeight = 0.18f;
        [SerializeField] private float minX = -4f, maxX = 6f;

        private class Leg
        {
            public Transform thigh, shin, end, marker;
            public Quaternion thighRest, shinRest;
            public Vector3 plant, from, to;
            public float t = 1f; // 1 = 디딘 상태, 0~1 = 발을 옮기는 중
            public bool Stepping => t < 1f;
        }

        private Leg left, right;
        private float hipsY;

        private void Awake()
        {
            left = new Leg { thigh = thighL, shin = shinL, end = footL, marker = targetMarkerL, thighRest = thighL.localRotation, shinRest = shinL.localRotation };
            right = new Leg { thigh = thighR, shin = shinR, end = footR, marker = targetMarkerR, thighRest = thighR.localRotation, shinRest = shinR.localRotation };
            float x = transform.position.x;
            left.plant = Ground(x + 0.2f);
            right.plant = Ground(x - 0.2f);
            hipsY = transform.position.y + legLength;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.I)) useIK = !useIK;

            // 좌우 이동 (몸통만 코드로 이동, 발은 디딘 자리에 남아 있다가 멀어지면 한 걸음)
            float input = DemoInput.Move.x;
            var p = transform.position;
            p.x = Mathf.Clamp(p.x + input * moveSpeed * Time.deltaTime, minX, maxX);
            transform.position = new Vector3(p.x, 0f, p.z);
            if (Mathf.Abs(input) > 0.01f) transform.rotation = Quaternion.Euler(0f, input > 0f ? 90f : -90f, 0f);

            UpdateStep(left, right, input);
            UpdateStep(right, left, input);
        }

        private void UpdateStep(Leg leg, Leg other, float input)
        {
            float x = transform.position.x;
            if (leg.Stepping)
            {
                leg.t = Mathf.Min(1f, leg.t + Time.deltaTime / stepTime);
                Vector3 pos = Vector3.Lerp(leg.from, leg.to, leg.t);
                pos.y += Mathf.Sin(leg.t * Mathf.PI) * stepHeight; // 발을 들어 올리는 호
                leg.plant = pos;
                return;
            }
            // 몸에서 너무 멀어졌고 다른 발이 디딘 상태면 앞으로 한 걸음
            if (Mathf.Abs(leg.plant.x - x) > stepLength * 0.5f && !other.Stepping)
            {
                float dir = Mathf.Abs(input) > 0.01f ? Mathf.Sign(input) : Mathf.Sign(x - leg.plant.x);
                leg.from = leg.plant;
                leg.to = Ground(x + dir * stepLength * 0.4f);
                leg.t = 0f;
            }
        }

        private void LateUpdate()
        {
            float bodyGround = Ground(transform.position.x).y;
            float lowFoot = Mathf.Min(left.plant.y, right.plant.y);

            // 엉덩이 높이: IK 면 낮은 발에 닿을 만큼 내림, 아니면 몸 중심 아래 바닥 + 다리 길이
            float targetY = useIK ? Mathf.Min(bodyGround, lowFoot) + legLength * 0.93f + footHeight : bodyGround + legLength + footHeight;
            hipsY = Mathf.Lerp(hipsY, targetY, 1f - Mathf.Exp(-12f * Time.deltaTime));
            hips.position = new Vector3(transform.position.x, hipsY, transform.position.z);

            Solve(left);
            Solve(right);

            float errL = footL.position.y - footHeight - Ground(footL.position.x).y;
            float errR = footR.position.y - footHeight - Ground(footR.position.x).y;
            GlossaryInfoPanel.Status = $"Foot IK: {(useIK ? "ON" : "OFF")}\n" +
                                       $"발과 바닥 사이 오차  왼발 {Fmt(errL)}  오른발 {Fmt(errR)}\n" +
                                       "(+ 는 공중에 뜸, − 는 바닥을 뚫음)";
        }

        private static string Fmt(float e) => Mathf.Abs(e) < 0.02f ? "<color=#9BE89B>0.00</color>" : $"{e:+0.00;-0.00}";

        private void Solve(Leg leg)
        {
            leg.thigh.localRotation = leg.thighRest;
            leg.shin.localRotation = leg.shinRest;
            Vector3 target = leg.plant + Vector3.up * footHeight;
            target.z = leg.thigh.position.z; // 좌우(골반 폭)는 허벅지 위치 그대로
            if (leg.marker != null) leg.marker.position = leg.plant + Vector3.up * 0.01f;
            if (!useIK) return;
            // 무릎은 몸 앞쪽으로 굽힘 (Pole / Hint)
            Vector3 pole = leg.shin.position + transform.forward + Vector3.up * 0.2f;
            SolveTwoBone(leg.thigh, leg.shin, leg.end, target, pole);
        }

        // 2본 IK: 세 관절(a 허벅지, b 무릎, c 발목)과 목표점으로 무릎 위치를 코사인 법칙으로 계산
        public static void SolveTwoBone(Transform a, Transform b, Transform c, Vector3 target, Vector3 pole)
        {
            Vector3 aPos = a.position;
            float lab = Vector3.Distance(aPos, b.position);
            float lbc = Vector3.Distance(b.position, c.position);
            Vector3 toTarget = target - aPos;
            float lat = Mathf.Clamp(toTarget.magnitude, 0.01f, lab + lbc - 0.001f); // 닿지 않으면 쭉 뻗음
            Vector3 dir = toTarget.normalized;

            Vector3 poleDir = Vector3.ProjectOnPlane(pole - aPos, dir);
            if (poleDir.sqrMagnitude < 1e-6f) poleDir = Vector3.ProjectOnPlane(a.forward, dir);
            poleDir.Normalize();

            // 허벅지와 (엉덩이→목표) 사이 각도
            float cosA = Mathf.Clamp((lab * lab + lat * lat - lbc * lbc) / (2f * lab * lat), -1f, 1f);
            float angleA = Mathf.Acos(cosA) * Mathf.Rad2Deg;
            Vector3 axis = Vector3.Cross(dir, poleDir).normalized;
            Vector3 knee = aPos + Quaternion.AngleAxis(angleA, axis) * dir * lab;

            a.rotation = Quaternion.FromToRotation(b.position - aPos, knee - aPos) * a.rotation;
            b.rotation = Quaternion.FromToRotation(c.position - b.position, aPos + dir * lat - b.position) * b.rotation;
        }

        private Vector3 Ground(float x)
        {
            var origin = new Vector3(x, 6f, transform.position.z);
            if (Physics.Raycast(origin, Vector3.down, out var hit, 20f, groundMask)) return hit.point;
            return new Vector3(x, 0f, transform.position.z);
        }
    }
}
