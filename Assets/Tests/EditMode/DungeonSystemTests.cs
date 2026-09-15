using NUnit.Framework;
using QFramework;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace RPG2D.Tests.EditMode
{
    public class DungeonSystemTests
    {
        private const string CurrentSceneName = "TestMainScene";

        private IArchitecture architecture;
        private IConfigSystem configSystem;
        private IDungeonModel dungeonModel;
        private IDungeonSystem dungeonSystem;
        private RecordingTravelSystem travelSystem;
        private IUnRegister failedRegistration;

        [SetUp]
        public void SetUp()
        {
            architecture = DungeonTestArchitecture.Interface;
            configSystem = architecture.GetSystem<IConfigSystem>();
            dungeonModel = architecture.GetModel<IDungeonModel>();
            dungeonSystem = architecture.GetSystem<IDungeonSystem>();
            travelSystem = (RecordingTravelSystem)architecture.GetSystem<ITravelSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            failedRegistration?.UnRegister();
            failedRegistration = null;
            architecture.Deinit();
            architecture = null;
            configSystem = null;
            dungeonModel = null;
            dungeonSystem = null;
            travelSystem = null;
        }

        [Test]
        public void EnterDungeon_WhenDungeonIsAlreadyLoading_RejectsRequestWithoutChangingState()
        {
            DungeonConfig config = GetDungeonConfig();

            Assert.That(dungeonSystem.EnterDungeon(config.DungeonId), Is.True);
            Assert.That(dungeonModel.DungeonState, Is.EqualTo(DungeonState.Loading));

            int revisionAfterFirstEntry = dungeonModel.DungeonRevision.Value;
            int roomStateCountAfterFirstEntry = dungeonModel.RoomStates.Count;
            Guid firstOperationId = travelSystem.LastRequest.OperationId;

            LogAssert.Expect(LogType.Warning, $"Can not enter dungeon: {config.DungeonId}");

            bool secondEntryAccepted = dungeonSystem.EnterDungeon(config.DungeonId);

            Assert.That(secondEntryAccepted, Is.False);
            Assert.That(dungeonModel.CurrentDungeonId, Is.EqualTo(config.DungeonId));
            Assert.That(dungeonModel.DungeonState, Is.EqualTo(DungeonState.Loading));
            Assert.That(dungeonModel.DungeonRevision.Value, Is.EqualTo(revisionAfterFirstEntry));
            Assert.That(dungeonModel.RoomStates.Count, Is.EqualTo(roomStateCountAfterFirstEntry));
            Assert.That(travelSystem.TravelCallCount, Is.EqualTo(1));
            Assert.That(travelSystem.LastRequest.OperationId, Is.EqualTo(firstOperationId));
        }

        [Test]
        public void FailCurrentDungeon_WhenDungeonIsAlreadyFailed_RejectsRequestWithoutRepeatingSideEffects()
        {
            DungeonConfig config = GetDungeonConfig();
            int failedEventCount = 0;
            DungeonFailedEvent receivedEvent = default;

            failedRegistration = architecture.RegisterEvent<DungeonFailedEvent>(failedEvent =>
            {
                failedEventCount++;
                receivedEvent = failedEvent;
            });

            Assert.That(dungeonSystem.EnterDungeon(config.DungeonId), Is.True);
            Assert.That(dungeonSystem.FailCurrentDungeon(), Is.True);
            Assert.That(dungeonModel.DungeonState, Is.EqualTo(DungeonState.Failed));
            Assert.That(failedEventCount, Is.EqualTo(1));

            int revisionAfterFirstFailure = dungeonModel.DungeonRevision.Value;

            bool repeatedFailureAccepted = dungeonSystem.FailCurrentDungeon();

            Assert.That(repeatedFailureAccepted, Is.False);
            Assert.That(dungeonModel.CurrentDungeonId, Is.EqualTo(config.DungeonId));
            Assert.That(dungeonModel.DungeonState, Is.EqualTo(DungeonState.Failed));
            Assert.That(dungeonModel.DungeonRevision.Value, Is.EqualTo(revisionAfterFirstFailure));
            Assert.That(failedEventCount, Is.EqualTo(1));
            Assert.That(receivedEvent.DungeonId, Is.EqualTo(config.DungeonId));
            Assert.That(receivedEvent.PlayerRoomId, Is.Empty);
            Assert.That(travelSystem.TravelCallCount, Is.EqualTo(1));
        }

        private DungeonConfig GetDungeonConfig()
        {
            Assert.That(configSystem.DungeonConfigDatabase, Is.Not.Null);
            DungeonConfig config = configSystem.DungeonConfigDatabase.Dungeons.FirstOrDefault(dungeon => dungeon != null);
            Assert.That(config, Is.Not.Null);
            return config;
        }

        private sealed class DungeonTestArchitecture : Architecture<DungeonTestArchitecture>
        {
            protected override void Init()
            {
                this.RegisterModel<IInventoryModel>(new InventoryModel());
                this.RegisterModel<IDungeonModel>(new DungeonModel());
                this.RegisterSystem<IAssetSystem>(new AssetSystem());
                this.RegisterSystem<IConfigSystem>(new ConfigSystem());
                this.RegisterSystem<ISceneSystem>(new TestSceneSystem());
                this.RegisterSystem<IEnemyRegistrySystem>(new EnemyRegistrySystem());
                this.RegisterSystem<IInventorySystem>(new InventorySystem());
                this.RegisterSystem<IRewardSystem>(new RewardSystem());
                this.RegisterSystem<ITravelSystem>(new RecordingTravelSystem());
                this.RegisterSystem<IDungeonSystem>(new DungeonSystem());
            }
        }

        private sealed class TestSceneSystem : AbstractSystem, ISceneSystem
        {
            public string CurrentSceneName => DungeonSystemTests.CurrentSceneName;
            public string LoadingSceneName => string.Empty;
            public bool IsLoading => false;
            public BindableProperty<float> LoadingProgress { get; } = new BindableProperty<float>(0f);

            protected override void OnInit()
            {
            }

            public void LoadStartScene()
            {
            }

            public void LoadMainGameScene()
            {
            }

            public bool LoadScene(string sceneName, Guid operationId)
            {
                return true;
            }
        }

        private sealed class RecordingTravelSystem : AbstractSystem, ITravelSystem
        {
            public bool IsTraveling => false;
            public int TravelCallCount { get; private set; }
            public TravelRequest LastRequest { get; private set; }

            protected override void OnInit()
            {
            }

            public bool Travel(TravelRequest request)
            {
                TravelCallCount++;
                LastRequest = request;
                return true;
            }
        }
    }
}
