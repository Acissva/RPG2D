using QFramework;

namespace RPG2D
{
    public class RemoveItemCommand : AbstractCommand<bool>
    {
        private readonly string itemId;
        private readonly int count;

        public RemoveItemCommand(string itemId, int count)
        {
            this.itemId = itemId;
            this.count = count;
        }

        protected override bool OnExecute()
        {
            return this.GetSystem<IInventorySystem>().RemoveItem(itemId, count);
        }
    }
}
