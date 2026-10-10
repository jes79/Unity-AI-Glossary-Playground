using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // TextMeshPro 한글 폰트 에셋
    // - 동적(Dynamic) 폰트 에셋은 처음 쓰는 글자를 그때그때 아틀라스 텍스처에 그려 넣음 → 입력창에 한글을 쳐 보면 아틀라스가 채워짐
    // - 한글 글리프가 없는 폰트(LiberationSans SDF)로 바꾸면 한글이 □ 로 보이는 것을 비교
    public class TmpFontDemo : MonoBehaviour
    {
        [Header("폰트 에셋")]
        [SerializeField] private TMP_FontAsset koreanFont;    // GlossaryKit.EnsureKoreanFont() 가 만든 Resources/Fonts/GlossaryFont
        [SerializeField] private TMP_FontAsset latinOnlyFont; // TMP 기본 LiberationSans SDF (한글 없음)

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI sampleText;
        [SerializeField] private TextMeshProUGUI fontNameLabel;
        [SerializeField] private TMP_InputField input;
        [SerializeField] private TextMeshProUGUI previewText;
        [SerializeField] private RawImage atlasView;
        [SerializeField] private TextMeshProUGUI atlasZoomLabel;

        private bool useKorean = true;
        private bool zoomed = true;

        private void Start()
        {
            if (koreanFont == null) koreanFont = GlossaryInfoPanel.Font;
            input.onValueChanged.AddListener(OnInputChanged);
            ApplyFont();
            ApplyZoom();
            OnInputChanged(input.text);
        }

        private void Update()
        {
            if (!input.isFocused)
            {
                if (DemoInput.KeyDown(Key.F)) ToggleFont();
                if (DemoInput.KeyDown(Key.Z)) ToggleAtlasZoom();
            }

            // 동적 폰트는 아틀라스가 가득 차면 텍스처가 추가될 수 있어 매 프레임 최신 텍스처를 표시
            if (koreanFont != null && koreanFont.atlasTexture != null) atlasView.texture = koreanFont.atlasTexture;

            if (koreanFont != null)
            {
                GlossaryInfoPanel.Status = $"폰트: {koreanFont.name}   Atlas Population Mode: {koreanFont.atlasPopulationMode}\n" +
                                           $"아틀라스에 들어간 글자 {koreanFont.characterTable.Count}개 · 글리프 {koreanFont.glyphTable.Count}개   " +
                                           $"아틀라스 {koreanFont.atlasWidth}x{koreanFont.atlasHeight} × {koreanFont.atlasTextures.Length}장";
            }
        }

        public void ToggleFont()
        {
            useKorean = !useKorean;
            ApplyFont();
        }

        public void ToggleAtlasZoom()
        {
            zoomed = !zoomed;
            ApplyZoom();
        }

        private void ApplyFont()
        {
            var font = useKorean || latinOnlyFont == null ? koreanFont : latinOnlyFont;
            sampleText.font = font;
            fontNameLabel.text = useKorean || latinOnlyFont == null
                ? $"현재 폰트: {koreanFont.name} (한글 포함 동적 폰트)"
                : $"현재 폰트: {latinOnlyFont.name} (영문 전용 → 한글이 □ 로 보임)";
        }

        private void ApplyZoom()
        {
            // 글자는 아틀라스 왼쪽 아래부터 채워지므로 확대 시 왼쪽 아래 1/4 만 보여 줌
            atlasView.uvRect = zoomed ? new Rect(0f, 0f, 0.25f, 0.25f) : new Rect(0f, 0f, 1f, 1f);
            atlasZoomLabel.text = zoomed ? "아틀라스 확대 (왼쪽 아래 1/4)  [Z]" : "아틀라스 전체  [Z]";
        }

        private void OnInputChanged(string value)
        {
            previewText.text = string.IsNullOrEmpty(value) ? "<color=#777>입력한 글자가 여기 크게 표시됩니다</color>" : value;
        }
    }
}
