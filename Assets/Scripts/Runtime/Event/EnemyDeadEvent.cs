using UnityEngine;

namespace RPG2D
{
    public struct EnemyDeadEvent
    {
        public string EnemyId;
        public int EnemyInstanceId;
        public Vector3 Position;
        public string DropTableId;
        public int ExperienceReward;
    }
}

