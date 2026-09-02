using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class QuestObjectiveInteractable : RPG2DController, IInteractable
    {
        [SerializeField] private string interactionId = "notice_board_gt_001";
        [SerializeField] private string promptText = "查看公告";

        [SerializeField] private string questId = "quest_gt_notice_report_001";
        [SerializeField] private string objectiveId = "objective_read_notice_board";

        [SerializeField] private string speakerName = "公告牌";
        [SerializeField, TextArea] private string message = "公告牌上写着：欢迎来到草地小镇。";

        private readonly List<DialogueLine> dialogueLines = new();

        public string InteractionId => interactionId;

        public string PromptText => promptText;

        public bool CanInteract(GameObject interactor)
        {
            return isActiveAndEnabled;
        }

        public void Interact(GameObject interactor)
        {
            ShowMessage();

            IQuestSystem questSystem = this.GetSystem<IQuestSystem>();

            if (questSystem.GetQuestStatus(questId) == QuestStatus.InProgress)
            {
                this.SendCommand(new AddQuestObjectiveProgressCommand(questId, objectiveId));
            }
        }

        private void ShowMessage()
        {
            dialogueLines.Clear();
            dialogueLines.Add(new DialogueLine(speakerName, message));

            this.GetSystem<IDialogueSystem>().StartDialogue(dialogueLines);
        }
    }
}
