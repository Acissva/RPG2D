using System;

namespace RPG2D
{
    public struct TravelCompletedEvent
    {
        public Guid OperationId;
        public string FromSceneName;
        public string SceneName;
        public string SpawnPointId;
    }
}
