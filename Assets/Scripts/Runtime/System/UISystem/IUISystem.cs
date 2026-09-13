using QFramework;
using System;
using UnityEngine;

namespace RPG2D
{
    public interface IUISystem : ISystem
    {
        void RegisterRoot(UIRootController root);
        void UnregisterRoot(UIRootController root);
        Transform GetRoot(UILayer layer);

        bool RegisterPanel(UIPanelController panel);
        void UnregisterPanel(UIPanelController panel);

        void OpenPanel(UIPanelId panelId, bool closeOtherPanels = true);
        void ClosePanel(UIPanelId panelId);
        void TogglePanel(UIPanelId panelId, bool closeOtherPanels = true);

        void CloseAllPanels();

        void ShowConfirmPopup(string message, Action onConfirm);

        bool IsPanelOpen(UIPanelId panelId);

        UIStatusSnapshot GetStatusSnapshot();

        void CleanupInvalidUIReferences();
    }
}
