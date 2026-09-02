using QFramework;
using UnityEngine;

namespace RPG2D
{
    public interface IConfigSystem : ISystem
    {
        PlayerConfig PlayerConfig { get; }

        EnemyConfig EnemyConfig { get; }
        QuestConfigDatabase QuestConfigDatabase { get; }
    }

    public class ConfigSystem : AbstractSystem, IConfigSystem
    {
        public PlayerConfig PlayerConfig { get; private set; }

        public EnemyConfig EnemyConfig { get; private set; }

        public QuestConfigDatabase QuestConfigDatabase { get; private set; }

        protected override void OnInit()
        {
            //TODO:暂时使用Resources加载，后续再考虑 Addressables 或专门的配置加载系统
            PlayerConfig = Resources.Load<PlayerConfig>("Configs/Game/Characters/PlayerConfig");
            EnemyConfig = Resources.Load<EnemyConfig>("Configs/Game/Characters/EnemyConfig");
            QuestConfigDatabase = Resources.Load<QuestConfigDatabase>("Configs/Game/Quest/QuestConfigDatabase");

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
        }
    }

}
