namespace RPG2D
{
    public struct QuestObjectiveProgressChangedEvent
    {
        public string QuestId;
        public string ObjectiveId;
        public int CurrentCount;
        public int RequiredCount;
    }
}
