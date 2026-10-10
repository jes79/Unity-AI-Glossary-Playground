using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.EnemyAI
{
    // 시야 판정 (Field of View): 3가지를 모두 통과해야 "보인다"
    //  1) 거리 ≤ viewRadius   2) 정면과의 각도 ≤ viewAngle/2   3) 사이에 벽(obstacleMask)이 없음 (Raycast)
    // 시야 부채꼴은 여러 개의 레이를 쏴서 벽에 잘린 모양 그대로 메시로 그림
    public class FieldOfView : MonoBehaviour
    {
        [Header("시야")]
        [SerializeField] private float viewRadius = 10f;
        [SerializeField, Range(0f, 360f)] private float viewAngle = 90f;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private bool obstaclesBlock = true;
        [SerializeField] private Transform target;

        [Header("감시 회전")]
        [SerializeField] private bool sweep = true;
        [SerializeField] private float sweepAngle = 70f;
        [SerializeField] private float sweepSpeed = 0.6f;

        [Header("표시")]
        [SerializeField] private MeshFilter viewMeshFilter;
        [SerializeField] private Renderer viewRenderer;
        [SerializeField] private Renderer body;
        [SerializeField] private int rayCount = 60;
        [SerializeField] private Color idleColor = new Color(1f, 0.9f, 0.3f, 0.25f);
        [SerializeField] private Color alertColor = new Color(1f, 0.2f, 0.2f, 0.4f);

        private Mesh mesh;
        private float baseYaw;
        private float sweepTime;
        private float seenTime;

        public bool CanSee { get; private set; }

        private void Awake()
        {
            mesh = new Mesh { name = "ViewMesh" };
            if (viewMeshFilter != null) viewMeshFilter.mesh = mesh;
            baseYaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.B)) obstaclesBlock = !obstaclesBlock;
            if (DemoInput.KeyDown(Key.T)) sweep = !sweep;

            var info = Check(out float dist, out float angle, out bool blocked);
            CanSee = info;

            if (CanSee)
            {
                seenTime += Time.deltaTime;
                // 발견하면 대상을 향해 돌아봄
                Vector3 d = target.position - transform.position;
                d.y = 0f;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), Time.deltaTime * 4f);
            }
            else
            {
                seenTime = 0f;
                if (sweep)
                {
                    sweepTime += Time.deltaTime * sweepSpeed;
                    float yaw = baseYaw + Mathf.Sin(sweepTime) * sweepAngle;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, yaw, 0f), Time.deltaTime * 3f);
                }
            }

            if (viewRenderer != null) viewRenderer.material.SetColor("_BaseColor", CanSee ? alertColor : idleColor);
            if (body != null) body.material.color = CanSee ? new Color(1f, 0.2f, 0.2f) : new Color(0.55f, 0.55f, 0.6f);

            GlossaryInfoPanel.Status =
                $"보임: {(CanSee ? "<color=#FF6060>발견!</color>" : "아니오")}   (발견 {seenTime:0.0}초)\n" +
                $"① 거리 {dist:0.0} / {viewRadius:0}m {(dist <= viewRadius ? "✔" : "✘")}   " +
                $"② 각도 {angle:0} / {viewAngle * 0.5f:0}° {(angle <= viewAngle * 0.5f ? "✔" : "✘")}   " +
                $"③ 가림 {(obstaclesBlock ? (blocked ? "벽에 막힘 ✘" : "없음 ✔") : "검사 안 함")}\n" +
                $"벽이 시야를 막음: {(obstaclesBlock ? "ON" : "OFF")}   감시 회전: {(sweep ? "ON" : "OFF")}";
        }

        private void LateUpdate() => DrawViewMesh();

        private bool Check(out float dist, out float angle, out bool blocked)
        {
            dist = float.MaxValue;
            angle = 180f;
            blocked = false;
            if (target == null) return false;

            Vector3 eye = transform.position;
            Vector3 to = target.position - eye;
            to.y = 0f;
            dist = to.magnitude;
            angle = Vector3.Angle(transform.forward, to);

            if (dist > viewRadius) return false;                     // 1) 거리
            if (angle > viewAngle * 0.5f) return false;              // 2) 시야각
            blocked = Physics.Raycast(eye, to.normalized, dist, obstacleMask); // 3) 벽 가림
            if (obstaclesBlock && blocked) return false;
            return true;
        }

        // 시야 부채꼴 메시: 로컬 좌표로 꼭짓점 생성 (중심 + 레이 끝점들)
        private void DrawViewMesh()
        {
            if (mesh == null) return;
            int n = Mathf.Max(2, rayCount);
            var verts = new Vector3[n + 2];
            var tris = new int[n * 3];
            verts[0] = Vector3.zero;
            float half = viewAngle * 0.5f;
            Vector3 origin = transform.position;
            for (int i = 0; i <= n; i++)
            {
                float a = -half + viewAngle * i / n;
                Vector3 localDir = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
                Vector3 worldDir = transform.TransformDirection(localDir);
                float len = viewRadius;
                if (obstaclesBlock && Physics.Raycast(origin, worldDir, out var hit, viewRadius, obstacleMask)) len = hit.distance;
                verts[i + 1] = localDir * len;
                if (i < n)
                {
                    tris[i * 3] = 0;
                    tris[i * 3 + 1] = i + 1;
                    tris[i * 3 + 2] = i + 2;
                }
            }
            mesh.Clear();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
        }
    }
}
