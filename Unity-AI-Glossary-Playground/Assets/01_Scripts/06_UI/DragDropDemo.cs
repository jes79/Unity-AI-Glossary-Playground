using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GlossaryDemo.UIDemo
{
    // 드래그 앤 드롭 데모: 시작할 때 아이템을 칸에 배치하고, 교환 모드 토글과 로그 표시
    public class DragDropDemo : MonoBehaviour
    {
        [SerializeField] private InventorySlot[] slots;
        [SerializeField] private DragItem[] items;
        [SerializeField] private int[] startSlots;
        [SerializeField] private TextMeshProUGUI modeLabel;

        private readonly List<string> log = new();

        private void Awake() => InventorySlot.Logged += OnLog;

        private void OnDestroy()
        {
            InventorySlot.Logged -= OnLog;
            InventorySlot.AllowSwap = true;
        }

        private void Start()
        {
            for (int i = 0; i < items.Length; i++) items[i].PlaceIn(slots[startSlots[i]]);
            RefreshLabel();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.S)) ToggleSwap();
            string dragging = DragItem.Dragging != null ? DragItem.Dragging.ItemName : "-";
            GlossaryInfoPanel.Status = $"드래그 중: {dragging}   모드: {(InventorySlot.AllowSwap ? "교환" : "빈 칸만")}\n" + string.Join("\n", log);
        }

        public void ToggleSwap()
        {
            InventorySlot.AllowSwap = !InventorySlot.AllowSwap;
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (modeLabel != null) modeLabel.text = InventorySlot.AllowSwap ? "모드: 슬롯끼리 교환 (S)" : "모드: 빈 칸에만 (S)";
        }

        private void OnLog(string message)
        {
            log.Add(message);
            if (log.Count > 3) log.RemoveAt(0);
        }
    }
}
