using QFramework;
using System;

namespace RPG2D
{
    public interface IRuntimeUpdateSystem : ISystem
    {
        void RegisterUpdate(Action onUpdate);
        void UnregisterUpdate(Action onUpdate);
    }
}
