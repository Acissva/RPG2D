using QFramework;

namespace RPG2D
{
    public interface IGameFlowSystem : ISystem
    {
        void StartNewGame();
        void ReturnToStartScene();
    }
}
