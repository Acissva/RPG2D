using System;

namespace RPG2D
{
    public class QuestObjectiveProgress
    {
        public string ObjectiveId { get; }
        public int CurrentCount { get; private set; }
        public int RequiredCount { get; }

        public bool IsCompleted => CurrentCount >= RequiredCount;

        public QuestObjectiveProgress(string objectiveId, int requiredCount)
        {
            ObjectiveId = objectiveId;
            RequiredCount = Math.Max(1, requiredCount);
            CurrentCount = 0;
        }

        public bool AddProgress(int amount)
        {
            if (amount <= 0 || IsCompleted)
            {
                return false;
            }

            int oldCount = CurrentCount;
            CurrentCount = Math.Min(CurrentCount + amount, RequiredCount);

            return CurrentCount != oldCount;
        }
    }
}
