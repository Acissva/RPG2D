using NUnit.Framework;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace RPG2D.Tests.PlayMode
{
    public class TimerSystemTests
    {
        private const string ExpectedExceptionMessage = "expected timer exception";

        private ITimerSystem timerSystem;

        [SetUp]
        public void SetUp()
        {
            timerSystem = global::RPG2D.RPG2D.Interface.GetSystem<ITimerSystem>();
            timerSystem.CancelScope(TimerScope.Scene);
            timerSystem.CancelScope(TimerScope.Session);
        }

        [TearDown]
        public void TearDown()
        {
            if (timerSystem == null)
            {
                return;
            }

            timerSystem.CancelScope(TimerScope.Scene);
            timerSystem.CancelScope(TimerScope.Session);
            timerSystem = null;
        }

        [UnityTest]
        public IEnumerator Cancel_WhenTaskIsPending_PreventsCallback()
        {
            bool callbackInvoked = false;
            TimerHandle handle = timerSystem.Schedule(0f, () => callbackInvoked = true, TimerScope.Scene);

            bool cancelled = timerSystem.Cancel(handle);

            yield return null;

            Assert.That(cancelled, Is.True);
            Assert.That(callbackInvoked, Is.False);
        }

        [UnityTest]
        public IEnumerator Update_WhenCallbackThrows_ContinuesWithRemainingDueCallbacks()
        {
            bool remainingCallbackInvoked = false;

            LogAssert.Expect(LogType.Exception, new Regex(Regex.Escape(ExpectedExceptionMessage)));

            timerSystem.Schedule(0f, () => throw new InvalidOperationException(ExpectedExceptionMessage), TimerScope.Scene);
            timerSystem.Schedule(0f, () => remainingCallbackInvoked = true, TimerScope.Scene);

            yield return null;

            Assert.That(remainingCallbackInvoked, Is.True);
        }
    }
}
