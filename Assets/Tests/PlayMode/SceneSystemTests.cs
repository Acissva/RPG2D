using NUnit.Framework;
using QFramework;
using System;
using System.Collections;
using UnityEngine.TestTools;

namespace RPG2D.Tests.PlayMode
{
    public class SceneSystemTests
    {
        private const int MaxFrameCount = 600;

        private IArchitecture architecture;
        private TestRuntimeUpdateSystem runtimeUpdateSystem;
        private ISceneSystem sceneSystem;
        private IUnRegister completedRegistration;

        [SetUp]
        public void SetUp()
        {
            architecture = SceneTestArchitecture.Interface;
            runtimeUpdateSystem = (TestRuntimeUpdateSystem)architecture.GetSystem<IRuntimeUpdateSystem>();
            sceneSystem = architecture.GetSystem<ISceneSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            completedRegistration?.UnRegister();
            completedRegistration = null;
            architecture.Deinit();
            architecture = null;
            runtimeUpdateSystem = null;
            sceneSystem = null;
        }

        [UnityTest]
        public IEnumerator LoadScene_WhenCompletedHandlerStartsNextLoad_AcceptsAndCompletesReentrantRequest()
        {
            Guid firstOperationId = Guid.NewGuid();
            Guid secondOperationId = Guid.NewGuid();
            string firstSceneName = string.Equals(sceneSystem.CurrentSceneName, SceneSystem.StartSceneName, StringComparison.Ordinal) ? SceneSystem.MainGameSceneName : SceneSystem.StartSceneName;
            string secondSceneName = string.Equals(firstSceneName, SceneSystem.StartSceneName, StringComparison.Ordinal) ? SceneSystem.MainGameSceneName : SceneSystem.StartSceneName;

            bool firstCompleted = false;
            bool reentrantRequestAccepted = false;
            bool secondCompleted = false;

            completedRegistration = architecture.RegisterEvent<SceneLoadCompletedEvent>(completedEvent =>
            {
                if (completedEvent.OperationId == firstOperationId)
                {
                    firstCompleted = true;
                    reentrantRequestAccepted = sceneSystem.LoadScene(secondSceneName, secondOperationId);
                    return;
                }

                if (completedEvent.OperationId == secondOperationId)
                {
                    secondCompleted = true;
                }
            });

            bool firstRequestAccepted = sceneSystem.LoadScene(firstSceneName, firstOperationId);

            for (int frame = 0; frame < MaxFrameCount && !secondCompleted; frame++)
            {
                yield return null;
                runtimeUpdateSystem.Tick();
            }

            Assert.That(firstRequestAccepted, Is.True);
            Assert.That(firstCompleted, Is.True);
            Assert.That(reentrantRequestAccepted, Is.True);
            Assert.That(secondCompleted, Is.True);
            Assert.That(sceneSystem.IsLoading, Is.False);
            Assert.That(sceneSystem.CurrentSceneName, Is.EqualTo(secondSceneName));
        }

        private sealed class SceneTestArchitecture : Architecture<SceneTestArchitecture>
        {
            protected override void Init()
            {
                this.RegisterSystem<IRuntimeUpdateSystem>(new TestRuntimeUpdateSystem());
                this.RegisterSystem<ISceneSystem>(new SceneSystem());
            }
        }

        private sealed class TestRuntimeUpdateSystem : AbstractSystem, IRuntimeUpdateSystem
        {
            private event Action Updated;

            protected override void OnInit()
            {
            }

            protected override void OnDeinit()
            {
                Updated = null;
            }

            public void RegisterUpdate(Action onUpdate)
            {
                Updated += onUpdate;
            }

            public void UnregisterUpdate(Action onUpdate)
            {
                Updated -= onUpdate;
            }

            public void Tick()
            {
                Updated?.Invoke();
            }
        }
    }
}
