using QFramework;
using UnityEngine;

namespace RPG2D
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(CombatTarget2D))]
    public class PlayerController : RPG2DController
    {
        private IPlayerModel playerModel;
        private IUIModel uiModel;
        private IInputReader inputReader;

        private PlayerMovement playerMovement;
        private CombatTarget2D combatTarget;

        private Vector2 moveDir;

        private void Awake()
        {
            playerModel = this.GetModel<IPlayerModel>();
            uiModel = this.GetModel<IUIModel>();
            inputReader = this.GetSystem<IRPG2DInputSystem>().InputReader;

            playerMovement = GetComponent<PlayerMovement>();
            combatTarget = GetComponent<CombatTarget2D>();

            playerModel.LifeState.RegisterWithInitValue(OnLifeStateChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void Update()
        {
            if (uiModel.InputBlocked.Value || playerModel.LifeState.Value != PlayerLifeState.Alive)
            {
                StopMove();
                return;
            }

            Move();
        }

        private void Move()
        {
            moveDir = inputReader.MoveDir;

            playerMovement.SetMoveInput(moveDir, playerModel.MoveSpeed.Value);

        }

        private void StopMove()
        {
            moveDir = Vector2.zero;
            playerMovement.SetMoveInput(Vector2.zero, 0f);
        }

        private void OnLifeStateChanged(PlayerLifeState lifeState)
        {
            bool isAlive = lifeState == PlayerLifeState.Alive;

            combatTarget.SetTargetable(isAlive);

            if (!isAlive)
            {
                StopMove();
            }
        }
    }
}
