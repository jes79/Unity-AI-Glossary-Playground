using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GlossaryDemo.Systems
{
    // 타이틀 / 게임 / 결과 씬이 공유하는 화면 스크립트 (role 로 구분)
    public class SceneFlowScreen : MonoBehaviour
    {
        public enum Role { Title, Game, Result }

        [SerializeField] private Role role;
        [SerializeField] private TextMeshProUGUI mainText;
        [SerializeField] private Button targetButton;        // 게임: 클릭할 과녁
        [SerializeField] private RectTransform targetArea;
        [SerializeField] private float gameTime = 10f;
        [SerializeField] private CanvasGroup loadingOverlay;
        [SerializeField] private Image loadingBar;

        private int score;
        private float timer;
        private bool loading;

        private void Start()
        {
            if (loadingOverlay != null) loadingOverlay.alpha = 0f;
            if (role == Role.Game)
            {
                score = 0;
                timer = gameTime;
                SceneFlow.PlayCount++;
                if (targetButton != null) targetButton.onClick.AddListener(HitTarget);
                MoveTarget();
            }
        }

        private void Update()
        {
            string scene = SceneManager.GetActiveScene().name;
            switch (role)
            {
                case Role.Title:
                    mainText.text = $"<size=160%><b>TITLE</b></size>\n플레이 횟수 {SceneFlow.PlayCount}   최고 점수 {SceneFlow.BestScore}";
                    if (DemoInput.JumpPressed) GoGame();
                    break;
                case Role.Game:
                    if (!loading)
                    {
                        timer -= Time.deltaTime;
                        if (timer <= 0f) FinishGame();
                    }
                    mainText.text = $"점수 <b>{score}</b>   남은 시간 {Mathf.Max(0f, timer):0.0}초";
                    break;
                case Role.Result:
                    mainText.text = $"<size=140%><b>RESULT</b></size>\n받은 점수: <size=150%><color=#FFD54A>{SceneFlow.LastScore}</color></size>\n최고 점수 {SceneFlow.BestScore}";
                    if (DemoInput.JumpPressed) GoGame();
                    break;
            }
            GlossaryInfoPanel.Status = $"현재 씬: <b>{scene}</b> (buildIndex {SceneManager.GetActiveScene().buildIndex})\n" +
                                       $"SceneFlow.LastScore = {SceneFlow.LastScore}   (static 변수라 씬이 바뀌어도 유지)";
        }

        private void HitTarget()
        {
            if (loading) return;
            score++;
            MoveTarget();
        }

        private void MoveTarget()
        {
            if (targetButton == null || targetArea == null) return;
            var r = targetArea.rect;
            ((RectTransform)targetButton.transform).anchoredPosition = new Vector2(Random.Range(r.xMin + 60f, r.xMax - 60f), Random.Range(r.yMin + 60f, r.yMax - 60f));
        }

        private void FinishGame()
        {
            SceneFlow.LastScore = score;                                   // ① 점수를 static 에 저장
            SceneFlow.BestScore = Mathf.Max(SceneFlow.BestScore, score);
            Load(SceneFlow.ResultScene);                                   // ② 결과 씬으로 이동
        }

        public void GoGame() => Load(SceneFlow.GameScene);
        public void GoTitle() => Load(SceneFlow.TitleScene);

        private void Load(string sceneName)
        {
            if (loading) return;
            StartCoroutine(LoadRoutine(sceneName));
        }

        // 화면을 어둡게 덮고 LoadSceneAsync 진행률을 막대로 표시
        private IEnumerator LoadRoutine(string sceneName)
        {
            loading = true;
            if (loadingOverlay != null) loadingOverlay.transform.SetAsLastSibling(); // 다른 UI 위에 덮기
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.25f)
            {
                if (loadingOverlay != null) loadingOverlay.alpha = t;
                yield return null;
            }
            var op = SceneFlow.LoadAsync(sceneName);
            if (op == null) { loading = false; yield break; }
            while (!op.isDone)
            {
                if (loadingBar != null) loadingBar.fillAmount = Mathf.Clamp01(op.progress / 0.9f);
                yield return null;
            }
        }
    }
}
