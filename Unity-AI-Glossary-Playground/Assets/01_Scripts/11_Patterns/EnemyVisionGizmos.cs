using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // 적의 시야 범위(부채꼴)와 공격 범위(원)를 OnDrawGizmos 로 Scene 뷰에 그림
    // Gizmos 는 에디터 전용 보조선 → 빌드에는 안 나옴. Game 뷰에서는 오른쪽 위 'Gizmos' 버튼을 켜야 보임
    // 비교를 위해 같은 모양을 LineRenderer 로도 그릴 수 있음 (빌드에서도 보이는 방식)
    public class EnemyVisionGizmos : MonoBehaviour
    {
        [Header("범위 (요청 예문: 시야 범위 + 공격 범위)")]
        [SerializeField, Range(1f, 15f)] private float viewRadius = 6f;
        [SerializeField, Range(10f, 360f)] private float viewAngle = 90f;
        [SerializeField, Range(0.5f, 5f)] private float attackRange = 2f;
        [SerializeField] private float scanSpeed = 40f; // 좌우로 둘러보는 속도
        [SerializeField] private float scanAngle = 60f;

        [Header("대상")]
        [SerializeField] private Transform player;
        [SerializeField] private Renderer body;
        [SerializeField] private Material idleMat, seenMat, attackMat;

        [Header("런타임 표시 (LineRenderer)")]
        [SerializeField] private LineRenderer viewLine;
        [SerializeField] private LineRenderer attackLine;
        [SerializeField] private LineRenderer targetLine;

        public static bool ShowRuntimeLines = true;

        private float baseYaw;
        private bool canSee, inAttack;

        public bool CanSee => canSee;
        public bool InAttack => inAttack;

        private void Start() => baseYaw = transform.eulerAngles.y;

        private void Update()
        {
            transform.rotation = Quaternion.Euler(0f, baseYaw + Mathf.Sin(Time.time * scanSpeed * Mathf.Deg2Rad) * scanAngle, 0f);

            Vector3 to = player.position - transform.position;
            to.y = 0f;
            float dist = to.magnitude;
            canSee = dist <= viewRadius && Vector3.Angle(transform.forward, to) <= viewAngle * 0.5f;
            inAttack = canSee && dist <= attackRange;
            body.sharedMaterial = inAttack ? attackMat : canSee ? seenMat : idleMat;

            DrawRuntimeLines();
        }

        // ── Gizmos: 항상 그림 (선택하지 않아도) ──
        private void OnDrawGizmos()
        {
            Vector3 c = transform.position + Vector3.up * 0.05f;

            // 시야 원 (노랑) + 공격 범위 원 (빨강)
            Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.6f);
            Gizmos.DrawWireSphere(c, viewRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(c, attackRange);

            // 시야 부채꼴 경계선
            Gizmos.color = Color.yellow;
            Vector3 left = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * transform.forward;
            Vector3 right = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * transform.forward;
            Gizmos.DrawLine(c, c + left * viewRadius);
            Gizmos.DrawLine(c, c + right * viewRadius);
            DrawArc(c, viewRadius, -viewAngle * 0.5f, viewAngle * 0.5f);

            // 플레이어가 보이면 초록 선
            if (Application.isPlaying && canSee && player != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(c, player.position);
            }
        }

        // ── 선택했을 때만 추가로 그림 ──
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.15f);
            Gizmos.DrawSphere(transform.position, attackRange); // 채워진 구
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, $"시야 {viewRadius}m / {viewAngle}°\n공격 {attackRange}m");
#endif
        }

        private void DrawArc(Vector3 c, float r, float fromDeg, float toDeg)
        {
            const int steps = 24;
            Vector3 prev = c + Quaternion.Euler(0f, fromDeg, 0f) * transform.forward * r;
            for (int i = 1; i <= steps; i++)
            {
                float a = Mathf.Lerp(fromDeg, toDeg, i / (float)steps);
                Vector3 p = c + Quaternion.Euler(0f, a, 0f) * transform.forward * r;
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }

        // 같은 모양을 LineRenderer 로 (게임 화면·빌드에서도 보임)
        private void DrawRuntimeLines()
        {
            viewLine.enabled = attackLine.enabled = ShowRuntimeLines;
            targetLine.enabled = ShowRuntimeLines && canSee;
            if (!ShowRuntimeLines) return;

            Vector3 c = transform.position + Vector3.up * 0.05f;
            const int steps = 24;
            viewLine.positionCount = steps + 3;
            viewLine.SetPosition(0, c);
            for (int i = 0; i <= steps; i++)
            {
                float a = Mathf.Lerp(-viewAngle * 0.5f, viewAngle * 0.5f, i / (float)steps);
                viewLine.SetPosition(i + 1, c + Quaternion.Euler(0f, a, 0f) * transform.forward * viewRadius);
            }
            viewLine.SetPosition(steps + 2, c);

            const int circle = 40;
            attackLine.positionCount = circle + 1;
            for (int i = 0; i <= circle; i++)
            {
                float a = i / (float)circle * Mathf.PI * 2f;
                attackLine.SetPosition(i, c + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * attackRange);
            }

            if (canSee)
            {
                targetLine.positionCount = 2;
                targetLine.SetPosition(0, c + Vector3.up);
                targetLine.SetPosition(1, player.position);
            }
        }
    }
}
