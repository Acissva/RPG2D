using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemySensor2D : MonoBehaviour
    {
        [Header("Scan")]
        [SerializeField, Min(0.02f)] private float scanInterval = 0.1f;    //扫描间隔
        [SerializeField, Min(1)] private int maxOverlapResults = 16;    //单次扫描最大碰撞体数量
        [SerializeField] private LayerMask targetLayers;    //目标所在层级
        [SerializeField] private bool scanAutomatically = true;     //是否自动扫描
        [SerializeField] private bool includeTriggerColliders = true;       //是否包含触发器碰撞体

        [Header("Faction")]
        [SerializeField] private FactionType selfFaction = FactionType.Enemy;       //自身阵营（单选）
        [SerializeField] private FactionMask targetFactions = FactionMask.Player;       //目标阵营（可多选）

        [Header("Line Of Sight")]   //实现检测相关
        [SerializeField] private bool requireLineOfSight;       //是否需要视线（有障碍物就看不到目标）
        [SerializeField] private LayerMask obstacleLayers;      //障碍物层级（阻挡视线）
        [SerializeField] private Transform sightOrigin;     //视线起点

        [Header("Fallback Radius")]     //后备数值：当敌人属性组件为空时，使用这些默认值
        [SerializeField, Min(0f)] private float fallbackDetectionRadius = 5f;       //后备检测半径
        [SerializeField, Min(0f)] private float fallbackAttackRange = 1.5f;     //后备攻击范围
        [SerializeField, Min(0f)] private float fallbackLeashRadius = 8f;      //后备脱战半径
        [SerializeField, Min(0f)] private float fallbackPatrolRadius = 5f;      //后备巡逻半径

        private readonly List<CombatTarget2D> detectedTargets = new List<CombatTarget2D>();     //本次扫描到的有效目标列表

        private EnemyController enemyController;
        private Collider2D[] overlapResults;    //碰撞结果预分配数组，避免每次扫描都 new 数组产生 GC
        private Vector2 spawnPosition;      //出生点
        private float scanTimer;    //扫描倒计时计时器

        public FactionType SelfFaction => selfFaction;
        public CombatTarget2D CurrentTarget { get; private set; }       //当前锁定的最优目标
        public Transform CurrentTargetTransform => CurrentTarget != null ? CurrentTarget.transform : null;
        public Vector2 CurrentTargetPosition => CurrentTarget != null ? CurrentTarget.TargetPosition : (Vector2)transform.position;
        public IReadOnlyList<CombatTarget2D> DetectedTargets => detectedTargets;
        public bool HasCurrentTarget => CurrentTarget != null;
        public bool HasTargetInAttackRange => CurrentTarget != null && IsTargetInAttackRange(CurrentTarget);
        public bool IsCurrentTargetOutsideLeash => CurrentTarget != null && IsPositionOutsideLeash(CurrentTarget.TargetPosition);
        public bool IsSelfOutsideLeash => IsPositionOutsideLeash(transform.position);

        private void Awake()
        {
            enemyController = GetComponent<EnemyController>();
            spawnPosition = transform.position;
            EnsureOverlapBuffer();      //初始化碰撞结果数组
            ConfigureDefaultTargetLayers();     //自动配置默认目标层级
        }

        private void Start()
        {
            spawnPosition = transform.position;
            scanTimer = 0f;
        }

        private void Reset()
        {
            ConfigureDefaultTargetLayers();
        }

        private void Update()
        {
            if (!scanAutomatically)
            {
                return;
            }

            scanTimer -= Time.deltaTime;
            if (scanTimer > 0f)
            {
                return;
            }

            scanTimer = scanInterval;
            Scan();
        }

        public void SetSpawnPosition(Vector2 position)
        {
            spawnPosition = position;
        }

        public void ClearCurrentTarget()
        {
            CurrentTarget = null;
        }

        public bool TryGetCurrentTarget(out CombatTarget2D target)
        {
            target = CurrentTarget;
            return target != null;
        }

        /// <summary>
        /// 执行一次完整扫描，更新目标列表并返回最优目标
        /// </summary>
        public CombatTarget2D Scan()
        {
            EnsureOverlapBuffer();      //确保数组长度正确
            detectedTargets.Clear();    //清空上一轮的目标列表，复用集合避免 GC

            if (targetLayers.value == 0)    //没设置目标图层，直接结束
            {
                ClearInvalidCurrentTarget();
                return CurrentTarget;
            }

            Vector2 origin = GetSightOriginPosition();      //获取检测起点
            float detectionRadius = GetDetectionRadius();       //获取检测半径

            //创建物理过滤条件
            var filter = new ContactFilter2D
            {
                useLayerMask = true,    //启用图层过滤
                layerMask = targetLayers,   //指定目标图层
                useTriggers = includeTriggerColliders   //是否包含触发器
            };

            // 2D 圆形范围检测，Physics2D.OverlapCircle + 预分配数组 是 Unity 2D 最高效的范围检测方式，完全不产生堆内存分配( 0 GC )
            int count = Physics2D.OverlapCircle(origin, detectionRadius, filter, overlapResults);

            for (int i = 0; i < count; i++)
            {
                CombatTarget2D target = ResolveTarget(overlapResults[i]);   // 从碰撞体找到 CombatTarget2D 组件
                if (target == null || detectedTargets.Contains(target))
                {
                    continue;
                }

                if (!CanSenseTarget(target, true))  //综合判断：是否能真正感知到这个目标
                {
                    continue;
                }

                detectedTargets.Add(target);
            }

            UpdateCurrentTarget();      //从有效目标中选出最优目标，更新 CurrentTarget
            return CurrentTarget;
        }

        /// <summary>
        /// 综合判断：是否能感知到指定目标
        /// </summary>
        /// <param name="target"></param>
        /// <param name="requireDetectionRange">是否要求目标在检测半径内</param>
        /// <returns></returns>
        public bool CanSenseTarget(CombatTarget2D target, bool requireDetectionRange)
        {
            if (target == null || !target.IsTargetable)     // 空目标 / 不可锁定
            {
                return false;
            }

            // 同根物体（自己身上的碰撞体），排除自己
            if (target.transform == transform || target.transform.IsChildOf(transform) || transform.IsChildOf(target.transform))
            {
                return false;
            }

            if (!IsFactionAllowed(target.Faction))  // 阵营不在可攻击列表里
            {
                return false;
            }

            if (IsPositionOutsideLeash(target.TargetPosition))      // 目标超出牵引范围，不追
            {
                return false;
            }

            if (requireDetectionRange && !IsTargetInDetectionRange(target))     // 不在检测半径内
            {
                return false;
            }

            if (requireLineOfSight && !HasLineOfSight(target))      // 需要视线但被挡住了
            {
                return false;
            }

            return true;
        }

        public bool IsTargetInDetectionRange(CombatTarget2D target)
        {
            return target != null && IsPositionInRange(target.TargetPosition, GetSightOriginPosition(), GetDetectionRadius());
        }

        public bool IsTargetInAttackRange(CombatTarget2D target)
        {
            return target != null && IsPositionInRange(target.TargetPosition, transform.position, GetAttackRange());
        }

        public bool IsPositionOutsideLeash(Vector2 position)
        {
            float leashRadius = GetLeashRadius();
            if (leashRadius <= 0f)
            {
                return false;
            }

            return !IsPositionInRange(position, spawnPosition, leashRadius);
        }

        /// <summary>
        /// 视线检测：两点之间有没有视线
        /// </summary>
        /// <param name="target"></param>
        /// <returns>true：有视线（无障碍），false：无视线（有障碍）</returns>
        public bool HasLineOfSight(CombatTarget2D target)
        {
            if (target == null || obstacleLayers.value == 0)
            {
                return true;
            }

            Vector2 origin = GetSightOriginPosition();
            Vector2 targetPosition = target.TargetPosition;
            RaycastHit2D hit = Physics2D.Linecast(origin, targetPosition, obstacleLayers);

            return hit.collider == null;
        }

        private void UpdateCurrentTarget()
        {
            ClearInvalidCurrentTarget();

            CombatTarget2D bestTarget = SelectBestDetectedTarget();
            if (bestTarget == null)
            {
                return;
            }

            if (CurrentTarget == null || IsBetterTarget(bestTarget, CurrentTarget))
            {
                CurrentTarget = bestTarget;
            }
        }

        private void ClearInvalidCurrentTarget()
        {
            if (CurrentTarget == null)
            {
                return;
            }

            if (!CanSenseTarget(CurrentTarget, false))
            {
                CurrentTarget = null;
            }
        }

        private CombatTarget2D SelectBestDetectedTarget()
        {
            CombatTarget2D bestTarget = null;

            for (int i = 0; i < detectedTargets.Count; i++)
            {
                CombatTarget2D target = detectedTargets[i];
                if (target == null)
                {
                    continue;
                }

                if (bestTarget == null || IsBetterTarget(target, bestTarget))
                {
                    bestTarget = target;
                }
            }

            return bestTarget;
        }

        private bool IsBetterTarget(CombatTarget2D candidate, CombatTarget2D current)
        {
            if (candidate == null)
            {
                return false;
            }

            if (current == null)
            {
                return true;
            }

            if (candidate.ThreatPriority != current.ThreatPriority)
            {
                return candidate.ThreatPriority > current.ThreatPriority;
            }

            float candidateDistance = ((Vector2)candidate.TargetPosition - (Vector2)transform.position).sqrMagnitude;
            float currentDistance = ((Vector2)current.TargetPosition - (Vector2)transform.position).sqrMagnitude;

            return candidateDistance < currentDistance;
        }

        // 因为碰撞体通常在子物体（比如身体、头部），而目标组件挂在根物体上
        /// <summary>
        /// 从碰撞体向上查找父物体中的 CombatTarget2D 组件
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>
        private CombatTarget2D ResolveTarget(Collider2D candidate)
        {
            if (candidate == null)
            {
                return null;
            }

            return candidate.GetComponentInParent<CombatTarget2D>();
        }

        /// <summary>
        /// 判断某个阵营是否在目标阵营掩码中
        /// </summary>
        private bool IsFactionAllowed(FactionType faction)
        {
            FactionMask factionMask = ToFactionMask(faction);
            return (targetFactions & factionMask) != 0;
        }

        private static FactionMask ToFactionMask(FactionType faction)
        {
            switch (faction)
            {
                case FactionType.Neutral:
                    return FactionMask.Neutral;
                case FactionType.Player:
                    return FactionMask.Player;
                case FactionType.Enemy:
                    return FactionMask.Enemy;
                default:
                    return FactionMask.None;
            }
        }

        private bool IsPositionInRange(Vector2 position, Vector2 origin, float range)
        {
            if (range <= 0f)
            {
                return false;
            }

            //用平方距离避免开根号
            float sqrRange = range * range;
            return (position - origin).sqrMagnitude <= sqrRange;
        }

        private Vector2 GetSightOriginPosition()
        {
            return sightOrigin != null ? sightOrigin.position : transform.position;
        }

        private float GetDetectionRadius()
        {
            if (enemyController != null && enemyController.Stats != null)
            {
                return enemyController.Stats.DetectionRadius;
            }

            return fallbackDetectionRadius;
        }

        private float GetAttackRange()
        {
            if (enemyController != null && enemyController.Stats != null)
            {
                return enemyController.Stats.AttackRange;
            }

            return fallbackAttackRange;
        }

        private float GetLeashRadius()
        {
            if (enemyController != null && enemyController.Stats != null)
            {
                return enemyController.Stats.LeashRadius;
            }

            return fallbackLeashRadius;
        }

        private float GetPatrolRadius()
        {
            if (enemyController != null && enemyController.Stats != null)
            {
                return enemyController.Stats.PatrolRadius;
            }

            return fallbackPatrolRadius;
        }

        /// <summary>
        /// 自动配置默认目标图层：如果没手动设置，就自动找 Damageable 层
        /// </summary>
        private void ConfigureDefaultTargetLayers()
        {
            if (targetLayers.value != 0)
            {
                return;
            }

            int damageableLayer = LayerMask.NameToLayer("Damageable");

            if (damageableLayer >= 0)   //若图层存在
            {
                targetLayers = 1 << damageableLayer;
            }
        }

        /// <summary>
        /// 确保碰撞结果数组存在且长度正确
        /// </summary>
        private void EnsureOverlapBuffer()
        {
            if (maxOverlapResults < 1)
            {
                maxOverlapResults = 1;
            }

            if (overlapResults == null || overlapResults.Length != maxOverlapResults)
            {
                overlapResults = new Collider2D[maxOverlapResults];
            }
        }

        private void OnValidate()
        {
            if (scanInterval < 0.02f)
            {
                scanInterval = 0.02f;
            }

            if (maxOverlapResults < 1)
            {
                maxOverlapResults = 1;
            }

            if (fallbackDetectionRadius < 0f)
            {
                fallbackDetectionRadius = 0f;
            }

            if (fallbackAttackRange < 0f)
            {
                fallbackAttackRange = 0f;
            }

            if (fallbackLeashRadius < 0f)
            {
                fallbackLeashRadius = 0f;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 origin = sightOrigin != null ? sightOrigin.position : transform.position;

            //  黄色：检测半径
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin, Application.isPlaying ? GetDetectionRadius() : fallbackDetectionRadius);

            // 红色：攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Application.isPlaying ? GetAttackRange() : fallbackAttackRange);

            // 绿色：脱战范围
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Application.isPlaying ? spawnPosition : (Vector2)transform.position,
                Application.isPlaying ? GetLeashRadius() : fallbackLeashRadius);

            //  蓝色：巡逻范围
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(Application.isPlaying ? spawnPosition : (Vector2)transform.position, 
                Application.isPlaying ? GetPatrolRadius() : fallbackPatrolRadius);

            if (CurrentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, CurrentTarget.TargetPosition);
            }
        }

    }

}
