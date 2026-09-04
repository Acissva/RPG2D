using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    [CreateAssetMenu(
        fileName = "DropTableConfig",
        menuName = "RPG2D/Config/Game/Drop Table Config")]
    public class DropTableConfig : ScriptableObject
    {
        [SerializeField] private string dropTableId = "drop_enemy_001";
        [SerializeField] private List<DropEntryConfig> entries = new();

        public string DropTableId => dropTableId;
        public IReadOnlyList<DropEntryConfig> Entries => entries;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(dropTableId))
            {
                dropTableId = "drop_enemy_001";
            }

            entries ??= new List<DropEntryConfig>();

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i]?.Validate();
            }
        }
    }

    [Serializable]
    public class DropEntryConfig
    {
        [SerializeField] private string itemId = "item_gt_slime_jelly_001";
        [SerializeField, Min(1)] private int minCount = 1;
        [SerializeField, Min(1)] private int maxCount = 1;
        [SerializeField, Min(1)] private int weight = 1;

        public string ItemId => itemId;
        public int MinCount => minCount;
        public int MaxCount => maxCount;
        public int Weight => weight;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                itemId = "item_gt_slime_jelly_001";
            }

            if (minCount < 1)
            {
                minCount = 1;
            }

            if (maxCount < minCount)
            {
                maxCount = minCount;
            }

            if (weight < 1)
            {
                weight = 1;
            }
        }
    }
}
