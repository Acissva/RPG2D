using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    [CreateAssetMenu(
        fileName = "DropTableConfigDatabase",
        menuName = "RPG2D/Config/Game/Drop/Drop Table Config Database")]
    public class DropTableConfigDatabase : ScriptableObject
    {
        [SerializeField] private List<DropTableConfig> dropTables = new();

        public IReadOnlyList<DropTableConfig> DropTables => dropTables;

        public DropTableConfig GetDropTableConfig(string dropTableId)
        {
            if (string.IsNullOrWhiteSpace(dropTableId))
            {
                return null;
            }

            for (int i = 0; i < dropTables.Count; i++)
            {
                DropTableConfig dropTable = dropTables[i];

                if (dropTable != null && dropTable.DropTableId == dropTableId)
                {
                    return dropTable;
                }
            }

            return null;
        }

        public bool ContainsDropTable(string dropTableId)
        {
            return GetDropTableConfig(dropTableId) != null;
        }

        private void OnValidate()
        {
            dropTables ??= new List<DropTableConfig>();
        }
    }
}
