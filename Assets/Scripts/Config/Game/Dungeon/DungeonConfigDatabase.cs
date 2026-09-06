using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    [CreateAssetMenu(
        fileName = "DungeonConfigDatabase",
        menuName = "RPG2D/Config/Game/Dungeon/Dungeon Config Database")]
    public class DungeonConfigDatabase : ScriptableObject
    {
        [SerializeField] private List<DungeonConfig> dungeons = new();

        public IReadOnlyList<DungeonConfig> Dungeons => dungeons;

        public DungeonConfig GetDungeonConfig(string dungeonId)
        {
            if (string.IsNullOrWhiteSpace(dungeonId))
            {
                return null;
            }

            for (int i = 0; i < dungeons.Count; i++)
            {
                DungeonConfig dungeon = dungeons[i];

                if (dungeon != null && dungeon.DungeonId == dungeonId)
                {
                    return dungeon;
                }
            }

            return null;
        }

        public bool ContainsDungeon(string dungeonId)
        {
            return GetDungeonConfig(dungeonId) != null;
        }

        private void OnValidate()
        {
            dungeons ??= new List<DungeonConfig>();
        }
    }
}
