using QFramework;
using System.Collections.Generic;

namespace RPG2D
{
    public interface IDungeonModel : IModel
    {
        string CurrentDungeonId { get; set; }
        DungeonState DungeonState { get; set; }
        Dictionary<string, DungeonRoomRuntimeState> RoomStates { get; }
        BindableProperty<int> DungeonRevision { get; }
        bool HasGrantedCompletionReward { get; set; }
        string LastReturnSceneName { get; set; }
        string LastReturnSpawnPointId { get; set; }
    }

    public class DungeonModel : AbstractModel, IDungeonModel
    {
        public string CurrentDungeonId { get; set; } = string.Empty;
        public DungeonState DungeonState { get; set; } = DungeonState.None;

        public Dictionary<string, DungeonRoomRuntimeState> RoomStates { get; } =
            new Dictionary<string, DungeonRoomRuntimeState>();

        public BindableProperty<int> DungeonRevision { get; } =
            new BindableProperty<int>(0);

        public bool HasGrantedCompletionReward { get; set; }

        public string LastReturnSceneName { get; set; } = string.Empty;
        public string LastReturnSpawnPointId { get; set; } = string.Empty;

        protected override void OnInit()
        {
        }
    }
}
