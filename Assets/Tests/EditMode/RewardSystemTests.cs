using NUnit.Framework;
using QFramework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace RPG2D.Tests.EditMode
{
    public class RewardSystemTests
    {
        private const string SourceId = "test_reward_batch";
        private const string MissingItemId = "__test_missing_item__";

        private IArchitecture architecture;
        private IInventorySystem inventorySystem;
        private IRewardSystem rewardSystem;
        private ItemConfigDatabase itemConfigDatabase;

        [SetUp]
        public void SetUp()
        {
            architecture = RewardTestArchitecture.Interface;
            inventorySystem = architecture.GetSystem<IInventorySystem>();
            rewardSystem = architecture.GetSystem<IRewardSystem>();
            itemConfigDatabase = architecture.GetSystem<IConfigSystem>().ItemConfigDatabase;
        }

        [TearDown]
        public void TearDown()
        {
            architecture.Deinit();
            architecture = null;
            inventorySystem = null;
            rewardSystem = null;
            itemConfigDatabase = null;
        }

        [Test]
        public void GrantRewards_WhenBatchContainsMissingItem_DoesNotGrantAnyReward()
        {
            Assert.That(itemConfigDatabase, Is.Not.Null);
            Assert.That(itemConfigDatabase.Items, Is.Not.Empty);
            Assert.That(itemConfigDatabase.Items[0], Is.Not.Null);

            string validItemId = itemConfigDatabase.Items[0].ItemId;
            Assert.That(itemConfigDatabase.ContainsItem(MissingItemId), Is.False);

            IReadOnlyList<RewardItemConfig> rewards = new[]
            {
                CreateReward(validItemId, 2),
                CreateReward(MissingItemId, 1)
            };

            LogAssert.Expect(LogType.Warning, $"奖励发放失败：找不到物品配置，SourceType={RewardSourceType.Quest}, SourceId={SourceId}, ItemId={MissingItemId}");

            bool granted = rewardSystem.GrantRewards(rewards, RewardSourceType.Quest, SourceId);

            Assert.That(granted, Is.False);
            Assert.That(inventorySystem.GetItemCount(validItemId), Is.Zero);
        }

        [Test]
        public void GrantRewards_WhenBatchIsValid_GrantsEveryReward()
        {
            Assert.That(itemConfigDatabase, Is.Not.Null);
            Assert.That(itemConfigDatabase.Items, Is.Not.Empty);
            Assert.That(itemConfigDatabase.Items[0], Is.Not.Null);

            string validItemId = itemConfigDatabase.Items[0].ItemId;

            IReadOnlyList<RewardItemConfig> rewards = new[]
            {
                CreateReward(validItemId, 2),
                CreateReward(validItemId, 3)
            };

            bool granted = rewardSystem.GrantRewards(rewards, RewardSourceType.Quest, SourceId);

            Assert.That(granted, Is.True);
            Assert.That(inventorySystem.GetItemCount(validItemId), Is.EqualTo(5));
        }

        private static RewardItemConfig CreateReward(string itemId, int count)
        {
            return JsonUtility.FromJson<RewardItemConfig>($"{{\"itemId\":\"{itemId}\",\"count\":{count}}}");
        }

        private sealed class RewardTestArchitecture : Architecture<RewardTestArchitecture>
        {
            protected override void Init()
            {
                this.RegisterModel<IInventoryModel>(new InventoryModel());
                this.RegisterSystem<IAssetSystem>(new AssetSystem());
                this.RegisterSystem<IConfigSystem>(new ConfigSystem());
                this.RegisterSystem<IInventorySystem>(new InventorySystem());
                this.RegisterSystem<IRewardSystem>(new RewardSystem());
            }
        }
    }
}
