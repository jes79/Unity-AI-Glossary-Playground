using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // 비동기 씬 로딩 + 진행률 바
    // SceneManager.LoadSceneAsync → AsyncOperation.progress (0 ~ 0.9 까지 로드, 0.9 에서 활성화 대기)
    // allowSceneActivation = false 로 두면 로드가 끝나도 넘어가지 않음 → "아무 키나 누르세요" 같은 게이트를 만들 수 있음
    public class AsyncSceneLoader : MonoBehaviour
    {
        [Header("대상 씬 (Build Settings 에 등록된 이름)")]
        [SerializeField] private string[] targetScenes = { "0000_Hub", "0601_CanvasUGUI", "0616_AsyncLoading" };
        [SerializeField] private Button[] targetButtons;

        [Header("로딩 화면")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private Image progressFill;
        [SerializeField] private TextMeshProUGUI percentText;
        [SerializeField] private TextMeshProUGUI detailText;
        [SerializeField] private TextMeshProUGUI tipText;
        [SerializeField] private GameObject continuePrompt;
        [SerializeField] private RectTransform spinner;

        [Header("옵션")]
        [Tooltip("로드가 끝나도 키 입력 전까지 씬 전환을 막음 (allowSceneActivation = false)")]
        [SerializeField] private bool waitForInput = true;
        [Tooltip("로딩이 너무 빨라 진행률 바가 안 보일 때를 위한 최소 표시 시간 (데모용)")]
        [SerializeField, Range(0f, 5f)] private float minimumShowTime = 2f;
        [SerializeField] private TextMeshProUGUI optionLabel;
        [SerializeField] private Slider minTimeSlider;
        [SerializeField] private TextMeshProUGUI minTimeText;

        [SerializeField, TextArea] private string[] tips =
        {
            "팁: 로딩 중 Time.timeScale 과 상관없이 돌도록 unscaledDeltaTime 을 씁니다.",
            "팁: AsyncOperation.progress 는 0.9 에서 멈추고, 활성화가 허용되면 1 이 됩니다.",
            "팁: 큰 씬은 로딩 화면 씬을 먼저 띄우고 그 안에서 다음 씬을 비동기로 불러옵니다.",
        };

        private AsyncOperation op;
        private bool loading;

        private void Start()
        {
            loadingScreen.SetActive(false);
            if (minTimeSlider != null) minTimeSlider.SetValueWithoutNotify(minimumShowTime);
            SetMinimumTime(minimumShowTime);
            RefreshOption();

            // Build Settings 에 없는 씬 버튼은 끔
            for (int i = 0; i < targetButtons.Length && i < targetScenes.Length; i++)
                targetButtons[i].interactable = Application.CanStreamedLevelBeLoaded(targetScenes[i]);
        }

        private void Update()
        {
            if (!loading && DemoInput.KeyDown(Key.W)) ToggleWaitForInput();
            if (loading) spinner.Rotate(0f, 0f, -240f * Time.unscaledDeltaTime);

            if (!loading)
                GlossaryInfoPanel.Status = $"입력 대기(allowSceneActivation 게이트): {(waitForInput ? "ON" : "OFF")}   최소 표시 {minimumShowTime:0.0}초\n" +
                                           "버튼으로 불러올 씬을 고르세요";
        }

        public void LoadTarget(int index)
        {
            if (loading || index < 0 || index >= targetScenes.Length) return;
            StartCoroutine(LoadRoutine(targetScenes[index]));
        }

        public void ToggleWaitForInput()
        {
            waitForInput = !waitForInput;
            RefreshOption();
        }

        public void SetMinimumTime(float value)
        {
            minimumShowTime = value;
            if (minTimeText != null) minTimeText.text = $"최소 표시 시간 {value:0.0}초 (0 = 실제 속도)";
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"{sceneName} 이(가) Build Settings 에 없음 (Tools > Glossary > Build All Scenes)");
                yield break;
            }

            loading = true;
            LectureMode.SceneChangeLocked = true; // 강의 모드가 이 사이에 다른 씬으로 가지 않게 (활성화 대기와 LoadScene 이 겹치면 멈춤)
            loadingScreen.SetActive(true);
            continuePrompt.SetActive(false);
            tipText.text = tips.Length > 0 ? tips[Random.Range(0, tips.Length)] : "";

            op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false; // 0.9 에서 멈추고 기다림

            float shown = 0f, elapsed = 0f;
            while (true)
            {
                elapsed += Time.unscaledDeltaTime;
                float real = Mathf.Clamp01(op.progress / 0.9f);                    // 실제 로드 비율 (0.9 를 100% 로 환산)
                float timeCap = minimumShowTime > 0f ? Mathf.Clamp01(elapsed / minimumShowTime) : 1f;
                shown = Mathf.MoveTowards(shown, Mathf.Min(real, timeCap), Time.unscaledDeltaTime * 2f); // 바가 부드럽게 차오르게

                progressFill.fillAmount = shown;
                percentText.text = $"{shown * 100f:0}%";
                detailText.text = $"{sceneName} 로딩 중...   op.progress = {op.progress:0.00}   isDone = {op.isDone}";
                GlossaryInfoPanel.Status = $"op.progress {op.progress:0.00} (0.9 = 로드 완료, 활성화 대기)   표시 {shown * 100f:0}%";

                if (shown >= 1f && op.progress >= 0.9f) break;
                yield return null;
            }

            if (waitForInput)
            {
                continuePrompt.SetActive(true);
                detailText.text = $"{sceneName} 준비 완료 — allowSceneActivation = false 로 대기 중";
                yield return new WaitUntil(AnyInput);
            }

            op.allowSceneActivation = true; // 이제 씬 전환 (이 오브젝트는 파괴됨)
        }

        private static bool AnyInput()
        {
            bool key = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
            // 강의 키(PageDown, End, Backspace, F3, F4 …)는 '아무 키'로 치지 않음 → 강의 넘기기와 씬 활성화가 겹치지 않게
            if (key && LectureMode.LectureKeyPressedThisFrame && (LectureMode.Enabled || DemoInput.KeyDown(LectureMode.ToggleKey)))
                key = false;
            return key || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
        }

        private void OnDestroy()
        {
            LectureMode.SceneChangeLocked = false;
        }

        private void RefreshOption()
        {
            if (optionLabel != null) optionLabel.text = waitForInput ? "완료 후 키 입력 대기 ON (W)" : "완료 후 바로 전환 (W)";
        }
    }
}
