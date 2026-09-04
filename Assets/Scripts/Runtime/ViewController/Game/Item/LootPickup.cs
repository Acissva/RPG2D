using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class LootPickup : RPG2DController
    {
        [SerializeField] private string itemId = "item_gt_slime_jelly_001";
        [SerializeField, Min(1)] private int count = 1;
        [SerializeField] private string sourceId;

        public void Init(string itemId, int count, string sourceId)
        {
            this.itemId = itemId;
            this.count = Mathf.Max(1, count);
            this.sourceId = sourceId;
            EnsureTrigger();
        }

        private void Awake()
        {
            EnsureTrigger();
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                itemId = "item_gt_slime_jelly_001";
            }

            if (count < 1)
            {
                count = 1;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            bool added = this.SendCommand(new AddItemCommand(itemId, count));

            if (!added)
            {
                Debug.LogWarning($"拾取掉落失败：ItemId={itemId}, Count={count}, SourceId={sourceId}", this);
                return;
            }

            Debug.Log($"拾取掉落：{itemId} x{count}，来源：{sourceId}");
            Destroy(gameObject);
        }

        private void EnsureTrigger()
        {
            Collider2D collider2D = GetComponent<Collider2D>();
            if (collider2D == null)
            {
                CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
                circle.radius = 0.2f;
                collider2D = circle;
            }

            collider2D.isTrigger = true;

            Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
            if (rigidbody2D == null)
            {
                rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
            }

            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            rigidbody2D.gravityScale = 0f;
        }
    }
}
