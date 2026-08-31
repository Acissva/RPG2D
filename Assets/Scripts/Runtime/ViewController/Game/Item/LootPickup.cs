using UnityEngine;

namespace RPG2D
{
    public class LootPickup : MonoBehaviour
    {
        [SerializeField] private string dropTableId;
        [SerializeField] private string sourceEnemyId;
        [SerializeField] private int experienceReward;

        public void Init(string dropTableId, string sourceEnemyId, int experienceReward)
        {
            this.dropTableId = dropTableId;
            this.sourceEnemyId = sourceEnemyId;
            this.experienceReward = Mathf.Max(0, experienceReward);
            EnsureTrigger();
        }

        private void Awake()
        {
            EnsureTrigger();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            Debug.Log($"拾取掉落：{dropTableId}，来源敌人：{sourceEnemyId}，经验：{experienceReward}");
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
                rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                rigidbody2D.gravityScale = 0f;
            }
        }
    }
}