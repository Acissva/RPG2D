using QFramework;
using System.Text;
using TMPro;
using UnityEngine;

namespace RPG2D
{
    public class InventoryPanelController : UIPanelController
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI itemListText;
        [SerializeField] private string title = "背包";
        [SerializeField] private string emptyText = "暂无物品";

        //StringBuilder 是更好的习惯，尤其 UI 可能频繁刷新时，相比直接字符串相加，可以减少不必要的字符串分配
        private readonly StringBuilder stringBuilder = new();
        private IInventorySystem inventorySystem;

        private void Awake()
        {
            inventorySystem = this.GetSystem<IInventorySystem>();

            this.RegisterEvent<InventoryChangedEvent>(OnInventoryChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            Refresh();
        }

        protected override void OnOpen()
        {
            Refresh();
        }

        private void OnInventoryChanged(InventoryChangedEvent e)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (itemListText == null || inventorySystem == null)
            {
                return;
            }

            InventorySnapshot snapshot = inventorySystem.GetInventorySnapshot();

            if (!snapshot.HasAnyItem)
            {
                itemListText.text = emptyText;
                return;
            }

            stringBuilder.Clear();

            for (int i = 0; i < snapshot.Items.Count; i++)
            {
                InventorySlotSnapshot item = snapshot.Items[i];

                stringBuilder.Append(item.DisplayName);
                stringBuilder.Append(" x");
                stringBuilder.Append(item.Count);

                if (i < snapshot.Items.Count - 1)
                {
                    stringBuilder.AppendLine();
                }
            }

            itemListText.text = stringBuilder.ToString();
        }
    }
}
