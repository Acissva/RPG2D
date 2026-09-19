using UnityEngine;

namespace RPG2D
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(EnemyMotor2D))]
    public class EnemyAnimationView : MonoBehaviour
    {
        private static readonly int IsMovingHash =
            Animator.StringToHash("IsMoving");

        private const float HorizontalDirectionEpsilon = 0.01f;

        private Animator animator;
        private EnemyMotor2D motor;
        [SerializeField] private SpriteRenderer bodySprite;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            motor = GetComponent<EnemyMotor2D>();

            if (bodySprite == null)
            {
                bodySprite = transform.Find("Visual/BodySprite")?.GetComponent<SpriteRenderer>();
            }

            if (bodySprite == null)
            {
                Debug.LogError($"{nameof(EnemyAnimationView)} requires a BodySprite.", this);

                enabled = false;
            }
        }

        private void Update()
        {
            animator.SetBool(IsMovingHash, motor.HasMoveTarget);
            UpdateFacingDirection();
        }

        private void UpdateFacingDirection()
        {
            float horizontalDirection = motor.FacingDirection.x;

            if (Mathf.Abs(horizontalDirection) <= HorizontalDirectionEpsilon)
            {
                return;
            }

            bodySprite.flipX = horizontalDirection < 0f;
        }
    }
}
