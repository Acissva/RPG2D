using QFramework;

namespace RPG2D
{
    public class CompleteQuestCommand : AbstractCommand
    {
        private readonly string questId;

        public CompleteQuestCommand(string questId)
        {
            this.questId = questId;
        }

        protected override void OnExecute()
        {
            this.GetSystem<IQuestSystem>().CompleteQuest(questId);
        }
    }
}
