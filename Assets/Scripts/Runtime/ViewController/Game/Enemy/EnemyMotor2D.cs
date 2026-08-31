using UnityEngine;

namespace RPG2D
{
    [RequireComponent(typeof(EnemyController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyMotor2D : MonoBehaviour
    {
        [Header("Move")]
        [SerializeField, Min(0f)] private float arriveDistance = 0.05f;
        [SerializeField] private bool clearVelocityWhenStop = true;

        [Header("Knockback")]
        [SerializeField, Min(0f)] private float defaultKnockbackForce = 10f;
        [SerializeField, Min(0f)] private float defaultKnockbackDuration = 0.12f;
        [SerializeField, Min(0f)] private float knockbackDrag = 10f;

        private EnemyController enemyController;
        private Rigidbody2D mRigidbody2D;

        private Vector2 spawnPosition;
        private Vector2 moveTarget;
        private Vector2 moveDirection;
        private Vector2 facingDirection = Vector2.down;

        private float currentMoveSpeed;
        //指是否有有效移动目标，速度为 0 时，即使有 moveTarget，hasMoveTarget 也为 false
        private bool hasMoveTarget;

        private Vector2 knockbackVelocity;
        private float knockbackTimer;

        public Vector2 SpawnPosition => spawnPosition;
        public Vector2 MoveTarget => moveTarget;
        public Vector2 MoveDirection => moveDirection;
        public Vector2 FacingDirection => facingDirection;

        public bool HasMoveTarget => hasMoveTarget;
        public bool IsKnockbackActive => knockbackTimer > 0f;

        public bool HasArrived
        {
            get
            {
                if (!hasMoveTarget)
                {
                    return true;
                }

                return ((Vector2)transform.position - moveTarget).sqrMagnitude <= arriveDistance * arriveDistance;
            }
        }

        private EnemyRuntimeStats Stats => enemyController != null ? enemyController.Stats : null;

        private void Awake()
        {
            enemyController = GetComponent<EnemyController>();
            mRigidbody2D = GetComponent<Rigidbody2D>();

            spawnPosition = transform.position;
            moveTarget = spawnPosition;
        }

        private void Start()
        {
            SetSpawnPosition(transform.position);
        }

        private void FixedUpdate()
        {
            if (Stats == null || Stats.IsDead)
            {
                StopImmediately();
                return;
            }

            Vector2 velocity = CalculateMoveVelocity();

            if (IsKnockbackActive)
            {
                velocity += CalculateKnockbackVelocity(Time.fixedDeltaTime);
            }

            mRigidbody2D.velocity = velocity;
        }

        public void SetSpawnPosition(Vector2 position)
        {
            spawnPosition = position;
        }

        public void Stop()
        {
            hasMoveTarget = false;
            moveDirection = Vector2.zero;
            currentMoveSpeed = 0f;

            if (clearVelocityWhenStop && mRigidbody2D != null && !IsKnockbackActive)
            {
                mRigidbody2D.velocity = Vector2.zero;
            }
        }

        public void StopImmediately()
        {
            hasMoveTarget = false;
            moveDirection = Vector2.zero;
            currentMoveSpeed = 0f;
            knockbackTimer = 0f;
            knockbackVelocity = Vector2.zero;

            if (mRigidbody2D != null)
            {
                mRigidbody2D.velocity = Vector2.zero;
            }
        }

        public void MoveTo(Vector2 target)
        {
            float speed = Stats != null ? Stats.MoveSpeed : 0f;
            MoveTo(target, speed);
        }

        public void MoveTo(Vector2 target, float speed)
        {
            moveTarget = target;
            currentMoveSpeed = Mathf.Max(0f, speed);
            hasMoveTarget = currentMoveSpeed > 0f;

            UpdateMoveDirection();
        }

        public void ChaseCurrentTarget(EnemySensor2D sensor)
        {
            if (sensor == null || !sensor.HasCurrentTarget)
            {
                Stop();
                return;
            }

            Chase(sensor.CurrentTargetPosition);
        }

        public void Chase(Transform target)
        {
            if (target == null)
            {
                Stop();
                return;
            }

            Chase(target.position);
        }

        public void Chase(Vector2 targetPosition)
        {
            float speed = Stats != null ? Stats.ChaseSpeed : 0f;
            MoveTo(targetPosition, speed);
            FaceTo(targetPosition);
        }

        public void ReturnToSpawn()
        {
            MoveTo(spawnPosition, Stats != null ? Stats.MoveSpeed : 0f);
            FaceTo(spawnPosition);
        }

        public Vector2 GetRandomPatrolPoint()
        {
            float patrolRadius = Stats != null ? Stats.PatrolRadius : 0f;

            if (patrolRadius <= 0f)
            {
                return spawnPosition;
            }

            return spawnPosition + Random.insideUnitCircle * patrolRadius;
        }

        public void MoveToRandomPatrolPoint()
        {
            MoveTo(GetRandomPatrolPoint(), Stats != null ? Stats.MoveSpeed : 0f);
        }

        public void FaceTo(Transform target)
        {
            if (target == null)
            {
                return;
            }

            FaceTo(target.position);
        }

        public void FaceTo(Vector2 worldPosition)
        {
            Vector2 direction = worldPosition - (Vector2)transform.position;
            FaceDirection(direction);
        }

        public void FaceDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            facingDirection = direction.normalized;
        }

        public void ApplyKnockback(DamageInfo damageInfo)
        {
            Vector2 direction = damageInfo.Direction;

            if (direction.sqrMagnitude <= 0.0001f && damageInfo.Attacker != null)
            {
                direction = (Vector2)transform.position - (Vector2)damageInfo.Attacker.transform.position;
            }

            ApplyKnockback(direction, defaultKnockbackForce, defaultKnockbackDuration);
        }

        public void ApplyKnockback(Vector2 direction, float force, float duration)
        {
            if (direction.sqrMagnitude <= 0.0001f || force <= 0f || duration <= 0f)
            {
                return;
            }

            float resistance = Stats != null ? Stats.KnockbackResistance : 0f;
            float finalForce = Mathf.Max(0f, force - resistance);

            if (finalForce <= 0f)
            {
                return;
            }

            knockbackVelocity = direction.normalized * finalForce;
            knockbackTimer = duration;
            FaceDirection(-direction);
        }

        private Vector2 CalculateMoveVelocity()
        {
            if (!hasMoveTarget || currentMoveSpeed <= 0f)
            {
                moveDirection = Vector2.zero;
                return Vector2.zero;
            }

            UpdateMoveDirection();

            if (HasArrived)
            {
                Stop();
                return Vector2.zero;
            }

            return moveDirection * currentMoveSpeed;
        }

        private Vector2 CalculateKnockbackVelocity(float fixedDeltaTime)
        {
            if (knockbackTimer <= 0f)
            {
                return Vector2.zero;
            }

            knockbackTimer -= fixedDeltaTime;

            Vector2 currentVelocity = knockbackVelocity;
            knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, knockbackDrag * fixedDeltaTime);

            if (knockbackTimer <= 0f)
            {
                knockbackVelocity = Vector2.zero;
            }

            return currentVelocity;
        }

        private void UpdateMoveDirection()
        {
            Vector2 toTarget = moveTarget - (Vector2)transform.position;

            if (toTarget.sqrMagnitude <= arriveDistance * arriveDistance)
            {
                moveDirection = Vector2.zero;
                return;
            }

            moveDirection = toTarget.normalized;
            facingDirection = moveDirection;
        }

    }
}