using System;

namespace RPG2D
{
    [Serializable]
    public class GameServerNoticeResponse
    {
        public int protocolVersion;
        public string title;
        public string content;
    }
}
