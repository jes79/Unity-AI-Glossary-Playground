using UnityEngine;

namespace GlossaryDemo.Patterns
{
    // 리스너 1: UI — 죽으면 GAME OVER 표시
    public class GameOverUIListener : DeathListenerBase
    {
        [SerializeField] private GameObject gameOverText;

        private void Start() => gameOverText.SetActive(false);

        private void Update()
        {
            // 부활하면 숨김 (부활 이벤트를 따로 두지 않고 상태로 판단)
            if (!subject.IsDead && gameOverText.activeSelf) gameOverText.SetActive(false);
        }

        protected override void OnPlayerDied() => gameOverText.SetActive(true);
    }
}
