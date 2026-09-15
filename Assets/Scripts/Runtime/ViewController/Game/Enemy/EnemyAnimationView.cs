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

        private Animator animator;
        private EnemyMotor2D motor;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            motor = GetComponent<EnemyMotor2D>();
        }

        private void Update()
        {
            animator.SetBool(IsMovingHash, motor.HasMoveTarget);
        }
    }
}
