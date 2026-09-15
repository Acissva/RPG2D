using QFramework;

namespace RPG2D
{
    public interface IQuestSystem : ISystem
    {
        QuestStatus GetQuestStatus(string questId);
        bool CanAcceptQuest(string questId);
        bool AcceptQuest(string questId);
        bool CanCompleteQuest(string questId);
        bool CompleteQuest(string questId);
        void SetTrackedQuest(string questId);
        QuestTrackerSnapshot GetTrackedQuestSnapshot();
        void ResetForNewGame();
    }
}
