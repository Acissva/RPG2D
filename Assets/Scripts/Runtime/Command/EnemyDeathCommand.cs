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

        public EnemyDeathCommand(string enemyId, int enemyInstanceId, Vector3 position, string dropTableId)
        {
            this.enemyId = enemyId;
            this.enemyInstanceId = enemyInstanceId;
            this.position = position;
            this.dropTableId = dropTableId;
        }

        protected override void OnExecute()
        {
            this.SendEvent(new EnemyDiedEvent
            {
                EnemyId = enemyId,
                EnemyInstanceId = enemyInstanceId,
                Position = position,
                DropTableId = dropTableId,
            });

            this.SendCommand(new DropLootCommand(dropTableId, position, enemyId));

            Debug.Log($"敌人【{enemyId}】死亡");
        }
    }
}
