using UnityEngine;
using UnityEngine.EventSystems;

namespace GlossaryDemo.UIDemo
{
    // 화면 가상 조이스틱 (터치·마우스 모두 동작: uGUI 포인터 이벤트 사용)
    // 이 컴포넌트는 "터치 영역"(화면 왼쪽 아래의 투명 Image)에 붙임
    // - Fixed: 배경 원이 제자리, Floating: 누른 곳으로 배경 원이 이동
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public enum Mode { Fixed, Floating }

        [SerializeField] private RectTransform background; // 바깥 원
        [SerializeField] private RectTransform handle;     // 안쪽 손잡이
        [SerializeField] private Mode mode = Mode.Fixed;
        [Tooltip("이 값보다 작게 기울이면 0 으로 처리 (손가락 떨림 방지)")]
        [SerializeField, Range(0f, 0.5f)] private float deadZone = 0.1f;
        [Tooltip("손잡이가 움직일 수 있는 거리 (배경 반지름 대비)")]
        [SerializeField, Range(0.2f, 1.5f)] private float handleRange = 1f;

        private RectTransform area;
        private Vector2 defaultPos;

        public Vector2 Value { get; private set; }  // -1 ~ 1
        public bool Pressed { get; private set; }
        public Mode CurrentMode => mode;

        private void Awake()
        {
            area = (RectTransform)transform;
            defaultPos = background.anchoredPosition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Pressed = true;
            if (mode == Mode.Floating &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(area, eventData.position, eventData.pressEventCamera, out Vector2 local))
            {
                // 로컬 좌표는 area 의 피벗 기준, 배경의 anchoredPosition 은 앵커(area 가운데) 기준 → 가운데 기준으로 변환
                background.anchoredPosition = local - area.rect.center;
            }
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out Vector2 local))
                return;

            float radius = background.rect.width * 0.5f;
            Vector2 v = Vector2.ClampMagnitude(local / radius, 1f); // 원 밖으로 끌어도 길이 1 로 제한
            handle.anchoredPosition = v * radius * handleRange;
            Value = v.magnitude < deadZone ? Vector2.zero : v;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Pressed = false;
            Value = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
            if (mode == Mode.Floating) background.anchoredPosition = defaultPos;
        }

        public void ToggleMode()
        {
            mode = mode == Mode.Fixed ? Mode.Floating : Mode.Fixed;
            background.anchoredPosition = defaultPos;
        }
    }
}
