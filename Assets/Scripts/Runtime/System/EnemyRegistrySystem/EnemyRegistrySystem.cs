using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class EnemyRegistrySystem : AbstractSystem, IEnemyRegistrySystem
    {
        private sealed class EnemyEntry
        {
            public EnemyEntry(int instanceId, string roomId, EnemyRuntimeStats stats, Transform targetTransform)
            {
                InstanceId = instanceId;
                RoomId = roomId ?? string.Empty;
                Stats = stats;
                ViewInfo = new EnemyViewInfo(instanceId, stats, targetTransform);
            }

            public int InstanceId { get; }
            public string RoomId { get; }
            public EnemyRuntimeStats Stats { get; }
            public EnemyViewInfo ViewInfo { get; }
        }

        private readonly List<EnemyEntry> enemies = new();
        private readonly List<EnemyViewInfo> enemyViews = new();

        public IReadOnlyList<EnemyViewInfo> EnemyViews => enemyViews;

        protected override void OnInit()
        {
            this.RegisterEvent<EnemyDiedEvent>(e =>
            {
                UnregisterEnemy(e.EnemyInstanceId);
            });

            this.RegisterEvent<SceneLoadStartedEvent>(e =>
            {
                ClearEnemies();
            });
        }

        public int CountAliveEnemiesInRoom(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return 0;
            }

            int count = 0;

            foreach (EnemyEntry enemy in enemies)
            {
                if (enemy.RoomId == roomId && enemy.Stats.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        public void RegisterEnemy(int enemyInstanceId, string roomId, EnemyRuntimeStats stats, Transform targetTransform)
        {
            if (enemyInstanceId == 0 || stats == null || targetTransform == null)
            {
                return;
            }

            UnregisterEnemy(enemyInstanceId);

            EnemyEntry enemy = new EnemyEntry(enemyInstanceId, roomId, stats, targetTransform);
            enemies.Add(enemy);
            enemyViews.Add(enemy.ViewInfo);
        }

        public void UnregisterEnemy(int enemyInstanceId)
        {
            if (enemyInstanceId == 0)
            {
                return;
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i].InstanceId != enemyInstanceId)
                {
                    continue;
                }

                enemyViews.Remove(enemies[i].ViewInfo);
                enemies.RemoveAt(i);
            }
        }

        private void ClearEnemies()
        {
            enemies.Clear();
            enemyViews.Clear();
        }
    }
}
