using NUnit.Framework;
using QFramework;
using System;
using UnityEngine;
using UnityEngine.TestTools;

namespace RPG2D.Tests.EditMode
{
    public class TravelSystemTests
    {
        private const string FromSceneName = "TestFromScene";
        private const string TargetSceneName = "TestTargetScene";
        private const string TargetSpawnPointId = "test_spawn_point";
        private const string ExpectedFailureReason = "expected scene failure";

        private IArchitecture architecture;
        private TestSceneSystem sceneSystem;
        private RecordingTravelSceneAdapter sceneAdapter;
        private ITravelSystem travelSystem;
        private IUnRegister terminalRegistration;

        [SetUp]
        public void SetUp()
        {
            architecture = TravelTestArchitecture.Interface;
            sceneSystem = (TestSceneSystem)architecture.GetSystem<ISceneSystem>();
            sceneAdapter = TravelTestArchitecture.SceneAdapter;
            travelSystem = architecture.GetSystem<ITravelSystem>();
            sceneSystem.CurrentSceneName = FromSceneName;
        }

        [TearDown]
        public void TearDown()
        {
            terminalRegistration?.UnRegister();
            terminalRegistration = null;
            architecture.Deinit();
            architecture = null;
            sceneSystem = null;
            sceneAdapter = null;
            travelSystem = null;
        }

        [Test]
        public void SceneLoadCompleted_WhenOperationIdDoesNotMatch_IgnoresEventUntilCurrentOperationCompletes()
        {
            TravelRequest request = new TravelRequest(TargetSceneName, TargetSpawnPointId);
            int completedEventCount = 0;
            TravelCompletedEvent receivedEvent = default;

            terminalRegistration = architecture.RegisterEvent<TravelCompletedEvent>(completedEvent =>
            {
                completedEventCount++;
                receivedEvent = completedEvent;
            });

            bool accepted = travelSystem.Travel(request);

            Assert.That(accepted, Is.True);
            Assert.That(travelSystem.IsTraveling, Is.True);
            Assert.That(sceneSystem.LastRequestedSceneName, Is.EqualTo(TargetSceneName));
            Assert.That(sceneSystem.LastOperationId, Is.EqualTo(request.OperationId));

            architecture.SendEvent(new SceneLoadCompletedEvent
            {
                OperationId = Guid.NewGuid(),
                SceneName = TargetSceneName
            });

            Assert.That(travelSystem.IsTraveling, Is.True);
            Assert.That(sceneAdapter.PlacementCallCount, Is.Zero);
            Assert.That(completedEventCount, Is.Zero);

            sceneSystem.CurrentSceneName = TargetSceneName;

            architecture.SendEvent(new SceneLoadCompletedEvent
            {
                OperationId = request.OperationId,
                SceneName = TargetSceneName
            });

            Assert.That(travelSystem.IsTraveling, Is.False);
            Assert.That(sceneAdapter.PlacementCallCount, Is.EqualTo(1));
            Assert.That(sceneAdapter.LastSpawnPointId, Is.EqualTo(TargetSpawnPointId));
            Assert.That(completedEventCount, Is.EqualTo(1));
            Assert.That(receivedEvent.OperationId, Is.EqualTo(request.OperationId));
            Assert.That(receivedEvent.FromSceneName, Is.EqualTo(FromSceneName));
            Assert.That(receivedEvent.SceneName, Is.EqualTo(TargetSceneName));
            Assert.That(receivedEvent.SpawnPointId, Is.EqualTo(TargetSpawnPointId));
        }

        [Test]
        public void SceneLoadFailed_WhenOperationIdDoesNotMatch_IgnoresEventUntilCurrentOperationFails()
        {
            TravelRequest request = new TravelRequest(TargetSceneName, TargetSpawnPointId);
            int failedEventCount = 0;
            TravelFailedEvent receivedEvent = default;

            terminalRegistration = architecture.RegisterEvent<TravelFailedEvent>(failedEvent =>
            {
                failedEventCount++;
                receivedEvent = failedEvent;
            });

            bool accepted = travelSystem.Travel(request);

            Assert.That(accepted, Is.True);
            Assert.That(travelSystem.IsTraveling, Is.True);

            architecture.SendEvent(new SceneLoadFailedEvent
            {
                OperationId = Guid.NewGuid(),
                FromSceneName = FromSceneName,
                ToSceneName = TargetSceneName,
                Reason = ExpectedFailureReason
            });

            Assert.That(travelSystem.IsTraveling, Is.True);
            Assert.That(sceneAdapter.PlacementCallCount, Is.Zero);
            Assert.That(failedEventCount, Is.Zero);

            LogAssert.Expect(LogType.Error, $"迁移失败：{FromSceneName} -> {TargetSceneName}，原因：{ExpectedFailureReason}");

            architecture.SendEvent(new SceneLoadFailedEvent
            {
                OperationId = request.OperationId,
                FromSceneName = FromSceneName,
                ToSceneName = TargetSceneName,
                Reason = ExpectedFailureReason
            });

            Assert.That(travelSystem.IsTraveling, Is.False);
            Assert.That(sceneAdapter.PlacementCallCount, Is.Zero);
            Assert.That(failedEventCount, Is.EqualTo(1));
            Assert.That(receivedEvent.OperationId, Is.EqualTo(request.OperationId));
            Assert.That(receivedEvent.FromSceneName, Is.EqualTo(FromSceneName));
            Assert.That(receivedEvent.TargetSceneName, Is.EqualTo(TargetSceneName));
            Assert.That(receivedEvent.TargetSpawnPointId, Is.EqualTo(TargetSpawnPointId));
            Assert.That(receivedEvent.Reason, Is.EqualTo(ExpectedFailureReason));
        }

        private sealed class TravelTestArchitecture : Architecture<TravelTestArchitecture>
        {
            public static RecordingTravelSceneAdapter SceneAdapter { get; private set; }

            protected override void Init()
            {
                SceneAdapter = new RecordingTravelSceneAdapter();
                this.RegisterSystem<ISceneSystem>(new TestSceneSystem());
                this.RegisterSystem<ITravelSystem>(new TravelSystem(SceneAdapter));
            }

            protected override void OnDeinit()
            {
                SceneAdapter = null;
            }
        }

        private sealed class TestSceneSystem : AbstractSystem, ISceneSystem
        {
            public string CurrentSceneName { get; set; }
            public string LoadingSceneName => LastRequestedSceneName;
            public bool IsLoading => false;
            public BindableProperty<float> LoadingProgress { get; } = new BindableProperty<float>(0f);
            public string LastRequestedSceneName { get; private set; }
            public Guid LastOperationId { get; private set; }

            protected override void OnInit()
            {
            }

            public void LoadStartScene()
            {
                LoadScene(SceneSystem.StartSceneName, Guid.NewGuid());
            }

            public void LoadMainGameScene()
            {
                LoadScene(SceneSystem.MainGameSceneName, Guid.NewGuid());
            }

            public bool LoadScene(string sceneName, Guid operationId)
            {
                LastRequestedSceneName = sceneName;
                LastOperationId = operationId;
                return true;
            }
        }

        private sealed class RecordingTravelSceneAdapter : ITravelSceneAdapter
        {
            public int PlacementCallCount { get; private set; }
            public string LastSpawnPointId { get; private set; }

            public bool TryPlacePlayerAtSpawnPoint(string spawnPointId, out string reason)
            {
                PlacementCallCount++;
                LastSpawnPointId = spawnPointId;
                reason = string.Empty;
                return true;
            }
        }
    }
}
