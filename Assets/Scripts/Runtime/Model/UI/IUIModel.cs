using QFramework;

namespace RPG2D
{
    public interface IUIModel : IModel
    {
        BindableProperty<UIPanelId> TopInputPanelId { get; }

        /// <summary>
        /// 是否阻挡玩家操作输入
        /// </summary>
        BindableProperty<bool> InputBlocked { get; }

        /// <summary>
        /// UI 状态修订版本号，仅在已打开面板栈、顶部输入面板或输入阻挡状态真实变化时递增，用于通知 UI 更新
        /// </summary>
        BindableProperty<int> UIRevision { get; }
    }

    public class UIModel : AbstractModel, IUIModel
    {
        public BindableProperty<UIPanelId> TopInputPanelId { get; } = new BindableProperty<UIPanelId>(UIPanelId.None);

        public BindableProperty<bool> InputBlocked { get; } = new BindableProperty<bool>(false);

        public BindableProperty<int> UIRevision { get; } = new BindableProperty<int>(0);

        protected override void OnInit()
        {

        }
    }
}

