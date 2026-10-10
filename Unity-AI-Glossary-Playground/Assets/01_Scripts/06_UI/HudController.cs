using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // uGUI HUD: 체력바(Image Filled) + 점수(TMP) + 일시정지 버튼(Button)
    // Canvas Render Mode 를 Overlay ↔ Camera 로 바꿔 차이를 비교할 수 있음
    public class HudController : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private Camera uiCamera;
        [Tooltip("Screen Space - Camera 일 때 카메라에서 Canvas 까지 거리. 월드 오브젝트(z=0, 거리 10)보다 멀면 UI 가 뒤로 감")]
        [SerializeField] private float planeDistance = 15f;

        [Header("HUD 요소")]
        [SerializeField] private Image hpFill;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private GameObject pauseOverlay;
        [SerializeField] private TextMeshProUGUI renderModeLabel;

        [Header("값")]
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private float damageAmount = 15f;
        [SerializeField] private int scorePerClick = 10;

        [Header("월드 오브젝트 (일시정지 확인용)")]
        [SerializeField] private Transform spinner;

        private float hp;
        private int score;
        private bool paused;

        private void Start()
        {
            hp = maxHp;
            pauseOverlay.SetActive(false);
            Refresh();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.H)) Damage();
            if (DemoInput.KeyDown(Key.J)) Heal();
            if (DemoInput.KeyDown(Key.K)) AddScore();
            if (DemoInput.KeyDown(Key.P) || DemoInput.KeyDown(Key.Escape)) TogglePause();
            if (DemoInput.KeyDown(Key.M)) ToggleRenderMode();

            // Time.deltaTime 기반 회전 → 일시정지(timeScale 0)면 멈춤
            if (spinner != null) spinner.Rotate(0f, 0f, 120f * Time.deltaTime);

            GlossaryInfoPanel.Status = $"Render Mode: {hudCanvas.renderMode}\n" +
                                       $"HP {hp:0}/{maxHp:0}   점수 {score}   timeScale {Time.timeScale:0}";
        }

        public void Damage()
        {
            if (paused) return;
            hp = Mathf.Max(0f, hp - damageAmount);
            Refresh();
        }

        public void Heal()
        {
            if (paused) return;
            hp = Mathf.Min(maxHp, hp + damageAmount);
            Refresh();
        }

        public void AddScore()
        {
            if (paused) return;
            score += scorePerClick;
            Refresh();
        }

        public void TogglePause()
        {
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
            pauseOverlay.SetActive(paused);
        }

        // Overlay: 항상 화면 맨 위에 그림 / Camera: 지정 카메라 앞 planeDistance 위치에 그림 (월드 오브젝트와 앞뒤가 생김)
        public void ToggleRenderMode()
        {
            if (hudCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                hudCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                hudCanvas.worldCamera = uiCamera;
                hudCanvas.planeDistance = planeDistance;
            }
            else
            {
                hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            Refresh();
        }

        private void Refresh()
        {
            hpFill.fillAmount = hp / maxHp;
            hpFill.color = Color.Lerp(new Color(0.9f, 0.25f, 0.25f), new Color(0.3f, 0.85f, 0.4f), hp / maxHp);
            hpText.text = $"HP {hp:0} / {maxHp:0}";
            scoreText.text = $"SCORE {score:N0}";
            if (renderModeLabel != null)
                renderModeLabel.text = hudCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? "Overlay 모드" : "Camera 모드";
        }

        private void OnDestroy() => Time.timeScale = 1f;
    }
}
