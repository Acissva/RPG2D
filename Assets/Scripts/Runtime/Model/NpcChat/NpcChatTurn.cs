namespace RPG2D
{
    public sealed class NpcChatTurn
    {
        public string PlayerText { get; }
        public string NpcText { get; }

        public NpcChatTurn(string playerText, string npcText)
        {
            PlayerText = playerText ?? string.Empty;
            NpcText = npcText ?? string.Empty;
        }
    }
}
