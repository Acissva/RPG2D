using QFramework;
using UnityEngine;

namespace RPG2D
{
    public sealed class AiNpcInteractable : RPG2DController, IInteractable
    {
        private IConfigSystem configSystem;
        private INpcChatSystem chatSystem;
        private INpcChatModel chatModel;
        private IPlayerModel playerModel;
        private IUIModel uiModel;
        private IUnRegister snapshotRegistration;
        // 标记当前会话是否由这个 NPC 实例开启，从而由它负责在离场时清理。用于处理多 AI NPC 的情况
        // 示例：A 的 ownsSession = true，B 仍为 false。B 被禁用：它没有开启这次会话，因此不结束会话
        private bool ownsSession;

        public string InteractionId => NpcChatRules.GuideNpcId;
        public string PromptText => "与" + (configSystem?.NpcChatConfig != null ? configSystem.NpcChatConfig.DisplayName : "向导") + "交谈";

        private void Awake()
        {
            configSystem = this.GetSystem<IConfigSystem>();
            chatSystem = this.GetSystem<INpcChatSystem>();
            chatModel = this.GetModel<INpcChatModel>();
            playerModel = this.GetModel<IPlayerModel>();
            uiModel = this.GetModel<IUIModel>();
        }

        private void OnEnable()
        {
            if (chatModel == null) return;
            snapshotRegistration = chatModel.Snapshot.RegisterWithInitValue(OnSnapshotChanged);
        }

        private void OnDisable()
        {
            snapshotRegistration?.UnRegister();
            snapshotRegistration = null;
            bool endOwnedSession = ownsSession;
            ownsSession = false;
            if (endOwnedSession) chatSystem?.EndChat();
        }

        public bool CanInteract(GameObject interactor)
        {
            if (!isActiveAndEnabled || interactor == null || !interactor.activeInHierarchy || interactor.GetComponent<PlayerController>() == null) return false;
            if (playerModel == null || playerModel.LifeState.Value != PlayerLifeState.Alive || uiModel.InputBlocked.Value || chatModel.Snapshot.Value.State != NpcChatState.Closed) return false;
            NpcChatConfig config = configSystem.NpcChatConfig;
            return config != null && config.TryValidate(out _);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor)) return;
            ownsSession = true;
            bool accepted = this.SendCommand(new BeginNpcChatCommand(InteractionId));
            if (!accepted || chatModel.Snapshot.Value.State == NpcChatState.Closed) ownsSession = false;
        }

        private void OnSnapshotChanged(NpcChatSnapshot snapshot)
        {
            if (snapshot.State == NpcChatState.Closed) ownsSession = false;
        }
    }
}
