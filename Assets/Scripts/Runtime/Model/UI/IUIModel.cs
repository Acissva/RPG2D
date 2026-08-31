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
    }

    public class UIModel : AbstractModel, IUIModel
    {
        public BindableProperty<UIPanelId> TopPanelId { get; } = new BindableProperty<UIPanelId>(UIPanelId.None);

        public BindableProperty<bool> InputBlocked { get; } = new BindableProperty<bool>(false);

        protected override void OnInit()
        {
            
        }
    }
}

