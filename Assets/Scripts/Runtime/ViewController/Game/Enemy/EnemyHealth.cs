using QFramework;
using UnityEngine;

namespace RPG2D
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyHealth : RPG2DController, IDamageable
    {
        private EnemyController enemyController;
        private Animator animator;

        private static readonly int dieAnimHash = Animator.StringToHash("Die");
        private static readonly int getHurtAnimHash = Animator.StringToHash("GetHurt");

        private bool isDead;
        private TimerHandle deathVisualTimer;

        private void Awake()
        {
            enemyController = GetComponent<EnemyController>();
            animator = GetComponent<Animator>();
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (isDead || enemyController == null || enemyController.Stats == null)
            {
                return;
            }

            int actualDamage = enemyController.Stats.ApplyDamage(damageInfo.Damage);

            if (animator != null)
            {
                animator.Play(getHurtAnimHash, 0, 0);
            }

            Debug.Log(
                $"{name} 受到 {actualDamage} 伤害，HP：" +
                $"{enemyController.Stats.CurrentHP} / {enemyController.Stats.MaxHP}");

            EnemyMotor2D motor = GetComponent<EnemyMotor2D>();
            if (motor != null)
            {
                motor.ApplyKnockback(damageInfo);
            }

            if (enemyController.Stats.IsDead)
            {
                Die();
            }
        }

        private void Die()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;

            EnemyRuntimeStats stats = enemyController.Stats;

            CombatTarget2D combatTarget = GetComponent<CombatTarget2D>();
            if (combatTarget != null)
            {
                combatTarget.SetTargetable(false);
            }

            EnemyMotor2D motor = GetComponent<EnemyMotor2D>();
            if (motor != null)
            {
                motor.StopImmediately();
            }

            if (animator != null)
            {
                animator.Play(dieAnimHash, 0, 0);
            }

            string enemyId = stats.EnemyId;
            int instanceID = gameObject.GetInstanceID();
            Vector3 pos = transform.position;
            string dropTableId = stats.DropTableId;
            int experienceReward = stats.ExperienceReward;

            this.SendCommand(new EnemyDeathCommand(enemyId, instanceID, pos, dropTableId, experienceReward));

            deathVisualTimer = this.GetSystem<ITimerSystem>().Schedule(
                1.5f,
                DestroyAfterDeathAnimation,
                TimerScope.Scene);
        }

        private void DestroyAfterDeathAnimation()
        {
            deathVisualTimer = default;
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (!deathVisualTimer.IsValid)
            {
                return;
            }

            this.GetSystem<ITimerSystem>().Cancel(deathVisualTimer);
            deathVisualTimer = default;
        }
    }
}
