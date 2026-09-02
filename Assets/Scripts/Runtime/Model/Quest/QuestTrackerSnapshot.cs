namespace RPG2D
{
    public class QuestTrackerSnapshot
    {
        /// <summary>
        /// 任务状态快照
        /// </summary>
        /// Snapshot 的意思是“给 UI 看的一张快照”。HUD 后面只读它，不直接碰任务字典。
        public static QuestTrackerSnapshot Empty { get; } = new QuestTrackerSnapshot(
            false,
            string.Empty,
            string.Empty,
            QuestStatus.NotStarted,
            string.Empty,
            0,
            0);

        public bool HasQuest { get; }
        public string QuestId { get; }
        public string DisplayName { get; }
        public QuestStatus Status { get; }
        public string ObjectiveDescription { get; }
        public int CurrentCount { get; }
        public int RequiredCount { get; }

        public QuestTrackerSnapshot(
            bool hasQuest,
            string questId,
            string displayName,
            QuestStatus status,
            string objectiveDescription,
            int currentCount,
            int requiredCount)
        {
            HasQuest = hasQuest;
            QuestId = questId;
            DisplayName = displayName;
            Status = status;
            ObjectiveDescription = objectiveDescription;
            CurrentCount = currentCount;
            RequiredCount = requiredCount;
        }
    }
}
