using System.Collections;
using UnityEngine;

namespace RPG2D
{
    public class TriangleSword : MonoBehaviour, IWeapon
    {
        private static readonly int IdleStateHash =
            Animator.StringToHash("TriangleSword_Idle");

        private static readonly int AttackR2LStateHash =
            Animator.StringToHash("TriangleSword_Attack_R2L");

        private static readonly int AttackL2RStateHash =
            Animator.StringToHash("TriangleSword_Attack_L2R");

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private AttackHitBox hitBox;

        [Header("Attack")]
        [SerializeField] private GameObject owner;
        [SerializeField, Min(0)] private int damage = 1;
        [SerializeField, Min(0.01f)] private float weaponDistance = 1f;
        [SerializeField, Min(0)] private float cooldownAfterCombo = 0.1f;

        [Header("Fail Safe")]
        [SerializeField, Min(0.1f)] private float attackMaxDuration = 1.2f;

        private readonly int[] attackStateHashes =
        {
            AttackR2LStateHash,
            AttackL2RStateHash
        };

        private int comboIndex;
        private float nextAttackTime;

        private bool canQueueNextAttack;
        private bool hasQueuedNextAttack;

        private Vector2 currentAimDir = Vector2.right;
        private Vector2 queuedAimDir = Vector2.right;

        private DamageInfo currentDamageInfo;
        private Coroutine failSafeRoutine;
        private bool isValid;

        public bool IsAttacking { get; private set; }

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (hitBox == null)
            {
                hitBox = GetComponentInChildren<AttackHitBox>(true);
            }

            if (owner == null)
            {
                //TODO：后续敌人也持有武器则需要修改
                var playerController = GetComponentInParent<PlayerController>();
                if (playerController != null)
                {
                    owner = playerController.gameObject;
                }
                else
                {
                    owner = transform.gameObject;
                }
            }

            if (animator == null)
            {
                Debug.LogError($"{nameof(TriangleSword)} requires an Animator.", this);
                enabled = false;
                return;
            }

            if (hitBox == null)
            {
                Debug.LogError($"{nameof(TriangleSword)} requires an AttackHitBox.", this);
                enabled = false;
                return;
            }

            if (weaponDistance <= 0f)
            {
                Debug.LogWarning($"{nameof(TriangleSword)} weaponDistance was <= 0, reset to 0.55.", this);
                weaponDistance = 0.55f;
            }

            hitBox.Close();
            isValid = true;
        }

        private void OnDisable()
        {
            hitBox?.Close();
            StopFailSafe();

            IsAttacking = false;
            canQueueNextAttack = false;
            hasQueuedNextAttack = false;
        }

        public bool TryAttack(Vector2 aimDir)
        {
            if (!isValid) return false;

            if (aimDir.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            aimDir.Normalize();

            if (IsAttacking)
            {
                if (!canQueueNextAttack)
                {
                    return false;
                }

                hasQueuedNextAttack = true;
                queuedAimDir = aimDir;
                return true;
            }

            if (Time.time < nextAttackTime)
            {
                return false;
            }

            StartAttack(0, aimDir);

            return true;
        }

        private void StartAttack(int index, Vector2 aimDir)
        {
            IsAttacking = true;
            canQueueNextAttack = false;
            hasQueuedNextAttack = false;

            comboIndex = Mathf.Clamp(index, 0, attackStateHashes.Length - 1);
            currentAimDir = aimDir;

            SetAimDirection(currentAimDir);
            PrepareDamageInfo();

            hitBox.Close();
            animator.Play(attackStateHashes[comboIndex], 0, 0f);

            RestartFailSafe();
        }

        private void PrepareDamageInfo()
        {
            currentDamageInfo = new DamageInfo
            {
                Attacker = owner,
                Damage = damage,
                Direction = currentAimDir,
                HitPoint = transform.position
            };
        }

        private void SetAimDirection(Vector2 aimDir)
        {
            transform.localPosition = aimDir * weaponDistance;

            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        public void Anim_OpenHitBox()
        {
            if (!IsAttacking) return;

            currentDamageInfo.Direction = currentAimDir;
            currentDamageInfo.HitPoint = transform.position;

            hitBox.Open(currentDamageInfo);
        }

        public void Anim_CloseHitBox()
        {
            hitBox?.Close();
        }

        public void Anim_OpenComboWindow()
        {
            if (!IsAttacking) return;

            canQueueNextAttack = true;
        }

        public void Anim_CloseComboWindow()
        {
            canQueueNextAttack = false;
        }

        public void Anim_EndAttack()
        {
            if (!IsAttacking) return;

            hitBox.Close();
            canQueueNextAttack = false;

            if (hasQueuedNextAttack)
            {
                int nextComboIndex = (comboIndex + 1) % attackStateHashes.Length;
                StartAttack(nextComboIndex, queuedAimDir);
            }
            else
            {
                FinishCombo();
            }
        }

        private void FinishCombo()
        {
            hitBox.Close();

            IsAttacking = false;
            canQueueNextAttack = false;
            hasQueuedNextAttack = false;
            comboIndex = 0;

            nextAttackTime = Time.time + cooldownAfterCombo;

            animator.Play(IdleStateHash, 0, 0f);

            StopFailSafe();
        }

        private void RestartFailSafe()
        {
            StopFailSafe();
            failSafeRoutine = StartCoroutine(AttackFailSafe());
        }

        private void StopFailSafe()
        {
            if (failSafeRoutine == null) return;

            StopCoroutine(failSafeRoutine);
            failSafeRoutine = null;
        }

        private IEnumerator AttackFailSafe()
        {
            yield return new WaitForSeconds(attackMaxDuration);

            if (IsAttacking)
            {
                Debug.LogWarning($"{nameof(TriangleSword)} attack ended by fail safe. Check animation events.", this);
                FinishCombo();
            }
        }
    }
}