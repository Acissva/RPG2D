using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class PlayerAttackController : RPG2DController
    {
        [SerializeField] private Camera worldCamera;

        private IWeapon currentWeapon;

        private IInputReader inputReader;
        private IUIModel uiModel;
        private IPlayerModel playerModel;

        public Vector2 AimDir { get; private set; } = Vector2.right;
        public bool IsAttacking => IsWeaponAvailable(currentWeapon) && currentWeapon.IsAttacking;

        private void Awake()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            currentWeapon = GetComponentInChildren<IWeapon>();

            inputReader = this.GetSystem<IRPG2DInputSystem>().InputReader;
            uiModel = this.GetModel<IUIModel>();
            playerModel = this.GetModel<IPlayerModel>();
        }

        private void Update()
        {
            if (uiModel.InputBlocked.Value || playerModel.LifeState.Value != PlayerLifeState.Alive)
            {
                return;
            }

            UpdateAimDir();

            if (inputReader.HasAction(InputActionType.Attack))
            {
                if (!TryGetCurrentWeapon(out IWeapon weapon))
                {
                    return;
                }

                if (weapon.TryAttack(AimDir))
                {
                    inputReader.ConsumeAction(InputActionType.Attack);
                }
            }
        }

        private bool TryGetCurrentWeapon(out IWeapon weapon)
        {
            if (!IsWeaponAvailable(currentWeapon))
            {
                currentWeapon = GetComponentInChildren<IWeapon>();
            }

            weapon = currentWeapon;
            return IsWeaponAvailable(weapon);
        }

        private static bool IsWeaponAvailable(IWeapon weapon)
        {
            return weapon is Behaviour behaviour &&
                   behaviour != null &&
                   behaviour.isActiveAndEnabled;
        }

        private void UpdateAimDir()
        {
            if (worldCamera == null) return;

            Vector3 pointerScreenPosition = inputReader.PointerScreenPosition;
            pointerScreenPosition.z = transform.position.z - worldCamera.transform.position.z;

            Vector2 pointerWorldPosition = worldCamera.ScreenToWorldPoint(pointerScreenPosition);
            Vector2 direction = pointerWorldPosition - (Vector2)transform.position;

            if (direction.sqrMagnitude > 0.0001f)
            {
                AimDir = direction.normalized;
            }
        }
    }
}
