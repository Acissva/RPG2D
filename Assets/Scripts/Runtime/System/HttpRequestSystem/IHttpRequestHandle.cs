using System;

namespace RPG2D
{
    public interface IHttpRequestHandle : IDisposable
    {
        bool IsDone { get; }
        void Cancel();
    }
}
