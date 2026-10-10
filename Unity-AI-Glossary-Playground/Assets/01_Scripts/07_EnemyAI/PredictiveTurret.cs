using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.EnemyAI
{
    // 투사체 예측 조준: 탄이 날아가는 동안 대상이 움직일 거리를 계산해 "만날 지점"을 조준
    //  대상 위치 P, 속도 V, 포탑 위치 S, 탄속 s 일 때 |P + V·t − S| = s·t 를 만족하는 t 를 구함 (2차 방정식)
    //  조준점 = P + V·t
    public class PredictiveTurret : MonoBehaviour
    {
        [SerializeField] private AITopDownPlayer target;
        [SerializeField] private Transform head;            // 회전하는 포신
        [SerializeField] private Transform muzzle;
        [SerializeField] private AIBullet bulletPrefab;
        [SerializeField] private Transform aimMarker;       // 예측 조준점 표시
        [SerializeField] private float bulletSpeed = 14f;
        [SerializeField] private float fireInterval = 0.6f;
        [SerializeField] private bool predictive = true;

        [Header("자동 원 운동 (비교용)")]
        [SerializeField] private bool autoCircle = true;
        [SerializeField] private float circleRadius = 8f;

        private float fireTimer;
        private int[] shots = new int[2], hits = new int[2]; // [0] 직접 조준, [1] 예측 조준

        private void Update()
        {
            if (DemoInput.KeyDown(Key.P)) predictive = !predictive;
            if (DemoInput.KeyDown(Key.C)) autoCircle = !autoCircle;
            if (DemoInput.KeyDown(Key.R)) { shots = new int[2]; hits = new int[2]; }

            // 자동 원 운동: 포탑 둘레를 일정 속도로 돎 (예측 효과를 보기 쉬움)
            target.useOverride = autoCircle;
            if (autoCircle)
            {
                Vector3 r = target.transform.position - transform.position;
                r.y = 0f;
                Vector3 tangent = Vector3.Cross(Vector3.up, r.normalized);
                float radial = (circleRadius - r.magnitude) * 0.5f; // 반지름 유지
                target.overrideDirection = (tangent + r.normalized * radial).normalized;
            }

            Vector3 aim = predictive ? PredictAim(out _) : target.transform.position;
            Vector3 flat = aim - head.position;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.01f) head.rotation = Quaternion.Slerp(head.rotation, Quaternion.LookRotation(flat), Time.deltaTime * 15f);
            if (aimMarker != null) aimMarker.position = new Vector3(aim.x, 0.05f, aim.z);

            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                fireTimer = fireInterval;
                int mode = predictive ? 1 : 0;
                Vector3 start = muzzle.position;
                Vector3 dir = aim - start;
                dir.y = 0f;
                shots[mode]++;
                var b = AIBullet.Spawn(bulletPrefab, start, dir.normalized * bulletSpeed, target.transform, _ => hits[mode]++, 2.5f);
                b.hitRadius = 0.7f;
            }

            GlossaryInfoPanel.Status =
                $"조준 방식: <b>{(predictive ? "예측 조준" : "현재 위치 조준")}</b>   자동 원 운동: {(autoCircle ? "ON" : "OFF")}\n" +
                $"대상 속도 {target.Velocity.magnitude:0.0} m/s   탄속 {bulletSpeed:0} m/s\n" +
                $"명중률  현재 위치: {Rate(0)}   예측: {Rate(1)}";
        }

        private string Rate(int i) => shots[i] == 0 ? "-" : $"{hits[i]}/{shots[i]} ({hits[i] * 100f / shots[i]:0}%)";

        // 요격 지점 계산
        private Vector3 PredictAim(out float time)
        {
            Vector3 P = target.transform.position, V = target.Velocity, S = muzzle.position;
            P.y = S.y;
            Vector3 D = P - S;
            float a = Vector3.Dot(V, V) - bulletSpeed * bulletSpeed;
            float b = 2f * Vector3.Dot(V, D);
            float c = Vector3.Dot(D, D);

            time = -1f;
            if (Mathf.Abs(a) < 0.0001f)
            {
                if (Mathf.Abs(b) > 0.0001f) time = -c / b; // 대상 속도 = 탄속인 특수한 경우
            }
            else
            {
                float disc = b * b - 4f * a * c;
                if (disc >= 0f)
                {
                    float sq = Mathf.Sqrt(disc);
                    float t1 = (-b - sq) / (2f * a), t2 = (-b + sq) / (2f * a);
                    time = Mathf.Min(t1, t2) > 0f ? Mathf.Min(t1, t2) : Mathf.Max(t1, t2); // 가장 빠른 양수 해
                }
            }
            if (time <= 0f) return P; // 해가 없으면(따라잡을 수 없음) 그냥 현재 위치
            return P + V * time;
        }
    }
}
