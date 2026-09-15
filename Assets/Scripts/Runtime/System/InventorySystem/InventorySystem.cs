using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class InventorySystem : AbstractSystem, IInventorySystem
    {
        private IInventoryModel inventoryModel;
        private IConfigSystem configSystem;

        protected override void OnInit()
        {
            inventoryModel = this.GetModel<IInventoryModel>();
            configSystem = this.GetSystem<IConfigSystem>();
        }

        public bool AddItem(string itemId, int count)
        {
            if (!IsValidItemRequest(itemId, count))
            {
                return false;
            }

            if (!inventoryModel.Items.TryGetValue(itemId, out InventoryItemStack stack))
            {
                stack = new InventoryItemStack(itemId, 0);
                inventoryModel.Items[itemId] = stack;
            }

            bool changed = stack.Add(count);

            if (changed)
            {
                IncreaseRevision();

                this.SendEvent(new ItemAddedEvent
                {
                    ItemId = itemId,
                    Count = count,
                    NewTotalCount = stack.Count
                });
            }

            return changed;
        }

        public bool RemoveItem(string itemId, int count)
        {
            if (!IsValidItemRequest(itemId, count))
            {
                return false;
            }

            if (!inventoryModel.Items.TryGetValue(itemId, out InventoryItemStack stack))
            {
                return false;
            }

            bool changed = stack.Remove(count);

            if (!changed)
            {
                return false;
            }

            if (stack.Count <= 0)
            {
                inventoryModel.Items.Remove(itemId);
            }

            IncreaseRevision();

            return true;
        }

        public int GetItemCount(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return 0;
            }

            return inventoryModel.Items.TryGetValue(itemId, out InventoryItemStack stack)
                ? stack.Count
                : 0;
        }

        public bool HasItem(string itemId, int count)
        {
            if (count <= 0)
            {
                return true;
            }

            return GetItemCount(itemId) >= count;
        }

        public InventorySnapshot GetInventorySnapshot()
        {
            if (inventoryModel.Items.Count <= 0)
            {
                return new InventorySnapshot(
                    false,
                    inventoryModel.InventoryRevision.Value,
                    new List<InventorySlotSnapshot>());
            }

            List<InventorySlotSnapshot> slots = new();

            foreach (InventoryItemStack stack in inventoryModel.Items.Values)
            {
                ItemConfig config = GetItemConfig(stack.ItemId);

                if (config == null)
                {
                    continue;
                }

                slots.Add(new InventorySlotSnapshot(
                    config.ItemId,
                    config.DisplayName,
                    config.Description,
                    config.Type,
                    stack.Count,
                    config.IconAssetKey));
            }

            return new InventorySnapshot(
                slots.Count > 0,
                inventoryModel.InventoryRevision.Value,
                slots);
        }

        private bool IsValidItemRequest(string itemId, int count)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                Debug.LogWarning("背包操作失败：ItemId 为空");
                return false;
            }

            if (count <= 0)
            {
                Debug.LogWarning($"背包操作失败：数量必须大于 0，ItemId={itemId}, Count={count}");
                return false;
            }

            if (GetItemConfig(itemId) == null)
            {
                Debug.LogWarning($"背包操作失败：找不到物品配置，ItemId={itemId}");
                return false;
            }

            return true;
        }

        private ItemConfig GetItemConfig(string itemId)
        {
            return configSystem.ItemConfigDatabase == null
                ? null
                : configSystem.ItemConfigDatabase.GetItemConfig(itemId);
        }

        private void IncreaseRevision()
        {
            inventoryModel.InventoryRevision.Value++;
        }

        public void ResetForNewGame()
        {
            if (inventoryModel.Items.Count == 0)
            {
                return;
            }

            inventoryModel.Items.Clear();
            IncreaseRevision();
        }
    }
}
