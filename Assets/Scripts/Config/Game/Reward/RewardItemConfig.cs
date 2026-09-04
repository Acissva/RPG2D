using System;
using UnityEngine;

namespace RPG2D
{
    [Serializable]
    public class RewardItemConfig
    {
        [SerializeField] private string itemId = "item_gt_herb_001";
        [SerializeField, Min(1)] private int count = 1;

        public string ItemId => itemId;
        public int Count => count;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                itemId = "item_gt_herb_001";
            }

            if (count < 1)
            {
                count = 1;
            }
        }
    }
}
