using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.PhysicsDemo
{
    // 0212 입력: 클릭한 부위를 맞혀 적을 죽이고(래그돌), R 로 모두 부활, A 로 "애니메이터 끄기" 비교
    public class RagdollDemo : MonoBehaviour
    {
        [SerializeField] private LayerMask ragdollMask;
        [SerializeField] private float hitForce = 25f;

        private RagdollEnemy[] enemies;
        private string last = "-";

        private void Start()
        {
            enemies = FindObjectsByType<RagdollEnemy>(FindObjectsSortMode.None);
            RagdollEnemy.DisableAnimatorOnDeath = true;
        }

        private void Update()
        {
            if (PhysicsClick.LeftWorld)
            {
                Ray ray = DemoInput.MouseRay();
                if (Physics.Raycast(ray, out var hit, 200f, ragdollMask))
                {
                    var enemy = hit.collider.GetComponentInParent<RagdollEnemy>();
                    if (enemy != null && !enemy.IsDead)
                    {
                        enemy.Die(hit.rigidbody, ray.direction * hitForce, hit.point);
                        last = $"{enemy.name} 의 '{hit.collider.name}' 명중 → 래그돌";
                    }
                }
            }
            if (DemoInput.KeyDown(Key.R)) foreach (var e in enemies) e.Revive();
            if (DemoInput.KeyDown(Key.A)) RagdollEnemy.DisableAnimatorOnDeath = !RagdollEnemy.DisableAnimatorOnDeath;

            GlossaryInfoPanel.Status = $"죽을 때 Animator 끄기: {(RagdollEnemy.DisableAnimatorOnDeath ? "ON (정상)" : "OFF (나쁜 예: 애니메이션이 물리를 덮어씀)")}\n마지막: {last}";
        }
    }
}
