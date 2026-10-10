using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // 바닥의 가시 지대: 안에 있으면 초당 피해. 플레이어의 PlayerHealth 컴포넌트에만 말을 검
    public class DamageZone : MonoBehaviour
    {
        [SerializeField] private PlayerHealth target;
        [SerializeField] private float damagePerSecond = 20f;
        [SerializeField] private Vector2 halfSize = new Vector2(1.5f, 1.5f);

        private void Update()
        {
            Vector3 d = target.transform.position - transform.position;
            if (Mathf.Abs(d.x) < halfSize.x && Mathf.Abs(d.z) < halfSize.y)
                target.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}
