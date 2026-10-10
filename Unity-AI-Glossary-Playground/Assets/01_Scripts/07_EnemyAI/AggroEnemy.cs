using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.EnemyAI
{
    // 어그로(위협 수준) 시스템: 각 플레이어가 준 데미지를 위협 수치로 누적하고, 가장 높은 대상을 공격
    // - switchRatio(110%) 규칙: 현재 대상보다 위협이 10% 이상 높아야 대상을 바꿈 (타깃이 너무 자주 바뀌는 것 방지)
    // - 비교 모드: "가장 가까운 대상" 을 공격하는 단순 AI
    public class AggroEnemy : MonoBehaviour
    {
        public enum TargetRule { HighestThreat, Nearest }

        [System.Serializable]
        public class Attacker
        {
            public string name;
            public Transform transform;
            public float damagePerHit = 10f;
            public float threatMultiplier = 1f; // 탱커는 위협 배율이 높음
            public float autoDps = 5f;
            [HideInInspector] public float threat;
            [HideInInspector] public float totalDamage;
        }

        [SerializeField] private Attacker[] attackers;
        [SerializeField] private TargetRule rule = TargetRule.HighestThreat;
        [SerializeField, Range(1f, 2f)] private float switchRatio = 1.1f;
        [SerializeField] private float threatDecayPerSecond = 0f;
        [SerializeField] private bool autoAttack = true;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float attackRange = 2f;

        [Header("표시")]
        [SerializeField] private LineRenderer targetLine;
        [SerializeField] private Image[] threatBars;
        [SerializeField] private TextMeshProUGUI[] threatTexts;

        private int target = -1;
        private int switchCount;

        private void Update()
        {
            for (int i = 0; i < attackers.Length && i < 3; i++)
                if (DemoInput.KeyDown(Key.Digit1 + i)) Hit(i, attackers[i].damagePerHit * 3f);
            if (DemoInput.KeyDown(Key.T)) Taunt(0);
            if (DemoInput.KeyDown(Key.M)) rule = rule == TargetRule.HighestThreat ? TargetRule.Nearest : TargetRule.HighestThreat;
            if (DemoInput.KeyDown(Key.A)) autoAttack = !autoAttack;
            if (DemoInput.KeyDown(Key.R)) ResetThreat();

            // 자동 공격: 각자 DPS 만큼 꾸준히 데미지 (위협 누적)
            if (autoAttack)
                for (int i = 0; i < attackers.Length; i++) Hit(i, attackers[i].autoDps * Time.deltaTime);

            if (threatDecayPerSecond > 0f)
                foreach (var a in attackers) a.threat = Mathf.Max(0f, a.threat - threatDecayPerSecond * Time.deltaTime);

            SelectTarget();
            ChaseTarget();
            UpdateUI();
        }

        // 데미지 = 위협. 배율을 곱해 누적
        public void Hit(int index, float damage)
        {
            var a = attackers[index];
            a.totalDamage += damage;
            a.threat += damage * a.threatMultiplier;
        }

        // 도발: 현재 최고 위협보다 10% 높게 만들어 강제로 대상을 가져옴
        public void Taunt(int index)
        {
            float max = 0f;
            foreach (var a in attackers) max = Mathf.Max(max, a.threat);
            attackers[index].threat = max * 1.1f + 1f;
        }

        private void ResetThreat()
        {
            foreach (var a in attackers) { a.threat = 0f; a.totalDamage = 0f; }
            target = -1;
            switchCount = 0;
        }

        private void SelectTarget()
        {
            int best = -1;
            if (rule == TargetRule.Nearest)
            {
                float bestDist = float.MaxValue;
                for (int i = 0; i < attackers.Length; i++)
                {
                    float d = Vector3.Distance(transform.position, attackers[i].transform.position);
                    if (d < bestDist) { bestDist = d; best = i; }
                }
            }
            else
            {
                float bestThreat = 0f;
                for (int i = 0; i < attackers.Length; i++)
                    if (attackers[i].threat > bestThreat) { bestThreat = attackers[i].threat; best = i; }

                // 110% 규칙: 현재 대상의 위협 × switchRatio 를 넘어야 교체
                if (target >= 0 && best != target && attackers[best].threat < attackers[target].threat * switchRatio)
                    best = target;
            }

            if (best != target && best >= 0)
            {
                if (target >= 0) switchCount++;
                target = best;
            }
        }

        private void ChaseTarget()
        {
            if (target < 0) { if (targetLine != null) targetLine.enabled = false; return; }
            Vector3 p = attackers[target].transform.position;
            p.y = transform.position.y;
            Vector3 d = p - transform.position;
            if (d.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), Time.deltaTime * 8f);
            if (d.magnitude > attackRange)
                transform.position = Vector3.MoveTowards(transform.position, p, moveSpeed * Time.deltaTime);

            if (targetLine != null)
            {
                targetLine.enabled = true;
                targetLine.positionCount = 2;
                targetLine.SetPosition(0, transform.position + Vector3.up * 0.2f);
                targetLine.SetPosition(1, attackers[target].transform.position + Vector3.up * 0.2f);
            }
        }

        private void UpdateUI()
        {
            float max = 1f;
            foreach (var a in attackers) max = Mathf.Max(max, a.threat);
            for (int i = 0; i < attackers.Length; i++)
            {
                var a = attackers[i];
                if (threatBars != null && i < threatBars.Length) threatBars[i].fillAmount = a.threat / max;
                if (threatTexts != null && i < threatTexts.Length)
                    threatTexts[i].text = $"{(i == target ? "▶ " : "")}{a.name}  위협 {a.threat:0}  (누적 데미지 {a.totalDamage:0}, 배율 ×{a.threatMultiplier:0.0})";
            }

            var sb = new StringBuilder();
            sb.Append($"대상 선택 규칙: <b>{(rule == TargetRule.HighestThreat ? "위협 최고(어그로)" : "가장 가까운 대상")}</b>\n");
            sb.Append($"현재 대상: {(target >= 0 ? attackers[target].name : "없음")}   대상 교체 횟수: {switchCount}\n");
            sb.Append($"자동 공격: {(autoAttack ? "ON" : "OFF")}   교체 기준: 현재 대상의 {switchRatio * 100f:0}% 초과");
            GlossaryInfoPanel.Status = sb.ToString();
        }
    }
}
