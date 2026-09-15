using UnityEngine;

namespace RPG2D
{
    public sealed class EnemyViewInfo
    {
        internal EnemyViewInfo(int instanceId, EnemyRuntimeStats stats, Transform targetTransform)
        {
            InstanceId = instanceId;
            Stats = stats;
            TargetTransform = targetTransform;
        }

        public int InstanceId { get; }
        public EnemyRuntimeStats Stats { get; }
        public Transform TargetTransform { get; }
    }
}
