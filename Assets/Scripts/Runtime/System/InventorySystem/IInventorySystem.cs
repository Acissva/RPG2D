using QFramework;

namespace RPG2D
{
    public interface IInventorySystem : ISystem
    {
        bool AddItem(string itemId, int count);
        bool RemoveItem(string itemId, int count);
        int GetItemCount(string itemId);
        bool HasItem(string itemId, int count);
        InventorySnapshot GetInventorySnapshot();
        void ResetForNewGame();
    }
}
