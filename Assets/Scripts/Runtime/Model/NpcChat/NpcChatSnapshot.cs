using System;
using System.Collections.Generic;

namespace RPG2D
{
    public enum NpcChatState
    {
        Closed,
        Ready,
        Waiting,
        Failed
    }

    public sealed class NpcChatSnapshot
    {
        public static readonly NpcChatSnapshot Empty = new NpcChatSnapshot(NpcChatState.Closed, string.Empty, string.Empty, string.Empty, Array.Empty<NpcChatTurn>(), string.Empty, string.Empty);

        public NpcChatState State { get; }
        public string NpcId { get; }
        public string DisplayName { get; }
        public string WelcomeMessage { get; }
        public IReadOnlyList<NpcChatTurn> History { get; }
        public string PendingText { get; }
        public string Error { get; }

        public NpcChatSnapshot(NpcChatState state, string npcId, string displayName, string welcomeMessage, IEnumerable<NpcChatTurn> history, string pendingText, string error)
        {
            State = state;
            NpcId = npcId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            WelcomeMessage = welcomeMessage ?? string.Empty;
            // History : 先把输入序列复制为自己的数组，再用只读包装提供给外部。防止外部改动原数据
            History = Array.AsReadOnly(history == null ? Array.Empty<NpcChatTurn>() : new List<NpcChatTurn>(history).ToArray());
            PendingText = pendingText ?? string.Empty;
            Error = error ?? string.Empty;
        }
    }
}
