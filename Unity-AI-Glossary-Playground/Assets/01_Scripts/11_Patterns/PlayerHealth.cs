using System;
using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // [컴포넌트 분리 3] 체력만 담당: 피해·회복·사망 이벤트
    // 이동·공격 스크립트는 체력이 어떻게 계산되는지 몰라도 됨
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private Renderer body;
        [SerializeField] private Color normalColor = new Color(0.3f, 0.65f, 1f);

        private float hp;
        private float flash;
        private MaterialPropertyBlock block;

        public float Hp => hp;
        public float MaxHp => maxHp;
        public event Action Died;

        private void Awake()
        {
            hp = maxHp;
            block = new MaterialPropertyBlock();
        }

        public void TakeDamage(float amount)
        {
            if (!enabled || hp <= 0f) return; // 체력 컴포넌트를 끄면 무적
            hp = Mathf.Max(0f, hp - amount);
            flash = 0.15f;
            if (hp <= 0f) Died?.Invoke();
        }

        public void ResetHp() => hp = maxHp;

        private void Update()
        {
            flash -= Time.deltaTime;
            block.SetColor("_BaseColor", flash > 0f ? Color.white : normalColor);
            body.SetPropertyBlock(block);
        }
    }
}
