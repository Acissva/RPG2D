using System;

namespace RPG2D
{
    public struct TravelRequest
    {
        public Guid OperationId;
        public string TargetSceneName;
        public string TargetSpawnPointId;

        public TravelRequest(string targetSceneName, string targetSpawnPointId)
        {
            OperationId = Guid.NewGuid();
            TargetSceneName = targetSceneName;
            TargetSpawnPointId = targetSpawnPointId;
        }
    }
}
