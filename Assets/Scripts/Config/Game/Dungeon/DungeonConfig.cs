using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public enum DungeonRoomType
    {
        Start,
        Combat,
        /// <summary>
        /// 宝藏
        /// </summary>
        Treasure,
        Exit,
        Boss
    }

    public enum DungeonCompletionRule
    {
        RequiredRoomsCompleted,
        SceneLoaded
    }

    [CreateAssetMenu(
        fileName = "DungeonConfig",
        menuName = "RPG2D/Config/Game/Dungeon/Dungeons/Dungeon Config")]
    public class DungeonConfig : ScriptableObject
    {
        [SerializeField] private string dungeonId = "dungeon_gt_test_001";
        [SerializeField] private string displayName = "草地小镇测试副本";
        [SerializeField] private string sceneName = "Dungeon_GT_Test_001";
        [SerializeField] private string entranceSpawnPointId = "spawn_dungeon_start";
        [SerializeField] private string returnSceneName = "MainGameScene";
        [SerializeField] private string returnSpawnPointId = "spawn_main_dungeon_gate";
        [SerializeField] private List<DungeonRoomConfig> rooms = new();
        [SerializeField] private List<RewardItemConfig> completionRewards = new();

        public string DungeonId => dungeonId;
        public string DisplayName => displayName;
        public string SceneName => sceneName;
        public string EntranceSpawnPointId => entranceSpawnPointId;
        public string ReturnSceneName => returnSceneName;
        public string ReturnSpawnPointId => returnSpawnPointId;
        public IReadOnlyList<DungeonRoomConfig> Rooms => rooms;
        public IReadOnlyList<RewardItemConfig> CompletionRewards => completionRewards;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(dungeonId))
            {
                dungeonId = "dungeon_gt_test_001";
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = dungeonId;
            }

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                sceneName = "Dungeon_GT_Test_001";
            }

            rooms ??= new List<DungeonRoomConfig>();
            completionRewards ??= new List<RewardItemConfig>();

            for (int i = 0; i < rooms.Count; i++)
            {
                rooms[i]?.Validate();
            }

            for (int i = 0; i < completionRewards.Count; i++)
            {
                completionRewards[i]?.Validate();
            }
        }
    }

    [Serializable]
    public class DungeonRoomConfig
    {
        [SerializeField] private string roomId = "room_gt_test_combat_001";
        [SerializeField] private DungeonRoomType roomType = DungeonRoomType.Combat;
        [SerializeField] private bool requiredForDungeonCompletion = true;
        [SerializeField] private string encounterId = "encounter_gt_test_001";
        [SerializeField, Min(0)] private int expectedEnemyCount = 1;
        [SerializeField] private List<RewardItemConfig> completionRewards = new();

        public string RoomId => roomId;
        public DungeonRoomType RoomType => roomType;
        public bool RequiredForDungeonCompletion => requiredForDungeonCompletion;
        public string EncounterId => encounterId;
        public int ExpectedEnemyCount => expectedEnemyCount;
        public IReadOnlyList<RewardItemConfig> CompletionRewards => completionRewards;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                roomId = "room_gt_test_combat_001";
            }

            if (string.IsNullOrWhiteSpace(encounterId))
            {
                encounterId = roomId;
            }

            if (expectedEnemyCount < 0)
            {
                expectedEnemyCount = 0;
            }

            completionRewards ??= new List<RewardItemConfig>();

            for (int i = 0; i < completionRewards.Count; i++)
            {
                completionRewards[i]?.Validate();
            }
        }
    }
}
