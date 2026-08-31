using QFramework;

namespace RPG2D
{
    public class CloseTopUIPanelCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var uiModel = this.GetModel<IUIModel>();
            var topPanelId = uiModel.TopPanelId.Value;

            if(topPanelId == UIPanelId.None)
            {
                return;
            }

            this.GetSystem<IUISystem>().ClosePanel(topPanelId);
        }
    }
}

