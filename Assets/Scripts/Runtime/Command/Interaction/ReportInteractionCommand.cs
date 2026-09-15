using QFramework;

namespace RPG2D
{
    public class ReportInteractionCommand : AbstractCommand
    {
        private readonly string interactionId;

        public ReportInteractionCommand(string interactionId)
        {
            this.interactionId = interactionId;
        }

        protected override void OnExecute()
        {
            if (string.IsNullOrWhiteSpace(interactionId))
            {
                return;
            }

            this.SendEvent(new InteractionPerformedEvent
            {
                InteractionId = interactionId
            });
        }
    }
}
