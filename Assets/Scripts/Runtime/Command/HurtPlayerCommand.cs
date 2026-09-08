using QFramework;

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
            this.GetSystem<IPlayerSystem>().TakeDamage(damage);
        }
    }
}
