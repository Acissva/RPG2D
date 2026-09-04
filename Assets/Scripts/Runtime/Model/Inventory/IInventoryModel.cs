using QFramework;
using System.Collections.Generic;

namespace RPG2D
{
    public interface IInventoryModel : IModel
    {
        /// <summary>
        /// 背包里的物品堆叠，Key 是稳定 ItemId。
        /// </summary>
        Dictionary<string, InventoryItemStack> Items { get; }

        /// <summary>
        /// 背包修订版本号，用于通知 UI 刷新。
        /// </summary>
        BindableProperty<int> InventoryRevision { get; }
    }

    public class InventoryModel : AbstractModel, IInventoryModel
    {
        public Dictionary<string, InventoryItemStack> Items { get; } =
            new Dictionary<string, InventoryItemStack>();

        public BindableProperty<int> InventoryRevision { get; } =
            new BindableProperty<int>(0);

        protected override void OnInit()
        {
        }
    }
}
