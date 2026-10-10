using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.Patterns
{
    // 입력과 체력바 표시. 체력바도 OnHealthChanged 이벤트의 구독자
    public class EventObserverDemo : MonoBehaviour
    {
        [SerializeField] private PlayerHealthSubject subject;
        [SerializeField] private DeathListenerBase[] listeners;
        [SerializeField] private Image hpFill;
        [SerializeField] private float damage = 25f;

        private void OnEnable() => subject.OnHealthChanged += UpdateBar;
        private void OnDisable() => subject.OnHealthChanged -= UpdateBar;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.H)) Damage();
            if (DemoInput.KeyDown(Key.R)) Revive();
            for (int i = 0; i < listeners.Length && i < 4; i++)
                if (DemoInput.KeyDown(Key.Digit1 + i)) listeners[i].ToggleSubscribed();

            GlossaryInfoPanel.Status = $"HP {subject.Hp:0}/{subject.MaxHp:0}   OnPlayerDied 구독자 {subject.SubscriberCount}명 (C# event)\n" +
                                       $"+ UnityEvent 리스너 {subject.OnPlayerDiedUnity.GetPersistentEventCount()}개 (인스펙터 연결: 파티클)";
        }

        public void Damage() => subject.TakeDamage(damage);
        public void Revive() => subject.Revive();

        private void UpdateBar(float hp, float max) => hpFill.fillAmount = hp / max;
    }
}
