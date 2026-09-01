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

        [Tooltip("Player 根节点")]
        [SerializeField] private GameObject interactorRoot;     //交互控制器一般不挂在Player根节点，所以这里需要获取Player根节点

        public IInteractable CurrentInteractable { get; private set; }
        public bool HasCurrentInteractable => CurrentInteractable != null;

        private GameObject InteractorGameObject => interactorRoot != null ? interactorRoot : gameObject;

        private void Reset()
        {
            CacheInteractorRoot();
        }

        private void CacheInteractorRoot()
        {
            if (interactorRoot != null)
            {
                return;
            }

            PlayerController playerController = GetComponentInParent<PlayerController>();
            interactorRoot = playerController != null ? playerController.gameObject : gameObject;
        }

        private void Awake()
        {
            inputReader = this.GetSystem<IRPG2DInputSystem>().InputReader;
            uiModel = this.GetModel<IUIModel>();

            CacheInteractorRoot();
        }

        private void Update()
        {
            if (uiModel.InputBlocked.Value)
            {
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
                CurrentInteractable.Interact(InteractorGameObject);
                RefreshCurrentInteractable();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            IInteractable interactable = collision.GetComponentInParent<IInteractable>();

            if (interactable == null || nearbyInteractables.Contains(interactable))
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

                if (bestInteractable == null || GetDistanceSqr(candidate) < GetDistanceSqr(bestInteractable))
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
            return IsSceneObjectAlive(interactable) && interactable.CanInteract(InteractorGameObject);
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
