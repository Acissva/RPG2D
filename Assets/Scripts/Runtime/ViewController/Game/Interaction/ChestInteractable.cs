using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class ChestInteractable : RPG2DController, IInteractable
    {
        [SerializeField] private string interactionId = "chest_gt_001";

        [SerializeField] private string closedPromptText = "打开宝箱";
        [SerializeField] private string openedPromptText = "查看宝箱";

        [SerializeField] private string speakerName = "旁白";

        [SerializeField] private string openedMessage = "宝箱已打开";
        [SerializeField] private string emptyMessage = "宝箱已经空了";

        [SerializeField] private List<RewardItemConfig> rewardItems = new();

        [SerializeField] private bool startOpened;
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Sprite openedSprite;
        [SerializeField] private Color openedColor = new Color(1f, 0.82f, 0.35f, 1f);

        private readonly List<DialogueLine> dialogueLines = new();

        private Sprite closedSprite;
        private Color closedColor;
        private bool isOpened;

        public string InteractionId => interactionId;

        public string PromptText => isOpened ? openedPromptText : closedPromptText;

        private void Reset()
        {
            CacheRenderer();
        }

        private void Awake()
        {
            CacheRenderer();

            if (targetRenderer != null)
            {
                closedSprite = targetRenderer.sprite;
                closedColor = targetRenderer.color;
            }

            isOpened = startOpened;
            RefreshVisual();
        }

        public bool CanInteract(GameObject interactor)
        {
            return isActiveAndEnabled;
        }

        public void Interact(GameObject interactor)
        {
            if (isOpened)
            {
                ShowMessage(emptyMessage);
                return;
            }

            if (rewardItems.Count > 0)
            {
                bool rewardsGranted = this.SendCommand(new GrantRewardCommand(
                    rewardItems,
                    RewardSourceType.Chest,
                    interactionId));

                if (!rewardsGranted)
                {
                    Debug.LogWarning($"宝箱打开失败：奖励未完整发放，InteractionId={interactionId}");
                    ShowMessage("宝箱暂时无法打开。");
                    return;
                }
            }

            isOpened = true;
            RefreshVisual();
            ShowMessage(openedMessage);
        }

        private void CacheRenderer()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void RefreshVisual()
        {
            if (targetRenderer == null)
            {
                return;
            }

            if (isOpened)
            {
                if (openedSprite != null)
                {
                    targetRenderer.sprite = openedSprite;
                }

                targetRenderer.color = openedColor;
                return;
            }

            if (closedSprite != null)
            {
                targetRenderer.sprite = closedSprite;
            }

            targetRenderer.color = closedColor;
        }

        private void ShowMessage(string text)
        {
            dialogueLines.Clear();
            dialogueLines.Add(new DialogueLine(speakerName, text));

            this.GetSystem<IDialogueSystem>().StartDialogue(dialogueLines);
        }

        private void OnValidate()
        {
            rewardItems ??= new List<RewardItemConfig>();

            for (int i = 0; i < rewardItems.Count; i++)
            {
                rewardItems[i]?.Validate();
            }
        }
    }
}
