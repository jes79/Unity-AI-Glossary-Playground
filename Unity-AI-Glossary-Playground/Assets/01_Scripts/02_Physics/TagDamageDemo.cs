using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.PhysicsDemo
{
    // 태그로 대상 고르기: 클릭한 오브젝트가 "Enemy" 태그일 때만 데미지
    // CompareTag("Enemy") 를 씀 (tag == "Enemy" 보다 빠르고 메모리 할당이 없음)
    // F 키로 태그 검사를 끄면 아무거나 맞아서 상자·NPC·플레이어까지 다침
    public class TagDamageDemo : MonoBehaviour
    {
        [SerializeField] private string targetTag = "Enemy";
        [SerializeField] private float damage = 25f;
        [SerializeField] private bool useTagFilter = true;
        [SerializeField] private LineRenderer shotLine;
        [SerializeField] private Transform gun;

        private string last = "-";
        private float lineTimer;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.F)) useTagFilter = !useTagFilter;

            if (PhysicsClick.LeftWorld && Physics.Raycast(DemoInput.MouseRay(), out var hit, 200f))
            {
                ShowLine(hit.point);
                var target = hit.collider.GetComponentInParent<DemoHealth>();
                string tagName = hit.collider.tag;

                // ★ 핵심: 태그 검사
                if (useTagFilter && !hit.collider.CompareTag(targetTag))
                {
                    last = $"{hit.collider.name} → 무시 (태그 '{tagName}' ≠ '{targetTag}')";
                    FloatingText.Spawn(hit.point + Vector3.up * 0.5f, "무시", new Color(0.7f, 0.75f, 0.85f), 4f);
                }
                else if (target != null)
                {
                    target.TakeDamage(damage);
                    last = $"{hit.collider.name} → 데미지 {damage:0} (태그 '{tagName}')";
                }
                else
                {
                    last = $"{hit.collider.name} → 체력 없음 (태그 '{tagName}')";
                }
            }

            lineTimer -= Time.deltaTime;
            if (shotLine != null) shotLine.enabled = lineTimer > 0f;

            GlossaryInfoPanel.Status = $"태그 검사: {(useTagFilter ? $"ON (CompareTag(\"{targetTag}\") 만 데미지)" : "OFF (맞으면 전부 데미지)")}\n마지막: {last}";
        }

        private void ShowLine(Vector3 to)
        {
            if (shotLine == null || gun == null) return;
            shotLine.SetPosition(0, gun.position);
            shotLine.SetPosition(1, to);
            lineTimer = 0.08f;
        }
    }
}
