using QFramework;

namespace RPG2D
{
    public sealed class BeginNpcChatCommand : AbstractCommand<bool>
    {
        private readonly string npcId;

        public BeginNpcChatCommand(string npcId)
        {
            this.npcId = npcId;
        }

        protected override bool OnExecute()
        {
            return this.GetSystem<INpcChatSystem>().BeginChat(npcId);
        }
    }
}
