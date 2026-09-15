using NUnit.Framework;
using QFramework;
using System.Linq;

namespace RPG2D.Tests.EditMode
{
    public class QuestSystemTests
    {
        private const string UnmatchedInteractionId = "__test_unmatched_interaction__";

        private IArchitecture architecture;
        private IConfigSystem configSystem;
        private IQuestSystem questSystem;

        [SetUp]
        public void SetUp()
        {
            architecture = QuestTestArchitecture.Interface;
            configSystem = architecture.GetSystem<IConfigSystem>();
            questSystem = architecture.GetSystem<IQuestSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            architecture.Deinit();
            architecture = null;
            configSystem = null;
            questSystem = null;
        }

        [Test]
        public void InteractionPerformed_WhenIdMatchesConfiguredObjective_OnlyThenAdvancesQuest()
        {
            Assert.That(configSystem.QuestConfigDatabase, Is.Not.Null);

            QuestConfig questConfig = configSystem.QuestConfigDatabase.Quests.FirstOrDefault(config => config != null && config.Objectives.Count > 0 && config.Objectives[0] != null && config.Objectives[0].ObjectiveType == QuestObjectiveType.InteractWithObject);

            Assert.That(questConfig, Is.Not.Null);

            QuestObjectiveConfig objective = questConfig.Objectives[0];

            Assert.That(objective.TargetInteractionId, Is.Not.Null.And.Not.Empty);
            Assert.That(objective.TargetInteractionId, Is.Not.EqualTo(UnmatchedInteractionId));
            Assert.That(questSystem.AcceptQuest(questConfig.QuestId), Is.True);

            QuestTrackerSnapshot initialSnapshot = questSystem.GetTrackedQuestSnapshot();

            Assert.That(initialSnapshot.HasQuest, Is.True);
            Assert.That(initialSnapshot.QuestId, Is.EqualTo(questConfig.QuestId));
            Assert.That(initialSnapshot.CurrentCount, Is.Zero);

            architecture.SendEvent(new InteractionPerformedEvent
            {
                InteractionId = UnmatchedInteractionId
            });

            QuestTrackerSnapshot unmatchedSnapshot = questSystem.GetTrackedQuestSnapshot();

            Assert.That(unmatchedSnapshot.CurrentCount, Is.Zero);

            architecture.SendEvent(new InteractionPerformedEvent
            {
                InteractionId = objective.TargetInteractionId
            });

            QuestTrackerSnapshot matchedSnapshot = questSystem.GetTrackedQuestSnapshot();

            Assert.That(matchedSnapshot.CurrentCount, Is.EqualTo(1));
        }

        private sealed class QuestTestArchitecture : Architecture<QuestTestArchitecture>
        {
            protected override void Init()
            {
                this.RegisterModel<IInventoryModel>(new InventoryModel());
                this.RegisterModel<IQuestModel>(new QuestModel());
                this.RegisterSystem<IAssetSystem>(new AssetSystem());
                this.RegisterSystem<IConfigSystem>(new ConfigSystem());
                this.RegisterSystem<IInventorySystem>(new InventorySystem());
                this.RegisterSystem<IRewardSystem>(new RewardSystem());
                this.RegisterSystem<IQuestSystem>(new QuestSystem());
            }
        }
    }
}
