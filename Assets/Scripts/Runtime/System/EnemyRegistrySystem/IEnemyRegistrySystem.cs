using QFramework;
using System.Collections.Generic;

namespace RPG2D
{
    public interface IEnemyRegistrySystem : ISystem
    {
        IReadOnlyList<EnemyController> Enemies { get; }
        IReadOnlyList<EnemyController> AreaEnemies { get; }

        void RegisterEnemy(EnemyController enemy);
        void UnregisterEnemy(EnemyController enemy);
        void UnregisterEnemy(int enemyInstanceId);
        void ClearAreaEnemies();
    }

    public class EnemyRegistrySystem : AbstractSystem, IEnemyRegistrySystem
    {
        private readonly List<EnemyController> enemies = new();
        private readonly List<EnemyController> areaEnemies = new();

        public IReadOnlyList<EnemyController> Enemies => enemies;
        public IReadOnlyList<EnemyController> AreaEnemies => areaEnemies;

        protected override void OnInit()
        {
            this.RegisterEvent<EnemyDeadEvent>(e =>
            {
                UnregisterEnemy(e.EnemyInstanceId);
            });

            this.RegisterEvent<SceneLoadStartedEvent>(e =>
            {
                ClearAreaEnemies();
            });
        }

        public void RegisterEnemy(EnemyController enemy)
        {
            if (enemy == null)
            {
                return;
            }

            if (!enemies.Contains(enemy))
            {
                enemies.Add(enemy);
            }

            if (!areaEnemies.Contains(enemy))
            {
                areaEnemies.Add(enemy);
            }
        }

        public void UnregisterEnemy(EnemyController enemy)
        {
            if (enemy == null)
            {
                return;
            }

            enemies.Remove(enemy);
            areaEnemies.Remove(enemy);
        }

        public void UnregisterEnemy(int enemyInstanceId)
        {
            RemoveEnemyByInstanceId(enemies, enemyInstanceId);
            RemoveEnemyByInstanceId(areaEnemies, enemyInstanceId);
        }

        public void ClearAreaEnemies()
        {
            areaEnemies.Clear();
        }

        private void RemoveEnemyByInstanceId(
            List<EnemyController> enemyList,
            int enemyInstanceId)
        {
            if (enemyInstanceId == 0)
            {
                return;
            }

            for (int i = enemyList.Count - 1; i >= 0; i--)
            {
                EnemyController enemy = enemyList[i];

                if (enemy == null || enemy.gameObject.GetInstanceID() == enemyInstanceId)
                {
                    enemyList.RemoveAt(i);
                }
            }
        }
    }
}
