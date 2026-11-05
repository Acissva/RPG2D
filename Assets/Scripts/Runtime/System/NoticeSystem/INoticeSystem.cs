using QFramework;

namespace RPG2D
{
    public interface INoticeSystem : ISystem
    {
        bool RefreshNotice();
        void CancelRequest();
    }
}
