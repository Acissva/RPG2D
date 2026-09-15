using QFramework;
using UnityEngine;

namespace RPG2D
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerDamageReceiver : RPG2DController, IDamageable
    {
        public void TakeDamage(DamageInfo damageInfo)
        {
            this.SendCommand(new HurtPlayerCommand(damageInfo.Damage));
        }
    }
}
