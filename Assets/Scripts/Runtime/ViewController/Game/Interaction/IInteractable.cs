using UnityEngine;

namespace RPG2D
{
    public interface IInteractable
    {
        string InteractionId { get; }
        string PromptText { get; }

        bool CanInteract(GameObject interactor);
        void Interact(GameObject interactor);
    }
}

