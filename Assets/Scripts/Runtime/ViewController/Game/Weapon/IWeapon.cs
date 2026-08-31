using UnityEngine;

namespace RPG2D
{
    public interface IWeapon
    {
        bool IsAttacking { get; }

        bool TryAttack(Vector2 aimDir);
    }
}

