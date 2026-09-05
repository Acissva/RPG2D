using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class DropLootCommand : AbstractCommand
    {
        private const string DropPrefabRoot = "Prefab/Drop/";
        private static Sprite defaultLootSprite;

        private readonly string dropTableId;
        private readonly Vector2 position;
        private readonly string sourceEnemyId;
        private readonly int experienceReward;

        public DropLootCommand(string dropTableId, Vector2 position, string sourceEnemyId, int experienceReward)
        {
            this.dropTableId = dropTableId;
            this.position = position;
            this.sourceEnemyId = sourceEnemyId;
            this.experienceReward = experienceReward;
        }

        public DropLootCommand(string dropTableId)
            : this(dropTableId, Vector2.zero, string.Empty, 0)
        {
        }

        protected override void OnExecute()
        {
            if (string.IsNullOrWhiteSpace(dropTableId))
            {
                return;
            }

            DropTableConfig dropTable = GetDropTableConfig();

            if (dropTable == null)
            {
                Debug.LogWarning($"生成掉落失败：找不到掉落表，DropTableId={dropTableId}");
                return;
            }

            if (dropTable.Entries.Count <= 0)
            {
                Debug.LogWarning($"生成掉落失败：掉落表为空，DropTableId={dropTableId}");
                return;
            }

            for (int i = 0; i < dropTable.Entries.Count; i++)
            {
                DropEntryConfig entry = dropTable.Entries[i];

                if (entry == null)
                {
                    continue;
                }

                if (!CanDropItem(entry.ItemId))
                {
                    Debug.LogWarning($"生成掉落失败：找不到物品配置，DropTableId={dropTableId}, ItemId={entry.ItemId}");
                    continue;
                }

                int count = Random.Range(entry.MinCount, entry.MaxCount + 1);
                CreateLoot(entry.ItemId, count, i);
            }

            if (experienceReward > 0)
            {
                Debug.Log($"敌人【{sourceEnemyId}】经验奖励暂未接入：{experienceReward}");
            }
        }

        private DropTableConfig GetDropTableConfig()
        {
            IConfigSystem configSystem = this.GetSystem<IConfigSystem>();

            return configSystem.DropTableConfigDatabase == null
                ? null
                : configSystem.DropTableConfigDatabase.GetDropTableConfig(dropTableId);
        }

        private void CreateLoot(string itemId, int count, int index)
        {
            GameObject prefab = this.GetSystem<IAssetSystem>().Load<GameObject>(DropPrefabRoot + dropTableId);
            Vector2 spawnPosition = position + new Vector2(index * 0.25f, 0f);

            GameObject loot = prefab != null
                ? Object.Instantiate(prefab, spawnPosition, Quaternion.identity, RPG2D.DropLootsParent)
                : CreateFallbackLoot(spawnPosition);

            loot.name = $"Loot_{itemId}";

            LootPickup pickup = loot.GetComponent<LootPickup>();

            if (pickup == null)
            {
                pickup = loot.AddComponent<LootPickup>();
            }

            pickup.Init(itemId, count, sourceEnemyId);
        }

        private GameObject CreateFallbackLoot(Vector3 spawnPosition)
        {
            GameObject loot = new GameObject("Loot");
            loot.transform.SetParent(RPG2D.DropLootsParent, true);
            loot.transform.position = spawnPosition;

            SpriteRenderer renderer = loot.AddComponent<SpriteRenderer>();
            renderer.sprite = GetDefaultLootSprite();
            renderer.color = Color.yellow;
            renderer.sortingOrder = 50;

            CircleCollider2D collider = loot.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.2f;

            Rigidbody2D rigidbody2D = loot.AddComponent<Rigidbody2D>();
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            rigidbody2D.gravityScale = 0f;

            return loot;
        }

        private static Sprite GetDefaultLootSprite()
        {
            if (defaultLootSprite != null)
            {
                return defaultLootSprite;
            }

            Texture2D texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[64];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 210, 40, 255);
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            defaultLootSprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 16f);
            return defaultLootSprite;
        }

        private bool CanDropItem(string itemId)
        {
            IConfigSystem configSystem = this.GetSystem<IConfigSystem>();

            return configSystem.ItemConfigDatabase != null
                && configSystem.ItemConfigDatabase.ContainsItem(itemId);
        }
    }
}
