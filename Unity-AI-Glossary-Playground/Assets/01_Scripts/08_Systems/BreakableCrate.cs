using TMPro;
using UnityEngine;

namespace GlossaryDemo.Systems
{
    // IDamageable 구현 ③: 상자 — 금이 가며 작아지고, 0 이 되면 파편으로 부서진 뒤 다시 생김
    public class BreakableCrate : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHp = 30;
        [SerializeField] private TextMeshPro label;
        [SerializeField] private Material debrisMaterial;

        private int hp;
        private float respawnTimer;
        private Renderer rend;
        private Collider col;
        private Vector3 baseScale;

        public bool IsAlive => hp > 0;
        public string Summary => IsAlive ? $"상자 {hp}/{maxHp}" : "상자 (부서짐)";

        private void Awake()
        {
            hp = maxHp;
            rend = GetComponent<Renderer>();
            col = GetComponent<Collider>();
            baseScale = transform.localScale;
        }

        public void TakeDamage(int amount, Vector3 hitPoint)
        {
            if (!IsAlive) return;
            hp -= amount;
            DamagePopup.Spawn(hitPoint + Vector3.up, $"-{amount}", new Color(0.9f, 0.75f, 0.5f));
            transform.localScale = baseScale * (0.75f + 0.25f * Mathf.Max(0, hp) / maxHp);
            if (hp <= 0) Break();
        }

        // 상자만의 규칙: 파괴 → 파편(물리) 생성
        private void Break()
        {
            hp = 0;
            rend.enabled = false;
            col.enabled = false;
            if (label != null) label.gameObject.SetActive(false);
            respawnTimer = 3f;
            for (int i = 0; i < 8; i++)
            {
                var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                piece.transform.position = transform.position + Random.insideUnitSphere * 0.4f;
                piece.transform.localScale = Vector3.one * 0.3f;
                if (debrisMaterial != null) piece.GetComponent<Renderer>().sharedMaterial = debrisMaterial;
                var rb = piece.AddComponent<Rigidbody>();
                rb.AddExplosionForce(250f, transform.position + Vector3.down * 0.3f, 3f);
                Destroy(piece, 2.5f);
            }
        }

        private void Update()
        {
            if (!IsAlive)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0f)
                {
                    hp = maxHp;
                    transform.localScale = baseScale;
                    rend.enabled = true;
                    col.enabled = true;
                    if (label != null) label.gameObject.SetActive(true);
                }
            }
            if (label != null && IsAlive) label.text = $"상자\nHP {hp}/{maxHp}";
        }
    }
}
