using QFramework;

namespace RPG2D
{
    public class EnterDungeonCommand : AbstractCommand<bool>
    {
        private readonly string dungeonId;

        public EnterDungeonCommand(string dungeonId)
        {
            this.dungeonId = dungeonId;
        }

        protected override bool OnExecute()
        {
            return this.GetSystem<IDungeonSystem>().EnterDungeon(dungeonId);
        }
    }
}
