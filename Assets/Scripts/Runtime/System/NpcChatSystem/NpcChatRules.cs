using System;

namespace RPG2D
{
    public static class NpcChatRules
    {
        public const int ProtocolVersion = 1;
        public const string GuideNpcId = "npc_ai_guide_001";
        public const int MaxMessageLength = 256;
        public const int MaxReplyLength = 1024;
        public const int MaxHistoryTurns = 4;
        public const int MaxRequestBytes = 64 * 1024;
        public const int MaxResponseBytes = 16 * 1024;

        public static bool IsValidRequestId(string value)
        {
            if (value == null || value.Length != 32) return false;
            foreach (char character in value)
            {
                if (!Uri.IsHexDigit(character)) return false;
            }
            return true;
        }

        public static bool IsValidText(string value, int maximum)
        {
            if (string.IsNullOrEmpty(value) || value.Length > maximum) return false;
            bool hasContent = false;
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                if (char.IsHighSurrogate(character))
                {
                    if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1])) return false;
                    index++;
                }
                else if (char.IsLowSurrogate(character))
                {
                    return false;
                }
                if (!char.IsWhiteSpace(character) && (character < '\u001c' || character > '\u001f')) hasContent = true;
            }
            return hasContent;
        }
    }
}
