using UnityEngine;

namespace GlossaryDemo.UIDemo
{
    // Safe Area 적용: Screen.safeArea(픽셀 사각형)를 0~1 비율로 바꿔 이 RectTransform 의 앵커로 지정
    // HUD 를 전부 이 오브젝트의 자식으로 두면 노치·홈 바 영역을 자동으로 피함
    // 에디터엔 노치가 없으므로 SafeAreaDemo 의 시뮬레이션 값이 있으면 그것을 대신 사용
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        [SerializeField] private bool apply = true;

        private RectTransform rt;
        private Rect lastArea;
        private Vector2Int lastScreen;
        private bool lastApply;

        // 시뮬레이션용 덮어쓰기 (null 이면 실제 Screen.safeArea 사용)
        public static Rect? SimulatedSafeArea;

        public bool Apply => apply;

        public static Rect CurrentSafeArea => SimulatedSafeArea ?? Screen.safeArea;

        private void Awake() => rt = (RectTransform)transform;

        public void SetApply(bool value) => apply = value;

        private void Update()
        {
            Rect area = CurrentSafeArea;
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (area == lastArea && screen == lastScreen && apply == lastApply) return; // 바뀔 때만 다시 계산 (회전·해상도 변경 대응)
            lastArea = area;
            lastScreen = screen;
            lastApply = apply;
            Refit(area, screen);
        }

        private void Refit(Rect area, Vector2Int screen)
        {
            if (!apply || screen.x == 0 || screen.y == 0)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
            }
            else
            {
                Vector2 min = area.position;
                Vector2 max = area.position + area.size;
                min.x /= screen.x; min.y /= screen.y;
                max.x /= screen.x; max.y /= screen.y;
                rt.anchorMin = min;
                rt.anchorMax = max;
            }
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private void OnDestroy() => SimulatedSafeArea = null;
    }
}
