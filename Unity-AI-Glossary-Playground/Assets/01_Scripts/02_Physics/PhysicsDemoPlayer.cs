using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // 물리 데모 공용 플레이어: Rigidbody 를 WASD 로 움직임 (FixedUpdate 에서 linearVelocity 설정)
    // 트리거/충돌 이벤트를 받으려면 둘 중 하나에 Rigidbody 가 있어야 하므로 플레이어에 Rigidbody 를 둠
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsDemoPlayer : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private bool canMove = true;

        private Rigidbody rb;
        private Vector3 spawnPos;
        private float stunTimer; // 넉백 등으로 잠깐 조작 불가

        public Rigidbody Body => rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            spawnPos = transform.position;
        }

        public void Stun(float seconds) => stunTimer = Mathf.Max(stunTimer, seconds);

        private void FixedUpdate()
        {
            stunTimer -= Time.fixedDeltaTime;
            rb.angularVelocity = Vector3.zero;
            if (transform.position.y < -10f) { rb.position = spawnPos; rb.linearVelocity = Vector3.zero; }
            if (!canMove || stunTimer > 0f) return;

            Vector2 input = DemoInput.Move;
            Vector3 v = new Vector3(input.x, 0f, input.y) * moveSpeed;
            rb.linearVelocity = new Vector3(v.x, rb.linearVelocity.y, v.z);
            if (v.sqrMagnitude > 0.1f) rb.MoveRotation(Quaternion.LookRotation(v));
        }
    }
}
