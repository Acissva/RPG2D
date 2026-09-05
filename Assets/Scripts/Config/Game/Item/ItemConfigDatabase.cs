using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    [CreateAssetMenu(
       fileName = "ItemConfigDatabase",
       menuName = "RPG2D/Config/Game/Item/Item Config Database")]
    public class ItemConfigDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemConfig> items = new();

        public IReadOnlyList<ItemConfig> Items => items;

        public ItemConfig GetItemConfig(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            for (int i = 0; i < items.Count; i++)
            {
                ItemConfig item = items[i];

                if (item != null && item.ItemId == itemId)
                {
                    return item;
                }
            }

            return null;
        }

        public bool ContainsItem(string itemId)
        {
            return GetItemConfig(itemId) != null;
        }

        private void OnValidate()
        {
            items ??= new List<ItemConfig>();
        }
    }
}
