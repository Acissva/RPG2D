using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class DungeonExitInteractable : RPG2DController, IInteractable
    {
        [SerializeField] private string interactionId = "dungeon_exit_gt_001";
        [SerializeField] private string readyPromptText = "返回主世界";
        [SerializeField] private string blockedPromptText = "出口被封锁";
        [SerializeField] private string speakerName = "旁白";
        [SerializeField, TextArea] private string blockedMessage = "清理完这里的敌人后再返回吧。";

        private readonly List<DialogueLine> dialogueLines = new();

        public string InteractionId => interactionId;

        public string PromptText => this.GetSystem<IDungeonSystem>().IsCurrentDungeonCompleted()
            ? readyPromptText
            : blockedPromptText;

        public bool CanInteract(GameObject interactor)
        {
            return isActiveAndEnabled;
        }

        public void Interact(GameObject interactor)
        {
            IDungeonSystem dungeonSystem = this.GetSystem<IDungeonSystem>();

            if (!dungeonSystem.IsCurrentDungeonCompleted())
            {
                ShowMessage(blockedMessage);
                return;
            }

            this.SendCommand(new ExitDungeonCommand());
        }

        private void ShowMessage(string text)
        {
            dialogueLines.Clear();
            dialogueLines.Add(new DialogueLine(speakerName, text));

            this.GetSystem<IDialogueSystem>().StartDialogue(dialogueLines);
        }
    }
}
