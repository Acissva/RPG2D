using QFramework;

namespace RPG2D
{
    public class ToggleUIPanelCommand : AbstractCommand
    {
        private readonly UIPanelId panelId;
        private readonly bool closeOtherPanels;

        public ToggleUIPanelCommand(UIPanelId panelId, bool closeOtherPanels = true)
        {
            this.panelId = panelId;
            this.closeOtherPanels = closeOtherPanels;
        }

        protected override void OnExecute()
        {
            this.GetSystem<IUISystem>().TogglePanel(panelId, closeOtherPanels);
        }
    }
}
