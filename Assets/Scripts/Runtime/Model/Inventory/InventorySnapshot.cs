using System.Collections.Generic;

namespace RPG2D
{
    public class InventorySnapshot
    {
        public static InventorySnapshot Empty { get; } = new InventorySnapshot(
            false,
            0,
            new List<InventorySlotSnapshot>());

        public bool HasAnyItem { get; }
        public int Revision { get; }
        public IReadOnlyList<InventorySlotSnapshot> Items { get; }

        public InventorySnapshot(
            bool hasAnyItem,
            int revision,
            IReadOnlyList<InventorySlotSnapshot> items)
        {
            HasAnyItem = hasAnyItem;
            Revision = revision;
            Items = items ?? new List<InventorySlotSnapshot>();
        }
    }
}
