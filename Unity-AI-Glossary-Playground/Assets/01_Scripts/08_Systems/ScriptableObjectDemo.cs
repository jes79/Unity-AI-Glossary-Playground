using TMPro;
using UnityEngine;

namespace GlossaryDemo.Systems
{
    // ScriptableObject 데모: 아이템 데이터를 사용하는 쪽 (효과 적용)
    // 아이템 쪽 코드에는 수치가 하나도 없음 → 수치는 전부 ItemData 에셋에 있음
    public class ScriptableObjectDemo : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI logText;
        [SerializeField] private int maxHp = 100, maxMp = 50, baseAttack = 10, baseDefense = 5;

        private int hp = 40, mp = 10;
        private ItemData weapon, armor;

        private void Start() => Refresh("아이템 카드의 [사용] 버튼을 눌러 보세요");

        public void UseItem(ItemData item)
        {
            string msg;
            switch (item.type)
            {
                case ItemType.Consumable:
                    hp = Mathf.Min(maxHp, hp + item.healAmount);
                    mp = Mathf.Min(maxMp, mp + item.manaAmount);
                    msg = $"{item.itemName} 사용 → {item.EffectText}";
                    break;
                case ItemType.Weapon:
                    weapon = item;
                    msg = $"{item.itemName} 장착 → 공격력 +{item.attackBonus}";
                    break;
                case ItemType.Armor:
                    armor = item;
                    msg = $"{item.itemName} 장착 → 방어력 +{item.defenseBonus}";
                    break;
                default:
                    msg = $"{item.itemName} 은(는) 재료라 사용할 수 없음 (판매가 {item.SellPrice} G)";
                    break;
            }
            Refresh(msg);
        }

        public void TakeHit()
        {
            hp = Mathf.Max(0, hp - 15);
            mp = Mathf.Max(0, mp - 5);
            Refresh("피격! HP -15, MP -5");
        }

        private void Refresh(string msg)
        {
            int atk = baseAttack + (weapon != null ? weapon.attackBonus : 0);
            int def = baseDefense + (armor != null ? armor.defenseBonus : 0);
            if (statsText != null)
                statsText.text = $"<b>플레이어</b>\nHP {hp}/{maxHp}   MP {mp}/{maxMp}\n" +
                                 $"공격력 {atk}   방어력 {def}\n" +
                                 $"무기: {(weapon != null ? weapon.itemName : "-")}   방어구: {(armor != null ? armor.itemName : "-")}";
            if (logText != null) logText.text = msg;
            GlossaryInfoPanel.Status = $"마지막 행동: {msg}";
        }
    }
}
