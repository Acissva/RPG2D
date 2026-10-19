using QFramework;

namespace RPG2D
{
    public interface IGameServerSystem : ISystem
    {
        bool RefreshNotice();
        void CancelRequest();
    }
}
