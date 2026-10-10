using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // 아이템을 받는 칸. IDropHandler.OnDrop 은 이 칸 위에서 드래그를 놓았을 때 호출됨
    public class InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Color normalColor = new Color32(58, 64, 82, 255);
        [SerializeField] private Color hoverColor = new Color32(90, 140, 255, 255);

        // 데모 비교: true = 아이템끼리 교환, false = 빈 칸에만 놓을 수 있음
        public static bool AllowSwap = true;
        public static event Action<string> Logged;

        public DragItem Item => GetComponentInChildren<DragItem>();

        public void OnDrop(PointerEventData eventData)
        {
            var dragged = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<DragItem>() : null;
            if (dragged == null) return;
            background.color = normalColor;

            var from = dragged.Slot;
            var existing = Item; // 드래그 중인 아이템은 Canvas 로 옮겨져 있어서 여기엔 기존 아이템만 잡힘
            if (from == this) return;

            if (existing != null)
            {
                if (!AllowSwap)
                {
                    Logged?.Invoke($"'{name}' 칸이 차 있어서 놓을 수 없음 → 원래 칸으로");
                    return; // DroppedOnSlot = false → OnEndDrag 가 되돌림
                }
                existing.PlaceIn(from); // 교환: 기존 아이템은 드래그 시작 칸으로
                Logged?.Invoke($"{dragged.ItemName} ↔ {existing.ItemName} 교환");
            }
            else
            {
                Logged?.Invoke($"{dragged.ItemName} → 빈 칸 {name} 로 이동");
            }

            dragged.PlaceIn(this);
            dragged.DroppedOnSlot = true;
        }

        // 드래그 중에 칸 위로 올라가면 강조
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (DragItem.Dragging != null) background.color = hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData) => background.color = normalColor;
    }
}
