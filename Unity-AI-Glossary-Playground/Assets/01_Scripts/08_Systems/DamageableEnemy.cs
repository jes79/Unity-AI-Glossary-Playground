using TMPro;
using UnityEngine;

namespace GlossaryDemo.Systems
{
    // IDamageable 구현 ②: 적 — 맞으면 뒤로 밀리고, 죽으면 쓰러졌다가 3초 뒤 부활
    public class DamageableEnemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHp = 50;
        [SerializeField] private Renderer body;
        [SerializeField] private TextMeshPro label;

        private int hp;
        private float flash;
        private float deadTimer;
        private Vector3 home;
        private Color baseColor;

        public bool IsAlive => hp > 0;
        public string Summary => $"적 {hp}/{maxHp}";

        private void Awake()
        {
            hp = maxHp;
            home = transform.position;
            baseColor = body.material.color;
        }

        public void TakeDamage(int amount, Vector3 hitPoint)
        {
            if (!IsAlive) return;
            hp -= amount;
            flash = 0.15f;
            // 적만의 규칙: 넉백
            Vector3 away = transform.position - hitPoint;
            away.y = 0f;
            transform.position += away.normalized * 0.4f;
            DamagePopup.Spawn(hitPoint + Vector3.up, $"-{amount}", new Color(1f, 0.9f, 0.3f));
            if (hp <= 0)
            {
                hp = 0;
                deadTimer = 3f;
                transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                DamagePopup.Spawn(transform.position + Vector3.up * 2f, "처치!", Color.white, 5f);
            }
        }

        private void Update()
        {
            if (!IsAlive)
            {
                deadTimer -= Time.deltaTime;
                if (deadTimer <= 0f)
                {
                    hp = maxHp;
                    transform.SetPositionAndRotation(home, Quaternion.identity);
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, home, Time.deltaTime);
            }
            flash -= Time.deltaTime;
            body.material.color = !IsAlive ? Color.gray : flash > 0f ? Color.white : baseColor;
            if (label != null) label.text = IsAlive ? $"적\nHP {hp}/{maxHp}" : "적 (쓰러짐)";
        }
    }
}
