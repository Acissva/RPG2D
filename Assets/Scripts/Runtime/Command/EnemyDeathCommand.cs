using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class EnemyDeathCommand : AbstractCommand
    {
        private readonly string enemyId;
        private readonly int enemyInstanceId;
        private readonly Vector3 position;
        private readonly string dropTableId;
        private readonly int experienceReward;

        public EnemyDeathCommand(string enemyId, int enemyInstanceId, Vector3 position, string dropTableId, int experienceReward)
        {
            this.enemyId = enemyId;
            this.enemyInstanceId = enemyInstanceId;
            this.position = position;
            this.dropTableId = dropTableId;
            this.experienceReward = experienceReward;
        }

        public EnemyDeathCommand(string enemyId, Vector3 position, string dropTableId, int experienceReward)
            : this(enemyId, 0, position, dropTableId, experienceReward)
        {
        }

        protected override void OnExecute()
        {
            this.SendEvent(new EnemyDeadEvent
            {
                EnemyId = enemyId,
                EnemyInstanceId = enemyInstanceId,
                Position = position,
                DropTableId = dropTableId,
                ExperienceReward = experienceReward
            });

            this.SendCommand(new DropLootCommand(dropTableId, position, enemyId, experienceReward));

            Debug.Log($"敌人【{enemyId}】死亡");
        }
    }
}
