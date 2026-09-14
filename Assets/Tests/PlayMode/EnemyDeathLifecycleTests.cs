using NUnit.Framework;
using QFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace RPG2D.Tests.PlayMode
{
    public class EnemyDeathLifecycleTests
    {
        private const string TestEnemyName = "EnemyDeathLifecycleTestEnemy";
        private const string SourceSceneName = "EnemyDeathLifecycleSource";
        private const string DestinationSceneName = "EnemyDeathLifecycleDestination";

        private IArchitecture architecture;
        private ITimerSystem timerSystem;
        private GameObject enemyObject;
        private GameObject existingLootRoot;
        private HashSet<int> existingLootInstanceIds;
        private IUnRegister diedRegistration;
        private int diedEventCount;
        private EnemyDiedEvent receivedDiedEvent;

        [SetUp]
        public void SetUp()
        {
            architecture = global::RPG2D.RPG2D.Interface;
            timerSystem = architecture.GetSystem<ITimerSystem>();
            timerSystem.CancelScope(TimerScope.Scene);
            timerSystem.CancelScope(TimerScope.Session);

            existingLootRoot = GameObject.Find("Loots");
            existingLootInstanceIds = new HashSet<int>();

            LootPickup[] existingLootPickups = UnityEngine.Object.FindObjectsOfType<LootPickup>();

            for (int i = 0; i < existingLootPickups.Length; i++)
            {
                existingLootInstanceIds.Add(existingLootPickups[i].gameObject.GetInstanceID());
            }

            diedEventCount = 0;
            receivedDiedEvent = default;

            diedRegistration = architecture.RegisterEvent<EnemyDiedEvent>(enemyDiedEvent =>
            {
                diedEventCount++;
                receivedDiedEvent = enemyDiedEvent;
            });
        }

        [TearDown]
        public void TearDown()
        {
            diedRegistration?.UnRegister();
            diedRegistration = null;

            if (enemyObject != null)
            {
                UnityEngine.Object.DestroyImmediate(enemyObject);
            }

            DestroyCreatedLoot();

            if (existingLootRoot == null)
            {
                GameObject currentLootRoot = GameObject.Find("Loots");

                if (currentLootRoot != null)
                {
                    UnityEngine.Object.DestroyImmediate(currentLootRoot);
                }
            }

            timerSystem?.CancelScope(TimerScope.Scene);
            timerSystem?.CancelScope(TimerScope.Session);

            architecture = null;
            timerSystem = null;
            enemyObject = null;
            existingLootRoot = null;
            existingLootInstanceIds = null;
        }

        [UnityTest]
        public IEnumerator SceneLoadStarted_AfterEnemyDies_CancelsDelayedDestroyWithoutRepeatingDeathEvent()
        {
            enemyObject = new GameObject(TestEnemyName);

            Rigidbody2D rigidbody2D = enemyObject.AddComponent<Rigidbody2D>();
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            rigidbody2D.gravityScale = 0f;

            EnemyController enemyController = enemyObject.AddComponent<EnemyController>();
            EnemyHealth enemyHealth = enemyObject.AddComponent<EnemyHealth>();

            Assert.That(enemyController.Stats, Is.Not.Null);

            int enemyInstanceId = enemyObject.GetInstanceID();
            int lethalDamage = enemyController.Stats.MaxHP + enemyController.Stats.Defense;

            enemyHealth.TakeDamage(new DamageInfo
            {
                Damage = lethalDamage
            });

            Assert.That(enemyController.Stats.IsDead, Is.True);
            Assert.That(diedEventCount, Is.EqualTo(1));
            Assert.That(receivedDiedEvent.EnemyId, Is.EqualTo(enemyController.Stats.EnemyId));
            Assert.That(receivedDiedEvent.EnemyInstanceId, Is.EqualTo(enemyInstanceId));

            architecture.SendEvent(new SceneLoadStartedEvent
            {
                OperationId = Guid.NewGuid(),
                FromSceneName = SourceSceneName,
                ToSceneName = DestinationSceneName
            });

            yield return new WaitForSeconds(1.6f);
            yield return null;

            Assert.That(enemyObject == null, Is.False);
            Assert.That(diedEventCount, Is.EqualTo(1));
            Assert.That(receivedDiedEvent.EnemyInstanceId, Is.EqualTo(enemyInstanceId));
        }

        private void DestroyCreatedLoot()
        {
            LootPickup[] currentLootPickups = UnityEngine.Object.FindObjectsOfType<LootPickup>();

            for (int i = 0; i < currentLootPickups.Length; i++)
            {
                GameObject lootObject = currentLootPickups[i].gameObject;

                if (!existingLootInstanceIds.Contains(lootObject.GetInstanceID()))
                {
                    UnityEngine.Object.DestroyImmediate(lootObject);
                }
            }
        }
    }
}
