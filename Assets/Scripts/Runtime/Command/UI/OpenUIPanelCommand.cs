using QFramework;

namespace RPG2D
{
    public class OpenUIPanelCommand : AbstractCommand
    {
        private readonly UIPanelId panelId;
        private readonly bool closeOtherPanels;

        public OpenUIPanelCommand(UIPanelId panelId, bool closeOtherPanels = true)
        {
            this.panelId = panelId;
            this.closeOtherPanels = closeOtherPanels;
        }

        protected override void OnExecute()
        {
            this.GetSystem<IUISystem>().OpenPanel(panelId, closeOtherPanels);
        }
    }
}

