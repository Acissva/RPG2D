using QFramework;

namespace RPG2D
{
    public class EnterDungeonRoomCommand : AbstractCommand
    {
        private readonly string roomId;

        public EnterDungeonRoomCommand(string roomId)
        {
            this.roomId = roomId;
        }

        protected override void OnExecute()
        {
            this.GetSystem<IDungeonSystem>().EnterRoom(roomId);
        }
    }
}
