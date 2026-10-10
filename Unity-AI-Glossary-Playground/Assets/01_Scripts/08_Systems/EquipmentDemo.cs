using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GlossaryDemo.Systems
{
    // 능력치 하나 = 기본값 + 보정치(장비, 버프 ...) 들의 합
    [System.Serializable]
    public class Stat
    {
        public int baseValue;
        private readonly Dictionary<object, int> modifiers = new Dictionary<object, int>();

        public Stat(int baseValue) { this.baseValue = baseValue; }

        public int Bonus
        {
            get
            {
                int b = 0;
                foreach (var m in modifiers.Values) b += m;
                return b;
            }
        }

        public int Value => baseValue + Bonus;
        public void SetModifier(object source, int value) => modifiers[source] = value; // source = 어느 장비가 준 보정인지
        public void RemoveModifier(object source) => modifiers.Remove(source);
    }

    // 장비 / 스탯: 무기를 장착하면 공격력 Stat 에 보정치가 추가되고, 해제하면 빠짐
    public class EquipmentDemo : MonoBehaviour
    {
        [SerializeField] private int baseAttack = 10;
        [SerializeField] private int baseDefense = 5;
        [SerializeField] private int dummyDefense = 3;
        [SerializeField] private ItemData[] equipables;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI logText;
        [SerializeField] private Image weaponSlotIcon;
        [SerializeField] private Image armorSlotIcon;
        [SerializeField] private Image[] listHighlights;

        private Stat attack, defense;
        private ItemData weapon, armor;
        private int lastDamage;

        private void Awake()
        {
            attack = new Stat(baseAttack);
            defense = new Stat(baseDefense);
        }

        private void Start() => Refresh("왼쪽 목록에서 장비를 눌러 장착하세요");

        public void ToggleEquip(int index)
        {
            var item = equipables[index];
            if (item.type == ItemType.Weapon)
            {
                if (weapon == item) { Unequip(ref weapon, attack); Refresh($"{item.itemName} 해제 → 공격력 -{item.attackBonus}"); return; }
                int before = attack.Value;
                weapon = item;
                attack.SetModifier("weapon", item.attackBonus); // 같은 슬롯은 덮어씀 (무기 1개만)
                Refresh($"{item.itemName} 장착 → 공격력 {before} → {attack.Value}");
            }
            else if (item.type == ItemType.Armor)
            {
                if (armor == item) { Unequip(ref armor, defense); Refresh($"{item.itemName} 해제 → 방어력 -{item.defenseBonus}"); return; }
                int before = defense.Value;
                armor = item;
                defense.SetModifier("armor", item.defenseBonus);
                Refresh($"{item.itemName} 장착 → 방어력 {before} → {defense.Value}");
            }
        }

        private void Unequip(ref ItemData slot, Stat stat)
        {
            slot = null;
            stat.RemoveModifier(stat == attack ? "weapon" : "armor");
        }

        public void UnequipAll()
        {
            weapon = armor = null;
            attack.RemoveModifier("weapon");
            defense.RemoveModifier("armor");
            Refresh("모든 장비 해제");
        }

        public void AttackDummy()
        {
            lastDamage = Mathf.Max(1, attack.Value - dummyDefense);
            Refresh($"허수아비 공격! 데미지 = 공격력 {attack.Value} - 방어 {dummyDefense} = {lastDamage}");
        }

        private void Refresh(string msg)
        {
            if (statsText != null)
                statsText.text =
                    $"<b>능력치</b>\n" +
                    $"공격력  {attack.baseValue} <color=#7CFC8A>+ {attack.Bonus}</color> = <b>{attack.Value}</b>\n" +
                    $"방어력  {defense.baseValue} <color=#7CFC8A>+ {defense.Bonus}</color> = <b>{defense.Value}</b>\n\n" +
                    $"무기:   {(weapon != null ? weapon.itemName : "-")}\n방어구: {(armor != null ? armor.itemName : "-")}";
            if (logText != null) logText.text = msg;

            SetSlot(weaponSlotIcon, weapon);
            SetSlot(armorSlotIcon, armor);
            if (listHighlights != null)
                for (int i = 0; i < listHighlights.Length && i < equipables.Length; i++)
                    listHighlights[i].color = equipables[i] == weapon || equipables[i] == armor ? new Color(0.3f, 0.75f, 0.4f) : new Color32(45, 49, 61, 255);

            GlossaryInfoPanel.Status = $"공격력 {attack.Value} (기본 {attack.baseValue} + 장비 {attack.Bonus})   방어력 {defense.Value}\n{msg}";
        }

        private static void SetSlot(Image img, ItemData item)
        {
            if (img == null) return;
            img.enabled = item != null;
            if (item != null) img.sprite = item.icon;
        }
    }
}
