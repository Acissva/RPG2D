using QFramework;

namespace RPG2D
{
    public class ReportCombatSourceDisengagedCommand : AbstractCommand
    {
        private readonly string sourceId;

        public ReportCombatSourceDisengagedCommand(string sourceId)
        {
            this.sourceId = sourceId;
        }

        protected override void OnExecute()
        {
            this.GetSystem<ICombatStateSystem>()
                .ReportSourceDisengaged(sourceId);
        }
    }
}
