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
    }

    public class DungeonSystem : AbstractSystem, IDungeonSystem
    {
        private IDungeonModel dungeonModel;
        private IConfigSystem configSystem;
        private ISceneSystem sceneSystem;
        private IEncounterSystem encounterSystem;
        private IEnemyRegistrySystem enemyRegistrySystem;
        private IRewardSystem rewardSystem;

        protected override void OnInit()
        {
            dungeonModel = this.GetModel<IDungeonModel>();
            configSystem = this.GetSystem<IConfigSystem>();
            sceneSystem = this.GetSystem<ISceneSystem>();
            encounterSystem = this.GetSystem<IEncounterSystem>();
            enemyRegistrySystem = this.GetSystem<IEnemyRegistrySystem>();
            rewardSystem = this.GetSystem<IRewardSystem>();

            this.RegisterEvent<SceneLoadCompletedEvent>(OnSceneLoadCompleted);
            this.RegisterEvent<EncounterClearedEvent>(OnEncounterCleared);
        }

        public bool CanEnterDungeon(string dungeonId)
        {
            if (string.IsNullOrWhiteSpace(dungeonId))
            {
                return false;
            }

            if (sceneSystem.IsLoading)
            {
                return false;
            }

            DungeonConfig config = GetDungeonConfig(dungeonId);

            if (config == null || string.IsNullOrWhiteSpace(config.SceneName))
            {
                return false;
            }

            return HasRequiredRooms(config);
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
            dungeonModel.CurrentRoomId = string.Empty;
            dungeonModel.DungeonState = DungeonState.Loading;
            dungeonModel.HasGrantedCompletionReward = false;
            InitializeRoomStates(config);
            dungeonModel.LastReturnSceneName = config.ReturnSceneName;
            dungeonModel.LastReturnSpawnPointId = config.ReturnSpawnPointId;
            IncreaseRevision();

            sceneSystem.LoadScene(config.SceneName);
            return true;
        }

        public bool ExitCurrentDungeon()
        {
            if (!IsCurrentDungeonCompleted())
            {
                Debug.LogWarning("Can not exit dungeon before it is completed.");
                return false;
            }

            string returnSceneName = string.IsNullOrWhiteSpace(dungeonModel.LastReturnSceneName)
                ? SceneSystem.MainGameSceneName
                : dungeonModel.LastReturnSceneName;

            dungeonModel.DungeonState = DungeonState.Exiting;
            IncreaseRevision();

            sceneSystem.LoadScene(returnSceneName);
            return true;
        }

        public bool IsCurrentDungeonCompleted()
        {
            return dungeonModel.DungeonState == DungeonState.Completed;
        }

        private void OnSceneLoadCompleted(SceneLoadCompletedEvent sceneLoadCompletedEvent)
        {
            if (dungeonModel.DungeonState == DungeonState.Loading)
            {
                TryStartDungeonAfterSceneLoaded(sceneLoadCompletedEvent.SceneName);
                return;
            }

            if (dungeonModel.DungeonState == DungeonState.Exiting)
            {
                ClearCurrentDungeon();
            }
        }

        private void TryStartDungeonAfterSceneLoaded(string sceneName)
        {
            DungeonConfig config = GetCurrentDungeonConfig();

            if (config == null || config.SceneName != sceneName)
            {
                return;
            }

            StartNextRequiredRoom(config);
        }

        private void StartNextRequiredRoom(DungeonConfig config)
        {
            DungeonRoomConfig roomConfig = GetFirstIncompleteRequiredRoom(config);

            if (roomConfig == null)
            {
                Debug.Log($"All required dungeon rooms are completed: {config.DungeonId}");
                CompleteDungeon(config);
                return;
            }

            StartRoomEncounter(config, roomConfig);
        }

        private void StartRoomEncounter(DungeonConfig config, DungeonRoomConfig roomConfig)
        {
            if (config == null || roomConfig == null)
            {
                return;
            }

            List<EnemyController> enemies = GetAliveEnemiesForRoom(roomConfig);

            if (roomConfig.ExpectedEnemyCount > 0 && enemies.Count != roomConfig.ExpectedEnemyCount)
            {
                LogDungeonRoomEnemyCountMismatch(roomConfig, enemies.Count);
            }

            DungeonRoomRuntimeState roomState = GetRoomState(roomConfig.RoomId);

            if (roomState == null)
            {
                Debug.LogWarning($"Can not find dungeon room runtime state: {roomConfig.RoomId}");
                return;
            }

            roomState.State = DungeonRoomState.Running;
            roomState.ActualEnemyCount = enemies.Count;

            dungeonModel.CurrentRoomId = roomConfig.RoomId;
            dungeonModel.DungeonState = DungeonState.Running;
            IncreaseRevision();

            encounterSystem.BeginEncounter(
                roomConfig.EncounterId,
                EncounterContextType.DungeonRoom,
                roomConfig.RoomId,
                enemies);
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

        private void OnEncounterCleared(EncounterClearedEvent encounterClearedEvent)
        {
            if (encounterClearedEvent.ContextType != EncounterContextType.DungeonRoom)
            {
                return;
            }

            if (dungeonModel.DungeonState != DungeonState.Running)
            {
                return;
            }

            if (encounterClearedEvent.ContextId != dungeonModel.CurrentRoomId)
            {
                return;
            }

            DungeonConfig config = GetCurrentDungeonConfig();

            if (config == null)
            {
                return;
            }

            DungeonRoomConfig roomConfig = GetRoomConfig(config, encounterClearedEvent.ContextId);

            if (roomConfig == null)
            {
                Debug.LogWarning($"Can not find cleared dungeon room: {encounterClearedEvent.ContextId}");
                return;
            }

            bool newlyCleared = MarkRoomCleared(roomConfig);

            encounterSystem.EndEncounter(encounterClearedEvent.EncounterId);

            if (!newlyCleared)
            {
                return;
            }

            CompleteRoom(config, roomConfig);

            if (AreRequiredRoomsCompleted(config))
            {
                CompleteDungeon(config);
                return;
            }

            Debug.Log($"Dungeon room completed: {roomConfig.RoomId}");
            StartNextRequiredRoom(config);
        }

        private bool MarkRoomCleared(DungeonRoomConfig roomConfig)
        {
            if (roomConfig == null || string.IsNullOrWhiteSpace(roomConfig.RoomId))
            {
                return false;
            }

            DungeonRoomRuntimeState roomState = GetRoomState(roomConfig.RoomId);

            if (roomState == null)
            {
                Debug.LogWarning($"Can not find cleared dungeon room runtime state: {roomConfig.RoomId}");
                return false;
            }

            if (roomState.State == DungeonRoomState.Cleared
                || roomState.State == DungeonRoomState.Completed)
            {
                return false;
            }

            roomState.State = DungeonRoomState.Cleared;
            IncreaseRevision();

            return true;
        }

        private void CompleteRoom(DungeonConfig config, DungeonRoomConfig roomConfig)
        {
            if (config == null || roomConfig == null || string.IsNullOrWhiteSpace(roomConfig.RoomId))
            {
                return;
            }

            DungeonRoomRuntimeState roomState = GetRoomState(roomConfig.RoomId);

            if (roomState == null)
            {
                Debug.LogWarning($"Can not find completed dungeon room runtime state: {roomConfig.RoomId}");
                return;
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
            IncreaseRevision();
        }

        private void CompleteDungeon(DungeonConfig config)
        {
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

        private DungeonRoomConfig GetFirstIncompleteRequiredRoom(DungeonConfig config)
        {
            if (config == null || config.Rooms == null)
            {
                return null;
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
                    return room;
                }
            }

            return null;
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

        private DungeonRoomRuntimeState GetRoomState(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return null;
            }

            dungeonModel.RoomStates.TryGetValue(roomId, out DungeonRoomRuntimeState roomState);
            return roomState;
        }

        private bool IsRoomCompleted(string roomId)
        {
            DungeonRoomRuntimeState roomState = GetRoomState(roomId);
            return roomState != null && roomState.IsCompleted;
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

        private bool HasRequiredRooms(DungeonConfig config)
        {
            if (config == null || config.Rooms == null)
            {
                return false;
            }

            for (int i = 0; i < config.Rooms.Count; i++)
            {
                DungeonRoomConfig room = config.Rooms[i];

                if (IsRoomRequiredForDungeonCompletion(room))
                {
                    return true;
                }
            }

            return false;
        }

        private bool AreRequiredRoomsCompleted(DungeonConfig config)
        {
            if (config == null || config.Rooms == null)
            {
                return false;
            }

            bool hasRequiredRoom = false;

            for (int i = 0; i < config.Rooms.Count; i++)
            {
                DungeonRoomConfig room = config.Rooms[i];

                if (room == null || !IsRoomRequiredForDungeonCompletion(room))
                {
                    continue;
                }

                hasRequiredRoom = true;

                if (!IsRoomCompleted(room.RoomId))
                {
                    return false;
                }
            }

            return hasRequiredRoom;
        }

        private bool IsRoomRequiredForDungeonCompletion(DungeonRoomConfig room)
        {
            return room != null && room.RequiredForDungeonCompletion;
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

            foreach (EnemyController enemy in enemyRegistrySystem.Enemies)
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
            dungeonModel.CurrentRoomId = string.Empty;
            dungeonModel.DungeonState = DungeonState.None;
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
    }
}
