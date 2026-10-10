using UnityEngine;

namespace GlossaryDemo.Systems
{
    public enum ItemType { Consumable, Weapon, Armor, Material }
    public enum ItemRarity { Common, Rare, Legendary }

    // 아이템 데이터 = ScriptableObject
    // 코드(동작)와 데이터(수치)를 분리해 .asset 파일로 저장 → 기획자가 Inspector 에서 수치만 바꿔도 됨
    // 여러 오브젝트가 같은 에셋을 "참조"하므로 메모리에 한 번만 존재
    [CreateAssetMenu(menuName = "Glossary/Item Data", fileName = "NewItem")]
    public class ItemData : ScriptableObject
    {
        [Header("기본 정보")]
        public string id = "item";
        public string itemName = "아이템";
        public Sprite icon;
        [TextArea] public string description;
        public ItemType type = ItemType.Consumable;
        public ItemRarity rarity = ItemRarity.Common;

        [Header("가격 / 스택")]
        public int price = 10;
        [Range(1, 999)] public int maxStack = 99;

        [Header("효과")]
        public int healAmount;   // 소모품: 회복량
        public int manaAmount;
        public int attackBonus;  // 장비: 공격력 +
        public int defenseBonus; // 장비: 방어력 +

        public int SellPrice => Mathf.Max(1, price / 2);

        public Color RarityColor => rarity switch
        {
            ItemRarity.Rare => new Color(0.35f, 0.65f, 1f),
            ItemRarity.Legendary => new Color(1f, 0.7f, 0.2f),
            _ => new Color(0.85f, 0.87f, 0.9f),
        };

        public string EffectText
        {
            get
            {
                var s = "";
                if (healAmount > 0) s += $"HP +{healAmount} ";
                if (manaAmount > 0) s += $"MP +{manaAmount} ";
                if (attackBonus > 0) s += $"공격력 +{attackBonus} ";
                if (defenseBonus > 0) s += $"방어력 +{defenseBonus} ";
                return s.Length == 0 ? "효과 없음 (재료)" : s.Trim();
            }
        }
    }

    // 인벤토리 한 칸 (아이템 + 개수)
    [System.Serializable]
    public class ItemStack
    {
        public ItemData item;
        public int count;

        public ItemStack(ItemData item, int count)
        {
            this.item = item;
            this.count = count;
        }

        public bool IsEmpty => item == null || count <= 0;
    }
}
