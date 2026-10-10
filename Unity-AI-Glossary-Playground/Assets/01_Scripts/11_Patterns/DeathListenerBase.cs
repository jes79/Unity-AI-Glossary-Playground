using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.Patterns
{
    // 옵저버(듣는 쪽) 공통 부분
    // OnEnable 에서 구독(+=), OnDisable 에서 해제(-=) → 컴포넌트를 끄면 더 이상 반응하지 않음
    // 해제를 잊으면 파괴된 오브젝트를 호출하는 오류·메모리 누수가 생기므로 짝을 맞추는 것이 중요
    public abstract class DeathListenerBase : MonoBehaviour
    {
        [SerializeField] protected PlayerHealthSubject subject;
        [SerializeField] private Image card;          // 화면의 리스너 카드 (이벤트를 받으면 번쩍)
        [SerializeField] private TextMeshProUGUI cardState;

        private static readonly Color OnColor = new Color32(52, 58, 76, 255);
        private static readonly Color OffColor = new Color32(35, 36, 40, 255);
        private static readonly Color FlashColor = new Color32(255, 200, 80, 255);

        protected virtual void OnEnable()
        {
            subject.OnPlayerDied += HandlePlayerDied; // 구독
            SetCard(true);
        }

        protected virtual void OnDisable()
        {
            subject.OnPlayerDied -= HandlePlayerDied; // 해제
            SetCard(false);
        }

        public void ToggleSubscribed() => enabled = !enabled;

        private void HandlePlayerDied()
        {
            OnPlayerDied();
            if (card != null && isActiveAndEnabled) StartCoroutine(Flash());
        }

        protected abstract void OnPlayerDied();

        private void SetCard(bool subscribed)
        {
            if (card != null) card.color = subscribed ? OnColor : OffColor;
            if (cardState != null) cardState.text = subscribed ? "<color=#9BE89B>구독 중</color>" : "<color=#888>구독 해제</color>";
        }

        private IEnumerator Flash()
        {
            yield return Ease.Run(0.6f, Ease.OutQuad, t => card.color = Color.Lerp(FlashColor, OnColor, t));
        }
    }
}
