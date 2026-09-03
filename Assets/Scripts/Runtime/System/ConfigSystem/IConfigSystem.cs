using QFramework;
using UnityEngine;

namespace RPG2D
{
    public interface IConfigSystem : ISystem
    {
        PlayerConfig PlayerConfig { get; }

        EnemyConfig EnemyConfig { get; }

        QuestConfigDatabase QuestConfigDatabase { get; }

        UIPanelConfigDatabase UIPanelConfigDatabase { get; }
    }

    public class ConfigSystem : AbstractSystem, IConfigSystem
    {
        public PlayerConfig PlayerConfig { get; private set; }

        public EnemyConfig EnemyConfig { get; private set; }

        public QuestConfigDatabase QuestConfigDatabase { get; private set; }

        public UIPanelConfigDatabase UIPanelConfigDatabase { get; private set; }

        private IAssetSystem assetSystem;

        protected override void OnInit()
        {
            assetSystem = this.GetSystem<IAssetSystem>();

            PlayerConfig = assetSystem.Load<PlayerConfig>("Configs/Game/Characters/PlayerConfig");
            EnemyConfig = assetSystem.Load<EnemyConfig>("Configs/Game/Characters/EnemyConfig");
            QuestConfigDatabase = assetSystem.Load<QuestConfigDatabase>("Configs/Game/Quest/QuestConfigDatabase");
            UIPanelConfigDatabase = assetSystem.Load<UIPanelConfigDatabase>("Configs/UI/UIPanelConfigDatabase");

            if (PlayerConfig == null)
            {
                Debug.LogError("Can not load PlayerConfig from Resources/Configs/Game/Characters/PlayerConfig");
            }

            if (EnemyConfig == null)
            {
                Debug.LogError("Can not load EnemyConfig from Resources/Configs/Game/Characters/EnemyConfig");
            }

            if (QuestConfigDatabase == null)
            {
                Debug.LogError("Can not load QuestConfigDatabase from Resources/Configs/Game/Quest/QuestConfigDatabase");
            }

            if (UIPanelConfigDatabase == null)
            {
                Debug.LogError("Can not load UIPanelConfigDatabase from Resources/Configs/UI/UIPanelConfigDatabase");
            }
        }
    }

}
