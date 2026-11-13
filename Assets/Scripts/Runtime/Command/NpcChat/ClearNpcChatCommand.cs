using QFramework;

namespace RPG2D
{
    public sealed class ClearNpcChatCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetSystem<INpcChatSystem>().ClearChat();
        }
    }
}
