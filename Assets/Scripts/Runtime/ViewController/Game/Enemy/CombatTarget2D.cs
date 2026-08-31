using System;
using UnityEngine;

namespace RPG2D
{
    //阵营类别
    public enum FactionType     //普通枚举，每个值代表一个阵营，一个单位只能属于一个阵营
    {
        Neutral,    //中立
        Player,
        Enemy
    }

    [Flags]          // 位标记特性：允许枚举值通过「按位或 |」组合，同时表示多个阵营
    public enum FactionMask
    {
        None = 0,
        Neutral = 1 << 0,
        Player = 1 << 1,
        Enemy = 1 << 2,
        All = Neutral | Player | Enemy
    }

    public class CombatTarget2D : MonoBehaviour
    {
        [SerializeField] private FactionType faction = FactionType.Player;
        /// <summary>
        /// 是否可以被锁定
        /// </summary>
        [SerializeField] private bool isTargetable = true;
        /// <summary>
        /// 威胁优先级
        /// </summary>
        [SerializeField] private int threatPriority;
        /// <summary>
        /// 瞄准点，攻击射线从这里计算
        /// </summary>
        [SerializeField] private Transform aimPoint;

        public FactionType Faction => faction;
        public int ThreatPriority => threatPriority;
        public Transform AimPoint => aimPoint;
        public Vector2 TargetPosition => aimPoint != null ? aimPoint.position : transform.position;
        public bool IsTargetable => isTargetable && isActiveAndEnabled && gameObject.activeInHierarchy;

        public void SetTargetable(bool value)
        {
            isTargetable = value;
        }

        public void SetThreatPriority(int value)
        {
            threatPriority = value;
        }

        public void SetAimPoint(Transform value)
        {
            aimPoint = value;
        }

    }
}