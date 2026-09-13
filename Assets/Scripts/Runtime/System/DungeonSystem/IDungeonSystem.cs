using QFramework;

namespace RPG2D
{
    public interface IDungeonSystem : ISystem
    {
        bool CanEnterDungeon(string dungeonId);
        bool EnterDungeon(string dungeonId);
        bool ExitCurrentDungeon();
        bool IsCurrentDungeonCompleted();
        bool IsRoomLocked(string roomId);
        void EnterRoom(string roomId);
        void LeaveRoom(string roomId);
        void ResetForNewGame();
        bool FailCurrentDungeon();
    }
}
