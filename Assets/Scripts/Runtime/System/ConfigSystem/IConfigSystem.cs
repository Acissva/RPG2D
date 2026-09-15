using QFramework;

namespace RPG2D
{
    public interface IConfigSystem : ISystem
    {
        PlayerConfig PlayerConfig { get; }

        EnemyConfig EnemyConfig { get; }

        QuestConfigDatabase QuestConfigDatabase { get; }

        UIPanelConfigDatabase UIPanelConfigDatabase { get; }

        ItemConfigDatabase ItemConfigDatabase { get; }

        DropTableConfigDatabase DropTableConfigDatabase { get; }

        DungeonConfigDatabase DungeonConfigDatabase { get; }
    }
}
