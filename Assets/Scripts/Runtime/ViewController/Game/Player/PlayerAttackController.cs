using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class PlayerAttackController : RPG2DController
    {
        [SerializeField] private Camera worldCamera;

        private IInputReader inputReader;
        private IUIModel uiModel;
        public IWeapon currentWeapon;

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
        }

        private void Update()
        {
            if (uiModel.InputBlocked.Value)
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

            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = transform.position.z - worldCamera.transform.position.z;

            Vector2 mouseWorldPos = worldCamera.ScreenToWorldPoint(mouseScreenPos);
            Vector2 playerPos = transform.position;

            Vector2 dir = mouseWorldPos - playerPos;

            if (dir.sqrMagnitude > 0.0001f)
            {
                AimDir = dir.normalized;
            }
        }

    }
}
