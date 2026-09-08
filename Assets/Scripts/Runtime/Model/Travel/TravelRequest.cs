namespace RPG2D
{
    public struct TravelRequest
    {
        public string TargetSceneName;
        public string TargetSpawnPointId;

        public TravelRequest(
            string targetSceneName,
            string targetSpawnPointId)
        {
            TargetSceneName = targetSceneName;
            TargetSpawnPointId = targetSpawnPointId;
        }
    }
}
