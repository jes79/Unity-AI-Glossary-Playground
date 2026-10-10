using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // Grid Layout Group 으로 4x5 인벤토리 칸을 자동 배치
    // 칸은 템플릿을 Instantiate 해서 부모(grid)에 넣기만 하면 위치는 Layout Group 이 정해 줌
    public class InventoryGridDemo : MonoBehaviour
    {
        [Header("Grid Layout Group (요청 예문: 4x5)")]
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField, Range(1, 8)] private int columns = 4;
        [SerializeField, Range(1, 8)] private int rows = 5;
        [SerializeField] private GameObject slotTemplate;

        [Header("아이템 표시")]
        [SerializeField, Range(0f, 1f)] private float itemChance = 0.45f;
        [SerializeField] private Color[] itemColors =
        {
            new Color(0.9f, 0.4f, 0.35f), new Color(0.35f, 0.7f, 0.95f), new Color(0.45f, 0.85f, 0.45f),
            new Color(0.95f, 0.8f, 0.3f), new Color(0.75f, 0.5f, 0.95f),
        };
        [SerializeField] private TextMeshProUGUI layoutStateLabel;

        private int slotCount;

        private void Start()
        {
            slotTemplate.SetActive(false);
            ApplyColumns();
            for (int i = 0; i < columns * rows; i++) AddSlot();
            RefreshLabel();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.Equals) || DemoInput.KeyDown(Key.NumpadPlus)) AddSlot();
            if (DemoInput.KeyDown(Key.Minus) || DemoInput.KeyDown(Key.NumpadMinus)) RemoveSlot();
            if (DemoInput.KeyDown(Key.C)) ToggleColumns();
            if (DemoInput.KeyDown(Key.L)) ToggleLayout();
            if (DemoInput.KeyDown(Key.S)) ToggleSpacing();

            GlossaryInfoPanel.Status = $"칸 {slotCount}개   constraint: {grid.constraint} = {grid.constraintCount}열\n" +
                                       $"cellSize {grid.cellSize}  spacing {grid.spacing}   Layout Group {(grid.enabled ? "ON" : "<color=#FF6060>OFF</color>")}";
        }

        public void AddSlot()
        {
            var slot = Instantiate(slotTemplate, grid.transform);
            slot.SetActive(true);
            slotCount++;
            slot.name = $"Slot_{slotCount:00}";

            // 일부 칸에 아이템 아이콘 표시
            var icon = slot.transform.Find("Icon").GetComponent<Image>();
            bool hasItem = Random.value < itemChance;
            icon.enabled = hasItem;
            if (hasItem) icon.color = itemColors[Random.Range(0, itemColors.Length)];
            var count = slot.transform.Find("Count").GetComponent<TextMeshProUGUI>();
            count.text = hasItem && Random.value < 0.5f ? Random.Range(2, 99).ToString() : "";
        }

        public void RemoveSlot()
        {
            int last = grid.transform.childCount - 1;
            if (last < 0) return;
            var child = grid.transform.GetChild(last).gameObject;
            if (child == slotTemplate) return;
            Destroy(child);
            slotCount--;
        }

        // 4열 ↔ 5열 (constraint 만 바꾸면 모든 칸이 다시 배치됨)
        public void ToggleColumns()
        {
            columns = columns == 4 ? 5 : 4;
            ApplyColumns();
        }

        // Layout Group 을 끄면 새로 추가한 칸은 자동 배치가 안 되어 한 자리에 겹침
        public void ToggleLayout()
        {
            grid.enabled = !grid.enabled;
            RefreshLabel();
        }

        public void ToggleSpacing()
        {
            grid.spacing = grid.spacing.x > 10f ? new Vector2(6f, 6f) : new Vector2(18f, 18f);
        }

        private void ApplyColumns()
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
        }

        private void RefreshLabel()
        {
            if (layoutStateLabel != null)
                layoutStateLabel.text = grid.enabled ? "Grid Layout Group : ON" : "Grid Layout Group : OFF (새 칸이 한 곳에 겹침)";
        }
    }
}
