using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class DungeonRoomController : RPG2DController
    {
        [SerializeField]
        private string roomId = "room_dungeon_001_combat_001";

        [SerializeField]
        private bool reportLeave = true;

        public string RoomId => roomId;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            this.SendCommand(
                new EnterDungeonRoomCommand(roomId));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!reportLeave || !other.CompareTag("Player"))
            {
                return;
            }

            this.SendCommand(
                new LeaveDungeonRoomCommand(roomId));
        }
    }
}
