using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class RewardSystem : AbstractSystem, IRewardSystem
    {
        private IInventorySystem inventorySystem;
        private IConfigSystem configSystem;

        protected override void OnInit()
        {
            inventorySystem = this.GetSystem<IInventorySystem>();
            configSystem = this.GetSystem<IConfigSystem>();
        }

        public bool GrantRewards(
            IReadOnlyList<RewardItemConfig> rewards,
            RewardSourceType sourceType,
            string sourceId)
        {
            if (!IsRewardBatchValid(rewards, sourceType, sourceId))
            {
                return false;
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                RewardItemConfig reward = rewards[i];
                bool added = inventorySystem.AddItem(reward.ItemId, reward.Count);

                if (!added)
                {
                    Debug.LogError(
                        $"奖励批次预检通过后写入失败：SourceType={sourceType}, SourceId={sourceId}, ItemId={reward.ItemId}, Count={reward.Count}");

                    return false;
                }
            }

            return true;
        }

        private bool IsRewardBatchValid(
            IReadOnlyList<RewardItemConfig> rewards,
            RewardSourceType sourceType,
            string sourceId)
        {
            if (rewards == null || rewards.Count <= 0)
            {
                Debug.LogWarning(
                    $"奖励发放失败：奖励列表为空，SourceType={sourceType}, SourceId={sourceId}");

                return false;
            }

            ItemConfigDatabase itemConfigDatabase = configSystem.ItemConfigDatabase;

            if (itemConfigDatabase == null)
            {
                Debug.LogWarning(
                    $"奖励发放失败：物品配置数据库不可用，SourceType={sourceType}, SourceId={sourceId}");

                return false;
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                RewardItemConfig reward = rewards[i];

                if (reward == null)
                {
                    Debug.LogWarning(
                        $"奖励发放失败：奖励条目为空，SourceType={sourceType}, SourceId={sourceId}, Index={i}");

                    return false;
                }

                if (string.IsNullOrWhiteSpace(reward.ItemId))
                {
                    Debug.LogWarning(
                        $"奖励发放失败：ItemId 为空，SourceType={sourceType}, SourceId={sourceId}, Index={i}");

                    return false;
                }

                if (reward.Count <= 0)
                {
                    Debug.LogWarning(
                        $"奖励发放失败：数量必须大于 0，SourceType={sourceType}, SourceId={sourceId}, ItemId={reward.ItemId}, Count={reward.Count}");

                    return false;
                }

                if (!itemConfigDatabase.ContainsItem(reward.ItemId))
                {
                    Debug.LogWarning(
                        $"奖励发放失败：找不到物品配置，SourceType={sourceType}, SourceId={sourceId}, ItemId={reward.ItemId}");

                    return false;
                }
            }

            return true;
        }
    }
}
