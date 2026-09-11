using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class PlayerAttackController : RPG2DController
    {
        [SerializeField] private Camera worldCamera;

        public IWeapon currentWeapon;

        private IInputReader inputReader;
        private IUIModel uiModel;
        private IPlayerModel playerModel;

        public Vector2 AimDir { get; private set; } = Vector2.right;
        public bool IsAttacking => currentWeapon != null && currentWeapon.IsAttacking;

        private void Awake()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            currentWeapon = GetComponentInChildren<IWeapon>();

            if (currentWeapon == null)
            {
                Debug.LogError("currentWeaponBehaviour must implement IWeapon.");
            }

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
                if (currentWeapon == null) return;

                if (currentWeapon.TryAttack(AimDir))
                {
                    inputReader.ConsumeAction(InputActionType.Attack);
                }
            }
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
