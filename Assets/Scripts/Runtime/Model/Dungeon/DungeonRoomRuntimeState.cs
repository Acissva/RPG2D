namespace RPG2D
{
    public enum DungeonRoomState
    {
        None,
        Running,
        Completed
    }

    public class DungeonRoomRuntimeState
    {
        public string RoomId { get; }
        public DungeonRoomState State { get; set; }
        public int ActualEnemyCount { get; set; }
        public bool HasGrantedCompletionReward { get; set; }

        public bool IsCompleted => State == DungeonRoomState.Completed;

        public DungeonRoomRuntimeState(string roomId)
        {
            RoomId = roomId ?? string.Empty;
            State = DungeonRoomState.None;
        }
    }
}
