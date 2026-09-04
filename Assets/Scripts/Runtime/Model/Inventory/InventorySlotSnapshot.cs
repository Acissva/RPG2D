namespace RPG2D
{
    public class InventorySlotSnapshot
    {
        public string ItemId { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public ItemType ItemType { get; }
        public int Count { get; }
        public int MaxStack { get; }
        public string IconAssetKey { get; }

        public InventorySlotSnapshot(
            string itemId,
            string displayName,
            string description,
            ItemType itemType,
            int count,
            int maxStack,
            string iconAssetKey)
        {
            ItemId = itemId;
            DisplayName = displayName;
            Description = description;
            ItemType = itemType;
            Count = count;
            MaxStack = maxStack;
            IconAssetKey = iconAssetKey;
        }
    }
}
