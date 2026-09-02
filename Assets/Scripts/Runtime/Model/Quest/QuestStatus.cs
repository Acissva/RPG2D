namespace RPG2D
{
    public enum QuestStatus
    {
        NotStarted,
        InProgress,
        /// <summary>
        /// 已完成任务目标，待交付
        /// </summary>
        ReadyToTurnIn,
        Completed,
    }
}
