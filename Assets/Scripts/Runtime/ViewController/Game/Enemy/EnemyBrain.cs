using QFramework;
using UnityEngine;

namespace RPG2D
{
    public enum EnemyBrainState
    {
        Spawn,
        Idle,
        Patrol,
        Alert,
        Chase,
        Attack,
        Hurt,
        Return,
        Dead
    }

    [RequireComponent(typeof(EnemyController))]
    [RequireComponent(typeof(EnemySensor2D))]
    [RequireComponent(typeof(EnemyMotor2D))]
    public class EnemyBrain : RPG2DController
    {
        [Header("AI")]
        [SerializeField, Min(0.02f)] private float decisionInterval = 0.1f;
        [SerializeField, Min(0f)] private float spawnDuration = 0f;
        [SerializeField, Min(0f)] private float alertDuration = 0.25f;

        [Header("Default State")]
        [SerializeField] private DefaultState defaultState = DefaultState.Idle;

        [Header("Attack")]
        [SerializeField] private bool requireTargetInRangeWhenHit = true;

        [Header("Debug")]
        [SerializeField] private bool logStateChanges;

        private EnemyController enemyController;
        private EnemySensor2D sensor;
        private EnemyMotor2D motor;

        private float decisionTimer;
        private float stateEnterTime;
        private float attackPhaseTimer;
        private float nextAttackTime;

        private int lastObservedHP;
        private bool hasAppliedAttackHit;
        private AttackPhase attackPhase = AttackPhase.None;

        public EnemyBrainState CurrentState { get; private set; }
        public float StateElapsed => Time.time - stateEnterTime;

        private EnemyRuntimeStats Stats =>
            enemyController != null ? enemyController.Stats : null;

        private enum AttackPhase
        {
            None,
            Windup,
            Active,
            Recovery
        }

        private enum DefaultState
        {
            Idle,
            Patrol
        }

        private void Awake()
        {
            enemyController = GetComponent<EnemyController>();
            sensor = GetComponent<EnemySensor2D>();
            motor = GetComponent<EnemyMotor2D>();
        }

        private void Start()
        {
            if (Stats == null)
            {
                Debug.LogError($"{nameof(EnemyBrain)} requires initialized EnemyRuntimeStats.", this);
                enabled = false;
                return;
            }

            Vector2 spawnPosition = transform.position;
            sensor.SetSpawnPosition(spawnPosition);
            motor.SetSpawnPosition(spawnPosition);

            lastObservedHP = Stats.CurrentHP.Value;
            ChangeState(EnemyBrainState.Spawn);
        }

        private void Update()
        {
            if (Stats == null)
            {
                return;
            }

            if (Stats.IsDead)
            {
                ChangeState(EnemyBrainState.Dead);
                return;
            }

            TryEnterHurtByHpChange();

            decisionTimer -= Time.deltaTime;
            bool shouldThink = decisionTimer <= 0f;

            if (shouldThink)
            {
                decisionTimer = decisionInterval;
                sensor.Scan();
            }

            TickState(Time.deltaTime, shouldThink);
        }

        private void TickState(float deltaTime, bool shouldThink)
        {
            switch (CurrentState)
            {
                case EnemyBrainState.Spawn:
                    TickSpawn();
                    break;

                case EnemyBrainState.Idle:
                    TickIdle(shouldThink);
                    break;

                case EnemyBrainState.Patrol:
                    TickPatrol(shouldThink);
                    break;

                case EnemyBrainState.Alert:
                    TickAlert();
                    break;

                case EnemyBrainState.Chase:
                    TickChase();
                    break;

                case EnemyBrainState.Attack:
                    TickAttack(deltaTime);
                    break;

                case EnemyBrainState.Hurt:
                    TickHurt();
                    break;

                case EnemyBrainState.Return:
                    TickReturn();
                    break;

                case EnemyBrainState.Dead:
                    motor.StopImmediately();
                    break;
            }
        }

        private void TickSpawn()
        {
            motor.Stop();

            if (StateElapsed >= spawnDuration)
            {
                ChangeState(GetDefaultState());
            }
        }

        private void TickIdle(bool shouldThink)
        {
            motor.Stop();

            if (!shouldThink)
            {
                return;
            }

            if (sensor.HasCurrentTarget)
            {
                ChangeState(EnemyBrainState.Alert);
            }
        }

        private void TickPatrol(bool shouldThink)
        {
            //保险措施：防止巡逻半径大于脱战半径
            if (sensor.IsSelfOutsideLeash)
            {
                ChangeState(EnemyBrainState.Return);
                return;
            }

            if (motor.HasArrived)
            {
                motor.MoveTo(motor.GetRandomPatrolPoint());
            }

            if (!shouldThink)
            {
                return;
            }

            if (sensor.HasCurrentTarget)
            {
                ChangeState(EnemyBrainState.Alert);
            }
        }

        private void TickAlert()
        {
            motor.Stop();

            if (!sensor.HasCurrentTarget)
            {
                ChangeState(GetDefaultState());
                return;
            }

            if (sensor.IsCurrentTargetOutsideLeash || sensor.IsSelfOutsideLeash)
            {
                ChangeState(EnemyBrainState.Return);
                return;
            }

            motor.FaceTo(sensor.CurrentTargetPosition);

            if (StateElapsed >= alertDuration)
            {
                ChangeState(EnemyBrainState.Chase);
            }
        }

        private void TickChase()
        {
            if (!sensor.HasCurrentTarget)
            {
                ChangeState(EnemyBrainState.Return);
                return;
            }

            if (sensor.IsCurrentTargetOutsideLeash || sensor.IsSelfOutsideLeash)
            {
                ChangeState(EnemyBrainState.Return);
                return;
            }

            if (sensor.HasTargetInAttackRange)
            {
                motor.Stop();
                motor.FaceTo(sensor.CurrentTargetPosition);

                if (Time.time >= nextAttackTime)
                {
                    ChangeState(EnemyBrainState.Attack);
                }

                return;
            }

            motor.ChaseCurrentTarget(sensor);
        }

        private void TickAttack(float deltaTime)
        {
            motor.Stop();

            if (sensor.HasCurrentTarget)
            {
                motor.FaceTo(sensor.CurrentTargetPosition);
            }

            attackPhaseTimer -= deltaTime;

            if (attackPhase == AttackPhase.Windup && attackPhaseTimer <= 0f)
            {
                BeginAttackActive();
            }

            if (attackPhase == AttackPhase.Active)
            {
                TryApplyAttackHit();

                if (attackPhaseTimer <= 0f)
                {
                    BeginAttackRecovery();
                }
            }

            if (attackPhase == AttackPhase.Recovery && attackPhaseTimer <= 0f)
            {
                FinishAttack();
            }
        }

        private void TickHurt()
        {
            motor.Stop();

            float stunDuration = Stats != null ? Stats.HitStunDuration : 0f;
            if (StateElapsed < stunDuration || motor.IsKnockbackActive)
            {
                return;
            }

            if (sensor.HasCurrentTarget && !sensor.IsCurrentTargetOutsideLeash && !sensor.IsSelfOutsideLeash)
            {
                ChangeState(EnemyBrainState.Chase);
                return;
            }

            if (sensor.IsSelfOutsideLeash)
            {
                ChangeState(EnemyBrainState.Return);
                return;
            }

            ChangeState(GetDefaultState());
        }

        private void TickReturn()
        {
            sensor.ClearCurrentTarget();
            motor.ReturnToSpawn();

            if (motor.HasArrived)
            {
                ChangeState(GetDefaultState());
            }
        }

        /*
         * Start() 会调用一次 ChangeState(EnemyBrainState.Spawn);
         * 但 CurrentState 默认值本来就是 Spawn，所以 ChangeState() 会直接 return，不会刷新 stateEnterTime
         * hasStateInitialized 解决该问题
         */
        private bool hasStateInitialized;
        private void ChangeState(EnemyBrainState nextState)
        {
            if (hasStateInitialized && CurrentState == nextState)
            {
                return;
            }

            hasStateInitialized = true;

            ExitState(CurrentState);

            CurrentState = nextState;
            stateEnterTime = Time.time;

            if (logStateChanges)
            {
                Debug.Log($"{name} EnemyBrain: {CurrentState}", this);
            }

            EnterState(nextState);
        }

        private void EnterState(EnemyBrainState state)
        {
            switch (state)
            {
                case EnemyBrainState.Idle:
                    motor.Stop();
                    break;

                case EnemyBrainState.Alert:
                    motor.Stop();
                    sensor.Scan();
                    break;

                case EnemyBrainState.Attack:
                    BeginAttackWindup();
                    break;

                case EnemyBrainState.Hurt:
                    attackPhase = AttackPhase.None;
                    motor.Stop();
                    break;

                case EnemyBrainState.Return:
                    sensor.ClearCurrentTarget();
                    motor.ReturnToSpawn();
                    break;

                case EnemyBrainState.Dead:
                    motor.StopImmediately();
                    enabled = false;
                    break;
            }
        }

        private void ExitState(EnemyBrainState state)
        {
            if (state == EnemyBrainState.Attack)
            {
                attackPhase = AttackPhase.None;
                hasAppliedAttackHit = false;
            }
        }

        private void BeginAttackWindup()
        {
            attackPhase = AttackPhase.Windup;
            attackPhaseTimer = Mathf.Max(0f, Stats.AttackWindupDuration);
            hasAppliedAttackHit = false;

            if (attackPhaseTimer <= 0f)
            {
                BeginAttackActive();
            }
        }

        private void BeginAttackActive()
        {
            attackPhase = AttackPhase.Active;
            attackPhaseTimer = Mathf.Max(0f, Stats.AttackActiveDuration);
            TryApplyAttackHit();

            if (attackPhaseTimer <= 0f)
            {
                BeginAttackRecovery();
            }
        }

        private void BeginAttackRecovery()
        {
            attackPhase = AttackPhase.Recovery;
            attackPhaseTimer = Mathf.Max(0f, Stats.AttackRecoveryDuration);
        }

        private void FinishAttack()
        {
            nextAttackTime = Time.time + Mathf.Max(0f, Stats.AttackCooldown);

            if (!sensor.HasCurrentTarget || sensor.IsCurrentTargetOutsideLeash || sensor.IsSelfOutsideLeash)
            {
                ChangeState(EnemyBrainState.Return);
                return;
            }

            ChangeState(EnemyBrainState.Chase);
        }

        private void TryApplyAttackHit()
        {
            if (hasAppliedAttackHit || !sensor.HasCurrentTarget)
            {
                return;
            }

            if (requireTargetInRangeWhenHit && !sensor.HasTargetInAttackRange)
            {
                return;
            }

            hasAppliedAttackHit = true;

            CombatTarget2D target = sensor.CurrentTarget;
            int damage = Mathf.Max(0, Stats.AttackPower);

            IDamageable damageable = target.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                Vector2 direction = target.TargetPosition - (Vector2)transform.position;

                damageable.TakeDamage(new DamageInfo
                {
                    Attacker = gameObject,
                    Damage = damage,
                    Direction = direction,
                    HitPoint = target.TargetPosition
                });

                return;
            }

            if (target.Faction == FactionType.Player)
            {
                this.SendCommand(new HurtPlayerCommand(damage));
            }
        }

        private void TryEnterHurtByHpChange()
        {
            if (Stats == null)
            {
                return;
            }

            if (Stats.CurrentHP.Value < lastObservedHP && Stats.IsAlive)
            {
                ChangeState(EnemyBrainState.Hurt);
            }

            lastObservedHP = Stats.CurrentHP.Value;
        }

        private EnemyBrainState GetDefaultState()
        {
            return defaultState == DefaultState.Patrol
                ? EnemyBrainState.Patrol
                : EnemyBrainState.Idle;
        }

    }
}