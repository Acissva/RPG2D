using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class HurtPlayerCommand : AbstractCommand
    {
        private int damage;

        public HurtPlayerCommand(int damage)
        {
            this.damage = damage;
        }

        protected override void OnExecute()
        {
            IPlayerModel playerModel = this.GetModel<IPlayerModel>();

            int currentHP = playerModel.HP.Value;
            int maxHP = playerModel.MaxHP.Value;
            int nextHP = Mathf.Clamp(currentHP - damage, 0, maxHP);

            playerModel.HP.Value = nextHP;

            Debug.Log("玩家受到伤害：" + damage + "\n" + "当前血量：" + playerModel.HP.Value);
        }
    }
}

