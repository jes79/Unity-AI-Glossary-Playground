using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.UIDemo
{
    // 두 체력바(딜레이 ON / OFF)에 같은 피해를 줘서 비교
    public class HealthBarDelayDemo : MonoBehaviour
    {
        [SerializeField] private DelayedHealthBar withDelay;
        [SerializeField] private DelayedHealthBar withoutDelay;
        [SerializeField] private RectTransform shakeTarget;

        private Vector2 shakeOrigin;

        private void Start() => shakeOrigin = shakeTarget.anchoredPosition;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Digit1)) Damage10();
            if (DemoInput.KeyDown(Key.Digit2)) Damage30();
            if (DemoInput.KeyDown(Key.Digit3)) Combo();
            if (DemoInput.KeyDown(Key.H)) Heal25();
            if (DemoInput.KeyDown(Key.R)) ResetAll();
            if (DemoInput.KeyDown(Key.D)) ToggleDelay();

            GlossaryInfoPanel.Status = $"위: 딜레이 {(withDelay.UseDelay ? "ON" : "OFF")}   아래: 딜레이 OFF (즉시)\nHP {withDelay.Hp:0}";
        }

        public void Damage10() => Hit(10f);
        public void Damage30() => Hit(30f);
        public void Heal25()
        {
            withDelay.Heal(25f);
            withoutDelay.Heal(25f);
        }

        public void ToggleDelay() => withDelay.SetUseDelay(!withDelay.UseDelay);

        public void ResetAll()
        {
            withDelay.ResetHp();
            withoutDelay.ResetHp();
        }

        // 0.15초 간격 5연타: 딜레이 바는 마지막 타격 0.5초 뒤에 한 번에 줄어듦
        public void Combo() => StartCoroutine(ComboRoutine());

        private IEnumerator ComboRoutine()
        {
            for (int i = 0; i < 5; i++)
            {
                Hit(6f);
                yield return new WaitForSeconds(0.15f);
            }
        }

        private void Hit(float amount)
        {
            withDelay.Damage(amount);
            withoutDelay.Damage(amount);
            StopCoroutine(nameof(Shake));
            StartCoroutine(nameof(Shake));
        }

        private IEnumerator Shake()
        {
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                shakeTarget.anchoredPosition = shakeOrigin + Random.insideUnitCircle * 8f * (1f - t / 0.2f);
                yield return null;
            }
            shakeTarget.anchoredPosition = shakeOrigin;
        }
    }
}
