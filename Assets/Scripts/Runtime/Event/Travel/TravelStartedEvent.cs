using System;

namespace RPG2D
{
    public struct TravelStartedEvent
    {
        public Guid OperationId;
        public string FromSceneName;
        public string TargetSceneName;
        public string TargetSpawnPointId;
    }
}
