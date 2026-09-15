using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public interface IEnemyRegistrySystem : ISystem
    {
        IReadOnlyList<EnemyViewInfo> EnemyViews { get; }

        int CountAliveEnemiesInRoom(string roomId);
        void RegisterEnemy(int enemyInstanceId, string roomId, EnemyRuntimeStats stats, Transform targetTransform);
        void UnregisterEnemy(int enemyInstanceId);
    }
}
