using QFramework;

namespace RPG2D
{
    public class CloseUIPanelCommand : AbstractCommand
    {
        private readonly UIPanelId panelId;

        public CloseUIPanelCommand(UIPanelId panelId)
        {
            this.panelId = panelId;
        }

        protected override void OnExecute()
        {
            this.GetSystem<IUISystem>().ClosePanel(panelId);
        }
    }
}
