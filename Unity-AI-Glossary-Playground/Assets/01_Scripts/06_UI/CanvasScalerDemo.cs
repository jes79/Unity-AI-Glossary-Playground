using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // Canvas Scaler 비교
    // 1) 실제 Canvas 의 CanvasScaler 모드를 바꿈 → Game 뷰 해상도를 바꿔 보면 차이가 보임
    // 2) 가상 기기 화면(시뮬레이션): 여러 해상도에서 CanvasScaler 가 계산하는 scaleFactor 를 같은 공식으로 적용해 미리보기
    public class CanvasScalerDemo : MonoBehaviour
    {
        public enum Mode { ConstantPixelSize, ScaleWithScreenSize }

        [Header("실제 Canvas")]
        [SerializeField] private CanvasScaler targetScaler;

        [Header("Canvas Scaler 설정 (요청 예문: 1920x1080 기준)")]
        [SerializeField] private Mode mode = Mode.ScaleWithScreenSize;
        [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);
        [Tooltip("0 = 가로 기준, 1 = 세로 기준, 0.5 = 반반 (가로·세로 모드가 섞이는 모바일에 무난)")]
        [SerializeField, Range(0f, 1f)] private float matchWidthOrHeight = 0.5f;

        [Header("시뮬레이션")]
        [SerializeField] private RectTransform deviceFrame;   // 기기 화면 테두리
        [SerializeField] private RectTransform virtualCanvas; // 기기 안의 가상 Canvas (자식 UI 는 1920x1080 기준 크기)
        [SerializeField] private TextMeshProUGUI deviceLabel;
        [SerializeField] private TextMeshProUGUI modeLabel;
        [SerializeField] private Slider matchSlider;
        [SerializeField] private Vector2 displayArea = new Vector2(1060f, 560f); // 미리보기가 들어갈 최대 크기 (UI 단위)
        [SerializeField] private Vector2Int[] resolutions =
        {
            new Vector2Int(1920, 1080), new Vector2Int(1280, 720), new Vector2Int(2560, 1440),
            new Vector2Int(2340, 1080), new Vector2Int(1080, 2340), new Vector2Int(2048, 1536),
        };
        [SerializeField] private string[] resolutionNames = { "FHD 모니터", "HD", "QHD 모니터", "폰 가로 (19.5:9)", "폰 세로", "태블릿 (4:3)" };

        private int resIndex;
        private float simScale;

        private void Start()
        {
            if (matchSlider != null) matchSlider.SetValueWithoutNotify(matchWidthOrHeight);
            Apply();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.R)) NextResolution();
            if (DemoInput.KeyDown(Key.M)) ToggleMode();

            float realScale = targetScaler.GetComponent<Canvas>().scaleFactor;
            GlossaryInfoPanel.Status = $"실제 화면 {Screen.width}x{Screen.height}  → 실제 Canvas scaleFactor {realScale:0.00}\n" +
                                       $"가상 기기 {resolutions[resIndex].x}x{resolutions[resIndex].y}  → scaleFactor {simScale:0.00}";
        }

        public void NextResolution()
        {
            resIndex = (resIndex + 1) % resolutions.Length;
            Apply();
        }

        public void ToggleMode()
        {
            mode = mode == Mode.ScaleWithScreenSize ? Mode.ConstantPixelSize : Mode.ScaleWithScreenSize;
            Apply();
        }

        public void SetMatch(float value)
        {
            matchWidthOrHeight = value;
            Apply();
        }

        // CanvasScaler(Scale With Screen Size, Match) 가 내부에서 쓰는 공식과 같음
        // 가로 비율과 세로 비율을 log2 공간에서 match 만큼 섞음
        public static float ComputeScaleFactor(Vector2 screen, Vector2 reference, float match)
        {
            float logW = Mathf.Log(screen.x / reference.x, 2f);
            float logH = Mathf.Log(screen.y / reference.y, 2f);
            return Mathf.Pow(2f, Mathf.Lerp(logW, logH, match));
        }

        private void Apply()
        {
            // 1) 실제 Canvas Scaler 설정
            targetScaler.uiScaleMode = mode == Mode.ScaleWithScreenSize ? CanvasScaler.ScaleMode.ScaleWithScreenSize : CanvasScaler.ScaleMode.ConstantPixelSize;
            targetScaler.referenceResolution = referenceResolution;
            targetScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            targetScaler.matchWidthOrHeight = matchWidthOrHeight;
            targetScaler.scaleFactor = 1f;

            // 2) 가상 기기: 기기 해상도를 미리보기 영역에 맞게 축소해서 보여 줌
            Vector2 res = resolutions[resIndex];
            float preview = Mathf.Min(displayArea.x / res.x, displayArea.y / res.y);
            deviceFrame.sizeDelta = res * preview;

            simScale = mode == Mode.ScaleWithScreenSize ? ComputeScaleFactor(res, referenceResolution, matchWidthOrHeight) : 1f;
            // 가상 Canvas 의 논리 크기 = 기기 픽셀 / scaleFactor (실제 Canvas 의 RectTransform 크기와 같은 계산)
            virtualCanvas.sizeDelta = res / simScale;
            virtualCanvas.localScale = Vector3.one * (simScale * preview);

            string name = resIndex < resolutionNames.Length ? resolutionNames[resIndex] : "";
            deviceLabel.text = $"{name}  {res.x:0}x{res.y:0}   scaleFactor {simScale:0.00}   논리 크기 {res.x / simScale:0}x{res.y / simScale:0}";
            modeLabel.text = mode == Mode.ScaleWithScreenSize
                ? $"모드: Scale With Screen Size (기준 {referenceResolution.x:0}x{referenceResolution.y:0}, Match {matchWidthOrHeight:0.00})"
                : "모드: Constant Pixel Size (해상도가 커지면 UI 가 작아지고, 작아지면 넘침)";
        }
    }
}
