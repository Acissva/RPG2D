namespace RPG2D
{
    public struct TravelFailedEvent
    {
        public string FromSceneName;
        public string TargetSceneName;
        public string TargetSpawnPointId;
        public string Reason;
    }
}
