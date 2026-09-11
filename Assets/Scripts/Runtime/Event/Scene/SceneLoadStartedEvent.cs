using System;

namespace RPG2D
{
    public struct SceneLoadStartedEvent
    {
        public Guid OperationId;
        public string FromSceneName;
        public string ToSceneName;
    }

}

