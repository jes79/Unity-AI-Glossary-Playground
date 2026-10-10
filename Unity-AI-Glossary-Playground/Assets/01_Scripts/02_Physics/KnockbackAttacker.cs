using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.PhysicsDemo
{
    // 0209 플레이어 공격: Space 로 앞쪽 범위를 때려 맞은 적을 넉백. K 로 넉백 ON/OFF
    public class KnockbackAttacker : MonoBehaviour
    {
        [SerializeField] private float attackRange = 1.6f;
        [SerializeField] private LayerMask enemyMask;
        [SerializeField] private Transform slashVisual;

        private Vector3 startPos;
        private int knockCount;

        private void Start()
        {
            startPos = transform.position;
            KnockbackReceiver.Enabled = true; // static 이라 씬을 다시 열 때 초기화
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.K)) KnockbackReceiver.Enabled = !KnockbackReceiver.Enabled;
            if (DemoInput.JumpPressed) Attack();

            float dist = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z), new Vector3(startPos.x, 0f, startPos.z));
            GlossaryInfoPanel.Status = $"넉백: {(KnockbackReceiver.Enabled ? "ON (거리 3m, 0.25초)" : "OFF")}\n" +
                                       $"적을 넉백시킨 횟수: {knockCount}   시작점에서 거리: {dist:0.0} m";
        }

        private void Attack()
        {
            Vector3 center = transform.position + transform.forward * attackRange;
            foreach (var col in Physics.OverlapSphere(center, attackRange, enemyMask))
            {
                if (col.TryGetComponent(out KnockbackReceiver kb))
                {
                    kb.ApplyKnockback(transform.position); // 공격자 = 나
                    knockCount++;
                }
            }
            if (slashVisual != null) StartCoroutine(Slash(center));
        }

        private IEnumerator Slash(Vector3 center)
        {
            slashVisual.gameObject.SetActive(true);
            slashVisual.position = center;
            slashVisual.localScale = Vector3.one * attackRange * 2f;
            yield return new WaitForSeconds(0.1f);
            slashVisual.gameObject.SetActive(false);
        }
    }
}
