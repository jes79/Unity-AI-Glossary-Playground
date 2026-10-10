using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.UIDemo
{
    // 노치 시뮬레이션: 에디터 Game 뷰엔 노치가 없으므로, 기기별 Safe Area 를 비율로 흉내 내 SafeAreaFitter 에 넘김
    // (실제 기기 값은 Window > General > Device Simulator 에서 확인 가능)
    public class SafeAreaDemo : MonoBehaviour
    {
        private struct Preset
        {
            public string Name;
            public float Left, Right, Top, Bottom; // 화면 대비 비율
            public Rect Notch;                     // 노치 모양 (정규화 좌표: x, y, w, h). w=0 이면 없음
            public bool HomeBar;
        }

        private static readonly Preset[] Presets =
        {
            new() { Name = "시뮬레이션 없음 (실제 Screen.safeArea)" },
            new() { Name = "아이폰 가로 · 노치 왼쪽", Left = 0.056f, Right = 0.056f, Bottom = 0.054f, Notch = new Rect(0f, 0.3f, 0.026f, 0.4f), HomeBar = true },
            new() { Name = "아이폰 가로 · 노치 오른쪽", Left = 0.056f, Right = 0.056f, Bottom = 0.054f, Notch = new Rect(0.974f, 0.3f, 0.026f, 0.4f), HomeBar = true },
            new() { Name = "안드로이드 펀치홀 (왼쪽)", Left = 0.04f, Notch = new Rect(0.008f, 0.465f, 0.02f, 0.07f) },
            new() { Name = "아이폰 세로처럼 (위 노치 + 아래 홈 바)", Top = 0.06f, Bottom = 0.04f, Notch = new Rect(0.35f, 0.955f, 0.3f, 0.045f), HomeBar = true },
        };

        [SerializeField] private SafeAreaFitter fitter;
        [SerializeField] private RectTransform notch;
        [SerializeField] private RectTransform homeBar;
        [SerializeField] private RectTransform[] unsafeTints; // 0 왼, 1 오, 2 아래, 3 위
        [SerializeField] private TextMeshProUGUI presetLabel;
        [SerializeField] private TextMeshProUGUI applyLabel;
        [SerializeField] private int startPreset = 1;

        private int index;

        private void Start()
        {
            index = startPreset;
            ApplyPreset();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.N)) NextPreset();
            if (DemoInput.KeyDown(Key.S)) ToggleApply();

            UpdateSimulatedArea();

            // 바깥(안전하지 않은) 영역을 빨갛게 표시
            Rect a = SafeAreaFitter.CurrentSafeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            float l = a.xMin / w, r = a.xMax / w, b = a.yMin / h, t = a.yMax / h;
            SetAnchors(unsafeTints[0], new Vector2(0f, 0f), new Vector2(l, 1f));
            SetAnchors(unsafeTints[1], new Vector2(r, 0f), new Vector2(1f, 1f));
            SetAnchors(unsafeTints[2], new Vector2(l, 0f), new Vector2(r, b));
            SetAnchors(unsafeTints[3], new Vector2(l, t), new Vector2(r, 1f));

            GlossaryInfoPanel.Status = $"화면 {Screen.width}x{Screen.height}   Screen.safeArea {Screen.safeArea}\n" +
                                       $"사용 중인 Safe Area {a}   적용 {(fitter.Apply ? "ON" : "<color=#FF6060>OFF</color>")}";
        }

        public void NextPreset()
        {
            index = (index + 1) % Presets.Length;
            ApplyPreset();
        }

        public void ToggleApply()
        {
            fitter.SetApply(!fitter.Apply);
            applyLabel.text = fitter.Apply ? "Safe Area 적용 ON (S)" : "Safe Area 적용 OFF (S)";
        }

        private void ApplyPreset()
        {
            var p = Presets[index];
            UpdateSimulatedArea();
            notch.gameObject.SetActive(p.Notch.width > 0f);
            if (p.Notch.width > 0f) SetAnchors(notch, p.Notch.min, p.Notch.max);
            homeBar.gameObject.SetActive(p.HomeBar);
            presetLabel.text = $"기기: {p.Name}  (N)";
        }

        // 비율 → 현재 화면 픽셀 사각형 (Game 뷰 크기가 바뀌어도 맞도록 매 프레임 계산)
        private void UpdateSimulatedArea()
        {
            var p = Presets[index];
            if (index == 0)
            {
                SafeAreaFitter.SimulatedSafeArea = null;
                return;
            }
            float w = Screen.width, h = Screen.height;
            SafeAreaFitter.SimulatedSafeArea = new Rect(p.Left * w, p.Bottom * h, (1f - p.Left - p.Right) * w, (1f - p.Top - p.Bottom) * h);
        }

        private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private void OnDestroy() => SafeAreaFitter.SimulatedSafeArea = null;
    }
}
