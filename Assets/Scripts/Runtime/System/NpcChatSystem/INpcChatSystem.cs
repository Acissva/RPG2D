using QFramework;

namespace RPG2D
{
    public interface INpcChatSystem : ISystem
    {
        bool BeginChat(string npcId);
        bool SendMessage(string message);
        void ClearChat();
        void EndChat();
    }
}
