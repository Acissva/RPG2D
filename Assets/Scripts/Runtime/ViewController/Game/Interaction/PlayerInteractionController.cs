using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class PlayerInteractionController : RPG2DController
    {
        private readonly List<IInteractable> nearbyInteractables = new();

        private IInputReader inputReader;
        private IUIModel uiModel;

        public IInteractable CurrentInteractable { get; private set; }
        public bool HasCurrentInteractable => CurrentInteractable != null;

        private void Awake()
        {
            inputReader = this.GetSystem<IRPG2DInputSystem>().InputReader;
            uiModel = this.GetModel<IUIModel>();
        }

        private void Update()
        {
            if (uiModel.InputBlocked.Value)
            {
                inputReader.ConsumeAction(InputActionType.Interact);
                CurrentInteractable = null;
                return;
            }

            RefreshCurrentInteractable();

            if (CurrentInteractable == null)
            {
                return;
            }

            if (inputReader.ConsumeAction(InputActionType.Interact))
            {
                CurrentInteractable.Interact(gameObject);
                RefreshCurrentInteractable();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            IInteractable interactable = collision.GetComponentInParent<IInteractable>();

            if(interactable == null || nearbyInteractables.Contains(interactable))
            {
                return;
            }

            nearbyInteractables.Add(interactable);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            IInteractable interactable = collision.GetComponentInParent<IInteractable>();

            if (interactable == null)
            {
                return;
            }

            nearbyInteractables.Remove(interactable);
        }

        private void RefreshCurrentInteractable()
        {
            RemoveInvalidInteractables();

            IInteractable bestInteractable = null;

            for (int i = 0; i < nearbyInteractables.Count; i++)
            {
                IInteractable candidate = nearbyInteractables[i];

                if (!IsInteractableAvailable(candidate))
                {
                    continue;
                }

                if(bestInteractable == null || GetDistanceSqr(candidate) < GetDistanceSqr(bestInteractable))
                {
                    bestInteractable = candidate;
                }
            }

            CurrentInteractable = bestInteractable;
        }

        private void RemoveInvalidInteractables()
        {
            for (int i = nearbyInteractables.Count - 1; i >= 0; i--)
            {
                if (!IsSceneObjectAlive(nearbyInteractables[i]))
                {
                    nearbyInteractables.RemoveAt(i);
                }
            }
        }

        private bool IsInteractableAvailable(IInteractable interactable)
        {
            return IsSceneObjectAlive(interactable) && interactable.CanInteract(gameObject);
        }

        private bool IsSceneObjectAlive(IInteractable interactable)
        {
            Component component = interactable as Component;
            return component != null && component.gameObject.activeInHierarchy;
        }

        private float GetDistanceSqr(IInteractable interactable)
        {
            Component component = interactable as Component;
            return ((Vector2)component.transform.position - (Vector2)transform.position).sqrMagnitude;
        }
    }
}
