using TMPro;
using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // 리스너 4: 통계/업적 — 사망 횟수 세기 (나중에 기능을 추가해도 플레이어 코드는 안 바뀜)
    public class DeathCounterListener : DeathListenerBase
    {
        [SerializeField] private TextMeshProUGUI counterText;

        private int deaths;

        protected override void OnPlayerDied()
        {
            deaths++;
            counterText.text = $"사망 {deaths}회";
        }
    }
}
