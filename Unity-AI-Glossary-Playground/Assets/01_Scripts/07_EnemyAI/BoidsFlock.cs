using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.EnemyAI
{
    // 플로킹(Boids): 각 물고기가 주변 이웃만 보고 3가지 규칙을 합쳐서 움직임
    //  1) 분리(Separation): 너무 가까운 이웃에게서 멀어짐
    //  2) 정렬(Alignment) : 이웃들의 평균 방향을 따라감
    //  3) 응집(Cohesion)  : 이웃들의 중심으로 모임
    // + 수조 경계 밖으로 나가지 않게 되돌리는 힘
    public class BoidsFlock : MonoBehaviour
    {
        [Header("무리")]
        [SerializeField] private Transform fishPrefab;
        [SerializeField] private int count = 80;
        [SerializeField] private Vector3 bounds = new Vector3(22f, 8f, 12f);

        [Header("규칙 가중치")]
        [SerializeField] private float separationWeight = 1.8f;
        [SerializeField] private float alignmentWeight = 1.0f;
        [SerializeField] private float cohesionWeight = 1.0f;
        [SerializeField] private bool useSeparation = true, useAlignment = true, useCohesion = true;

        [Header("감지 / 속도")]
        [SerializeField] private float neighborRadius = 2.5f;
        [SerializeField] private float separationRadius = 0.9f;
        [SerializeField] private float minSpeed = 2f;
        [SerializeField] private float maxSpeed = 4.5f;
        [SerializeField] private float maxSteer = 4f;

        private Transform[] fish;
        private Vector3[] velocity;
        private Vector3[] steer;

        private void Start() => Spawn();

        private void Spawn()
        {
            if (fish != null) foreach (var f in fish) if (f != null) Destroy(f.gameObject);
            fish = new Transform[count];
            velocity = new Vector3[count];
            steer = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                Vector3 p = transform.position + new Vector3(Random.Range(-0.5f, 0.5f) * bounds.x, Random.Range(-0.5f, 0.5f) * bounds.y, Random.Range(-0.5f, 0.5f) * bounds.z);
                fish[i] = Instantiate(fishPrefab, p, Quaternion.identity, transform);
                velocity[i] = Random.onUnitSphere * Random.Range(minSpeed, maxSpeed);
            }
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) useSeparation = !useSeparation;
            if (DemoInput.KeyDown(Key.Digit2)) useAlignment = !useAlignment;
            if (DemoInput.KeyDown(Key.Digit3)) useCohesion = !useCohesion;
            if (DemoInput.KeyDown(Key.R)) Spawn();

            float dt = Time.deltaTime;
            int n = fish.Length;
            float nr2 = neighborRadius * neighborRadius, sr2 = separationRadius * separationRadius;
            float avgNeighbors = 0f;

            // 모든 물고기의 조향(steer)을 먼저 계산하고, 그 다음 한꺼번에 이동 (순서 영향 제거)
            for (int i = 0; i < n; i++)
            {
                Vector3 pos = fish[i].position;
                Vector3 sep = Vector3.zero, ali = Vector3.zero, coh = Vector3.zero;
                int neighbors = 0;
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    Vector3 d = pos - fish[j].position;
                    float d2 = d.sqrMagnitude;
                    if (d2 > nr2) continue;
                    neighbors++;
                    ali += velocity[j];
                    coh += fish[j].position;
                    if (d2 < sr2 && d2 > 0.0001f) sep += d / d2; // 가까울수록 강하게 밀어냄
                }
                avgNeighbors += neighbors;

                Vector3 force = Vector3.zero;
                if (neighbors > 0)
                {
                    if (useSeparation) force += SteerTowards(sep, velocity[i]) * separationWeight;
                    if (useAlignment) force += SteerTowards(ali / neighbors, velocity[i]) * alignmentWeight;
                    if (useCohesion) force += SteerTowards(coh / neighbors - pos, velocity[i]) * cohesionWeight;
                }
                force += BoundsForce(pos) * 3f;
                steer[i] = force;
            }

            for (int i = 0; i < n; i++)
            {
                Vector3 v = velocity[i] + steer[i] * dt;
                float speed = Mathf.Clamp(v.magnitude, minSpeed, maxSpeed);
                v = v.normalized * speed;
                velocity[i] = v;
                fish[i].position += v * dt;
                fish[i].rotation = Quaternion.Slerp(fish[i].rotation, Quaternion.LookRotation(v), dt * 6f);
            }

            GlossaryInfoPanel.Status =
                $"물고기 {n}마리   평균 이웃 수 {avgNeighbors / Mathf.Max(1, n):0.0}\n" +
                $"1 분리 {(useSeparation ? "ON" : "<color=#FF7A7A>OFF</color>")} (×{separationWeight})   " +
                $"2 정렬 {(useAlignment ? "ON" : "<color=#FF7A7A>OFF</color>")} (×{alignmentWeight})   " +
                $"3 응집 {(useCohesion ? "ON" : "<color=#FF7A7A>OFF</color>")} (×{cohesionWeight})";
        }

        // 원하는 방향(desired)으로 최대 속도로 가기 위한 조향력 (Reynolds steering)
        private Vector3 SteerTowards(Vector3 desired, Vector3 current)
        {
            if (desired.sqrMagnitude < 0.0001f) return Vector3.zero;
            Vector3 s = desired.normalized * maxSpeed - current;
            return Vector3.ClampMagnitude(s, maxSteer);
        }

        // 수조 경계에 가까워지면 안쪽으로 되돌림
        private Vector3 BoundsForce(Vector3 pos)
        {
            Vector3 local = pos - transform.position;
            Vector3 half = bounds * 0.5f;
            Vector3 f = Vector3.zero;
            for (int a = 0; a < 3; a++)
            {
                float margin = 1.5f;
                if (local[a] > half[a] - margin) f[a] = -(local[a] - (half[a] - margin));
                else if (local[a] < -half[a] + margin) f[a] = (-half[a] + margin) - local[a];
            }
            return f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.4f);
            Gizmos.DrawWireCube(transform.position, bounds);
        }
    }
}
