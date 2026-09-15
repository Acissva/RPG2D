using QFramework;
using System.Collections.Generic;

namespace RPG2D
{
    public interface IRewardSystem : ISystem
    {
        bool GrantRewards(
            IReadOnlyList<RewardItemConfig> rewards,
            RewardSourceType sourceType,
            string sourceId);
    }
}
