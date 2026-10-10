using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.Systems
{
    // 경험치 / 레벨업: 다음 레벨까지 필요한 경험치 = baseXp × multiplier^(레벨-1)  (기본 100, ×1.2)
    // 한 번에 많은 경험치를 얻으면 while 로 여러 번 레벨업 (남은 경험치는 이월)
    public class LevelUpDemo : MonoBehaviour
    {
        [SerializeField] private int baseXp = 100;
        [SerializeField] private float multiplier = 1.2f;   // 레벨마다 1.2배
        [SerializeField] private int linearStep = 20;       // 비교용: 선형 증가량
        [SerializeField] private bool exponential = true;

        [Header("UI")]
        [SerializeField] private Image xpFill;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private TextMeshProUGUI popupText;
        [SerializeField] private TextMeshProUGUI curveText;
        [SerializeField] private Image[] chartBars;        // 레벨 1~N 필요 경험치 막대
        [SerializeField] private TextMeshProUGUI[] chartLabels;
        [SerializeField] private float chartHeight = 300f;

        private int level = 1;
        private int xp;
        private int totalXp;
        private float popupTimer;
        private float shownFill;

        public int RequiredXp(int lv) => exponential
            ? Mathf.RoundToInt(baseXp * Mathf.Pow(multiplier, lv - 1))
            : baseXp + linearStep * (lv - 1);

        private void Start()
        {
            DrawChart();
            Refresh();
        }

        public void GainXp(int amount)
        {
            xp += amount;
            totalXp += amount;
            int ups = 0;
            while (xp >= RequiredXp(level)) // 여러 번 레벨업 가능
            {
                xp -= RequiredXp(level);
                level++;
                ups++;
            }
            if (ups > 0)
            {
                popupText.text = ups > 1 ? $"LEVEL UP ×{ups}!  Lv.{level}" : $"LEVEL UP!  Lv.{level}";
                popupTimer = 1.5f;
                shownFill = 0f;
                DrawChart();
            }
            Refresh();
        }

        public void ResetLevel()
        {
            level = 1;
            xp = totalXp = 0;
            shownFill = 0f;
            DrawChart();
            Refresh();
        }

        public void ToggleCurve()
        {
            exponential = !exponential;
            ResetLevel();
        }

        private void Update()
        {
            float target = (float)xp / RequiredXp(level);
            shownFill = Mathf.MoveTowards(shownFill, target, Time.deltaTime * 1.5f); // 막대가 부드럽게 차오름
            xpFill.fillAmount = shownFill;

            popupTimer -= Time.deltaTime;
            popupText.alpha = Mathf.Clamp01(popupTimer);
            popupText.transform.localScale = Vector3.one * (1f + Mathf.Max(0f, popupTimer - 1f) * 0.6f);
        }

        private void Refresh()
        {
            levelText.text = $"Lv. {level}";
            xpText.text = $"{xp} / {RequiredXp(level)} XP";
            curveText.text = exponential
                ? $"곡선: 지수  (필요 XP = {baseXp} × {multiplier}^(레벨-1))"
                : $"곡선: 선형 (비교용)  (필요 XP = {baseXp} + {linearStep} × (레벨-1))";
            GlossaryInfoPanel.Status = $"레벨 {level}   현재 {xp}/{RequiredXp(level)} XP   누적 {totalXp} XP\n" +
                                       $"다음 레벨들: {RequiredXp(level)} → {RequiredXp(level + 1)} → {RequiredXp(level + 2)} → {RequiredXp(level + 3)}";
        }

        // 레벨별 필요 경험치 막대 그래프
        private void DrawChart()
        {
            if (chartBars == null || chartBars.Length == 0) return;
            float max = 0f;
            for (int i = 0; i < chartBars.Length; i++) max = Mathf.Max(max, RequiredXp(i + 1));
            for (int i = 0; i < chartBars.Length; i++)
            {
                int req = RequiredXp(i + 1);
                var rt = chartBars[i].rectTransform;
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, chartHeight * req / max);
                chartBars[i].color = i + 1 < level ? new Color(0.35f, 0.75f, 0.45f) : i + 1 == level ? new Color(1f, 0.8f, 0.3f) : new Color(0.35f, 0.45f, 0.65f);
                if (chartLabels != null && i < chartLabels.Length) chartLabels[i].text = req.ToString();
            }
        }
    }
}
