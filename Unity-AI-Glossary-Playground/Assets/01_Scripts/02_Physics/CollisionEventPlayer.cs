using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GlossaryDemo.PhysicsDemo
{
    // 충돌/트리거 콜백 6종을 모두 받아 로그로 보여 줌
    // - OnCollisionEnter : 적(Enemy 태그)과 부딪힌 순간 → 체력 -10 (요청 예문)
    // - OnCollisionStay / Exit : 붙어 있는 동안 / 떨어지는 순간
    // - OnTriggerEnter / Stay / Exit : 독 늪(트리거) 들어감 / 안에 있음(초당 피해) / 나옴
    public class CollisionEventPlayer : MonoBehaviour
    {
        [SerializeField] private DemoHealth health;
        [SerializeField] private TextMeshProUGUI logText;
        [SerializeField] private float enemyDamage = 10f;
        [SerializeField] private float poisonDps = 5f;

        private readonly Queue<string> log = new Queue<string>();
        private readonly Dictionary<string, int> counts = new Dictionary<string, int>();
        private float poisonAccum;

        private void OnCollisionEnter(Collision c)
        {
            Count("OnCollisionEnter");
            if (c.collider.CompareTag("Enemy"))
            {
                health.TakeDamage(enemyDamage);
                Log($"<color=#FF8080>OnCollisionEnter</color>  {c.collider.name} → 체력 -{enemyDamage:0}");
            }
            else if (c.collider.gameObject.layer != LayerMask.NameToLayer("Ground")) // 바닥은 너무 자주 닿아서 로그 생략
            {
                Log($"OnCollisionEnter  {c.collider.name}");
            }
        }

        private void OnCollisionStay(Collision c) => Count("OnCollisionStay");   // 매 물리 프레임 (로그는 너무 많아서 횟수만)

        private void OnCollisionExit(Collision c)
        {
            Count("OnCollisionExit");
            if (c.collider.CompareTag("Enemy")) Log($"OnCollisionExit  {c.collider.name} 에서 떨어짐");
        }

        private void OnTriggerEnter(Collider other)
        {
            Count("OnTriggerEnter");
            Log($"<color=#A0FF80>OnTriggerEnter</color>  {other.name} 들어감");
        }

        private void OnTriggerStay(Collider other)
        {
            Count("OnTriggerStay");
            // 안에 있는 동안 초당 poisonDps 피해 (1 이 쌓일 때마다 적용)
            poisonAccum += poisonDps * Time.fixedDeltaTime;
            if (poisonAccum >= 1f)
            {
                health.TakeDamage(Mathf.Floor(poisonAccum), false);
                poisonAccum -= Mathf.Floor(poisonAccum);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Count("OnTriggerExit");
            Log($"<color=#A0FF80>OnTriggerExit</color>  {other.name} 나옴");
        }

        private void Update()
        {
            if (health.IsDead) { health.ResetHp(); Log("체력 0 → 데모라서 바로 회복"); }
            GlossaryInfoPanel.Status = $"체력: {health.Hp:0} / {health.MaxHp:0}\n" +
                                       $"CollisionEnter {Get("OnCollisionEnter")}  Stay {Get("OnCollisionStay")}  Exit {Get("OnCollisionExit")}\n" +
                                       $"TriggerEnter {Get("OnTriggerEnter")}  Stay {Get("OnTriggerStay")}  Exit {Get("OnTriggerExit")}";
        }

        private void Count(string key) => counts[key] = Get(key) + 1;
        private int Get(string key) => counts.TryGetValue(key, out int v) ? v : 0;

        private void Log(string line)
        {
            log.Enqueue($"{Time.time:0.0}s  {line}");
            while (log.Count > 12) log.Dequeue();
            if (logText != null) logText.text = "<b>콜백 로그</b>\n" + string.Join("\n", log);
        }
    }
}
