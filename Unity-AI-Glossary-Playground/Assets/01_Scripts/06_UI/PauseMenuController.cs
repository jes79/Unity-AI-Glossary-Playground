using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // ESC 일시정지 메뉴 (계속 / 설정 / 메인으로)
    // Time.timeScale = 0 → Time.deltaTime 과 물리가 멈춤. 메뉴 연출은 Time.unscaledDeltaTime 으로 계속 움직임
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private RectTransform menuSpinner;   // unscaled 로 도는 장식 (일시정지 중에도 UI 는 움직인다는 표시)
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TextMeshProUGUI volumeText;
        [SerializeField] private TextMeshProUGUI modeLabel;

        [Header("비교")]
        [Tooltip("끄면 메뉴만 열리고 게임은 계속 진행됨 (timeScale 을 안 바꾸면 생기는 문제)")]
        [SerializeField] private bool useTimeScale = true;
        [SerializeField] private bool pauseAudio = true;

        private bool paused;

        private void Start()
        {
            menuRoot.SetActive(false);
            settingsPanel.SetActive(false);
            volumeSlider.SetValueWithoutNotify(AudioListener.volume);
            SetVolume(AudioListener.volume);
            RefreshModeLabel();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Escape))
            {
                if (paused && settingsPanel.activeSelf) settingsPanel.SetActive(false); // 설정창이 열려 있으면 먼저 닫기
                else TogglePause();
            }
            if (DemoInput.KeyDown(Key.T)) ToggleUseTimeScale();

            if (paused) menuSpinner.Rotate(0f, 0f, -180f * Time.unscaledDeltaTime);

            GlossaryInfoPanel.Status = $"일시정지 {(paused ? "ON" : "OFF")}   Time.timeScale = {Time.timeScale:0}   timeScale 사용: {(useTimeScale ? "ON" : "<color=#FF6060>OFF</color>")}\n" +
                                       $"Time.time {Time.time:0.0}   Time.unscaledTime {Time.unscaledTime:0.0}";
        }

        public void TogglePause()
        {
            if (paused) Resume();
            else Pause();
        }

        public void Pause()
        {
            paused = true;
            menuRoot.SetActive(true);
            if (useTimeScale) Time.timeScale = 0f;
            if (pauseAudio) AudioListener.pause = true;
        }

        public void Resume()
        {
            paused = false;
            menuRoot.SetActive(false);
            settingsPanel.SetActive(false);
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        public void ToggleSettings() => settingsPanel.SetActive(!settingsPanel.activeSelf);

        public void SetVolume(float value)
        {
            AudioListener.volume = value;
            volumeText.text = $"전체 볼륨 {value * 100f:0}%";
        }

        // "메인으로": 다른 씬으로 가기 전에 timeScale 을 반드시 1 로 되돌림 (안 그러면 다음 씬이 멈춘 채 시작)
        public void GoToMain()
        {
            Resume();
            if (Application.CanStreamedLevelBeLoaded(GlossaryCatalog.HubScene))
                SceneManager.LoadScene(GlossaryCatalog.HubScene);
            else
                Debug.LogWarning("허브 씬이 Build Settings 에 없음 (Tools > Glossary > Build All Scenes)");
        }

        public void ToggleUseTimeScale()
        {
            useTimeScale = !useTimeScale;
            if (paused) Time.timeScale = useTimeScale ? 0f : 1f;
            RefreshModeLabel();
        }

        private void RefreshModeLabel()
        {
            if (modeLabel != null) modeLabel.text = useTimeScale ? "timeScale 0 사용 (T)" : "timeScale 안 씀 (T)";
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
