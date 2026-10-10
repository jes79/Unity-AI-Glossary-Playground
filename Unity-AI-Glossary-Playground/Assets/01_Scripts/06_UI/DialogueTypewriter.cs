using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // NPC 대화창 + 타자 효과
    // TMP 의 maxVisibleCharacters 를 0 부터 늘려서 한 글자씩 보여 줌 (리치 텍스트 태그는 글자 수에서 자동으로 빠짐)
    // 출력 중 클릭 → 전부 표시, 다 나온 뒤 클릭 → 다음 대사
    public class DialogueTypewriter : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private GameObject nextIndicator;
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI portraitGlyph;
        [SerializeField] private Slider speedSlider;
        [SerializeField] private TextMeshProUGUI skipLabel;

        [Header("타자 효과")]
        [SerializeField, Range(5f, 100f)] private float charsPerSecond = 30f;
        [Tooltip("출력 중 클릭하면 남은 글자를 한 번에 표시")]
        [SerializeField] private bool clickToComplete = true;
        [SerializeField] private float punctuationPause = 0.15f; // 마침표·쉼표 뒤 잠깐 멈춤

        [Header("효과음 (글자마다 짧은 소리)")]
        [SerializeField] private AudioSource blipSource;
        [SerializeField] private int blipEvery = 2;

        [Header("대사")]
        [SerializeField] private string[] speakers;
        [SerializeField, TextArea(2, 4)] private string[] lines;
        [SerializeField] private Color[] speakerColors;

        private int lineIndex = -1;
        private int visible;
        private int total;
        private float timer;
        private bool typing;

        private void Start()
        {
            if (speedSlider != null) speedSlider.SetValueWithoutNotify(charsPerSecond);
            RefreshSkipLabel();
            NextLine();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space) || DemoInput.KeyDown(Key.Enter)) Advance();
            if (DemoInput.KeyDown(Key.S)) ToggleClickToComplete();

            if (typing)
            {
                timer -= Time.deltaTime;
                while (typing && timer <= 0f)
                {
                    visible++;
                    bodyText.maxVisibleCharacters = visible;
                    char c = bodyText.textInfo.characterInfo[Mathf.Clamp(visible - 1, 0, total - 1)].character;
                    timer += 1f / charsPerSecond + (c == '.' || c == ',' || c == '?' || c == '!' ? punctuationPause : 0f);
                    if (blipSource != null && c != ' ' && visible % Mathf.Max(1, blipEvery) == 0) blipSource.Play();
                    if (visible >= total) typing = false;
                }
            }

            // ▼ 표시: 다 나왔을 때만 깜박임
            nextIndicator.SetActive(!typing && Mathf.Repeat(Time.time, 0.8f) < 0.5f);

            GlossaryInfoPanel.Status = $"대사 {lineIndex + 1}/{lines.Length}   글자 {visible}/{total}   " +
                                       $"속도 {charsPerSecond:0}자/초\n클릭 시 전부 표시: {(clickToComplete ? "ON" : "OFF")}   상태: {(typing ? "출력 중" : "대기 (클릭 → 다음)")}";
        }

        // 대화창 클릭 / Space
        public void Advance()
        {
            if (typing)
            {
                if (!clickToComplete) return;
                visible = total;                    // 남은 글자 한 번에 표시
                bodyText.maxVisibleCharacters = total;
                typing = false;
                return;
            }
            NextLine();
        }

        // 이 컴포넌트가 붙은 대화창(Image)을 클릭하면 호출 (Button 대신 써서 Space 키 Submit 과 겹치지 않게 함)
        public void OnPointerClick(PointerEventData eventData) => Advance();

        public void SetSpeed(float value) => charsPerSecond = value;

        public void ToggleClickToComplete()
        {
            clickToComplete = !clickToComplete;
            RefreshSkipLabel();
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null); // 버튼 선택 해제 (Space 가 버튼을 다시 누르지 않게)
        }

        private void NextLine()
        {
            lineIndex = (lineIndex + 1) % lines.Length;
            int s = lineIndex % speakers.Length;
            nameText.text = speakers[s];
            if (portrait != null && speakerColors.Length > 0) portrait.color = speakerColors[s % speakerColors.Length];
            if (portraitGlyph != null) portraitGlyph.text = speakers[s].Substring(0, 1);

            bodyText.text = lines[lineIndex];
            bodyText.maxVisibleCharacters = 0;
            bodyText.ForceMeshUpdate();                   // textInfo 를 최신으로 (태그 제외 실제 글자 수)
            total = bodyText.textInfo.characterCount;
            visible = 0;
            timer = 0.1f;
            typing = total > 0;
        }

        private void RefreshSkipLabel()
        {
            if (skipLabel != null) skipLabel.text = clickToComplete ? "클릭 시 전부 표시 ON (S)" : "클릭 시 전부 표시 OFF (S)";
        }
    }
}
