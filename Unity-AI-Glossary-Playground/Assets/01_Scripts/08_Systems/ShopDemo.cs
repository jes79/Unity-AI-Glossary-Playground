using TMPro;
using UnityEngine;

namespace GlossaryDemo.Systems
{
    // 상점 / 재화: 골드로 사고(가격) 팔기(가격의 50%)
    // 구매 = ① 돈이 충분한가 ② 가방에 자리가 있는가 를 먼저 확인한 뒤에 골드 차감 + 아이템 추가
    public class ShopDemo : MonoBehaviour
    {
        [SerializeField] private int startGold = 500;
        [SerializeField] private ItemData[] stock;
        [SerializeField] private TextMeshProUGUI[] ownedTexts;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI logText;
        [SerializeField] private int bagSize = 20;

        private int gold;
        private float shownGold;
        private Inventory bag;
        private string log = "구매 / 판매 버튼을 눌러 보세요";

        private void Awake()
        {
            gold = startGold;
            shownGold = gold;
            bag = new Inventory(bagSize);
        }

        private void Start() => Refresh();

        public void Buy(int index)
        {
            var item = stock[index];
            if (gold < item.price)
            {
                log = $"<color=#FF7A7A>골드 부족! {item.itemName} 은(는) {item.price} G (보유 {gold} G)</color>";
            }
            else if (bag.Add(item, 1) > 0) // 자리가 없으면 Add 가 남은 개수 1 을 돌려줌
            {
                log = "<color=#FF7A7A>가방이 가득 찼습니다</color>";
            }
            else
            {
                gold -= item.price;
                log = $"{item.itemName} 구매  -{item.price} G";
            }
            Refresh();
        }

        public void Sell(int index)
        {
            var item = stock[index];
            if (!bag.Remove(item, 1))
            {
                log = $"<color=#FF7A7A>{item.itemName} 을(를) 가지고 있지 않음</color>";
            }
            else
            {
                gold += item.SellPrice;
                log = $"{item.itemName} 판매  +{item.SellPrice} G (구매가의 50%)";
            }
            Refresh();
        }

        public void AddGold(int amount)
        {
            gold += amount;
            log = $"골드 +{amount}";
            Refresh();
        }

        private void Update()
        {
            shownGold = Mathf.MoveTowards(shownGold, gold, Mathf.Max(50f, Mathf.Abs(gold - shownGold) * 6f) * Time.deltaTime);
            if (goldText != null) goldText.text = $"{Mathf.RoundToInt(shownGold):N0} G";
        }

        private void Refresh()
        {
            for (int i = 0; i < stock.Length && i < ownedTexts.Length; i++)
                ownedTexts[i].text = $"보유 {bag.Count(stock[i])}";
            if (logText != null) logText.text = log;
            GlossaryInfoPanel.Status = $"골드 {gold} G   가방 {bag.UsedSlots}/{bag.Size}칸\n{log}";
        }
    }
}
