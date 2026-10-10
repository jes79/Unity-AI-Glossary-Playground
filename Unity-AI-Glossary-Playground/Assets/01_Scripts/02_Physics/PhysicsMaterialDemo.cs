using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.PhysicsDemo
{
    // Physics Material: Collider 에 붙여 마찰(Friction)·탄성(Bounciness)을 정함
    // - 공 3개: Bounciness 0 / 0.5 / 0.8 (요청값 0.8) — 같은 높이에서 떨어뜨려 튀는 높이 비교
    // - 상자 3개: Friction 0 / 0.3 / 0.8 — 같은 경사로에서 미끄러지는 정도 비교
    // - C 키: 공의 Bounce Combine 을 Maximum ↔ Average 로 전환
    //   두 Collider 가 부딪히면 양쪽 재질 값을 Combine 규칙으로 합침. 바닥(탄성 0)과 Average 로 합치면 0.8 → 0.4 로 줄어듦
    public class PhysicsMaterialDemo : MonoBehaviour
    {
        [SerializeField] private Rigidbody[] balls;
        [SerializeField] private Rigidbody[] boxes;
        [SerializeField] private float autoResetInterval = 6f;

        private Vector3[] ballStart, boxStart;
        private Quaternion[] boxRot;
        private float timer;
        private bool maximum = true;
        private float[] peak, rising, prevVy; // 마지막으로 튄 높이 / 측정 중 높이 / 이전 프레임 세로 속도

        private void Start()
        {
            ballStart = new Vector3[balls.Length];
            for (int i = 0; i < balls.Length; i++) ballStart[i] = balls[i].position;
            boxStart = new Vector3[boxes.Length];
            boxRot = new Quaternion[boxes.Length];
            for (int i = 0; i < boxes.Length; i++) { boxStart[i] = boxes[i].position; boxRot[i] = boxes[i].rotation; }
            peak = new float[balls.Length];
            rising = new float[balls.Length];
            prevVy = new float[balls.Length];
            timer = autoResetInterval;
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (DemoInput.KeyDown(Key.R) || timer <= 0f) ResetAll();
            if (DemoInput.KeyDown(Key.C))
            {
                maximum = !maximum;
                foreach (var b in balls)
                {
                    // collider.material = 이 Collider 전용 복사본 (원본 에셋은 안 바뀜)
                    var mat = b.GetComponent<Collider>().material;
                    mat.bounceCombine = maximum ? PhysicsMaterialCombine.Maximum : PhysicsMaterialCombine.Average;
                }
                ResetAll();
            }

            string s = $"공 Bounce Combine: {(maximum ? "Maximum (공 값 그대로)" : "Average (바닥 0 과 평균 → 절반)")}\n";
            for (int i = 0; i < balls.Length; i++)
            {
                var mat = balls[i].GetComponent<Collider>().sharedMaterial;
                // 튀어 오르는 동안의 최고 높이를 재고, 내려가기 시작하면 "마지막으로 튄 높이"로 기록
                float vy = balls[i].linearVelocity.y;
                if (prevVy[i] <= 0f && vy > 0.1f) rising[i] = 0f;                      // 방금 바닥에서 튐
                if (vy > 0f) rising[i] = Mathf.Max(rising[i], balls[i].position.y - 0.5f); // 공 반지름 0.5 빼고 바닥 기준
                if (prevVy[i] > 0f && vy <= 0f) peak[i] = rising[i];
                prevVy[i] = vy;
                s += $"Bounciness {mat.bounciness:0.0} → 튄 높이 {peak[i]:0.0}m   ";
            }
            GlossaryInfoPanel.Status = s.TrimEnd() + "\nR : 다시 떨어뜨리기 (6초마다 자동)";
        }

        public void ResetAll()
        {
            timer = autoResetInterval;
            for (int i = 0; i < balls.Length; i++) Place(balls[i], ballStart[i], Quaternion.identity);
            for (int i = 0; i < boxes.Length; i++) Place(boxes[i], boxStart[i], boxRot[i]);
            for (int i = 0; i < peak.Length; i++) peak[i] = 0f;
        }

        private static void Place(Rigidbody rb, Vector3 pos, Quaternion rot)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = pos;
            rb.rotation = rot;
            rb.transform.SetPositionAndRotation(pos, rot);
        }
    }
}
