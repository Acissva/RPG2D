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
                Debug.LogError($"{nameof(DungeonConfig)} {dungeonId} 的 SceneName 为空，已恢复默认值。", this);
                sceneName = "Dungeon_GT_Test_001";
            }

            if (string.IsNullOrWhiteSpace(entranceSpawnPointId))
            {
                Debug.LogError($"{nameof(DungeonConfig)} {dungeonId} 的 EntranceSpawnPointId 为空。", this);
            }

            if (string.IsNullOrWhiteSpace(returnSceneName))
            {
                Debug.LogError($"{nameof(DungeonConfig)} {dungeonId} 的 ReturnSceneName 为空。", this);
            }

            if (string.IsNullOrWhiteSpace(returnSpawnPointId))
            {
                Debug.LogError($"{nameof(DungeonConfig)} {dungeonId} 的 ReturnSpawnPointId 为空。", this);
            }

            rooms ??= new List<DungeonRoomConfig>();
            completionRewards ??= new List<RewardItemConfig>();

            if (rooms.Count == 0)
            {
                Debug.LogError($"{nameof(DungeonConfig)} {dungeonId} 没有配置任何房间。", this);
            }

            HashSet<string> roomIds = new();

            for (int i = 0; i < rooms.Count; i++)
            {
                DungeonRoomConfig room = rooms[i];

                if (room == null)
                {
                    Debug.LogError($"{nameof(DungeonConfig)} {dungeonId} 的 Rooms[{i}] 为空。", this);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(room.RoomId))
                {
                    Debug.LogError($"{nameof(DungeonConfig)} {dungeonId} 中存在 RoomId 为空的房间，索引={i}。", this);
                }

                room.Validate();

                if (!roomIds.Add(room.RoomId))
                {
                    Debug.LogError($"{nameof(DungeonConfig)} {dungeonId} 中存在重复 RoomId：{room.RoomId}。", this);
                }

                for (int rewardIndex = 0; rewardIndex < room.CompletionRewards.Count; rewardIndex++)
                {
                    RewardItemConfig reward = room.CompletionRewards[rewardIndex];

                    if (reward == null)
                    {
                        Debug.LogError($"{nameof(DungeonConfig)} {dungeonId} 的房间 {room.RoomId} 存在空奖励条目，索引={rewardIndex}。", this);
                    }
                }
            }

            for (int i = 0; i < completionRewards.Count; i++)
            {
                RewardItemConfig reward = completionRewards[i];

                if (reward == null)
                {
                    Debug.LogError($"{nameof(DungeonConfig)} {dungeonId} 的 CompletionRewards[{i}] 为空。", this);
                    continue;
                }

                reward.Validate();
            }
        }
    }

    [Serializable]
    public class DungeonRoomConfig
    {
        [SerializeField] private string roomId = "room_gt_test_combat_001";
        [SerializeField] private DungeonRoomType roomType = DungeonRoomType.Combat;
        [SerializeField] private bool requiredForDungeonCompletion = true;
        [SerializeField, Min(0)] private int expectedEnemyCount = 1;
        [SerializeField] private List<RewardItemConfig> completionRewards = new();

        public string RoomId => roomId;
        public DungeonRoomType RoomType => roomType;
        public bool RequiredForDungeonCompletion => requiredForDungeonCompletion;
        public int ExpectedEnemyCount => expectedEnemyCount;
        public IReadOnlyList<RewardItemConfig> CompletionRewards => completionRewards;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                roomId = "room_gt_test_combat_001";
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
