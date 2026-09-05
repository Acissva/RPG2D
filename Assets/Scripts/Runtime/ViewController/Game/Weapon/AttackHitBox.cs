using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class AttackHitBox : MonoBehaviour
    {
        //攻击范围
        [SerializeField] private Collider2D hitCollider;
        //攻击对象层级
        [SerializeField] private LayerMask targetLayers;

        //本次攻击已经命中过的目标
        private readonly HashSet<IDamageable> hitTargets = new();
        private readonly List<Collider2D> overlapResults = new();

        private DamageInfo damageInfo;
        private bool isOpen;
        private bool isValid;

        private void Awake()
        {
            if (hitCollider == null)
            {
                hitCollider = GetComponent<Collider2D>();
            }

            if (hitCollider == null)
            {
                Debug.LogError($"{nameof(AttackHitBox)} requires a Collider2D.", this);
                enabled = false;
                return;
            }

            if (targetLayers.value == 0)
            {
                Debug.LogWarning($"{nameof(AttackHitBox)} targetLayers is empty. This hitbox will not hit anything.", this);
            }

            hitCollider.isTrigger = true;
            hitCollider.enabled = false;
            isValid = true;
        }

        //命中开始帧调用
        public void Open(DamageInfo info)
        {
            if (!isValid) return;

            damageInfo = info;
            hitTargets.Clear();

            isOpen = true;
            hitCollider.enabled = true;

            //立刻检测当前已经重叠的目标
            HitCurrentOverlap();
        }

        public void Close()
        {
            isOpen = false;

            if (hitCollider != null)
            {
                hitCollider.enabled = false;
            }

            hitTargets.Clear();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TryHit(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            TryHit(collision);
        }

        //该方法是为了处理一种常见情况：
        //如果攻击判定开启的瞬间，敌人已经站在 Hitbox 范围内，Unity 不一定会触发 OnTriggerEnter2D，所以需要主动扫一次当前重叠对象。
        private void HitCurrentOverlap()
        {
            if (!isValid) return;

            overlapResults.Clear();

            var filter = new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true,
                layerMask = targetLayers
            };

            int count = hitCollider.OverlapCollider(filter, overlapResults);

            for (int i = 0; i < count; i++)
            {
                TryHit(overlapResults[i]);
            }
        }

        private void TryHit(Collider2D other)
        {
            if (!isOpen || other == null) return;

            if (((1 << other.gameObject.layer) & targetLayers.value) == 0)
            {
                return;
            }

            //敌人的 Collider 可能挂在子节点上，但血量脚本可能挂在 Enemy 根节点上
            //GetComponentInParent 会先查询自己，然后向上查询父物体，直到根节点
            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable == null) return;
            if (hitTargets.Contains(damageable)) return;
            if (damageInfo.Attacker != null && other.transform.IsChildOf(damageInfo.Attacker.transform))
            {
                return;
            }

            hitTargets.Add(damageable);

            damageInfo.HitPoint = other.ClosestPoint(transform.position);
            damageable.TakeDamage(damageInfo);
        }
    }
}
