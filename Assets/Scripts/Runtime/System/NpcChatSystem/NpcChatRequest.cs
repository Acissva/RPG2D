using System;

namespace RPG2D
{
    [Serializable]
    public class NpcChatRequest
    {
        public int protocolVersion;
        public string requestId;
        public string npcId;
        public string message;
        public NpcChatTurnDto[] history;
    }
}
