namespace RPG2D
{
    /// <summary>
    /// 外部取消任务时使用的不透明凭证，不暴露计时任务本身
    /// </summary>
    public readonly struct TimerHandle
    {
        internal TimerHandle(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Id 为 0 时，天然表示无效句柄
        /// </summary>
        internal int Id { get; }

        public bool IsValid => Id != 0;
    }
}
