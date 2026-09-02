using QFramework;

namespace RPG2D
{
    public class AddQuestObjectiveProgressCommand : AbstractCommand
    {
        private readonly string questId;
        private readonly string objectiveId;
        private readonly int amount;

        public AddQuestObjectiveProgressCommand(string questId, string objectiveId, int amount = 1)
        {
            this.questId = questId;
            this.objectiveId = objectiveId;
            this.amount = amount;
        }

        protected override void OnExecute()
        {
            this.GetSystem<IQuestSystem>().AddObjectiveProgress(questId, objectiveId, amount);
        }
    }
}
