using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Patterns
{
    // Raycast 가 어디로 나가는지 Debug.DrawRay / Debug.DrawLine 으로 시각화
    // - Debug.DrawRay 는 Scene 뷰(항상)와 Game 뷰(Gizmos 버튼 ON)에만 보임. 빌드에는 안 나옴
    // - 비교용 LineRenderer 는 어디서나 보임
    public class RaycastDebugTurret : MonoBehaviour
    {
        [SerializeField] private Transform head;
        [SerializeField] private Transform muzzle;
        [SerializeField] private float maxDistance = 15f;
        [SerializeField] private LayerMask hitMask;
        [SerializeField] private bool autoSweep;
        [SerializeField] private float sweepSpeed = 40f;

        [Header("런타임 표시")]
        [SerializeField] private LineRenderer line;
        [SerializeField] private Transform hitMarker;
        [SerializeField] private bool showLineRenderer = true;

        private string lastHitName;
        private Plane ground = new Plane(Vector3.up, Vector3.zero);

        public string LastHitName => lastHitName;
        public bool AutoSweep => autoSweep;
        public bool ShowLine => showLineRenderer;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.A)) ToggleSweep();
            if (DemoInput.KeyDown(Key.L)) ToggleLine();

            Aim();

            Vector3 origin = muzzle.position;
            Vector3 dir = head.forward;
            bool hit = Physics.Raycast(origin, dir, out RaycastHit info, maxDistance, hitMask);

            if (hit)
            {
                // 맞은 지점까지 빨간 선 + 맞은 면의 법선(노멀)을 파란 선으로
                Debug.DrawLine(origin, info.point, Color.red);
                Debug.DrawRay(info.point, info.normal, Color.cyan);
                // 클릭하면 콘솔에 로그
                if (Clicked) Debug.Log($"[Raycast] {info.collider.name} 명중  거리 {info.distance:0.00}  지점 {info.point}");
            }
            else
            {
                // 아무것도 안 맞으면 최대 거리까지 초록 선
                Debug.DrawRay(origin, dir * maxDistance, Color.green);
                if (Clicked) Debug.Log("[Raycast] 아무것도 맞지 않음");
            }

            string hitName = hit ? info.collider.name : null;
            if (hitName != lastHitName && hitName != null) Debug.Log($"[Raycast] 새 대상: {hitName}");
            lastHitName = hitName;

            // 런타임 LineRenderer 로 같은 선
            line.enabled = showLineRenderer;
            if (showLineRenderer)
            {
                line.SetPosition(0, origin);
                line.SetPosition(1, hit ? info.point : origin + dir * maxDistance);
                line.startColor = line.endColor = hit ? Color.red : Color.green;
            }
            hitMarker.gameObject.SetActive(hit);
            if (hit) hitMarker.position = info.point;

            GlossaryInfoPanel.Status = $"대상: {(hit ? $"{info.collider.name}  거리 {info.distance:0.00}" : "없음")}\n" +
                                       $"조준: {(autoSweep ? "자동 회전" : "마우스")}   LineRenderer {(showLineRenderer ? "ON" : "OFF (Gizmos 버튼 켜야 선이 보임)")}";
        }

        // UI 버튼을 누른 클릭은 제외
        private static bool Clicked => DemoInput.LeftClick &&
            (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject());

        private void Aim()
        {
            if (autoSweep)
            {
                head.rotation = Quaternion.Euler(0f, Mathf.Sin(Time.time * sweepSpeed * Mathf.Deg2Rad) * 80f, 0f);
                return;
            }
            // 마우스가 가리키는 바닥 지점을 향해 회전
            Ray ray = DemoInput.MouseRay();
            if (ground.Raycast(ray, out float enter))
            {
                Vector3 p = ray.GetPoint(enter);
                Vector3 to = p - head.position;
                to.y = 0f;
                if (to.sqrMagnitude > 0.01f) head.rotation = Quaternion.LookRotation(to);
            }
        }

        public void ToggleSweep() => autoSweep = !autoSweep;
        public void ToggleLine() => showLineRenderer = !showLineRenderer;

        // 로그 레벨 버튼
        public void LogInfo() => Debug.Log("Debug.Log : 일반 정보 (흰색)");
        public void LogWarning() => Debug.LogWarning("Debug.LogWarning : 주의 (노란색)");
        public void LogError() => Debug.LogError("Debug.LogError : 오류 (빨간색) — 데모용, 실제 오류 아님");
    }
}
