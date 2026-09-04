using QFramework;
using System.Collections.Generic;

namespace RPG2D
{
    public class GrantRewardCommand : AbstractCommand<bool>
    {
        private readonly IReadOnlyList<RewardItemConfig> rewards;
        private readonly RewardSourceType sourceType;
        private readonly string sourceId;

        public GrantRewardCommand(
            IReadOnlyList<RewardItemConfig> rewards,
            RewardSourceType sourceType,
            string sourceId)
        {
            this.rewards = rewards;
            this.sourceType = sourceType;
            this.sourceId = sourceId;
        }

        protected override bool OnExecute()
        {
            return this.GetSystem<IRewardSystem>().GrantRewards(rewards, sourceType, sourceId);
        }
    }
}
