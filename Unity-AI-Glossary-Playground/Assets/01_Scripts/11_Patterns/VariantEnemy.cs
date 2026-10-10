using TMPro;
using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // 프리팹 / 프리팹 변형 데모용 적
    // 기본 프리팹(Enemy_Base)에 기본값이 있고, Variant(Fast/Strong)는 일부 값만 덮어씀(Override)
    public class VariantEnemy : MonoBehaviour
    {
        [SerializeField] private string displayName = "기본 적";
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int attackPower = 10;
        [SerializeField] private TextMeshPro label;
        [SerializeField] private float despawnX = 11f;

        private void Start()
        {
            // 크기가 다른 Variant 도 발이 바닥에 닿도록 높이 맞춤 (캡슐 높이 2 × scale / 2)
            var p = transform.position;
            p.y = transform.localScale.y;
            transform.position = p;

            label.text = $"<b>{displayName}</b>\n속도 {moveSpeed:0.#}  HP {maxHp}  공격 {attackPower}";
        }

        private void Update()
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            label.transform.rotation = Camera.main.transform.rotation;
            if (transform.position.x > despawnX) Destroy(gameObject);
        }
    }
}
