using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class SceneEnemyTracker : RPG2DController
    {
        [SerializeField] private string SceneId = "Main";
        [SerializeField] private bool collectEnemiesInChildrenOnStart = true;
        [SerializeField] private List<EnemyController> trackedEnemies = new List<EnemyController>();

        private readonly HashSet<int> aliveEnemyInstanceIds = new HashSet<int>();
        private int initialEnemyCount;
        private bool isCleared;

        private void Awake()
        {
            this.RegisterEvent<EnemyDeadEvent>(OnEnemyDead)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void Start()
        {
            RebuildEnemyCache();
        }

        public void RebuildEnemyCache()
        {
            aliveEnemyInstanceIds.Clear();

            if (collectEnemiesInChildrenOnStart)
            {
                trackedEnemies.Clear();
                //TODO:后续根据需要修改需要监听的Enemies，例如，某个区域（房间）生成一些Enemy后，将这些Enemy交给Tracker，来监听这些Enemy是否全部死亡
                trackedEnemies.AddRange(GetComponentsInChildren<EnemyController>(true));
            }

            foreach (EnemyController enemy in trackedEnemies)
            {
                if (enemy != null && enemy.Stats != null && enemy.Stats.IsAlive)
                {
                    aliveEnemyInstanceIds.Add(enemy.gameObject.GetInstanceID());
                }
            }

            initialEnemyCount = aliveEnemyInstanceIds.Count;
            isCleared = initialEnemyCount <= 0;
        }

        private void OnEnemyDead(EnemyDeadEvent enemyDeadEvent)
        {
            if (enemyDeadEvent.EnemyInstanceId == 0)
            {
                return;
            }

            if (!aliveEnemyInstanceIds.Remove(enemyDeadEvent.EnemyInstanceId))
            {
                return;
            }

            if (aliveEnemyInstanceIds.Count <= 0 && !isCleared)
            {
                isCleared = true;

                this.SendCommand(new NoticeSceneEnemyClearedCommand(SceneId, initialEnemyCount));

                Debug.Log($"场景【{SceneId}】敌人清空");
            }
        }
    }
}