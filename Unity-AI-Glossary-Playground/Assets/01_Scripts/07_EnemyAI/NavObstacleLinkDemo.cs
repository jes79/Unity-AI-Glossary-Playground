using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace GlossaryDemo.EnemyAI
{
    // NavMesh Obstacle(Carve) + NavMesh Link(점프 연결) 데모
    // - 움직이는 상자: carving ON 이면 NavMesh 에 구멍을 내서 경로가 상자를 돌아감
    //                 OFF 이면 회피(avoidance)만 하므로 상자에 막혀 밀리거나 비비게 됨
    // - 높은 단상: 경사가 없어 걸어서 못 올라감 → NavMeshLink 로만 연결 (포물선 점프로 이동)
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavObstacleLinkDemo : MonoBehaviour
    {
        [SerializeField] private NavMeshObstacle movingCrate;
        [SerializeField] private Vector3 crateA = new Vector3(-2f, 0.75f, 0f);
        [SerializeField] private Vector3 crateB = new Vector3(2f, 0.75f, 0f);
        [SerializeField] private float crateSpeed = 0.5f;
        [SerializeField] private Transform[] goals;        // 1, 2, 3 키로 보내는 목적지
        [SerializeField] private Transform clickMarker;
        [SerializeField] private LineRenderer pathLine;
        [SerializeField] private bool parabolaJump = true;  // 링크 통과를 포물선 점프로 직접 처리
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float jumpDuration = 0.6f;

        private NavMeshAgent agent;
        private bool jumping;
        private bool crateMoving = true;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.autoTraverseOffMeshLink = false; // 링크 통과를 스크립트가 직접 처리
        }

        private void Update()
        {
            // 상자 왕복
            if (movingCrate != null && crateMoving)
            {
                float t = Mathf.PingPong(Time.time * crateSpeed, 1f);
                movingCrate.transform.position = Vector3.Lerp(crateA, crateB, Mathf.SmoothStep(0f, 1f, t));
            }

            if (DemoInput.KeyDown(Key.C) && movingCrate != null) movingCrate.carving = !movingCrate.carving;
            if (DemoInput.KeyDown(Key.M)) crateMoving = !crateMoving;
            if (DemoInput.KeyDown(Key.J)) parabolaJump = !parabolaJump;
            for (int i = 0; i < goals.Length && i < 3; i++)
                if (DemoInput.KeyDown(Key.Digit1 + i)) SetGoal(goals[i].position);

            // 클릭한 곳으로 이동
            if (DemoInput.LeftClick && Physics.Raycast(DemoInput.MouseRay(), out var hit, 200f))
                SetGoal(hit.point);

            // 링크 위에 올라서면 직접 이동시킴
            if (agent.isOnOffMeshLink && !jumping)
                StartCoroutine(TraverseLink());

            if (pathLine != null)
            {
                pathLine.enabled = agent.hasPath;
                if (agent.hasPath)
                {
                    var c = agent.path.corners;
                    pathLine.positionCount = c.Length;
                    for (int i = 0; i < c.Length; i++) pathLine.SetPosition(i, c[i] + Vector3.up * 0.15f);
                }
            }

            GlossaryInfoPanel.Status =
                $"상자 Carve: {(movingCrate != null && movingCrate.carving ? "<color=#7CFC8A>ON</color> (NavMesh 에 구멍 → 돌아감)" : "<color=#FF7A7A>OFF</color> (회피만 → 막힘)")}\n" +
                $"링크 이동: {(parabolaJump ? "포물선 점프" : "직선 이동")}   링크 위: {agent.isOnOffMeshLink}\n" +
                $"경로 상태: {agent.pathStatus}   남은 거리 {(agent.hasPath ? agent.remainingDistance : 0f):0.0}m";
        }

        private void SetGoal(Vector3 pos)
        {
            if (!agent.isOnNavMesh) return;
            agent.SetDestination(pos);
            if (clickMarker != null) clickMarker.position = pos + Vector3.up * 0.05f;
        }

        // NavMeshLink 통과: 시작점 → 끝점을 포물선(또는 직선)으로 이동한 뒤 CompleteOffMeshLink
        private IEnumerator TraverseLink()
        {
            jumping = true;
            var data = agent.currentOffMeshLinkData;
            Vector3 start = transform.position;
            Vector3 end = data.endPos + Vector3.up * agent.baseOffset;
            float duration = parabolaJump ? jumpDuration : Vector3.Distance(start, end) / agent.speed;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float h = parabolaJump ? jumpHeight * 4f * t * (1f - t) : 0f; // 0→1→0 포물선
                transform.position = Vector3.Lerp(start, end, t) + Vector3.up * h;
                yield return null;
            }
            agent.CompleteOffMeshLink();
            jumping = false;
        }
    }
}
