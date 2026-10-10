using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.UIDemo
{
    // 툴팁 창 하나를 모든 아이템이 같이 씀
    // - showDelay 초 동안 올려 두면 표시 (스치듯 지나갈 땐 안 뜸)
    // - 마우스를 따라다니고, 화면 오른쪽/아래 끝에서는 반대쪽으로 뒤집어 화면 밖으로 안 나가게 함
    // - 툴팁 자체는 CanvasGroup.blocksRaycasts = false (툴팁이 마우스를 가리면 Exit 가 발생해 깜박이는 문제 방지)
    public class TooltipView : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private RectTransform panel;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private TextMeshProUGUI descText;
        [SerializeField] private TextMeshProUGUI modeLabel;

        [Header("동작")]
        [SerializeField, Range(0f, 1f)] private float showDelay = 0.3f;
        [SerializeField] private Vector2 offset = new Vector2(24f, -24f);
        [SerializeField] private bool followMouse = true;
        [SerializeField] private float fadeSpeed = 10f;

        private TooltipTrigger current;
        private float hoverTime;

        public TooltipTrigger Current => current;

        private void Start()
        {
            group.alpha = 0f;
            RefreshModeLabel();
        }

        public void Show(TooltipTrigger trigger)
        {
            current = trigger;
            hoverTime = 0f;
            titleText.text = trigger.ItemName;
            rarityText.text = trigger.Rarity;
            rarityText.color = trigger.RarityColor;
            descText.text = trigger.Description;
        }

        public void Hide(TooltipTrigger trigger)
        {
            if (current == trigger) current = null;
        }

        public void ToggleFollowMouse()
        {
            followMouse = !followMouse;
            RefreshModeLabel();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.F)) ToggleFollowMouse();

            if (current != null) hoverTime += Time.unscaledDeltaTime;
            bool visible = current != null && hoverTime >= showDelay;
            group.alpha = Mathf.MoveTowards(group.alpha, visible ? 1f : 0f, fadeSpeed * Time.unscaledDeltaTime);
            if (current != null) Position();

            GlossaryInfoPanel.Status = $"올린 아이템: {(current != null ? current.ItemName : "-")}   대기 {Mathf.Min(hoverTime, showDelay):0.00}/{showDelay:0.00}초\n" +
                                       $"위치: {(followMouse ? "마우스 따라감" : "아이템 옆 고정")}   피벗 {panel.pivot}";
        }

        private void Position()
        {
            // 화면 좌표 → Canvas 로컬 좌표 (Overlay Canvas 는 카메라 null)
            Vector2 screen = followMouse
                ? DemoInput.MousePosition
                : RectTransformUtility.WorldToScreenPoint(null, current.Rect.TransformPoint(new Vector3(current.Rect.rect.xMax, current.Rect.rect.yMax)));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local);

            // 내용에 맞춰 크기가 바뀌므로(ContentSizeFitter) 현재 크기로 화면 밖 여부 판단
            Vector2 size = panel.rect.size;
            Rect area = canvasRect.rect;
            bool flipX = local.x + offset.x + size.x > area.xMax;
            bool flipY = local.y + offset.y - size.y < area.yMin;
            panel.pivot = new Vector2(flipX ? 1f : 0f, flipY ? 0f : 1f);
            Vector2 o = new Vector2(flipX ? -offset.x : offset.x, flipY ? -offset.y : offset.y);
            panel.anchoredPosition = local + o;
        }

        private void RefreshModeLabel()
        {
            if (modeLabel != null) modeLabel.text = followMouse ? "툴팁: 마우스 따라감 (F)" : "툴팁: 아이템 옆 고정 (F)";
        }
    }
}
