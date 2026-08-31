using QFramework;
using System.Collections.Generic;

namespace RPG2D
{
    public interface IEnemyRegistrySystem : ISystem
    {
        IReadOnlyList<EnemyController> Enemies { get; }

        void RegisterEnemy(EnemyController enemy);
        void UnregisterEnemy(EnemyController enemy);
        void UnregisterEnemy(int enemyInstanceId);

    }

    public class EnemyRegistrySystem : AbstractSystem, IEnemyRegistrySystem
    {
        private List<EnemyController> enemies = new();

        public IReadOnlyList<EnemyController> Enemies => enemies;

        protected override void OnInit()
        {
            this.RegisterEvent<EnemyDeadEvent>(e =>
            {
                UnregisterEnemy(e.EnemyInstanceId);
            });
        }

        public void RegisterEnemy(EnemyController enemy)
        {
            if (enemy == null) return;
            if (enemies.Contains(enemy)) return;

            enemies.Add(enemy);
        }

        public void UnregisterEnemy(EnemyController enemy)
        {
            if (enemy == null) return;
            if (!enemies.Contains(enemy)) return;

            enemies.Remove(enemy);
        }

        public void UnregisterEnemy(int enemyInstanceId)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if(enemies[i].gameObject.GetInstanceID() == enemyInstanceId)
                {
                    enemies.RemoveAt(i);
                    return;
                }
            }
        }

    }
}

