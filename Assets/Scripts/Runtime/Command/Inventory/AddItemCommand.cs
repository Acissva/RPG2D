using QFramework;

namespace RPG2D
{
    public class AddItemCommand : AbstractCommand<bool>
    {
        private readonly string itemId;
        private readonly int count;

        public AddItemCommand(string itemId, int count)
        {
            this.itemId = itemId;
            this.count = count;
        }

        protected override bool OnExecute()
        {
            return this.GetSystem<IInventorySystem>().AddItem(itemId, count);
        }
    }
}
