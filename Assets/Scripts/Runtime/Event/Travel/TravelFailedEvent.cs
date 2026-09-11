using System;

namespace RPG2D
{
    public struct TravelFailedEvent
    {
        public Guid OperationId;
        public string FromSceneName;
        public string TargetSceneName;
        public string TargetSpawnPointId;
        public string Reason;
    }
}
