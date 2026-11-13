using QFramework;

namespace RPG2D
{
    public sealed class EndNpcChatCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetSystem<INpcChatSystem>().EndChat();
        }
    }
}
