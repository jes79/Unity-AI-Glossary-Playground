using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GlossaryDemo.UIDemo
{
    // Scroll View 에 상점 아이템 목록을 "데이터 → 항목 프리팹 Instantiate" 방식으로 동적 생성
    // Content 에 Vertical Layout Group + Content Size Fitter 를 달아서, 항목이 늘면 Content 높이도 늘어남
    public class ShopScrollDemo : MonoBehaviour
    {
        [System.Serializable]
        public struct ShopItem
        {
            public string name;
            public int price;
            public Color color;
        }

        [Header("Scroll View")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject itemTemplate; // Content 안의 비활성 항목 (프리팹 역할)

        [Header("상점 데이터")]
        [SerializeField] private int startGold = 5000;
        [SerializeField] private int initialCount = 12;
        [SerializeField] private string[] itemNames = { "체력 물약", "마나 물약", "강철 검", "가죽 갑옷", "화염 반지", "귀환 주문서", "폭탄", "화살 묶음", "마법 지팡이", "용의 비늘" };
        [SerializeField] private TextMeshProUGUI goldText;

        private readonly List<GameObject> spawned = new();
        private int gold;
        private int serial;

        private void Start()
        {
            itemTemplate.SetActive(false);
            gold = startGold;
            for (int i = 0; i < initialCount; i++) AddItem(RandomItem());
            RefreshGold();
        }

        private void Update()
        {
            if (DemoInput.KeyDown(Key.A)) AddTen();
            if (DemoInput.KeyDown(Key.C)) ClearAll();
            if (DemoInput.KeyDown(Key.T)) ScrollToTop();
            if (DemoInput.KeyDown(Key.B)) ScrollToBottom();
            if (DemoInput.KeyDown(Key.M)) ToggleMovementType();

            GlossaryInfoPanel.Status = $"항목 {spawned.Count}개   Content 높이 {content.rect.height:0}\n" +
                                       $"verticalNormalizedPosition {scrollRect.verticalNormalizedPosition:0.00} (1=맨 위)   Movement {scrollRect.movementType}";
        }

        private ShopItem RandomItem()
        {
            return new ShopItem
            {
                name = itemNames[Random.Range(0, itemNames.Length)],
                price = Random.Range(1, 40) * 50,
                color = Color.HSVToRGB(Random.value, 0.55f, 0.95f),
            };
        }

        // 데이터 하나 → 항목 UI 하나
        public void AddItem(ShopItem data)
        {
            var go = Instantiate(itemTemplate, content);
            go.SetActive(true);
            serial++;
            go.name = $"Item_{serial:000}_{data.name}";
            go.transform.Find("Icon").GetComponent<Image>().color = data.color;
            go.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = $"{data.name}  <size=70%><color=#999>#{serial}</color></size>";
            go.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = $"{data.price:N0} G";

            var buy = go.transform.Find("Buy").GetComponent<Button>();
            var buyLabel = buy.GetComponentInChildren<TextMeshProUGUI>();
            // 런타임에 만든 항목은 코드로 리스너 연결 (람다로 해당 데이터·버튼을 캡처)
            buy.onClick.AddListener(() =>
            {
                if (gold < data.price) { buyLabel.text = "골드 부족"; return; }
                gold -= data.price;
                buy.interactable = false;
                buyLabel.text = "구매 완료";
                RefreshGold();
            });
            spawned.Add(go);
        }

        public void AddTen()
        {
            for (int i = 0; i < 10; i++) AddItem(RandomItem());
        }

        public void ClearAll()
        {
            foreach (var go in spawned) Destroy(go);
            spawned.Clear();
        }

        public void ScrollToTop() => scrollRect.verticalNormalizedPosition = 1f;
        public void ScrollToBottom() => scrollRect.verticalNormalizedPosition = 0f;

        // Elastic: 끝에서 고무줄처럼 튕김 / Clamped: 끝에서 딱 멈춤
        public void ToggleMovementType()
        {
            scrollRect.movementType = scrollRect.movementType == ScrollRect.MovementType.Elastic
                ? ScrollRect.MovementType.Clamped
                : ScrollRect.MovementType.Elastic;
        }

        private void RefreshGold() => goldText.text = $"보유 골드  {gold:N0} G";
    }
}
