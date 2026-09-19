using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class MessageInteractable : RPG2DController, IInteractable
    {
        [SerializeField] private string interactionId = "message_001";
        [SerializeField] private string promptText = "查看";
        [SerializeField] private string speakerName = "旁白";
        [SerializeField, TextArea] private string message = "这里还没有配置内容";

        private readonly List<DialogueLine> dialogueLines = new();

        public string InteractionId => interactionId;

        public string PromptText => promptText;

        public bool CanInteract(GameObject interactor)
        {
            return isActiveAndEnabled;
        }

        public void Interact(GameObject interactor)
        {
            ShowMessage(message);
        }

        private void ShowMessage(string text)
        {
            dialogueLines.Clear();
            dialogueLines.Add(new DialogueLine(speakerName, text));

            this.GetSystem<IDialogueSystem>().StartDialogue(dialogueLines);
        }
    }
}
