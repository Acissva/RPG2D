using QFramework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG2D
{
    public class ConfirmPopupController : UIPanelController
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private Action onConfirm;

        private void Awake()
        {
            if(confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }

            if(cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
            }
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(OnConfirmClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(OnCancelClicked);
            }
        }

        public void SetContent(string message, Action onConfirm)
        {
            this.onConfirm = onConfirm;

            if(messageText != null)
            {
                messageText.text = message;
            }
        }

        private void OnConfirmClicked()
        {
            var confirmAction = onConfirm;
            onConfirm = null;

            this.SendCommand(new CloseUIPanelCommand(PanelId));
            confirmAction?.Invoke();
        }

        private void OnCancelClicked()
        {
            onConfirm = null;
            this.SendCommand(new CloseUIPanelCommand(PanelId));
        }

    }
}

