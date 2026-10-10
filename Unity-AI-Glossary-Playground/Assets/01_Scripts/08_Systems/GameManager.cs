using System;
using UnityEngine;

namespace GlossaryDemo.Systems
{
    public enum GameState { Ready, Playing, Paused, GameOver }

    // 게임 매니저: 게임 전체 상태를 한 곳에서 관리하는 싱글톤
    // 다른 스크립트는 GameManager.Instance.State 를 읽거나 OnStateChanged 이벤트를 구독해 반응
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Ready;
        public event Action<GameState, GameState> OnStateChanged; // (이전, 다음)

        public float Score { get; private set; }
        public float BestScore { get; private set; }

        private void Awake()
        {
            // 싱글톤: 이미 있으면 자신을 제거 (씬을 여러 번 불러도 하나만 유지)
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        private void Update()
        {
            switch (State)
            {
                case GameState.Ready:
                    if (DemoInput.JumpPressed) ChangeState(GameState.Playing);
                    break;
                case GameState.Playing:
                    Score += Time.deltaTime;
                    if (PausePressed()) ChangeState(GameState.Paused);
                    break;
                case GameState.Paused:
                    if (PausePressed()) ChangeState(GameState.Playing);
                    break;
                case GameState.GameOver:
                    if (DemoInput.KeyDown(UnityEngine.InputSystem.Key.R)) ChangeState(GameState.Ready);
                    break;
            }
        }

        private static bool PausePressed() =>
            DemoInput.KeyDown(UnityEngine.InputSystem.Key.P) || DemoInput.KeyDown(UnityEngine.InputSystem.Key.Escape);

        // 모든 상태 전환은 이 함수 하나를 거침 → 들어갈 때 할 일을 한 곳에서 처리
        public void ChangeState(GameState next)
        {
            if (next == State) return;
            var prev = State;
            State = next;

            switch (next)
            {
                case GameState.Ready:
                    Score = 0f;
                    Time.timeScale = 1f;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f; // 일시정지 = 시간 정지
                    break;
                case GameState.GameOver:
                    BestScore = Mathf.Max(BestScore, Score);
                    break;
            }
            OnStateChanged?.Invoke(prev, next);
        }

        public void GameOver() { if (State == GameState.Playing) ChangeState(GameState.GameOver); }

        // UI 버튼용
        public void StartGame() { if (State == GameState.Ready) ChangeState(GameState.Playing); }
        public void TogglePause()
        {
            if (State == GameState.Playing) ChangeState(GameState.Paused);
            else if (State == GameState.Paused) ChangeState(GameState.Playing);
        }
        public void Retry() { if (State == GameState.GameOver) ChangeState(GameState.Ready); }
    }
}
