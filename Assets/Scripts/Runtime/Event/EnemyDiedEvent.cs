using UnityEngine;

namespace RPG2D
{
    public struct EnemyDiedEvent
    {
        public string EnemyId;
        public int EnemyInstanceId;
        public Vector3 Position;
        public string DropTableId;
    }
}
