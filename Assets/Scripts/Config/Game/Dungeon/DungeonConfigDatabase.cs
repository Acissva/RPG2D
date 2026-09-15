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

            HashSet<string> dungeonIds = new();

            for (int i = 0; i < dungeons.Count; i++)
            {
                DungeonConfig dungeon = dungeons[i];

                if (dungeon == null)
                {
                    Debug.LogError($"{nameof(DungeonConfigDatabase)} 的 Dungeons[{i}] 为空。", this);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(dungeon.DungeonId))
                {
                    Debug.LogError($"{nameof(DungeonConfigDatabase)} 中存在 DungeonId 为空的配置，索引={i}。", dungeon);
                    continue;
                }

                if (!dungeonIds.Add(dungeon.DungeonId))
                {
                    Debug.LogError($"{nameof(DungeonConfigDatabase)} 中存在重复 DungeonId：{dungeon.DungeonId}。", dungeon);
                }
            }
        }
    }
}
