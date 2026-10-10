using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // 딜레이 감소 체력바
    // 앞쪽 빨간 바(front)는 즉시 줄고, 뒤쪽 흰 바(delay)는 delay 초 기다렸다가 따라 줄어듦 → "얼마나 깎였는지" 가 눈에 보임
    public class DelayedHealthBar : MonoBehaviour
    {
        [Header("바 이미지 (Image Type = Filled, Horizontal)")]
        [SerializeField] private Image frontFill;
        [SerializeField] private Image delayFill;
        [SerializeField] private TextMeshProUGUI hpText;

        [Header("딜레이 감소 (요청 예문: 0.5초 뒤 따라옴)")]
        [SerializeField] private bool useDelay = true;
        [SerializeField, Range(0f, 2f)] private float delay = 0.5f;
        [Tooltip("흰 바가 따라 줄어드는 속도 (초당 비율)")]
        [SerializeField, Range(0.1f, 5f)] private float catchUpSpeed = 1.2f;

        [SerializeField] private float maxHp = 100f;

        private float hp;
        private float delayTimer;

        public bool UseDelay => useDelay;
        public float Hp => hp;

        private void Awake()
        {
            hp = maxHp;
            frontFill.fillAmount = delayFill.fillAmount = 1f;
            Refresh();
        }

        public void Damage(float amount)
        {
            hp = Mathf.Max(0f, hp - amount);
            frontFill.fillAmount = hp / maxHp;   // 빨간 바는 즉시
            delayTimer = delay;                 // 연속으로 맞으면 대기 시간이 다시 시작
            if (!useDelay) delayFill.fillAmount = frontFill.fillAmount;
            Refresh();
        }

        public void Heal(float amount)
        {
            hp = Mathf.Min(maxHp, hp + amount);
            frontFill.fillAmount = hp / maxHp;
            delayFill.fillAmount = Mathf.Max(delayFill.fillAmount, frontFill.fillAmount); // 회복은 흰 바도 바로 맞춤
            Refresh();
        }

        public void ResetHp()
        {
            hp = maxHp;
            frontFill.fillAmount = delayFill.fillAmount = 1f;
            Refresh();
        }

        public void SetUseDelay(bool value) => useDelay = value;

        private void Update()
        {
            if (!useDelay)
            {
                delayFill.fillAmount = frontFill.fillAmount;
                return;
            }

            if (delayTimer > 0f)
            {
                delayTimer -= Time.deltaTime;
                return;
            }

            // 대기 시간이 지나면 흰 바가 빨간 바 쪽으로 줄어듦
            delayFill.fillAmount = Mathf.MoveTowards(delayFill.fillAmount, frontFill.fillAmount, catchUpSpeed * Time.deltaTime);
        }

        private void Refresh()
        {
            if (hpText != null) hpText.text = $"{hp:0} / {maxHp:0}";
        }
    }
}
