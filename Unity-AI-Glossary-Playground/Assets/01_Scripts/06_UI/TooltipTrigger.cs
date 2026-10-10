using UnityEngine;
using UnityEngine.EventSystems;

namespace GlossaryDemo.UIDemo
{
    // 마우스를 올리면(IPointerEnterHandler) 툴팁을 띄우고, 벗어나면(IPointerExitHandler) 숨김
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string itemName = "아이템";
        [SerializeField] private string rarity = "일반";
        [SerializeField] private Color rarityColor = Color.white;
        [SerializeField, TextArea(2, 5)] private string description = "설명";

        private TooltipView view;

        public string ItemName => itemName;
        public string Rarity => rarity;
        public Color RarityColor => rarityColor;
        public string Description => description;
        public RectTransform Rect => (RectTransform)transform;

        private void Start() => view = FindAnyObjectByType<TooltipView>();

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (view != null) view.Show(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (view != null) view.Hide(this);
        }
    }
}
