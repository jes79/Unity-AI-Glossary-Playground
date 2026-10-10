using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.Patterns
{
    // [컴포넌트 분리 2] 공격만 담당: J 키 → 앞쪽 범위 안의 TrainingDummy 에 피해
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private float damage = 20f;
        [SerializeField] private float range = 1.6f;
        [SerializeField] private float radius = 1f;
        [SerializeField] private float cooldown = 0.35f;
        [SerializeField] private Transform slashVisual;

        private float nextTime;
        private readonly Collider[] hits = new Collider[8];

        private void Start() => slashVisual.gameObject.SetActive(false);

        private void Update()
        {
            if (DemoInput.KeyDown(Key.J) && Time.time >= nextTime)
            {
                nextTime = Time.time + cooldown;
                Attack();
            }
        }

        private void Attack()
        {
            Vector3 center = transform.position + transform.forward * range;
            int n = Physics.OverlapSphereNonAlloc(center, radius, hits);
            for (int i = 0; i < n; i++)
            {
                var dummy = hits[i].GetComponentInParent<TrainingDummy>();
                if (dummy != null) dummy.Hit(damage);
            }
            StopAllCoroutines();
            StartCoroutine(Slash());
        }

        private IEnumerator Slash()
        {
            slashVisual.gameObject.SetActive(true);
            yield return Ease.Run(0.15f, Ease.OutQuad, t => slashVisual.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.6f, t));
            slashVisual.gameObject.SetActive(false);
        }

        // 컴포넌트가 꺼지면 진행 중인 연출도 정리
        private void OnDisable() => slashVisual.gameObject.SetActive(false);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward * range, radius);
        }
    }
}
