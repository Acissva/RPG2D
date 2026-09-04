using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public interface IRewardSystem : ISystem
    {
        bool GrantRewards(
            IReadOnlyList<RewardItemConfig> rewards,
            RewardSourceType sourceType,
            string sourceId);
    }

    public class RewardSystem : AbstractSystem, IRewardSystem
    {
        private IInventorySystem inventorySystem;

        protected override void OnInit()
        {
            inventorySystem = this.GetSystem<IInventorySystem>();
        }

        public bool GrantRewards(
            IReadOnlyList<RewardItemConfig> rewards,
            RewardSourceType sourceType,
            string sourceId)
        {
            if (rewards == null || rewards.Count <= 0)
            {
                return false;
            }

            bool anyGranted = false;

            for (int i = 0; i < rewards.Count; i++)
            {
                RewardItemConfig reward = rewards[i];

                if (reward == null)
                {
                    continue;
                }

                bool added = inventorySystem.AddItem(reward.ItemId, reward.Count);
                anyGranted |= added;

                if (!added)
                {
                    Debug.LogWarning(
                        $"奖励发放失败：SourceType={sourceType}, SourceId={sourceId}, ItemId={reward.ItemId}, Count={reward.Count}");
                }
            }

            if (anyGranted)
            {
                this.SendEvent(new RewardGrantedEvent
                {
                    SourceType = sourceType,
                    SourceId = sourceId
                });
            }

            return anyGranted;
        }
    }
}
