using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class SimpleDialogueInteractable : RPG2DController, IInteractable
    {
        [SerializeField] private string interactionId = "npc_gt_guide_001";
        [SerializeField] private string promptText = "交谈";
        [SerializeField] private List<DialogueLine> dialogueLines = new();

        public string InteractionId => interactionId;

        public string PromptText => promptText;

        public bool CanInteract(GameObject interactor)
        {
            return isActiveAndEnabled && dialogueLines != null && dialogueLines.Count > 0;
        }

        public void Interact(GameObject interactor)
        {
            this.GetSystem<IDialogueSystem>().StartDialogue(dialogueLines);
        }
    }
}
