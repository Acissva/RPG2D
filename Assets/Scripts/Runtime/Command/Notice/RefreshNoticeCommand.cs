using QFramework;

namespace RPG2D
{
    public class RefreshNoticeCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetSystem<INoticeSystem>().RefreshNotice();
        }
    }
}
