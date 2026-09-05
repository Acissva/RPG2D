using QFramework;

namespace RPG2D
{
    public interface IUIModel : IModel
    {
        BindableProperty<UIPanelId> TopPanelId { get; }

        /// <summary>
        /// 是否阻挡玩家操作输入
        /// </summary>
        BindableProperty<bool> InputBlocked { get; }

        /// <summary>
        /// UI 状态修订版本号，用于通知依赖 UI 打开状态的表现刷新。
        /// </summary>
        BindableProperty<int> UIRevision { get; }
    }

    public class UIModel : AbstractModel, IUIModel
    {
        public BindableProperty<UIPanelId> TopPanelId { get; } = new BindableProperty<UIPanelId>(UIPanelId.None);

        public BindableProperty<bool> InputBlocked { get; } = new BindableProperty<bool>(false);

        public BindableProperty<int> UIRevision { get; } = new BindableProperty<int>(0);

        protected override void OnInit()
        {

        }
    }
}

