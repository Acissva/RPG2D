using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class InitialPlayerCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var configSystem = this.GetSystem<IConfigSystem>();
            var playerModel = this.GetModel<IPlayerModel>();

            if(configSystem.PlayerConfig == null)
            {
                Debug.LogError("Can not load playerConfig in configSystem");
                return;
            }

            playerModel.HP.Value = configSystem.PlayerConfig.InitialHP;
            playerModel.MaxHP.Value = configSystem.PlayerConfig.MaxHP;
            playerModel.MoveSpeed.Value = configSystem.PlayerConfig.MoveSpeed;
        }
    }

}
