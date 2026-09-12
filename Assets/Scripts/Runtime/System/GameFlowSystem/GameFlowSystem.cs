using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class GameFlowSystem : AbstractSystem, IGameFlowSystem
    {
        public const string MainRespawnPointId = "spawn_main_respawn";

        private const float RespawnDelaySeconds = 3f;
        private TimerHandle respawnTimer;

        private IPlayerSystem playerSystem;
        private ITravelSystem travelSystem;
        private IDungeonSystem dungeonSystem;
        private ICombatStateSystem combatStateSystem;
        private IDungeonModel dungeonModel;
        private ITimerSystem timerSystem;
        private IInventorySystem inventorySystem;
        private IQuestSystem questSystem;
        private IDialogueSystem dialogueSystem;
        private ISceneSystem sceneSystem;

        protected override void OnInit()
        {
            playerSystem = this.GetSystem<IPlayerSystem>();
            travelSystem = this.GetSystem<ITravelSystem>();
            dungeonSystem = this.GetSystem<IDungeonSystem>();
            combatStateSystem = this.GetSystem<ICombatStateSystem>();
            dungeonModel = this.GetModel<IDungeonModel>();
            timerSystem = this.GetSystem<ITimerSystem>();
            inventorySystem = this.GetSystem<IInventorySystem>();
            questSystem = this.GetSystem<IQuestSystem>();
            dialogueSystem = this.GetSystem<IDialogueSystem>();
            sceneSystem = this.GetSystem<ISceneSystem>();

            this.RegisterEvent<PlayerDiedEvent>(OnPlayerDied);
        }

        public void StartNewGame()
        {
            if (!playerSystem.InitializeNewGame())
            {
                return;
            }

            ResetSession();
            inventorySystem.ResetForNewGame();
            questSystem.ResetForNewGame();
            dungeonSystem.ResetForNewGame();
            combatStateSystem.ClearCombatState();
            dialogueSystem.EndDialogue();

            sceneSystem.LoadMainGameScene();
        }

        public void ReturnToStartScene()
        {
            ResetSession();
            sceneSystem.LoadStartScene();
        }

        private void ResetSession()
        {
            timerSystem.CancelScope(TimerScope.Session);
            respawnTimer = default;
        }

        private void OnPlayerDied(PlayerDiedEvent playerDiedEvent)
        {
            combatStateSystem.ClearCombatState();

            DungeonState currentDungeonState = dungeonModel.DungeonState;

            if (currentDungeonState == DungeonState.None)
            {
                ScheduleRespawn(new TravelRequest(SceneSystem.MainGameSceneName, MainRespawnPointId), "主世界");
                return;
            }

            if (currentDungeonState == DungeonState.Loading || currentDungeonState == DungeonState.Running || currentDungeonState == DungeonState.Completed)
            {
                ScheduleDungeonFailureRespawn();
                return;
            }

            Debug.LogWarning($"玩家死亡时副本状态不支持启动新的重生：{currentDungeonState}");
        }

        private void ScheduleDungeonFailureRespawn()
        {
            string returnSceneName = string.IsNullOrWhiteSpace(dungeonModel.LastReturnSceneName) ? SceneSystem.MainGameSceneName : dungeonModel.LastReturnSceneName;
            string returnSpawnPointId = dungeonModel.LastReturnSpawnPointId;

            if (!dungeonSystem.FailCurrentDungeon())
            {
                Debug.LogError("副本死亡重生失败：无法将当前副本标记为 Failed。");
                return;
            }

            if (string.IsNullOrWhiteSpace(returnSpawnPointId))
            {
                Debug.LogError("副本死亡重生失败：返回出生点 ID 为空。");
                return;
            }

            ScheduleRespawn(new TravelRequest(returnSceneName, returnSpawnPointId), "副本失败");
        }

        private void ScheduleRespawn(TravelRequest request, string context)
        {
            CancelPendingRespawn();

            respawnTimer = timerSystem.Schedule(RespawnDelaySeconds, () =>
            {
                respawnTimer = default;
                RequestRespawn(request, context);
            },
            TimerScope.Session);
        }

        private void RequestRespawn(TravelRequest request, string context)
        {
            if (!playerSystem.BeginRespawn(request.OperationId))
            {
                Debug.LogError($"{context}重生失败：玩家无法进入 Respawning 状态。");
                return;
            }

            bool accepted = travelSystem.Travel(request);

            if (accepted)
            {
                return;
            }

            playerSystem.CancelRespawn(request.OperationId);
            Debug.LogError($"{context}重生失败：迁移请求被拒绝。");
        }

        private void CancelPendingRespawn()
        {
            if (!respawnTimer.IsValid)
            {
                return;
            }

            timerSystem.Cancel(respawnTimer);
            respawnTimer = default;
        }
    }
}
