using QFramework;

namespace RPG2D
{
    public class ReportCombatSourceEngagedCommand : AbstractCommand
    {
        private readonly string sourceId;
        private readonly string sourceType;
        private readonly string contextId;

        public ReportCombatSourceEngagedCommand(
            string sourceId,
            string sourceType = "",
            string contextId = "")
        {
            this.sourceId = sourceId;
            this.sourceType = sourceType;
            this.contextId = contextId;
        }

        protected override void OnExecute()
        {
            this.GetSystem<ICombatStateSystem>()
                .ReportSourceEngaged(sourceId, sourceType, contextId);
        }
    }
}
