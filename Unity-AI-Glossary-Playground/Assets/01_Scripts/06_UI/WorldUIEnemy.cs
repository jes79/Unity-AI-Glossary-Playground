using UnityEngine;

namespace GlossaryDemo.UIDemo
{
    // 원을 그리며 걸어 다니는 적. 머리 위 WorldHealthBar 에 체력을 알려 줌
    public class WorldUIEnemy : MonoBehaviour
    {
        [SerializeField] private string displayName = "적";
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private float walkRadius = 3f;
        [SerializeField] private float walkSpeed = 0.6f; // 라디안/초
        [SerializeField] private Vector3 center;
        [SerializeField] private WorldHealthBar healthBar;

        private float hp;
        private float angle;

        public string DisplayName => displayName;

        private void Start()
        {
            hp = maxHp;
            angle = Random.value * Mathf.PI * 2f;
            healthBar.SetRatio(1f, displayName);
        }

        private void Update()
        {
            angle += walkSpeed * Time.deltaTime;
            Vector3 next = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * walkRadius;
            next.y = transform.position.y;
            Vector3 dir = next - transform.position;
            if (dir.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.LookRotation(dir); // 몸이 진행 방향으로 회전
            transform.position = next;
        }

        public void Damage(float amount)
        {
            hp -= amount;
            if (hp <= 0f) hp = maxHp; // 데모: 0이 되면 다시 가득 참
            healthBar.SetRatio(hp / maxHp, $"{displayName}  {hp:0}/{maxHp:0}");
        }
    }
}
