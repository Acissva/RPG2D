using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class ConfigSystem : AbstractSystem, IConfigSystem
    {
        public PlayerConfig PlayerConfig { get; private set; }

        public EnemyConfig EnemyConfig { get; private set; }

        public QuestConfigDatabase QuestConfigDatabase { get; private set; }

        public UIPanelConfigDatabase UIPanelConfigDatabase { get; private set; }

        public ItemConfigDatabase ItemConfigDatabase { get; private set; }

        public DropTableConfigDatabase DropTableConfigDatabase { get; private set; }

        public DungeonConfigDatabase DungeonConfigDatabase { get; private set; }

        private IAssetSystem assetSystem;

        private const string PlayerConfigPath = "Config/Game/Character/Player/PlayerConfig";
        private const string EnemyConfigPath = "Config/Game/Character/Enemy/EnemyConfig";
        private const string QuestConfigDatabasePath = "Config/Game/Quest/QuestConfigDatabase";
        private const string UIPanelConfigDatabasePath = "Config/UI/UIPanelConfigDatabase";
        private const string ItemConfigDatabasePath = "Config/Game/Item/ItemConfigDatabase";
        private const string DropTableConfigDatabasePath = "Config/Game/Drop/DropTableConfigDatabase";
        private const string DungeonConfigDatabasePath = "Config/Game/Dungeon/DungeonConfigDatabase";

        protected override void OnInit()
        {
            assetSystem = this.GetSystem<IAssetSystem>();

            PlayerConfig = assetSystem.Load<PlayerConfig>(PlayerConfigPath);
            EnemyConfig = assetSystem.Load<EnemyConfig>(EnemyConfigPath);
            QuestConfigDatabase = assetSystem.Load<QuestConfigDatabase>(QuestConfigDatabasePath);
            UIPanelConfigDatabase = assetSystem.Load<UIPanelConfigDatabase>(UIPanelConfigDatabasePath);
            ItemConfigDatabase = assetSystem.Load<ItemConfigDatabase>(ItemConfigDatabasePath);
            DropTableConfigDatabase = assetSystem.Load<DropTableConfigDatabase>(DropTableConfigDatabasePath);
            DungeonConfigDatabase = assetSystem.Load<DungeonConfigDatabase>(DungeonConfigDatabasePath);

            if (PlayerConfig == null)
            {
                Debug.LogError("Can not load PlayerConfig from " + PlayerConfigPath);
            }

            if (EnemyConfig == null)
            {
                Debug.LogError("Can not load EnemyConfig from " + EnemyConfigPath);
            }

            if (QuestConfigDatabase == null)
            {
                Debug.LogError("Can not load QuestConfigDatabase from " + QuestConfigDatabasePath);
            }

            if (UIPanelConfigDatabase == null)
            {
                Debug.LogError("Can not load UIPanelConfigDatabase from " + UIPanelConfigDatabasePath);
            }

            if (ItemConfigDatabase == null)
            {
                Debug.LogError("Can not load ItemConfigDatabase from " + ItemConfigDatabasePath);
            }

            if (DropTableConfigDatabase == null)
            {
                Debug.LogError("Can not load DropTableConfigDatabase from " + DropTableConfigDatabasePath);
            }

            if (DungeonConfigDatabase == null)
            {
                Debug.LogError("Can not load DungeonConfigDatabase from " + DungeonConfigDatabasePath);
            }
        }
    }
}
