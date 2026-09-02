using QFramework;

namespace RPG2D
{
    public class SetTrackedQuestCommand : AbstractCommand
    {
        private readonly string questId;

        public SetTrackedQuestCommand(string questId)
        {
            this.questId = questId;
        }

        protected override void OnExecute()
        {
            this.GetSystem<IQuestSystem>().SetTrackedQuest(questId);
        }
    }
}
