using TMPro;
using UnityEngine;

namespace GlossaryDemo.Systems
{
    // IDamageable 구현 ①: 플레이어 — 방어력만큼 데미지를 줄이고, 0 이 되면 다시 일어남
    public class DamageablePlayer : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int defense = 3;
        [SerializeField] private Renderer body;
        [SerializeField] private TextMeshPro label;

        private int hp;
        private float flash;
        private Color baseColor;

        public bool IsAlive => hp > 0;
        public string Summary => $"플레이어 {hp}/{maxHp} (방어력 {defense})";

        private void Awake()
        {
            hp = maxHp;
            baseColor = body.material.color;
        }

        public void TakeDamage(int amount, Vector3 hitPoint)
        {
            int dmg = Mathf.Max(1, amount - defense); // 플레이어만의 규칙: 방어력 적용
            hp -= dmg;
            flash = 0.15f;
            DamagePopup.Spawn(hitPoint + Vector3.up, $"-{dmg}", new Color(1f, 0.6f, 0.6f));
            if (hp <= 0)
            {
                hp = maxHp;
                DamagePopup.Spawn(transform.position + Vector3.up * 2.5f, "부활!", Color.white, 5f);
            }
        }

        private void Update()
        {
            flash -= Time.deltaTime;
            body.material.color = flash > 0f ? Color.white : baseColor;
            if (label != null) label.text = $"플레이어\nHP {hp}/{maxHp}";
        }
    }
}
