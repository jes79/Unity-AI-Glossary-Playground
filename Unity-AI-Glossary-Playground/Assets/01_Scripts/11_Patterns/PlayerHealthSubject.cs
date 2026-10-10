using System;
using UnityEngine;
using UnityEngine.Events;

namespace GlossaryDemo.Patterns
{
    // 옵저버 패턴의 "방송하는 쪽"(Subject)
    // 플레이어는 누가 듣는지 모름. 죽으면 OnPlayerDied 이벤트만 쏨 → UI·사운드·카메라가 각자 반응
    public class PlayerHealthSubject : MonoBehaviour
    {
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private SpriteRenderer body;

        [Header("UnityEvent: 인스펙터에서 끌어다 연결하는 방식")]
        [SerializeField] private UnityEvent onPlayerDiedUnity = new();

        // C# event (Action): 코드에서 += / -= 로 구독
        public event Action OnPlayerDied;
        public event Action<float, float> OnHealthChanged; // (현재, 최대)

        private float hp;

        public float Hp => hp;
        public float MaxHp => maxHp;
        public bool IsDead => hp <= 0f;
        public UnityEvent OnPlayerDiedUnity => onPlayerDiedUnity;

        // 지금 구독 중인 리스너 수 (데모 표시용)
        public int SubscriberCount => OnPlayerDied?.GetInvocationList().Length ?? 0;

        private void Start()
        {
            hp = maxHp;
            OnHealthChanged?.Invoke(hp, maxHp);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;
            hp = Mathf.Max(0f, hp - amount);
            OnHealthChanged?.Invoke(hp, maxHp);
            if (body != null) body.color = Color.Lerp(new Color(0.4f, 0.4f, 0.45f), new Color(0.3f, 0.65f, 1f), hp / maxHp);

            if (IsDead)
            {
                // 방송! 듣는 쪽이 몇 개든, 하나도 없든 플레이어 코드는 그대로
                OnPlayerDied?.Invoke();
                onPlayerDiedUnity.Invoke();
            }
        }

        public void Revive()
        {
            hp = maxHp;
            if (body != null) body.color = new Color(0.3f, 0.65f, 1f);
            OnHealthChanged?.Invoke(hp, maxHp);
        }
    }
}
