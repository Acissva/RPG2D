using QFramework;

namespace RPG2D
{
    public class AcceptQuestCommand : AbstractCommand
    {
        private readonly string questId;

        public AcceptQuestCommand(string questId)
        {
            this.questId = questId;
        }

        protected override void OnExecute()
        {
            this.GetSystem<IQuestSystem>().AcceptQuest(questId);
        }
    }
}
