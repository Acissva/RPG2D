using QFramework;
using RPG2D.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class EnemiesStatusHUD : RPG2DController
    {
        private IAssetSystem assetSystem;
        private IEnemyRegistrySystem enemyRegistrySystem;
        private IObjectPoolUtility poolUtility;

        [Header("相机")]
        [SerializeField] private Camera renderTargetCamera;

        [Header("血条")]
        private Dictionary<int, HealthBarView> healthBarViews = new();
        [SerializeField] private GameObject healthBarViewPrefab;
        [SerializeField] private Vector2 healthBarViewOffset = new Vector2(0, 1);

        private void Awake()
        {
            assetSystem = this.GetSystem<IAssetSystem>();
            enemyRegistrySystem = this.GetSystem<IEnemyRegistrySystem>();
            poolUtility = this.GetUtility<IObjectPoolUtility>();

            if (renderTargetCamera == null)
            {
                renderTargetCamera = Camera.main;

                if (renderTargetCamera == null)
                {
                    enabled = false;
                    return;
                }
            }

            if (healthBarViewPrefab == null)
            {
                healthBarViewPrefab = assetSystem.Load<GameObject>("Prefabs/UI/HealthBar");
            }
            else
            {
                if (!healthBarViewPrefab.GetComponent<HealthBarView>())
                {
                    Debug.LogError("EnemiesStatusHUD init fault : HealthBarViewPrefab requires HealthBarView");
                    enabled = false;
                    return;
                }
            }

            if (healthBarViewPrefab == null)
            {
                Debug.LogError("Can not find HealthBar Prefab : Prefabs/UI/HealthBar");
                enabled = false;
                return;
            }

            this.RegisterEvent<EnemyDeadEvent>(e =>
            {
                RemoveEnemy(e.EnemyInstanceId);
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            var enemies = enemyRegistrySystem.Enemies;

            foreach (EnemyController enemy in enemies)
            {
                AddEnemy(enemy);
            }
        }

        private void Update()
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            var enemies = enemyRegistrySystem.Enemies;

            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.Stats == null)
                {
                    continue;
                }

                if (healthBarViews.TryGetValue(enemy.gameObject.GetInstanceID(), out HealthBarView healthBarView))
                {
                    Vector3 tempPos = renderTargetCamera.WorldToScreenPoint(enemy.transform.position + new Vector3(healthBarViewOffset.x, healthBarViewOffset.y, 0));
                    healthBarView.transform.position = new Vector3(tempPos.x, tempPos.y, healthBarView.transform.position.z);
                }
                else if (!enemy.Stats.IsDead)
                {
                    AddEnemy(enemy);
                }
            }
        }

        private void AddEnemy(EnemyController enemy)
        {
            GameObject healthBarViewObj = poolUtility.Spawn(healthBarViewPrefab, Vector2.zero, Quaternion.identity, transform);
            HealthBarView healthBarView = healthBarViewObj.GetComponent<HealthBarView>();

            if (healthBarView == null)
            {
                poolUtility.Despawn(healthBarViewObj);
                return;
            }

            healthBarViews.Add(enemy.gameObject.GetInstanceID(), healthBarView);

            enemy.Stats.CurrentHP.Register(e =>
            {
                healthBarView.Refresh(enemy.Stats.HPRatio);
            }).UnRegisterWhenGameObjectDestroyed(enemy);

            healthBarView.Refresh(1);
        }

        private void RemoveEnemy(int enemyInstanceId)
        {
            if (healthBarViews.TryGetValue(enemyInstanceId, out var view))
            {
                poolUtility.Despawn(view.gameObject);
                healthBarViews.Remove(enemyInstanceId);
            }
        }


    }

}
