using QFramework;
using System;

namespace RPG2D
{
    [Serializable]
    public class EnemyRuntimeStats
    {
        public string EnemyId { get; private set; }

        public int MaxHP { get; private set; }
        public BindableProperty<int> CurrentHP { get; private set; } = new BindableProperty<int>();
        public int AttackPower { get; private set; }
        public int Defense { get; private set; }

        public float MoveSpeed { get; private set; }
        public float ChaseSpeed { get; private set; }
        public float KnockbackResistance { get; private set; }

        /// <summary>
        /// 视野半径
        /// </summary>
        public float DetectionRadius { get; private set; }
        public float AttackRange { get; private set; }
        /// <summary>
        /// 脱战半径
        /// </summary>
        public float LeashRadius { get; private set; }
        /// <summary>
        /// 巡逻半径
        /// </summary>
        public float PatrolRadius { get; private set; }

        /// <summary>
        /// 受击硬直时间
        /// </summary>
        public float HitStunDuration { get; private set; }
        public float AttackCooldown { get; private set; }
        /// <summary>
        /// 攻击前摇时间
        /// </summary>
        public float AttackWindupDuration { get; private set; }
        /// <summary>
        /// 攻击命中窗口时间
        /// </summary>
        public float AttackActiveDuration { get; private set; }
        /// <summary>
        /// 攻击后摇时间
        /// </summary>
        public float AttackRecoveryDuration { get; private set; }

        /// <summary>
        /// 掉落物列表
        /// </summary>
        public string DropTableId { get; private set; }

        public bool IsAlive => CurrentHP.Value > 0;
        public bool IsDead => CurrentHP.Value <= 0;
        public float HPRatio => MaxHP <= 0 ? 0f : (float)CurrentHP.Value / MaxHP;

        public void Init(EnemyConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            EnemyId = config.Identity.EnemyId;

            MaxHP = config.Stats.MaxHP;
            CurrentHP.Value = MaxHP;
            AttackPower = config.Stats.AttackPower;
            Defense = config.Stats.Defense;

            MoveSpeed = config.Movement.MoveSpeed;
            ChaseSpeed = config.Movement.ChaseSpeed;
            KnockbackResistance = config.Movement.KnockbackResistance;

            DetectionRadius = config.Perception.DetectionRadius;
            AttackRange = config.Perception.AttackRange;
            LeashRadius = config.Perception.LeashRadius;
            PatrolRadius = config.Perception.PatrolRadius;

            HitStunDuration = config.Combat.HitStunDuration;
            AttackCooldown = config.Combat.AttackCooldown;
            AttackWindupDuration = config.Combat.AttackWindupDuration;
            AttackActiveDuration = config.Combat.AttackActiveDuration;
            AttackRecoveryDuration = config.Combat.AttackRecoveryDuration;

            DropTableId = config.Reward.DropTableId;
        }

        /// <summary>
        /// 承受直接伤害
        /// </summary>
        /// <param name="rawDamage"></param>
        /// <returns> 实际承受伤害 </returns>
        public int ApplyDamage(int rawDamage)
        {
            if (IsDead)
            {
                return 0;
            }

            int damage = rawDamage - Defense;
            if (damage < 1)
            {
                damage = 1;
            }

            int oldHP = CurrentHP.Value;
            CurrentHP.Value -= damage;

            if (CurrentHP.Value < 0)
            {
                CurrentHP.Value = 0;
            }

            return oldHP - CurrentHP.Value;
        }

        /// <summary>
        /// 治疗
        /// </summary>
        /// <param name="amount"></param>
        /// <returns> 实际治疗量 </returns>
        public int Heal(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return 0;
            }

            int oldHP = CurrentHP.Value;
            CurrentHP.Value += amount;

            if (CurrentHP.Value > MaxHP)
            {
                CurrentHP.Value = MaxHP;
            }

            return CurrentHP.Value - oldHP;
        }

        public void SetCurrentHP(int hp)
        {
            if (hp < 0)
            {
                CurrentHP.Value = 0;
                return;
            }

            CurrentHP.Value = hp > MaxHP ? MaxHP : hp;
        }

        /// <summary>
        /// 重置为最大生命值
        /// </summary>
        public void ResetHP()
        {
            CurrentHP.Value = MaxHP;
        }
    }
}
