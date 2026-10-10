using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.EnemyAI
{
    // 웨이포인트 순찰: 지정 지점들을 순서대로 이동하고, 각 지점에서 waitTime 초 대기
    public class WaypointPatrol : MonoBehaviour
    {
        public enum Mode { Loop, PingPong, Random }

        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float waitTime = 2f;   // 각 지점에서 대기
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float turnSpeed = 8f;
        [SerializeField] private Mode mode = Mode.Loop;
        [SerializeField] private TextMeshPro label;
        [SerializeField] private Renderer[] waypointRenderers;

        private int index;
        private int direction = 1;
        private float waitTimer;
        private bool waiting;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.M)) mode = (Mode)(((int)mode + 1) % 3);
            if (DemoInput.KeyDown(Key.Equals) || DemoInput.KeyDown(Key.NumpadPlus)) waitTime += 0.5f;
            if (DemoInput.KeyDown(Key.Minus) || DemoInput.KeyDown(Key.NumpadMinus)) waitTime = Mathf.Max(0f, waitTime - 0.5f);
            if (waypoints == null || waypoints.Length == 0) return;

            if (waiting)
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    waiting = false;
                    index = NextIndex();
                }
            }
            else
            {
                Vector3 target = waypoints[index].position;
                target.y = transform.position.y;
                Vector3 d = target - transform.position;
                if (d.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), Time.deltaTime * turnSpeed);
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                if (d.magnitude < 0.05f)
                {
                    waiting = true;          // 도착 → 대기 시작
                    waitTimer = waitTime;
                }
            }

            // 현재 목표 지점 강조
            if (waypointRenderers != null)
                for (int i = 0; i < waypointRenderers.Length; i++)
                    waypointRenderers[i].material.color = i == index ? new Color(1f, 0.85f, 0.2f) : new Color(0.35f, 0.7f, 1f);

            if (label != null) label.text = waiting ? $"대기 {waitTimer:0.0}s" : $"→ WP{index + 1}";

            GlossaryInfoPanel.Status = $"모드: {mode}   대기 시간: {waitTime:0.0}초\n" +
                                       $"목표: 웨이포인트 {index + 1}/{waypoints.Length}   {(waiting ? $"대기 중 ({waitTimer:0.0}초 남음)" : "이동 중")}";
        }

        private int NextIndex()
        {
            int n = waypoints.Length;
            switch (mode)
            {
                case Mode.Loop: return (index + 1) % n;                  // 1→2→3→4→1
                case Mode.PingPong:                                       // 1→2→3→4→3→2→1
                    if (index + direction >= n || index + direction < 0) direction = -direction;
                    return index + direction;
                default:
                    int r = Random.Range(0, n - 1);
                    return r >= index ? r + 1 : r;                        // 같은 지점은 제외
            }
        }
    }
}
