using QFramework;

namespace RPG2D
{
    public class ExitDungeonCommand : AbstractCommand<bool>
    {
        protected override bool OnExecute()
        {
            return this.GetSystem<IDungeonSystem>().ExitCurrentDungeon();
        }
    }
}
