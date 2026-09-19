using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class QuestGiverInteractable : RPG2DController, IInteractable
    {
        [SerializeField] private string interactionId = "npc_guide_001";
        [SerializeField] private string questId = "quest_notice_report_001";

        [SerializeField] private string notStartedPromptText = "接取任务";
        [SerializeField] private string inProgressPromptText = "交谈";
        [SerializeField] private string readyToTurnInPromptText = "交付任务";
        [SerializeField] private string completedPromptText = "交谈";

        [SerializeField] private List<DialogueLine> notStartedDialogueLines = new();
        [SerializeField] private List<DialogueLine> inProgressDialogueLines = new();
        [SerializeField] private List<DialogueLine> readyToTurnInDialogueLines = new();
        [SerializeField] private List<DialogueLine> completedDialogueLines = new();


        public string InteractionId => interactionId;

        public string PromptText
        {
            get
            {
                QuestStatus status = this.GetSystem<IQuestSystem>().GetQuestStatus(questId);

                return status switch
                {
                    QuestStatus.NotStarted => notStartedPromptText,
                    QuestStatus.InProgress => inProgressPromptText,
                    QuestStatus.ReadyToTurnIn => readyToTurnInPromptText,
                    QuestStatus.Completed => completedPromptText,
                    _ => inProgressPromptText,
                };
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return isActiveAndEnabled && !string.IsNullOrWhiteSpace(questId);
        }

        public void Interact(GameObject interactor)
        {
            IQuestSystem questSystem = this.GetSystem<IQuestSystem>();
            QuestStatus status = questSystem.GetQuestStatus(questId);

            this.GetSystem<IDialogueSystem>().StartDialogue(GetDialogueLines(status));

            if (status == QuestStatus.NotStarted)
            {
                if (!questSystem.CanAcceptQuest(questId))
                {
                    Debug.LogWarning($"Can not accept quest: {questId}", this);
                    return;
                }

                this.SendCommand(new AcceptQuestCommand(questId));
                return;
            }

            if (status == QuestStatus.ReadyToTurnIn)
            {
                this.SendCommand(new CompleteQuestCommand(questId));
            }
        }

        private IReadOnlyList<DialogueLine> GetDialogueLines(QuestStatus status)
        {
            return status switch
            {
                QuestStatus.NotStarted => notStartedDialogueLines,
                QuestStatus.InProgress => inProgressDialogueLines,
                QuestStatus.ReadyToTurnIn => readyToTurnInDialogueLines,
                QuestStatus.Completed => completedDialogueLines,
                _ => inProgressDialogueLines,
            };
        }
    }
}
