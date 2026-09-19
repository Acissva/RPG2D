using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class DungeonEntranceInteractable : RPG2DController, IInteractable
    {
        [SerializeField] private string interactionId = "dungeon_entrance_001";
        [SerializeField] private string promptText = "进入副本";
        [SerializeField] private string dungeonId = "dungeon_001";
        [SerializeField] private string speakerName = "旁白";
        [SerializeField, TextArea] private string blockedMessage = "现在还不能进入这里。";

        private readonly List<DialogueLine> dialogueLines = new();

        public string InteractionId => interactionId;
        public string PromptText => promptText;

        public bool CanInteract(GameObject interactor)
        {
            return isActiveAndEnabled && !string.IsNullOrWhiteSpace(dungeonId);
        }

        public void Interact(GameObject interactor)
        {
            IDungeonSystem dungeonSystem = this.GetSystem<IDungeonSystem>();

            if (!dungeonSystem.CanEnterDungeon(dungeonId))
            {
                ShowMessage(blockedMessage);
                return;
            }

            this.SendCommand(new EnterDungeonCommand(dungeonId));
        }

        private void ShowMessage(string text)
        {
            dialogueLines.Clear();
            dialogueLines.Add(new DialogueLine(speakerName, text));

            this.GetSystem<IDialogueSystem>().StartDialogue(dialogueLines);
        }
    }
}
