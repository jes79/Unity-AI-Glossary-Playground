using UnityEngine;
using UnityEngine.EventSystems;

namespace GlossaryDemo.UIDemo
{
    // 끌 수 있는 인벤토리 아이템
    // IBeginDragHandler: 잡는 순간 / IDragHandler: 끄는 동안 매 프레임 / IEndDragHandler: 놓는 순간
    [RequireComponent(typeof(CanvasGroup))]
    public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private string itemName = "아이템";

        private RectTransform rt;
        private CanvasGroup group;
        private Canvas rootCanvas;

        public string ItemName => itemName;
        public InventorySlot Slot { get; private set; }   // 현재 들어 있는 칸
        public bool DroppedOnSlot { get; set; }           // 이번 드래그에서 칸에 놓였는지

        public static DragItem Dragging { get; private set; }

        private void Awake()
        {
            rt = (RectTransform)transform;
            group = GetComponent<CanvasGroup>();
            rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        }

        public void PlaceIn(InventorySlot slot)
        {
            Slot = slot;
            rt.SetParent(slot.transform, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Dragging = this;
            DroppedOnSlot = false;
            // 최상위 Canvas 로 옮겨야 다른 칸들보다 위에 그려짐
            rt.SetParent(rootCanvas.transform, true);
            rt.SetAsLastSibling();
            // 끄는 동안 아이템이 레이캐스트를 막으면, 아래 칸이 OnDrop 을 받지 못함 → 꺼 둠
            group.blocksRaycasts = false;
            group.alpha = 0.8f;
            transform.localScale = Vector3.one * 1.1f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // 마우스 이동량(화면 픽셀)을 Canvas 단위로 바꿔서 더함
            rt.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            group.blocksRaycasts = true;
            group.alpha = 1f;
            transform.localScale = Vector3.one;
            Dragging = null;
            // OnDrop 은 OnEndDrag 보다 먼저 호출됨. 칸에 놓이지 않았으면 원래 칸으로 되돌림
            if (!DroppedOnSlot) PlaceIn(Slot);
        }
    }
}
