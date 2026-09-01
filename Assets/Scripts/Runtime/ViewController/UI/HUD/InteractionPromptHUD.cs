using TMPro;
using UnityEngine;

namespace RPG2D
{
    public class InteractionPromptHUD : MonoBehaviour
    {
        [SerializeField] private PlayerInteractionController playerInteraction;
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private string interactKeyName = "F";
        [SerializeField] private string fallbackPromptText = "交互";

        private void Awake()
        {
            if (promptText == null)
            {
                promptText = GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (contentRoot == null && promptText != null)
            {
                contentRoot = promptText.gameObject;
            }

            SetVisible(false);
        }

        private void Start()
        {
            EnsurePlayerInteraction();
        }

        private void Update()
        {
            EnsurePlayerInteraction();

            if (playerInteraction == null || !playerInteraction.HasCurrentInteractable)
            {
                SetVisible(false);
                return;
            }

            string actionText = playerInteraction.CurrentInteractable.PromptText;
            if (string.IsNullOrWhiteSpace(actionText))
            {
                actionText = fallbackPromptText;
            }

            if (promptText != null)
            {
                promptText.text = $"按 {interactKeyName} {actionText}";
            }

            SetVisible(true);
        }

        private void EnsurePlayerInteraction()
        {
            if (playerInteraction != null)
            {
                return;
            }

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                return;
            }

            playerInteraction = playerObj.GetComponentInChildren<PlayerInteractionController>();
        }

        private void SetVisible(bool visible)
        {
            if (contentRoot != null && contentRoot.activeSelf != visible)
            {
                contentRoot.SetActive(visible);
            }
        }
    }
}
