using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class DropLootCommand : AbstractCommand
    {
        private const string DropPrefabRoot = "Prefabs/Drops/";
        private static Sprite defaultLootSprite;

        private string dropTableId;
        private Vector2 position;
        private string sourceEnemyId;
        private int experienceReward;

        public DropLootCommand(string dropTableId, Vector2 position, string sourceEnemyId, int experienceReward)
        {
            this.dropTableId = dropTableId;
            this.position = position;
            this.sourceEnemyId = sourceEnemyId;
            this.experienceReward = experienceReward;
        }

        public DropLootCommand(string dropTableId)
            : this(dropTableId, Vector3.zero, string.Empty, 0)
        {
        }

        protected override void OnExecute()
        {
            if (string.IsNullOrWhiteSpace(dropTableId))
            {
                return;
            }

            //TODO:后续修改为掉落多物体
            GameObject prefab = Resources.Load<GameObject>(DropPrefabRoot + dropTableId);
            GameObject loot = prefab != null
                ? Object.Instantiate(prefab, position, Quaternion.identity, RPG2D.DropLootsParent)
                : CreateFallbackLoot(position);

            loot.name = $"Loot_{dropTableId}";

            LootPickup pickup = loot.GetComponent<LootPickup>();

            if(pickup == null)
            {
                pickup = loot.AddComponent<LootPickup>();
            }

            pickup.Init(dropTableId, sourceEnemyId, experienceReward);
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

    }
}