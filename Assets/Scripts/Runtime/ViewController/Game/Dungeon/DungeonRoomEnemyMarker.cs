using UnityEngine;

namespace RPG2D
{
    public class DungeonRoomEnemyMarker : MonoBehaviour
    {
        [SerializeField] private string roomId = "room_dungeon_001_combat_001";

        public string RoomId => roomId;
    }
}
