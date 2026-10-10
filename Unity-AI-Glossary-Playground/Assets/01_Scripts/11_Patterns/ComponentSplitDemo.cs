using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.Patterns
{
    // 컴포넌트 분리 데모: 이동 / 공격 / 체력 스크립트를 하나씩 끄고 켜서 서로 독립적임을 확인
    public class ComponentSplitDemo : MonoBehaviour
    {
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerAttack attack;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private Image hpFill;
        [SerializeField] private TextMeshProUGUI[] toggleLabels;
        [SerializeField] private Vector3 respawnPoint = new Vector3(0f, 1f, -3f);

        private void OnEnable() => health.Died += OnDied;
        private void OnDisable() => health.Died -= OnDied;

        private void Start() => RefreshLabels();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) ToggleMovement();
            if (DemoInput.KeyDown(Key.Digit2)) ToggleAttack();
            if (DemoInput.KeyDown(Key.Digit3)) ToggleHealth();

            hpFill.fillAmount = health.Hp / health.MaxHp;
            GlossaryInfoPanel.Status = $"PlayerMovement {(movement.enabled ? "ON" : "<color=#FF6060>OFF</color>")}   " +
                                       $"PlayerAttack {(attack.enabled ? "ON" : "<color=#FF6060>OFF</color>")}   " +
                                       $"PlayerHealth {(health.enabled ? "ON" : "<color=#FF6060>OFF (무적)</color>")}\nHP {health.Hp:0}/{health.MaxHp:0}";
        }

        public void ToggleMovement() { movement.enabled = !movement.enabled; RefreshLabels(); }
        public void ToggleAttack() { attack.enabled = !attack.enabled; RefreshLabels(); }
        public void ToggleHealth() { health.enabled = !health.enabled; RefreshLabels(); }

        private void OnDied()
        {
            // 사망 처리는 이벤트를 받은 쪽에서: 위치 초기화 + 체력 회복
            var cc = health.GetComponent<CharacterController>();
            cc.enabled = false;
            health.transform.position = respawnPoint;
            cc.enabled = true;
            health.ResetHp();
        }

        private void RefreshLabels()
        {
            if (toggleLabels.Length < 3) return;
            toggleLabels[0].text = $"1  이동 {(movement.enabled ? "ON" : "OFF")}";
            toggleLabels[1].text = $"2  공격 {(attack.enabled ? "ON" : "OFF")}";
            toggleLabels[2].text = $"3  체력 {(health.enabled ? "ON" : "OFF")}";
        }
    }
}
