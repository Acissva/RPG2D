using UnityEngine;

namespace RPG2D
{
    public enum ItemType
    {
        Material,
        Consumable,
        Quest,
    }

    [CreateAssetMenu(
       fileName = "ItemConfig",
       menuName = "RPG2D/Config/Game/Item/Items/Item Config")]
    public class ItemConfig : ScriptableObject
    {
        [SerializeField] private string itemId = "item_herb_001";
        [SerializeField] private string displayName = "草药";
        [SerializeField, TextArea] private string description = "小镇附近常见的草药。";
        [SerializeField] private ItemType itemType = ItemType.Material;
        [SerializeField] private string iconAssetKey;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public ItemType Type => itemType;
        public string IconAssetKey => iconAssetKey;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                itemId = "item_herb_001";
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = itemId;
            }
        }
    }
}
