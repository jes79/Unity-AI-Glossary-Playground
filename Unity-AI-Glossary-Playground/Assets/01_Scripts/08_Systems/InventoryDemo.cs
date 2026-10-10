using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.Systems
{
    // 20칸 인벤토리 UI. 버튼으로 아이템을 넣고, 슬롯을 클릭하면 1개 사용/버림
    public class InventoryDemo : MonoBehaviour
    {
        [SerializeField] private int slotCount = 20;
        [SerializeField] private ItemData[] addableItems;      // 버튼 순서와 같은 아이템
        [SerializeField] private int[] addAmounts;             // 버튼별 추가 개수
        [SerializeField] private Image[] slotIcons;
        [SerializeField] private TextMeshProUGUI[] slotCounts;
        [SerializeField] private Image[] slotFrames;
        [SerializeField] private TextMeshProUGUI detailText;

        private Inventory inventory;
        private string log = "위 버튼으로 아이템을 넣어 보세요";

        private void Awake()
        {
            inventory = new Inventory(slotCount);
            inventory.Changed += Refresh; // 데이터가 바뀌면 UI 갱신 (이벤트로 분리)
        }

        private void Start() => Refresh();

        public void AddByButton(int index)
        {
            var item = addableItems[index];
            int amount = addAmounts[index];
            int left = inventory.Add(item, amount);
            log = left == 0
                ? $"{item.itemName} ×{amount} 추가"
                : $"<color=#FF7A7A>{item.itemName} ×{amount} 중 {left}개는 인벤토리가 가득 차서 못 넣음</color>";
            Refresh();
        }

        public void AddRandom()
        {
            int i = Random.Range(0, addableItems.Length);
            var item = addableItems[i];
            int amount = item.maxStack > 1 ? Random.Range(1, 60) : 1;
            int left = inventory.Add(item, amount);
            log = left == 0 ? $"랜덤: {item.itemName} ×{amount} 추가" : $"<color=#FF7A7A>랜덤: {item.itemName} {left}개 넘침</color>";
            Refresh();
        }

        public void ClickSlot(int index)
        {
            var s = inventory.slots[index];
            if (s == null || s.IsEmpty) { log = $"{index + 1}번 칸은 비어 있음"; Refresh(); return; }
            string name = s.item.itemName;
            inventory.RemoveAt(index, 1);
            log = $"{index + 1}번 칸: {name} 1개 사용/버림";
            Refresh();
        }

        public void Sort() { inventory.Sort(); log = "정렬: 같은 아이템을 합치고 종류순으로 정리"; Refresh(); }
        public void Clear() { inventory.Clear(); log = "모두 비움"; Refresh(); }

        private void Refresh()
        {
            for (int i = 0; i < slotIcons.Length; i++)
            {
                var s = i < inventory.Size ? inventory.slots[i] : null;
                bool has = s != null && !s.IsEmpty;
                slotIcons[i].enabled = has;
                if (has) slotIcons[i].sprite = s.item.icon;
                slotCounts[i].text = has && s.item.maxStack > 1 ? s.count.ToString() : "";
                slotCounts[i].color = has && s.count >= s.item.maxStack ? new Color(1f, 0.8f, 0.3f) : Color.white;
                if (slotFrames != null && i < slotFrames.Length)
                    slotFrames[i].color = has ? Color.Lerp(new Color32(45, 49, 61, 255), s.item.RarityColor, 0.25f) : new Color32(38, 41, 52, 255);
            }

            if (detailText != null) detailText.text = log;
            GlossaryInfoPanel.Status = $"사용 중인 칸 {inventory.UsedSlots}/{inventory.Size}   (최대 스택 99, 장비는 1)\n{log}";
        }
    }
}
