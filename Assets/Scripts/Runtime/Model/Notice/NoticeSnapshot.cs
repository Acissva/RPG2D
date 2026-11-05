namespace RPG2D
{
    public enum NoticeRequestState
    {
        /// <summary>
        /// 尚未请求，或本轮请求已取消，此状态下可能保有过往结果记录
        /// </summary>
        Idle,
        /// <summary>
        /// 请求中
        /// </summary>
        Loading,
        /// <summary>
        /// 最近一次请求成功，保留成功结果
        /// </summary>
        Ready,
        /// <summary>
        /// 请求失败
        /// </summary>
        Failed
    }

    public sealed class NoticeSnapshot
    {
        public static NoticeSnapshot Empty = new NoticeSnapshot(NoticeRequestState.Idle, string.Empty, string.Empty, string.Empty);

        public NoticeRequestState State { get; }
        public string Title { get; }
        public string Content { get; }
        public string Error { get; }
        public bool HasNotice => !string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Content);

        public NoticeSnapshot(NoticeRequestState state, string title, string content, string error)
        {
            State = state;
            Title = title ?? string.Empty;
            Content = content ?? string.Empty;
            Error = error ?? string.Empty;
        }
    }
}
