using System;
using System.Collections.Generic;

namespace GlossaryDemo.Systems
{
    // 슬롯 기반 인벤토리 (UI 와 분리된 순수 데이터/로직)
    // - 같은 아이템은 maxStack(기본 99)까지 한 칸에 겹침
    // - 넣을 때: ① 같은 아이템이 있는 칸부터 채우고 ② 남으면 빈 칸에 새로 넣음
    [Serializable]
    public class Inventory
    {
        public ItemStack[] slots;
        public event Action Changed;

        public Inventory(int size)
        {
            slots = new ItemStack[size];
        }

        public int Size => slots.Length;

        // 넣고 남은(못 넣은) 개수를 반환
        public int Add(ItemData item, int amount)
        {
            if (item == null || amount <= 0) return amount;

            // ① 기존 스택 채우기
            for (int i = 0; i < slots.Length && amount > 0; i++)
            {
                var s = slots[i];
                if (s == null || s.IsEmpty || s.item != item || s.count >= item.maxStack) continue;
                int put = Math.Min(amount, item.maxStack - s.count);
                s.count += put;
                amount -= put;
            }

            // ② 빈 칸에 새 스택
            for (int i = 0; i < slots.Length && amount > 0; i++)
            {
                if (slots[i] != null && !slots[i].IsEmpty) continue;
                int put = Math.Min(amount, item.maxStack);
                slots[i] = new ItemStack(item, put);
                amount -= put;
            }

            Changed?.Invoke();
            return amount;
        }

        public bool RemoveAt(int index, int amount = 1)
        {
            var s = slots[index];
            if (s == null || s.IsEmpty) return false;
            s.count -= amount;
            if (s.count <= 0) slots[index] = null;
            Changed?.Invoke();
            return true;
        }

        public int Count(ItemData item)
        {
            int total = 0;
            foreach (var s in slots) if (s != null && s.item == item) total += s.count;
            return total;
        }

        // 아이템 전체에서 amount 개 제거 (뒤쪽 칸부터)
        public bool Remove(ItemData item, int amount)
        {
            if (Count(item) < amount) return false;
            for (int i = slots.Length - 1; i >= 0 && amount > 0; i--)
            {
                var s = slots[i];
                if (s == null || s.item != item) continue;
                int take = Math.Min(amount, s.count);
                s.count -= take;
                amount -= take;
                if (s.count <= 0) slots[i] = null;
            }
            Changed?.Invoke();
            return true;
        }

        public int UsedSlots
        {
            get
            {
                int n = 0;
                foreach (var s in slots) if (s != null && !s.IsEmpty) n++;
                return n;
            }
        }

        public void Clear()
        {
            Array.Clear(slots, 0, slots.Length);
            Changed?.Invoke();
        }

        // 정렬: 같은 아이템을 합친 뒤 종류·이름 순으로 다시 채움
        public void Sort()
        {
            var totals = new Dictionary<ItemData, int>();
            foreach (var s in slots)
                if (s != null && !s.IsEmpty)
                    totals[s.item] = (totals.TryGetValue(s.item, out int n) ? n : 0) + s.count;
            var keys = new List<ItemData>(totals.Keys);
            keys.Sort((a, b) => a.type != b.type ? a.type.CompareTo(b.type) : string.CompareOrdinal(a.itemName, b.itemName));
            Array.Clear(slots, 0, slots.Length);
            foreach (var k in keys) Add(k, totals[k]);
            Changed?.Invoke();
        }
    }
}
