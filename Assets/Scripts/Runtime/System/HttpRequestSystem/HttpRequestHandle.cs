using System;

namespace RPG2D
{
    public sealed class HttpRequestHandle : IHttpRequestHandle
    {
        private Action onCancel;

        public bool IsDone { get; private set; }

        internal HttpRequestHandle(Action onCancel)
        {
            this.onCancel = onCancel ?? throw new ArgumentNullException(nameof(onCancel));
        }

        public void Cancel()
        {
            if (IsDone)
            {
                return;
            }

            Action cancel = onCancel;
            Complete();
            cancel?.Invoke();
        }

        public void Dispose()
        {
            Cancel();
        }

        internal void Complete()
        {
            IsDone = true;
            onCancel = null;
        }
    }
}
