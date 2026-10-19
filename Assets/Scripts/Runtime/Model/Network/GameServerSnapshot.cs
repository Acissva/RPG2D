namespace RPG2D
{
    public enum GameServerRequestState
    {
        Idle,
        Loading,
        Ready,
        Failed
    }

    public sealed class GameServerSnapshot
    {
        public static GameServerSnapshot Empty = new GameServerSnapshot(GameServerRequestState.Idle, string.Empty, string.Empty, string.Empty);

        public GameServerRequestState State { get; }
        public string Title { get; }
        public string Content { get; }
        public string Error { get; }
        public bool HasNotice => !string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Content);

        public GameServerSnapshot(GameServerRequestState state, string title, string content, string error)
        {
            State = state;
            Title = title ?? string.Empty;
            Content = content ?? string.Empty;
            Error = error ?? string.Empty;
        }
    }
}
