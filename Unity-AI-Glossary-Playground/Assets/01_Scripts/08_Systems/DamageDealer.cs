using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Systems
{
    // 공격하는 쪽: 맞은 대상이 IDamageable 이기만 하면 종류와 상관없이 TakeDamage 호출
    public class DamageDealer : MonoBehaviour
    {
        [SerializeField] private int[] damageLevels = { 5, 10, 25 };
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private Transform explosionMarker;

        private int level = 1;
        private string lastLog = "물체를 클릭해 보세요";

        private void Update()
        {
            for (int i = 0; i < damageLevels.Length && i < 3; i++)
                if (DemoInput.KeyDown(Key.Digit1 + i)) level = i;
            int dmg = damageLevels[level];

            bool hasHit = Physics.Raycast(DemoInput.MouseRay(), out var hit, 200f);
            if (explosionMarker != null)
            {
                explosionMarker.gameObject.SetActive(hasHit && DemoInput.KeyHeld(Key.LeftShift));
                if (hasHit) explosionMarker.position = new Vector3(hit.point.x, 0.03f, hit.point.z);
            }

            if (DemoInput.LeftClick && hasHit)
            {
                if (DemoInput.KeyHeld(Key.LeftShift)) Explode(hit.point, dmg);
                else if (hit.collider.GetComponentInParent<IDamageable>() is IDamageable target)
                {
                    target.TakeDamage(dmg, hit.point); // 대상이 무엇이든 같은 호출
                    lastLog = $"{hit.collider.name} 에 {dmg} 데미지 → {target.GetType().Name}.TakeDamage()";
                }
                else
                {
                    lastLog = $"{hit.collider.name} 은(는) IDamageable 이 없어서 무시됨";
                    DamagePopup.Spawn(hit.point + Vector3.up * 0.5f, "IDamageable 없음", Color.gray, 4f);
                }
            }

            var sb = new StringBuilder();
            sb.Append($"공격력: {dmg} (1/2/3 키)   {lastLog}\n");
            var list = new List<string>();
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (mb is IDamageable d) list.Add(d.Summary);
            list.Sort();
            sb.Append(string.Join("   ", list));
            GlossaryInfoPanel.Status = sb.ToString();
        }

        // 범위 공격: 원 안의 모든 콜라이더 중 IDamageable 인 것만 골라 데미지
        private void Explode(Vector3 center, int dmg)
        {
            var done = new HashSet<IDamageable>();
            foreach (var c in Physics.OverlapSphere(center, explosionRadius))
            {
                var d = c.GetComponentInParent<IDamageable>();
                if (d != null && done.Add(d)) d.TakeDamage(dmg * 2, c.ClosestPoint(center));
            }
            DamagePopup.Spawn(center + Vector3.up, "폭발!", new Color(1f, 0.5f, 0.2f), 7f);
            lastLog = $"폭발: 반경 {explosionRadius}m 안 IDamageable {done.Count}개에 {dmg * 2} 데미지";
        }
    }
}
