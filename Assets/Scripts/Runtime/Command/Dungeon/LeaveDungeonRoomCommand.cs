using QFramework;

namespace RPG2D
{
    public class LeaveDungeonRoomCommand : AbstractCommand
    {
        private readonly string roomId;

        public LeaveDungeonRoomCommand(string roomId)
        {
            this.roomId = roomId;
        }

        protected override void OnExecute()
        {
            this.GetSystem<IDungeonSystem>().LeaveRoom(roomId);
        }
    }
}
