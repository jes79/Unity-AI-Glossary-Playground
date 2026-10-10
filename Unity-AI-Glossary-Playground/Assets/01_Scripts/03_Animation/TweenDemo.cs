using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.AnimationDemo
{
    // 트윈 데모: 버튼을 누르면 0.2초 동안 커졌다 작아지는 펀치 효과 (DOTween 의 DOPunchScale 대체)
    // T 로 트윈을 끄면 같은 버튼이 아무 반응 없이 눌림 → 손맛 차이 비교
    public class TweenDemo : MonoBehaviour
    {
        [Header("펀치")]
        [SerializeField] private bool useTween = true;
        [SerializeField] private float punchAmount = 0.2f;    // 0.2 = 20% 커짐
        [SerializeField] private float punchDuration = 0.2f;  // 0.2초
        [SerializeField, Range(1, 8)] private int vibrato = 1;

        [Header("대상")]
        [SerializeField] private RectTransform[] buttons;
        [SerializeField] private RectTransform card;
        [SerializeField] private Image colorTarget;
        [SerializeField] private Vector2 cardShown = new Vector2(500f, -150f);
        [SerializeField] private Vector2 cardHidden = new Vector2(1250f, -150f);

        private static readonly string[] EaseNames = { "Linear", "OutQuad", "OutBack", "OutElastic", "OutBounce" };
        private static readonly Func<float, float>[] Eases = { Ease.Linear, Ease.OutQuad, Ease.OutBack, Ease.OutElastic, Ease.OutBounce };
        private int easeIndex = 2;
        private bool cardVisible;
        private int colorIndex;
        private readonly Color[] colors = { new Color32(88, 140, 255, 255), new Color32(255, 120, 90, 255), new Color32(110, 220, 140, 255), new Color32(250, 200, 70, 255) };
        private int clicks;

        private void Update()
        {
            if (DemoInput.KeyDown(Key.T)) ToggleTween();
            if (DemoInput.KeyDown(Key.E)) easeIndex = (easeIndex + 1) % Eases.Length;
            if (DemoInput.KeyDown(Key.V)) vibrato = vibrato == 1 ? 6 : 1;
            if (DemoInput.KeyDown(Key.Space)) PunchIndex(0);

            GlossaryInfoPanel.Status = $"트윈: {(useTween ? "ON" : "OFF (즉시 변경)")}   펀치 {punchAmount:0.0} / {punchDuration:0.00}초 / vibrato {vibrato}\n" +
                                       $"이동 이징: {EaseNames[easeIndex]}   활성 트윈 수: {MiniTween.ActiveCount}   클릭 {clicks}";
        }

        // 버튼 OnClick 에 연결: 누른 버튼 자신을 펀치
        public void PunchIndex(int index)
        {
            clicks++;
            if (!useTween || index < 0 || index >= buttons.Length) return;
            MiniTween.PunchScale(buttons[index], Vector3.one * punchAmount, punchDuration, vibrato);
        }

        public void ToggleCard()
        {
            cardVisible = !cardVisible;
            Vector2 to = cardVisible ? cardShown : cardHidden;
            if (useTween) MiniTween.MoveTo(card, to, 0.5f, Eases[easeIndex]);
            else card.anchoredPosition = to;
        }

        public void NextColor()
        {
            colorIndex = (colorIndex + 1) % colors.Length;
            if (useTween) MiniTween.ColorTo(colorTarget, colors[colorIndex], 0.4f);
            else colorTarget.color = colors[colorIndex];
        }

        public void ToggleTween() => useTween = !useTween;
    }
}
