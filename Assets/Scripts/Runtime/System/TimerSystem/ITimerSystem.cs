using QFramework;
using System;

namespace RPG2D
{
    public interface ITimerSystem : ISystem
    {
        float CurrentSeconds { get; }
        TimerHandle Schedule(float seconds, Action callback, TimerScope scope);
        bool Cancel(TimerHandle handle);
        void CancelScope(TimerScope scope);
    }
}
