using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // 플레이어를 향해 Rigidbody 로 천천히 다가오는 적 (충돌 데모용)
    [RequireComponent(typeof(Rigidbody))]
    public class ChaserEnemy : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float speed = 2.5f;
        [SerializeField] private float pauseAfterHit = 1f; // 부딪힌 뒤 잠깐 멈춤 (계속 붙어 있지 않게)

        private Rigidbody rb;
        private float pause;

        private void Awake() => rb = GetComponent<Rigidbody>();

        public void Pause(float seconds) => pause = Mathf.Max(pause, seconds);

        private void FixedUpdate()
        {
            pause -= Time.fixedDeltaTime;
            if (target == null || pause > 0f) return;
            Vector3 to = target.position - rb.position;
            to.y = 0f;
            Vector3 v = to.sqrMagnitude > 0.01f ? to.normalized * speed : Vector3.zero;
            rb.linearVelocity = new Vector3(v.x, rb.linearVelocity.y, v.z);
        }

        private void OnCollisionEnter(Collision c)
        {
            if (c.collider.CompareTag("Player"))
            {
                pause = pauseAfterHit;
                // 살짝 뒤로 물러남
                Vector3 back = (rb.position - c.transform.position);
                back.y = 0f;
                rb.linearVelocity = back.normalized * 4f;
            }
        }
    }
}
