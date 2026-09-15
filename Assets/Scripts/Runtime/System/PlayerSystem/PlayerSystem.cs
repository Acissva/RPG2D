using QFramework;
using System;
using UnityEngine;

namespace RPG2D
{
    public class PlayerSystem : AbstractSystem, IPlayerSystem
    {
        private IPlayerModel playerModel;
        private IConfigSystem configSystem;

        private Guid respawnTravelOperationId;

        protected override void OnInit()
        {
            playerModel = this.GetModel<IPlayerModel>();
            configSystem = this.GetSystem<IConfigSystem>();

            this.RegisterEvent<TravelCompletedEvent>(OnTravelCompleted);
            this.RegisterEvent<TravelFailedEvent>(OnTravelFailed);
        }

        public bool InitializeNewGame()
        {
            PlayerConfig config = configSystem.PlayerConfig;

            if (config == null)
            {
                Debug.LogError(
                    "初始化新游戏玩家数据失败：ConfigSystem 中没有 PlayerConfig。");

                return false;
            }

            int maxHP = Mathf.Max(1, config.MaxHP);
            int initialHP = Mathf.Clamp(config.InitialHP, 1, maxHP);
            float moveSpeed = Mathf.Max(0f, config.MoveSpeed);

            playerModel.MaxHP.Value = maxHP;
            playerModel.HP.Value = initialHP;
            playerModel.MoveSpeed.Value = moveSpeed;
            playerModel.LifeState.Value = PlayerLifeState.Alive;

            respawnTravelOperationId = Guid.Empty;

            return true;
        }

        public bool TakeDamage(int damage)
        {
            if (damage <= 0 || playerModel.LifeState.Value != PlayerLifeState.Alive)
            {
                return false;
            }

            int currentHP = Mathf.Clamp(playerModel.HP.Value, 0, playerModel.MaxHP.Value);

            int nextHP = Mathf.Clamp(currentHP - damage, 0, playerModel.MaxHP.Value);

            if (nextHP == currentHP)
            {
                return false;
            }

            playerModel.HP.Value = nextHP;

            Debug.Log($"玩家受到伤害：{damage}\n当前血量：{nextHP}");

            if (nextHP == 0)
            {
                playerModel.LifeState.Value = PlayerLifeState.Dead;

                this.SendEvent(new PlayerDiedEvent
                {
                    Damage = damage,
                    PreviousHP = currentHP
                });

                Debug.Log("玩家死亡");
            }

            return true;
        }

        public bool BeginRespawn(Guid travelOperationId)
        {
            if (travelOperationId == Guid.Empty || playerModel.LifeState.Value != PlayerLifeState.Dead)
            {
                return false;
            }

            respawnTravelOperationId = travelOperationId;
            playerModel.LifeState.Value = PlayerLifeState.Respawning;
            return true;
        }

        public bool CancelRespawn(Guid travelOperationId)
        {
            if (travelOperationId != respawnTravelOperationId || playerModel.LifeState.Value != PlayerLifeState.Respawning)
            {
                return false;
            }

            respawnTravelOperationId = Guid.Empty;
            playerModel.LifeState.Value = PlayerLifeState.Dead;
            return true;
        }

        private void OnTravelCompleted(TravelCompletedEvent travelCompletedEvent)
        {
            if (playerModel.LifeState.Value != PlayerLifeState.Respawning)
            {
                return;
            }

            if (travelCompletedEvent.OperationId != respawnTravelOperationId)
            {
                return;
            }

            respawnTravelOperationId = Guid.Empty;

            int restoredHP = Mathf.Max(1, playerModel.MaxHP.Value);

            playerModel.MaxHP.Value = restoredHP;
            playerModel.HP.Value = restoredHP;
            playerModel.LifeState.Value = PlayerLifeState.Alive;

            this.SendEvent(new PlayerRespawnedEvent
            {
                SceneName = travelCompletedEvent.SceneName,
                SpawnPointId = travelCompletedEvent.SpawnPointId,
                RestoredHP = restoredHP
            });

            Debug.Log($"玩家重生完成：{travelCompletedEvent.SceneName}/" +
                $"{travelCompletedEvent.SpawnPointId}，" +
                $"恢复生命值：{restoredHP}");
        }

        private void OnTravelFailed(TravelFailedEvent travelFailedEvent)
        {
            if (!CancelRespawn(travelFailedEvent.OperationId))
            {
                return;
            }

            Debug.LogError(
                $"玩家重生迁移失败，保持死亡状态：" +
                $"{travelFailedEvent.Reason}");
        }
    }
}
