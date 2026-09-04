using System;

namespace RPG2D
{
    public class InventoryItemStack
    {
        public string ItemId { get; }
        public int Count { get; private set; }

        public InventoryItemStack(string itemId, int count)
        {
            ItemId = itemId ?? string.Empty;
            Count = Math.Max(0, count);
        }

        public bool Add(int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            Count += amount;
            return true;
        }

        public bool Remove(int amount)
        {
            if (amount <= 0 || Count < amount)
            {
                return false;
            }

            Count -= amount;
            return true;
        }
    }
}
