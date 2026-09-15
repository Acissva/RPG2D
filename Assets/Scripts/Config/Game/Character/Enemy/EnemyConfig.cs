using System;
using UnityEngine;

namespace RPG2D
{
    [CreateAssetMenu(
        fileName = "EnemyConfig",
        menuName = "RPG2D/Config/Game/Character/Enemy/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [SerializeField] private EnemyIdentityConfig identity = new EnemyIdentityConfig();
        [SerializeField] private EnemyStatsConfig stats = new EnemyStatsConfig();
        [SerializeField] private EnemyMovementConfig movement = new EnemyMovementConfig();
        [SerializeField] private EnemyPerceptionConfig perception = new EnemyPerceptionConfig();
        [SerializeField] private EnemyCombatConfig combat = new EnemyCombatConfig();
        [SerializeField] private EnemyRewardConfig reward = new EnemyRewardConfig();

        public EnemyIdentityConfig Identity => identity;
        public EnemyStatsConfig Stats => stats;
        public EnemyMovementConfig Movement => movement;
        public EnemyPerceptionConfig Perception => perception;
        public EnemyCombatConfig Combat => combat;
        public EnemyRewardConfig Reward => reward;

        private void OnValidate()
        {
            identity ??= new EnemyIdentityConfig();
            stats ??= new EnemyStatsConfig();
            movement ??= new EnemyMovementConfig();
            perception ??= new EnemyPerceptionConfig();
            combat ??= new EnemyCombatConfig();
            reward ??= new EnemyRewardConfig();

            identity.Validate();
            stats.Validate();
            movement.Validate();
            perception.Validate();
            combat.Validate();
            reward.Validate();
        }
    }

    [Serializable]
    public class EnemyIdentityConfig
    {
        [SerializeField] private string enemyId = "enemy_001";

        public string EnemyId => enemyId;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                enemyId = "enemy_001";
            }
        }
    }

    [Serializable]
    public class EnemyStatsConfig
    {
        [SerializeField, Min(1)] private int maxHP = 10;
        [SerializeField, Min(0)] private int attackPower = 1;
        [SerializeField, Min(0)] private int defense = 0;

        public int MaxHP => maxHP;
        public int AttackPower => attackPower;
        public int Defense => defense;

        public void Validate()
        {
            if (maxHP < 1) maxHP = 1;
            if (attackPower < 0) attackPower = 0;
            if (defense < 0) defense = 0;
        }
    }

    [Serializable]
    public class EnemyMovementConfig
    {
        [SerializeField, Min(0f)] private float moveSpeed = 2f;
        [SerializeField, Min(0f)] private float chaseSpeed = 3f;

        [Tooltip("击退抗性")]
        [SerializeField, Min(0f)] private float knockbackResistance = 0f;

        public float MoveSpeed => moveSpeed;
        public float ChaseSpeed => chaseSpeed;
        public float KnockbackResistance => knockbackResistance;

        public void Validate()
        {
            if (moveSpeed < 0f) moveSpeed = 0f;
            if (chaseSpeed < 0f) chaseSpeed = 0f;
            if (knockbackResistance < 0f) knockbackResistance = 0f;
        }
    }

    [Serializable]
    public class EnemyPerceptionConfig
    {
        [Tooltip("检测半径（黄色）")]
        [SerializeField, Min(0f)] private float detectionRadius = 5f;

        [Tooltip("攻击距离（红色）")]
        [SerializeField, Min(0f)] private float attackRange = 1.2f;

        [Tooltip("脱战半径（绿色）")]
        [SerializeField, Min(0f)] private float leashRadius = 10f;

        [Tooltip("巡逻半径（蓝色）")]
        [SerializeField, Min(0f)] private float patrolRadius = 3f;

        public float DetectionRadius => detectionRadius;
        public float AttackRange => attackRange;
        public float LeashRadius => leashRadius;
        public float PatrolRadius => patrolRadius;

        public void Validate()
        {
            if (detectionRadius < 0f) detectionRadius = 0f;
            if (attackRange < 0f) attackRange = 0f;
            if (leashRadius < 0f) leashRadius = 0f;
            if (patrolRadius < 0f) patrolRadius = 0f;

            //脱战半径 >= 检测半径
            if (leashRadius < detectionRadius)
            {
                leashRadius = detectionRadius;
            }

            //巡逻半径 <= 脱战半径
            if (patrolRadius > leashRadius)
            {
                patrolRadius = leashRadius;
            }
        }
    }

    [Serializable]
    public class EnemyCombatConfig
    {
        [Tooltip("受击硬直时间")]
        [SerializeField, Min(0f)] private float hitStunDuration = 0.5f;

        [Tooltip("攻击冷却时间")]
        [SerializeField, Min(0f)] private float attackCooldown = 1f;

        [Tooltip("攻击前摇时间")]
        [SerializeField, Min(0f)] private float attackWindupDuration = 0.3f;

        [Tooltip("攻击命中窗口")]
        [SerializeField, Min(0f)] private float attackActiveDuration = 0.15f;

        [Tooltip("攻击后摇时间")]
        [SerializeField, Min(0f)] private float attackRecoveryDuration = 0.4f;

        public float HitStunDuration => hitStunDuration;
        public float AttackCooldown => attackCooldown;
        public float AttackWindupDuration => attackWindupDuration;
        public float AttackActiveDuration => attackActiveDuration;
        public float AttackRecoveryDuration => attackRecoveryDuration;

        public void Validate()
        {
            if (hitStunDuration < 0f) hitStunDuration = 0f;
            if (attackCooldown < 0f) attackCooldown = 0f;
            if (attackWindupDuration < 0f) attackWindupDuration = 0f;
            if (attackActiveDuration < 0f) attackActiveDuration = 0f;
            if (attackRecoveryDuration < 0f) attackRecoveryDuration = 0f;
        }
    }

    [Serializable]
    public class EnemyRewardConfig
    {
        [Tooltip("掉落表 ID")]
        [SerializeField] private string dropTableId = "drop_enemy_001";

        public string DropTableId => dropTableId;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(dropTableId))
            {
                dropTableId = "drop_enemy_001";
            }
        }
    }
}
