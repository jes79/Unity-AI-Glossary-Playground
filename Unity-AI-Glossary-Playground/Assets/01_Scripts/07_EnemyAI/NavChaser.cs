using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace GlossaryDemo.EnemyAI
{
    // NavMesh Agent 로 플레이어 추적
    // NavMesh(구워진 걸을 수 있는 영역) 위에서 agent.SetDestination 만 하면 벽을 돌아가는 경로를 알아서 계산함
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavChaser : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float repathInterval = 0.2f; // 목적지 갱신 주기 (매 프레임 할 필요 없음)
        [SerializeField] private LineRenderer pathLine;
        [SerializeField] private bool chasing = true;
        [SerializeField] private bool showPath = true;

        private NavMeshAgent agent;
        private float timer;

        private void Awake() => agent = GetComponent<NavMeshAgent>();

        private void Update()
        {
            if (DemoInput.JumpPressed) chasing = !chasing;
            if (DemoInput.KeyDown(Key.P)) showPath = !showPath;

            timer -= Time.deltaTime;
            if (timer <= 0f && agent.isOnNavMesh)
            {
                timer = repathInterval;
                agent.isStopped = !chasing;
                if (chasing && target != null) agent.SetDestination(target.position);
            }

            // 계산된 경로의 꺾이는 점(corners)을 선으로 표시
            if (pathLine != null)
            {
                pathLine.enabled = showPath && chasing && agent.hasPath;
                if (pathLine.enabled)
                {
                    var corners = agent.path.corners;
                    pathLine.positionCount = corners.Length;
                    for (int i = 0; i < corners.Length; i++) pathLine.SetPosition(i, corners[i] + Vector3.up * 0.15f);
                }
            }

            GlossaryInfoPanel.Status = !agent.isOnNavMesh
                ? "<color=#FF7A7A>에이전트가 NavMesh 위에 없음 (NavMesh 굽기 확인)</color>"
                : $"추적: {(chasing ? "ON" : "OFF")}   경로 상태: {agent.pathStatus}\n" +
                  $"남은 거리: {(agent.hasPath ? agent.remainingDistance : 0f):0.0}m   꺾이는 점: {(agent.hasPath ? agent.path.corners.Length : 0)}개\n" +
                  $"속도 {agent.velocity.magnitude:0.0}/{agent.speed:0.0} m/s   정지 거리 {agent.stoppingDistance:0.0}m";
        }
    }
}
