using System.Collections.Generic;
using UnityEngine;

namespace GlossaryDemo.Movement
{
    // 가속/감속 이동: 목표 속도까지 accelerationTime 초에 걸쳐 빨라지고, 손을 떼면 decelerationTime 초에 걸쳐 멈춤
    // useAcceleration = false 면 즉시 최고 속도 / 즉시 정지 (비교용)
    // 아래쪽 LineRenderer 에 최근 3초 속도 그래프를 그림
    public class AccelerationRunner : MonoBehaviour
    {
        [SerializeField] private bool useAcceleration = true;
        [SerializeField] private float maxSpeed = 8f;
        [SerializeField, Range(0f, 2f)] private float accelerationTime = 0.2f; // 0 → 최고 속도까지 걸리는 시간
        [SerializeField, Range(0f, 2f)] private float decelerationTime = 0.1f; // 최고 속도 → 0 까지 걸리는 시간
        [SerializeField] private float minX = -8.5f, maxX = 8.5f;

        [Header("속도 그래프")]
        [SerializeField] private LineRenderer graph;
        [SerializeField] private Vector2 graphOrigin = new Vector2(-8.5f, -1f);
        [SerializeField] private Vector2 graphSize = new Vector2(17f, 1.2f);
        [SerializeField] private float graphSeconds = 3f;

        private float speed; // 현재 속도 (부호 = 방향)
        private readonly Queue<float> samples = new Queue<float>();

        public float Speed => speed;
        public bool UseAcceleration => useAcceleration;
        public float AccelerationTime => accelerationTime;
        public float DecelerationTime => decelerationTime;

        public void SetTimes(float accel, float decel)
        {
            accelerationTime = accel;
            decelerationTime = decel;
        }

        private void Update()
        {
            float target = DemoInput.Move.x * maxSpeed;

            if (useAcceleration)
            {
                // 입력이 있고 같은 방향이면 가속, 없거나 반대 방향이면 감속
                bool accelerating = Mathf.Abs(target) > 0.01f && Mathf.Sign(target) == Mathf.Sign(speed == 0f ? target : speed);
                float time = accelerating ? accelerationTime : decelerationTime;
                float rate = time > 0f ? maxSpeed / time : float.PositiveInfinity; // 초당 속도 변화량
                speed = Mathf.MoveTowards(speed, target, rate * Time.deltaTime);
            }
            else
            {
                speed = target; // 즉시 반응
            }

            Vector3 p = transform.position + Vector3.right * speed * Time.deltaTime;
            if (p.x < minX || p.x > maxX) { p.x = Mathf.Clamp(p.x, minX, maxX); speed = 0f; }
            transform.position = p;

            DrawGraph();
        }

        private void DrawGraph()
        {
            if (graph == null) return;
            int maxSamples = Mathf.Max(2, Mathf.RoundToInt(graphSeconds * 60f));
            samples.Enqueue(Mathf.Abs(speed) / maxSpeed);
            while (samples.Count > maxSamples) samples.Dequeue();

            graph.positionCount = samples.Count;
            int i = 0;
            foreach (float s in samples)
            {
                float x = graphOrigin.x + graphSize.x * i / (maxSamples - 1);
                graph.SetPosition(i++, new Vector3(x, graphOrigin.y + s * graphSize.y, 0f));
            }
        }
    }
}
