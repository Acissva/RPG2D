using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public interface IDungeonSystem : ISystem
    {
        bool CanEnterDungeon(string dungeonId);
        bool EnterDungeon(string dungeonId);
        bool ExitCurrentDungeon();
        bool IsCurrentDungeonCompleted();
        bool CompleteRoom(string roomId);
        bool IsRoomCompleted(string roomId);
        void RefreshDungeonCompletion();
        string GetPlayerRoomId();
        bool IsRoomLocked(string roomId);
        DungeonRoomRuntimeState GetRoomRuntimeState(string roomId);
        void EnterRoom(string roomId);
        void LeaveRoom(string roomId);
        void ResetForNewGame();
        bool FailCurrentDungeon();
    }

    public class DungeonSystem : AbstractSystem, IDungeonSystem
    {
        private IDungeonModel dungeonModel;
        private IConfigSystem configSystem;
        private ISceneSystem sceneSystem;
        private IEnemyRegistrySystem enemyRegistrySystem;
        private IRewardSystem rewardSystem;
        private ITravelSystem travelSystem;

        protected override void OnInit()
        {
            dungeonModel = this.GetModel<IDungeonModel>();
            configSystem = this.GetSystem<IConfigSystem>();
            sceneSystem = this.GetSystem<ISceneSystem>();
            enemyRegistrySystem = this.GetSystem<IEnemyRegistrySystem>();
            rewardSystem = this.GetSystem<IRewardSystem>();
            travelSystem = this.GetSystem<ITravelSystem>();

            this.RegisterEvent<EnemyDeadEvent>(OnEnemyDead);
            this.RegisterEvent<TravelCompletedEvent>(OnTravelCompleted);
            this.RegisterEvent<TravelFailedEvent>(OnTravelFailed);
        }

        public bool CanEnterDungeon(string dungeonId)
        {
            if (string.IsNullOrWhiteSpace(dungeonId))
            {
                return false;
            }

            if (sceneSystem.IsLoading || travelSystem.IsTraveling)
            {
                return false;
            }

            DungeonConfig config = GetDungeonConfig(dungeonId);

            if (config == null || string.IsNullOrWhiteSpace(config.SceneName))
            {
                return false;
            }

            return true;
        }

        public bool EnterDungeon(string dungeonId)
        {
            if (!CanEnterDungeon(dungeonId))
            {
                Debug.LogWarning($"Can not enter dungeon: {dungeonId}");
                return false;
            }

            DungeonConfig config = GetDungeonConfig(dungeonId);

            dungeonModel.CurrentDungeonId = config.DungeonId;
            dungeonModel.DungeonState = DungeonState.Loading;
            dungeonModel.PlayerRoomId = string.Empty;
            dungeonModel.HasGrantedCompletionReward = false;
            InitializeRoomStates(config);
            dungeonModel.LastReturnSceneName = config.ReturnSceneName;
            dungeonModel.LastReturnSpawnPointId = config.ReturnSpawnPointId;
            IncreaseRevision();

            bool accepted = travelSystem.Travel(new TravelRequest(config.SceneName, config.EntranceSpawnPointId));

            if (accepted)
            {
                return true;
            }

            Debug.LogError($"进入副本迁移请求被拒绝：{config.DungeonId}");

            ClearCurrentDungeon();
            return false;
        }

        public bool ExitCurrentDungeon()
        {
            if (!IsCurrentDungeonCompleted())
            {
                Debug.LogWarning("Can not exit dungeon before it is completed.");
                return false;
            }

            string returnSceneName =
                string.IsNullOrWhiteSpace(
                    dungeonModel.LastReturnSceneName)
                    ? SceneSystem.MainGameSceneName
                    : dungeonModel.LastReturnSceneName;

            string returnSpawnPointId =
                dungeonModel.LastReturnSpawnPointId;

            if (string.IsNullOrWhiteSpace(returnSpawnPointId))
            {
                Debug.LogError("退出副本失败：返回出生点 ID 为空。");
                return false;
            }

            dungeonModel.DungeonState = DungeonState.Exiting;
            IncreaseRevision();

            bool accepted = travelSystem.Travel(new TravelRequest(returnSceneName, returnSpawnPointId));

            if (accepted)
            {
                return true;
            }

            dungeonModel.DungeonState = DungeonState.Completed;
            IncreaseRevision();

            Debug.LogError("退出副本迁移请求被拒绝。");
            return false;
        }

        public bool IsCurrentDungeonCompleted()
        {
            return dungeonModel.DungeonState == DungeonState.Completed;
        }

        private void OnTravelCompleted(TravelCompletedEvent travelCompletedEvent)
        {
            if (dungeonModel.DungeonState == DungeonState.Loading)
            {
                TryStartDungeonAfterSceneLoaded(travelCompletedEvent.SceneName);
                return;
            }

            if (dungeonModel.DungeonState == DungeonState.Exiting || dungeonModel.DungeonState == DungeonState.Failed)
            {
                ClearCurrentDungeon();
            }
        }

        private void OnTravelFailed(TravelFailedEvent travelFailedEvent)
        {
            if (dungeonModel.DungeonState == DungeonState.Loading)
            {
                Debug.LogError($"进入副本迁移失败：" + $"{travelFailedEvent.Reason}");

                ClearCurrentDungeon();
                return;
            }

            if (dungeonModel.DungeonState == DungeonState.Exiting)
            {
                dungeonModel.DungeonState = DungeonState.Completed;
                IncreaseRevision();

                Debug.LogError($"退出副本迁移失败：" + $"{travelFailedEvent.Reason}");
            }
        }

        private bool CompleteRoom(DungeonConfig config, DungeonRoomConfig roomConfig)
        {
            if (config == null || roomConfig == null || string.IsNullOrWhiteSpace(roomConfig.RoomId))
            {
                return false;
            }

            DungeonRoomRuntimeState roomState = GetRoomRuntimeState(roomConfig.RoomId);

            if (roomState == null)
            {
                Debug.LogWarning($"Can not find completed dungeon room runtime state: {roomConfig.RoomId}");
                return false;
            }

            if (roomState.State == DungeonRoomState.Completed)
            {
                return false;
            }

            if (!roomState.HasGrantedCompletionReward && roomConfig.CompletionRewards.Count > 0)
            {
                roomState.HasGrantedCompletionReward = true;

                rewardSystem.GrantRewards(
                    roomConfig.CompletionRewards,
                    RewardSourceType.Dungeon,
                    $"{config.DungeonId}/{roomConfig.RoomId}");
            }

            roomState.State = DungeonRoomState.Completed;
            roomState.IsLocked = false;
            IncreaseRevision();

            return true;
        }

        public bool CompleteRoom(string roomId)
        {
            DungeonConfig config = GetCurrentDungeonConfig();

            if (config == null)
            {
                return false;
            }

            DungeonRoomConfig roomConfig = GetRoomConfig(config, roomId);

            if (roomConfig == null)
            {
                Debug.LogWarning($"Can not find completed dungeon room: {roomId}");
                return false;
            }

            bool completed = CompleteRoom(config, roomConfig);
            RefreshDungeonCompletion();

            return completed;
        }

        public bool IsRoomCompleted(string roomId)
        {
            DungeonRoomRuntimeState roomState = GetRoomRuntimeState(roomId);
            return roomState != null && roomState.IsCompleted;
        }

        public string GetPlayerRoomId()
        {
            return dungeonModel.PlayerRoomId;
        }

        public bool IsRoomLocked(string roomId)
        {
            DungeonRoomRuntimeState roomState = GetRoomRuntimeState(roomId);
            return roomState != null && roomState.IsLocked;
        }

        public DungeonRoomRuntimeState GetRoomRuntimeState(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return null;
            }

            dungeonModel.RoomStates.TryGetValue(
                roomId,
                out DungeonRoomRuntimeState roomState);

            return roomState;
        }

        public void EnterRoom(string roomId)
        {
            if (dungeonModel.DungeonState != DungeonState.Running)
            {
                return;
            }

            DungeonConfig config = GetCurrentDungeonConfig();
            DungeonRoomConfig roomConfig = GetRoomConfig(config, roomId);

            if (roomConfig == null)
            {
                Debug.LogWarning($"Can not enter dungeon room: {roomId}");
                return;
            }

            DungeonRoomRuntimeState roomState = GetRoomRuntimeState(roomId);

            if (roomState == null)
            {
                Debug.LogWarning(
                    $"Can not find dungeon room runtime state: {roomId}");
                return;
            }

            bool shouldLockRoom =
                IsCombatOrBossRoom(roomConfig)
                && !roomState.IsCompleted;

            bool hasChanged =
                dungeonModel.PlayerRoomId != roomId
                || !roomState.HasPlayerEntered
                || roomState.IsLocked != shouldLockRoom;

            if (!hasChanged)
            {
                return;
            }

            dungeonModel.PlayerRoomId = roomId;
            roomState.HasPlayerEntered = true;
            roomState.IsLocked = shouldLockRoom;
            IncreaseRevision();
        }

        public void LeaveRoom(string roomId)
        {
            if (dungeonModel.DungeonState != DungeonState.Running
                || string.IsNullOrWhiteSpace(roomId)
                || dungeonModel.PlayerRoomId != roomId)
            {
                return;
            }

            dungeonModel.PlayerRoomId = string.Empty;
            IncreaseRevision();
        }

        public void RefreshDungeonCompletion()
        {
            if (dungeonModel.DungeonState != DungeonState.Running)
            {
                return;
            }

            DungeonConfig config = GetCurrentDungeonConfig();

            if (config == null)
            {
                return;
            }

            CheckCombatRoomsCompletion(config);

            if (AreRequiredRoomsCompleted(config))
            {
                CompleteDungeon(config);
            }
        }

        private void TryStartDungeonAfterSceneLoaded(string sceneName)
        {
            DungeonConfig config = GetCurrentDungeonConfig();

            if (config == null || config.SceneName != sceneName)
            {
                return;
            }

            dungeonModel.DungeonState = DungeonState.Running;
            MarkAllRoomsRunning(config);
            IncreaseRevision();

            RefreshDungeonCompletion();
        }

        private void MarkAllRoomsRunning(DungeonConfig config)
        {
            if (config == null || config.Rooms == null)
            {
                return;
            }

            for (int i = 0; i < config.Rooms.Count; i++)
            {
                DungeonRoomConfig room = config.Rooms[i];

                if (room == null || string.IsNullOrWhiteSpace(room.RoomId))
                {
                    continue;
                }

                DungeonRoomRuntimeState roomState = GetRoomRuntimeState(room.RoomId);

                if (roomState == null)
                {
                    Debug.LogWarning($"Can not find dungeon room runtime state: {room.RoomId}");
                    continue;
                }

                if (roomState.State == DungeonRoomState.None)
                {
                    roomState.State = DungeonRoomState.Running;
                }

                int actualEnemyCount = GetAliveEnemiesForRoom(room).Count;
                roomState.ActualEnemyCount = actualEnemyCount;

                if (IsCombatOrBossRoom(room)
                    && room.ExpectedEnemyCount > 0
                    && actualEnemyCount != room.ExpectedEnemyCount)
                {
                    LogDungeonRoomEnemyCountMismatch(room, actualEnemyCount);
                }
            }
        }

        private void OnEnemyDead(EnemyDeadEvent enemyDeadEvent)
        {
            if (dungeonModel.DungeonState != DungeonState.Running)
            {
                return;
            }

            RefreshDungeonCompletion();
        }

        private void CheckCombatRoomsCompletion(DungeonConfig config)
        {
            if (config == null || config.Rooms == null)
            {
                return;
            }

            for (int i = 0; i < config.Rooms.Count; i++)
            {
                DungeonRoomConfig room = config.Rooms[i];

                if (!IsCombatOrBossRoom(room) || IsRoomCompleted(room.RoomId))
                {
                    continue;
                }

                if (IsCombatRoomCleared(room))
                {
                    CompleteRoom(config, room);
                    Debug.Log($"Dungeon room completed: {room.RoomId}");
                }
            }
        }

        private bool IsCombatRoomCleared(DungeonRoomConfig roomConfig)
        {
            if (!IsCombatOrBossRoom(roomConfig))
            {
                return false;
            }

            return GetAliveEnemiesForRoom(roomConfig).Count <= 0;
        }

        private void CompleteDungeon(DungeonConfig config)
        {
            if (config == null || dungeonModel.DungeonState == DungeonState.Completed)
            {
                return;
            }

            dungeonModel.DungeonState = DungeonState.Completed;
            IncreaseRevision();

            GrantCompletionRewards(config);

            this.SendEvent(new DungeonCompletedEvent
            {
                DungeonId = config.DungeonId,
                ClearedRoomCount = CountCompletedRooms()
            });

            Debug.Log($"Dungeon completed: {config.DungeonId}");
        }

        private void GrantCompletionRewards(DungeonConfig config)
        {
            if (dungeonModel.HasGrantedCompletionReward)
            {
                return;
            }

            dungeonModel.HasGrantedCompletionReward = true;
            IncreaseRevision();

            rewardSystem.GrantRewards(
                config.CompletionRewards,
                RewardSourceType.Dungeon,
                config.DungeonId);
        }

        private DungeonConfig GetCurrentDungeonConfig()
        {
            return GetDungeonConfig(dungeonModel.CurrentDungeonId);
        }

        private DungeonConfig GetDungeonConfig(string dungeonId)
        {
            return configSystem.DungeonConfigDatabase == null
                ? null
                : configSystem.DungeonConfigDatabase.GetDungeonConfig(dungeonId);
        }

        private void InitializeRoomStates(DungeonConfig config)
        {
            dungeonModel.RoomStates.Clear();

            if (config == null || config.Rooms == null)
            {
                return;
            }

            for (int i = 0; i < config.Rooms.Count; i++)
            {
                DungeonRoomConfig room = config.Rooms[i];

                if (room == null || string.IsNullOrWhiteSpace(room.RoomId))
                {
                    continue;
                }

                if (dungeonModel.RoomStates.ContainsKey(room.RoomId))
                {
                    Debug.LogWarning($"Duplicate dungeon room id: {room.RoomId}");
                    continue;
                }

                dungeonModel.RoomStates[room.RoomId] =
                    new DungeonRoomRuntimeState(room.RoomId);
            }
        }

        private DungeonRoomConfig GetRoomConfig(DungeonConfig config, string roomId)
        {
            if (config == null || config.Rooms == null || string.IsNullOrWhiteSpace(roomId))
            {
                return null;
            }

            for (int i = 0; i < config.Rooms.Count; i++)
            {
                DungeonRoomConfig room = config.Rooms[i];

                if (room != null && room.RoomId == roomId)
                {
                    return room;
                }
            }

            return null;
        }


        private int CountCompletedRooms()
        {
            int count = 0;

            foreach (DungeonRoomRuntimeState roomState in dungeonModel.RoomStates.Values)
            {
                if (roomState != null && roomState.IsCompleted)
                {
                    count++;
                }
            }

            return count;
        }

        private bool AreRequiredRoomsCompleted(DungeonConfig config)
        {
            if (config == null)
            {
                return false;
            }

            if (config.Rooms == null)
            {
                return true;
            }

            for (int i = 0; i < config.Rooms.Count; i++)
            {
                DungeonRoomConfig room = config.Rooms[i];

                if (room == null || !IsRoomRequiredForDungeonCompletion(room))
                {
                    continue;
                }

                if (!IsRoomCompleted(room.RoomId))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsRoomRequiredForDungeonCompletion(DungeonRoomConfig room)
        {
            return room != null && room.RequiredForDungeonCompletion;
        }

        private bool IsCombatOrBossRoom(DungeonRoomConfig room)
        {
            return room != null
                && (room.RoomType == DungeonRoomType.Combat
                    || room.RoomType == DungeonRoomType.Boss);
        }

        private void LogDungeonRoomEnemyCountMismatch(
            DungeonRoomConfig roomConfig,
            int actualEnemyCount)
        {
            if (roomConfig == null)
            {
                return;
            }

            Debug.LogWarning(
                $"Dungeon room enemy count mismatch. RoomId={roomConfig.RoomId}, Expected={roomConfig.ExpectedEnemyCount}, Actual={actualEnemyCount}. Check DungeonRoomEnemyMarker.RoomId on room enemies.");
        }

        private List<EnemyController> GetAliveEnemiesForRoom(DungeonRoomConfig roomConfig)
        {
            List<EnemyController> enemies = new List<EnemyController>();

            if (roomConfig == null || string.IsNullOrWhiteSpace(roomConfig.RoomId))
            {
                return enemies;
            }

            foreach (EnemyController enemy in GetCurrentAliveEnemies())
            {
                if (enemy == null)
                {
                    continue;
                }

                if (!enemy.TryGetComponent(out DungeonRoomEnemyMarker marker))
                {
                    continue;
                }

                if (marker.RoomId == roomConfig.RoomId)
                {
                    enemies.Add(enemy);
                }
            }

            return enemies;
        }

        private List<EnemyController> GetCurrentAliveEnemies()
        {
            List<EnemyController> enemies = new List<EnemyController>();

            foreach (EnemyController enemy in enemyRegistrySystem.AreaEnemies)
            {
                if (enemy == null || enemy.Stats == null || !enemy.Stats.IsAlive)
                {
                    continue;
                }

                enemies.Add(enemy);
            }

            return enemies;
        }

        private void ClearCurrentDungeon()
        {
            dungeonModel.CurrentDungeonId = string.Empty;
            dungeonModel.DungeonState = DungeonState.None;
            dungeonModel.PlayerRoomId = string.Empty;
            dungeonModel.HasGrantedCompletionReward = false;
            dungeonModel.RoomStates.Clear();
            dungeonModel.LastReturnSceneName = string.Empty;
            dungeonModel.LastReturnSpawnPointId = string.Empty;
            IncreaseRevision();
        }

        private void IncreaseRevision()
        {
            dungeonModel.DungeonRevision.Value++;
        }

        public void ResetForNewGame()
        {
            ClearCurrentDungeon();
        }

        public bool FailCurrentDungeon()
        {
            if (string.IsNullOrWhiteSpace(dungeonModel.CurrentDungeonId))
            {
                return false;
            }

            DungeonState currentState = dungeonModel.DungeonState;

            if (currentState != DungeonState.Loading && currentState != DungeonState.Running && currentState != DungeonState.Completed)
            {
                return false;
            }

            string failedDungeonId = dungeonModel.CurrentDungeonId;
            string failedRoomId = dungeonModel.PlayerRoomId;

            dungeonModel.DungeonState = DungeonState.Failed;
            IncreaseRevision();

            this.SendEvent(new DungeonFailedEvent
            {
                DungeonId = failedDungeonId,
                PlayerRoomId = failedRoomId
            });

            Debug.Log($"副本失败：{failedDungeonId}，玩家所在房间：{failedRoomId}");
            return true;
        }
    }
}
