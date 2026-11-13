using QFramework;

namespace RPG2D
{
    public sealed class SendNpcChatCommand : AbstractCommand<bool>
    {
        private readonly string message;

        public SendNpcChatCommand(string message)
        {
            this.message = message;
        }

        protected override bool OnExecute()
        {
            return this.GetSystem<INpcChatSystem>().SendMessage(message);
        }
    }
}
