using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace GlossaryDemo.Effects
{
    // Vignette: 화면 가장자리를 어둡게(또는 색으로) 물들임
    // 체력이 lowHpRatio(30%) 이하가 되면 빨간 Vignette 가 심장 박동처럼 깜빡임. 체력이 낮을수록 더 빠르고 강하게
    public class LowHpVignetteDemo : MonoBehaviour
    {
        [SerializeField] private Volume volume;
        [SerializeField] private Image hpFill;

        [Header("체력")]
        [SerializeField] private float maxHp = 100f;
        [SerializeField, Range(0f, 1f)] private float lowHpRatio = 0.3f;

        [Header("Vignette")]
        [SerializeField] private Color normalColor = Color.black;
        [SerializeField] private float normalIntensity = 0.2f;
        [SerializeField] private Color lowHpColor = new Color(0.8f, 0f, 0f);
        [SerializeField] private float pulseMin = 0.3f;
        [SerializeField] private float pulseMax = 0.55f;
        [SerializeField] private bool usePulse = true;

        private Vignette vignette;
        private float hp;
        private float pulsePhase;

        private void Start()
        {
            hp = maxHp;
            volume.profile.TryGet(out vignette);
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space) || DemoInput.KeyDown(Key.D)) Damage();
            if (DemoInput.KeyDown(Key.H)) Heal();
            if (DemoInput.KeyDown(Key.P)) TogglePulse();

            float ratio = hp / maxHp;
            bool low = ratio <= lowHpRatio;

            float targetIntensity;
            Color targetColor;
            if (low)
            {
                // 박동 속도: 30% 일 때 1.2Hz → 0% 에 가까울수록 2.4Hz
                float danger = 1f - ratio / lowHpRatio;
                pulsePhase += Time.deltaTime * Mathf.Lerp(1.2f, 2.4f, danger) * Mathf.PI * 2f;
                // 심장 박동처럼 빠르게 올라갔다 천천히 내려오는 모양 (sin 을 거듭제곱)
                float beat = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(pulsePhase), 3f);
                float pulse = usePulse ? beat : 0.5f;
                targetIntensity = Mathf.Lerp(pulseMin, pulseMax + danger * 0.1f, pulse);
                targetColor = lowHpColor;
            }
            else
            {
                targetIntensity = normalIntensity;
                targetColor = normalColor;
            }

            vignette.intensity.value = Mathf.MoveTowards(vignette.intensity.value, targetIntensity, Time.deltaTime * 6f);
            vignette.color.value = Color.Lerp(vignette.color.value, targetColor, 1f - Mathf.Exp(-8f * Time.deltaTime));

            hpFill.fillAmount = Mathf.MoveTowards(hpFill.fillAmount, ratio, Time.deltaTime);
            hpFill.color = low ? new Color(0.9f, 0.25f, 0.2f) : new Color(0.35f, 0.85f, 0.4f);

            GlossaryInfoPanel.Status = $"HP {hp:0} / {maxHp:0} ({ratio * 100f:0}%)   {(low ? "<color=#FF6060>위험! 30% 이하</color>" : "정상")}\n" +
                                       $"Vignette Intensity {vignette.intensity.value:0.00}   맥박 {FxUtil.OnOff(usePulse)}";
        }

        public void Damage() => hp = Mathf.Max(0f, hp - 10f);
        public void Heal() => hp = Mathf.Min(maxHp, hp + 10f);
        public void TogglePulse() => usePulse = !usePulse;
    }
}
