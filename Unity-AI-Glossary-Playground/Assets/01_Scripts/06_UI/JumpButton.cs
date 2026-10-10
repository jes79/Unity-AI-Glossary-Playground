using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // 모바일 점프 버튼: Button.onClick 은 "뗄 때" 발생하므로, 누르는 순간 반응하도록 IPointerDownHandler 사용
    public class JumpButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color pressedColor = new Color(1f, 0.85f, 0.4f, 0.8f);

        private bool pressedThisFrame;

        public bool Held { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            Held = true;
            pressedThisFrame = true;
            image.color = pressedColor;
            transform.localScale = Vector3.one * 0.92f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Held = false;
            image.color = normalColor;
            transform.localScale = Vector3.one;
        }

        // 읽은 쪽에서 한 번 소비
        public bool Consume()
        {
            bool v = pressedThisFrame;
            pressedThisFrame = false;
            return v;
        }
    }
}
