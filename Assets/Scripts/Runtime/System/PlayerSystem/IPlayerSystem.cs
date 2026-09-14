using QFramework;
using System;

namespace RPG2D
{
    public interface IPlayerSystem : ISystem
    {
        bool InitializeNewGame();
        bool TakeDamage(int damage);
        bool BeginRespawn(Guid travelOperationId);
        bool CancelRespawn(Guid travelOperationId);
    }
}
