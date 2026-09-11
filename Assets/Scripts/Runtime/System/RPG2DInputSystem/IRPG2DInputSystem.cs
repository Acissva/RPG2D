using QFramework;

namespace RPG2D
{
    public interface IRPG2DInputSystem : ISystem
    {
        IInputReader InputReader { get; }
    }
}
