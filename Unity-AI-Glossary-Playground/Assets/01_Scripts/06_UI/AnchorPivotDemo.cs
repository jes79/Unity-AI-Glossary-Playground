using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.UIDemo
{
    // Anchor / Pivot
    // - 가상 화면(simScreen)의 크기를 늘였다 줄이며, Anchor 가 오른쪽 위인 점수와 가운데인 점수가 어떻게 움직이는지 비교
    // - Pivot 을 바꾸면 회전 중심과 anchoredPosition 기준점이 바뀌는 것을 확인
    public class AnchorPivotDemo : MonoBehaviour
    {
        [Header("가상 화면")]
        [SerializeField] private RectTransform simScreen;
        [SerializeField] private Vector2 minSize = new Vector2(700f, 420f);
        [SerializeField] private Vector2 maxSize = new Vector2(1200f, 640f);
        [SerializeField] private float resizePeriod = 5f;
        [SerializeField] private bool autoResize = true;

        [Header("Anchor 비교 대상")]
        [SerializeField] private RectTransform scoreTopRight; // Anchor = 오른쪽 위 (요청 예문)
        [SerializeField] private RectTransform scoreCenter;   // Anchor = 가운데 (잘못된 예)

        [Header("Pivot")]
        [SerializeField] private RectTransform pivotBox;
        [SerializeField] private RectTransform pivotMarker;
        [SerializeField] private TextMeshProUGUI pivotLabel;
        [SerializeField] private float rotateSpeed = 60f;

        private static readonly Vector2[] Pivots = { new(0.5f, 0.5f), new(0f, 0f), new(1f, 1f), new(0f, 1f) };
        private int pivotIndex;
        private float time;

        private void Start() => ApplyPivot();

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Space)) ToggleAutoResize();
            if (DemoInput.KeyDown(Key.P)) NextPivot();

            if (autoResize) time += Time.deltaTime;
            float t = 0.5f - 0.5f * Mathf.Cos(time / resizePeriod * Mathf.PI * 2f);
            simScreen.sizeDelta = Vector2.Lerp(maxSize, minSize, t);

            pivotBox.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

            bool centerInside = IsInside(scoreCenter, simScreen);
            GlossaryInfoPanel.Status = $"가상 화면 크기 {simScreen.rect.width:0}x{simScreen.rect.height:0}\n" +
                                       $"초록(Anchor 오른쪽 위): 항상 모서리   빨강(Anchor 가운데): {(centerInside ? "화면 안" : "<color=#FF6060>화면 밖으로 밀림</color>")}\n" +
                                       $"Pivot {pivotBox.pivot}  (anchoredPosition 은 Pivot 위치를 가리킴)";
        }

        public void ToggleAutoResize() => autoResize = !autoResize;

        public void NextPivot()
        {
            pivotIndex = (pivotIndex + 1) % Pivots.Length;
            ApplyPivot();
        }

        private void ApplyPivot()
        {
            // Pivot 만 바꾸면 anchoredPosition 이 같아도 상자가 그려지는 위치가 달라짐 (기준점이 바뀌므로)
            pivotBox.pivot = Pivots[pivotIndex];
            pivotMarker.anchorMin = pivotMarker.anchorMax = Pivots[pivotIndex]; // 마커를 피벗 위치에 붙임
            pivotMarker.anchoredPosition = Vector2.zero;
            pivotLabel.text = $"Pivot ({Pivots[pivotIndex].x:0.0}, {Pivots[pivotIndex].y:0.0})  [P]";
        }

        private static bool IsInside(RectTransform child, RectTransform area)
        {
            var corners = new Vector3[4];
            child.GetWorldCorners(corners);
            foreach (var c in corners)
            {
                Vector2 local = area.InverseTransformPoint(c);
                if (!area.rect.Contains(local)) return false;
            }
            return true;
        }
    }
}
