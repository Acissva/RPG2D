using QFramework;

namespace RPG2D
{
    public class RefreshServerNoticeCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetSystem<IGameServerSystem>().RefreshNotice();
        }
    }
}
