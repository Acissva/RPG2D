using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class EnemyController : RPG2DController
    {
        private EnemyConfig config;

        private Rigidbody2D mRigidbody2D;
        private Vector2 spawnPosition;
        private Vector2 moveTarget;

        public EnemyConfig Config => config;
        public EnemyRuntimeStats Stats { get; private set; }

        private void Awake()
        {
            if (!TryGetComponent<Rigidbody2D>(out mRigidbody2D))
            {
                Debug.LogError($"{typeof(EnemyController)} requires Rigidbody2D");
            }

            config = this.GetSystem<IConfigSystem>().EnemyConfig;

            if (config == null)
            {
                Debug.LogError($"{typeof(EnemyController)} requires EnemyConfig");
                enabled = false;
                return;
            }

            Stats = new EnemyRuntimeStats();

            Stats.Init(config);

            this.GetSystem<IEnemyRegistrySystem>().RegisterEnemy(this);
        }

        private void Start()
        {
            spawnPosition = transform.position;
            moveTarget = spawnPosition;
        }

        private void OnDestroy()
        {
            this.GetSystem<IEnemyRegistrySystem>().UnregisterEnemy(this);
        }

    }
}

