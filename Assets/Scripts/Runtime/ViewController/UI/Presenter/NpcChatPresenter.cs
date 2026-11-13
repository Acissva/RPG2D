using QFramework;
using UnityEngine;

namespace RPG2D
{
    [DisallowMultipleComponent]
    public sealed class NpcChatPresenter : RPG2DController
    {
        private INpcChatSystem chatSystem;
        private IUISystem uiSystem;
        private IUnRegister snapshotRegistration;
        private IUnRegister closedRegistration;
        private bool initialized;

        public void Initialize()
        {
            if (initialized) return;
            chatSystem = this.GetSystem<INpcChatSystem>();
            uiSystem = this.GetSystem<IUISystem>();
            initialized = true;
            closedRegistration = this.RegisterEvent<UIPanelClosedEvent>(OnPanelClosed);
            snapshotRegistration = this.GetModel<INpcChatModel>().Snapshot.RegisterWithInitValue(OnSnapshotChanged);
        }

        private void OnSnapshotChanged(NpcChatSnapshot snapshot)
        {
            if (snapshot.State == NpcChatState.Closed)
            {
                if (uiSystem.IsPanelOpen(UIPanelId.NpcChat)) uiSystem.ClosePanel(UIPanelId.NpcChat);
                return;
            }
            if (uiSystem.IsPanelOpen(UIPanelId.NpcChat)) return;
            uiSystem.OpenPanel(UIPanelId.NpcChat);
            if (!uiSystem.IsPanelOpen(UIPanelId.NpcChat))
            {
                Debug.LogWarning("AI 交谈面板未能打开，已结束本次会话，请检查 UI 配置和资源。");
                chatSystem.EndChat();
            }
        }

        private void OnPanelClosed(UIPanelClosedEvent panelClosedEvent)
        {
            if (panelClosedEvent.PanelId == UIPanelId.NpcChat) chatSystem.EndChat();
        }

        private void OnDestroy()
        {
            snapshotRegistration?.UnRegister();
            closedRegistration?.UnRegister();
            snapshotRegistration = null;
            closedRegistration = null;
            if (initialized) chatSystem?.EndChat();
            initialized = false;
        }
    }
}
