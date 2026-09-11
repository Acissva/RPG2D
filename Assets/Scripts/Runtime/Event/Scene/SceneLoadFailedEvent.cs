using System;

namespace RPG2D
{
    public struct SceneLoadFailedEvent
    {
        public Guid OperationId;
        public string FromSceneName;
        public string ToSceneName;
        public string Reason;
    }
}
