using System;

namespace RPG2D
{
    [Serializable]
    public class NpcChatResponse
    {
        public int protocolVersion;
        public string requestId;
        public string npcId;
        public string reply;
    }
}
