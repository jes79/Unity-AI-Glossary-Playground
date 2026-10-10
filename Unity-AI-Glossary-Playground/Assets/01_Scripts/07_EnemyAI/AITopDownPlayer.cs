using UnityEngine;

namespace GlossaryDemo.EnemyAI
{
    // 적 AI 데모들이 공통으로 쓰는 "목표물" 플레이어 (WASD 로 움직이는 캡슐)
    // - CharacterController 로 이동, 실제 이동 속도(Velocity)를 계산해 예측 조준 등에 제공
    // - 체력/피격 깜빡임/사망 시 시작 위치로 리스폰
    [RequireComponent(typeof(CharacterController))]
    public class AITopDownPlayer : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private Renderer body;

        // 다른 스크립트가 이동 방향을 대신 넣을 때 사용 (예: 예측 조준 데모의 자동 원 운동)
        [HideInInspector] public bool useOverride;
        [HideInInspector] public Vector3 overrideDirection;
        [HideInInspector] public bool inputEnabled = true;

        public float Hp { get; private set; }
        public float MaxHp => maxHp;
        public float MoveSpeed => moveSpeed;
        public Vector3 Velocity { get; private set; } // 실제 프레임 이동량으로 계산한 속도
        public int DeathCount { get; private set; }

        private CharacterController cc;
        private Vector3 spawnPos;
        private Vector3 lastPos;
        private Color baseColor;
        private float flashTimer;

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (body == null) body = GetComponentInChildren<Renderer>();
            if (body != null) baseColor = body.material.color;
            spawnPos = transform.position;
            lastPos = transform.position;
            Hp = maxHp;
        }

        private void Update()
        {
            Vector3 dir = Vector3.zero;
            if (useOverride) dir = overrideDirection;
            else if (inputEnabled)
            {
                Vector2 m = DemoInput.Move;
                dir = new Vector3(m.x, 0f, m.y);
            }

            cc.Move((dir * moveSpeed + Vector3.down * 5f) * Time.deltaTime);
            if (dir.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(dir);

            if (Time.deltaTime > 0f)
            {
                Vector3 v = (transform.position - lastPos) / Time.deltaTime;
                v.y = 0f;
                Velocity = v;
            }
            lastPos = transform.position;

            if (body != null)
            {
                flashTimer -= Time.deltaTime;
                body.material.color = flashTimer > 0f ? Color.white : baseColor;
            }
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f) return;
            Hp -= amount;
            flashTimer = 0.12f;
            if (Hp <= 0f)
            {
                DeathCount++;
                Hp = maxHp;
                Teleport(spawnPos);
            }
        }

        public void Teleport(Vector3 pos)
        {
            cc.enabled = false;
            transform.position = pos;
            lastPos = pos;
            cc.enabled = true;
        }
    }
}
