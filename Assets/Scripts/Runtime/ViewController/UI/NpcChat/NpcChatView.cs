using QFramework;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG2D
{
    [DisallowMultipleComponent]
    public sealed class NpcChatView : RPG2DController
    {
        [SerializeField] private TextMeshProUGUI npcNameText;
        [SerializeField] private TextMeshProUGUI historyText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private ScrollRect historyScroll;

        private INpcChatModel model;
        private IUnRegister snapshotRegistration;
        private NpcChatSnapshot previousSnapshot;

        private void Awake()
        {
            UIPanelController panel = GetComponentInParent<UIPanelController>(true);
            if (panel == null || panel.PanelId != UIPanelId.NpcChat || npcNameText == null || historyText == null || statusText == null || errorText == null || inputField == null || inputField.textComponent == null || sendButton == null || clearButton == null || closeButton == null || historyScroll == null || historyScroll.content == null)
            {
                Debug.LogError("NpcChatView 需要 NpcChat 面板、四项文本、有效输入框、三个按钮和有效 ScrollRect。", this);
                if (sendButton != null) sendButton.interactable = false;
                enabled = false;
                return;
            }

            ConfigurePlainText(npcNameText);
            ConfigurePlainText(historyText);
            ConfigurePlainText(statusText);
            ConfigurePlainText(errorText);
            ConfigurePlainText(inputField.textComponent);
            if (inputField.placeholder is TMP_Text placeholder) ConfigurePlainText(placeholder);
            inputField.contentType = TMP_InputField.ContentType.Standard;
            inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            inputField.characterLimit = 0;
            inputField.richText = false;
            inputField.restoreOriginalTextOnEscape = false;
            model = this.GetModel<INpcChatModel>();
        }

        private void OnEnable()
        {
            if (model == null) return;
            previousSnapshot = null;
            inputField.SetTextWithoutNotify(model.Snapshot.Value.PendingText);
            sendButton.onClick.AddListener(OnSendClicked);
            clearButton.onClick.AddListener(OnClearClicked);
            closeButton.onClick.AddListener(OnCloseClicked);
            inputField.onValueChanged.AddListener(OnDraftChanged);
            snapshotRegistration = model.Snapshot.RegisterWithInitValue(Render);
        }

        private void OnDisable()
        {
            snapshotRegistration?.UnRegister();
            snapshotRegistration = null;
            if (sendButton != null) sendButton.onClick.RemoveListener(OnSendClicked);
            if (clearButton != null) clearButton.onClick.RemoveListener(OnClearClicked);
            if (closeButton != null) closeButton.onClick.RemoveListener(OnCloseClicked);
            if (inputField != null)
            {
                inputField.onValueChanged.RemoveListener(OnDraftChanged);
                inputField.SetTextWithoutNotify(string.Empty);
            }
            previousSnapshot = null;
        }

        private void OnSendClicked()
        {
            if (!isActiveAndEnabled || !CanSend(model.Snapshot.Value)) return;
            this.SendCommand(new SendNpcChatCommand(inputField.text));
        }

        private void OnClearClicked()
        {
            if (!isActiveAndEnabled || model.Snapshot.Value.State == NpcChatState.Closed) return;
            inputField.SetTextWithoutNotify(string.Empty);
            this.SendCommand<ClearNpcChatCommand>();
        }

        private void OnCloseClicked()
        {
            if (isActiveAndEnabled) this.SendCommand<EndNpcChatCommand>();
        }

        private void OnDraftChanged(string value)
        {
            RefreshControls(model.Snapshot.Value);
        }

        private void Render(NpcChatSnapshot snapshot)
        {
            bool completed = previousSnapshot != null && previousSnapshot.State == NpcChatState.Waiting && snapshot.State == NpcChatState.Ready;
            bool emptySession = snapshot.State == NpcChatState.Ready && snapshot.History.Count == 0 && snapshot.PendingText.Length == 0;
            if (completed || emptySession || snapshot.State == NpcChatState.Closed) inputField.SetTextWithoutNotify(string.Empty);
            npcNameText.text = snapshot.DisplayName;
            string content = BuildHistory(snapshot);
            bool changed = historyText.text != content;
            historyText.text = content;
            RefreshControls(snapshot);
            previousSnapshot = snapshot;
            if (changed)
            {
                Canvas.ForceUpdateCanvases();
                historyScroll.StopMovement();
                historyScroll.verticalNormalizedPosition = 0f;
            }
        }

        private void RefreshControls(NpcChatSnapshot snapshot)
        {
            bool opened = snapshot.State != NpcChatState.Closed;
            bool waiting = snapshot.State == NpcChatState.Waiting;
            inputField.interactable = opened && !waiting;
            sendButton.interactable = CanSend(snapshot);
            clearButton.interactable = opened;
            closeButton.interactable = opened;
            statusText.text = waiting ? "等待回复…" : opened ? $"成功历史 {snapshot.History.Count}/4 轮 · 输入 {inputField.text.Length}/256" : string.Empty;
            bool invalidDraft = opened && !waiting && inputField.text.Length > 0 && !NpcChatRules.IsValidText(inputField.text, NpcChatRules.MaxMessageLength);
            errorText.text = invalidDraft ? "输入最多 256 个有效 UTF-16 代码单元，且不能全为空白。" : snapshot.Error;
        }

        private bool CanSend(NpcChatSnapshot snapshot)
        {
            return snapshot.State != NpcChatState.Closed && snapshot.State != NpcChatState.Waiting && NpcChatRules.IsValidText(inputField.text, NpcChatRules.MaxMessageLength);
        }

        private static string BuildHistory(NpcChatSnapshot snapshot)
        {
            var text = new StringBuilder(snapshot.WelcomeMessage);
            foreach (NpcChatTurn turn in snapshot.History)
            {
                text.Append("\n\n你：\n").Append(turn.PlayerText);
                text.Append("\n\n").Append(snapshot.DisplayName).Append("：\n").Append(turn.NpcText);
            }
            if (snapshot.PendingText.Length > 0)
            {
                text.Append(snapshot.State == NpcChatState.Waiting ? "\n\n你（等待中）：\n" : "\n\n你（未完成）：\n").Append(snapshot.PendingText);
            }
            return text.ToString();
        }

        private static void ConfigurePlainText(TMP_Text text)
        {
            text.richText = false;
            text.parseCtrlCharacters = false;
            text.raycastTarget = false;
        }
    }
}
