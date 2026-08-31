using QFramework;
using UnityEngine;

namespace RPG2D
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerController : RPG2DController
    {
        private IPlayerModel playerModel;
        private IUIModel uiModel;
        private IInputReader inputReader;

        private PlayerMovement playerMovement;

        private Vector2 moveDir;

        private void Awake()
        {
            playerModel = this.GetModel<IPlayerModel>();
            uiModel = this.GetModel<IUIModel>();
            inputReader = this.GetSystem<IRPG2DInputSystem>().InputReader;

            playerMovement = GetComponent<PlayerMovement>();

            //TODO:初始化玩家数据，暂时放在这里，有了主菜单后放到开始命令中
            this.SendCommand<InitialPlayerCommand>();

        }

        private void Update()
        {
            if (uiModel.InputBlocked.Value)
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

    }
}

