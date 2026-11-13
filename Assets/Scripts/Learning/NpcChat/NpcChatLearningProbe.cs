using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class NpcChatLearningProbe : RPG2DController
    {
        [SerializeField, TextArea(2, 4)] private string message = "我刚到小镇，附近有什么值得留意的地方？";
        [SerializeField] private NpcChatState state;
        [SerializeField] private int historyCount;
        [SerializeField, TextArea(2, 4)] private string welcomeMessage;
        [SerializeField, TextArea(4, 12)] private string lastReply;
        [SerializeField] private string error;
        private IUnRegister snapshotRegistration;
        private INpcChatSystem chatSystem;
        private bool attemptedThisPlay;

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            chatSystem = this.GetSystem<INpcChatSystem>();
            snapshotRegistration = this.GetModel<INpcChatModel>().Snapshot.RegisterWithInitValue(OnSnapshotChanged);
        }

        private void OnDisable()
        {
            snapshotRegistration?.UnRegister();
            snapshotRegistration = null;
            chatSystem?.EndChat();
            chatSystem = null;
        }

        [ContextMenu("1. Begin Chat (No API Call)")]
        public void BeginChat()
        {
            if (!CanOperate()) return;
            bool accepted = this.SendCommand(new BeginNpcChatCommand(NpcChatRules.GuideNpcId));
            Debug.Log("AI 学习入口：开启会话 " + (accepted ? "成功。" : "未接受，请检查当前状态和配置。"));
        }

        [ContextMenu("2. Send Once (Uses Model API)")]
        public void SendOnce()
        {
            if (!CanOperate() || attemptedThisPlay) return;
            NpcChatState current = this.GetModel<INpcChatModel>().Snapshot.Value.State;
            if (current != NpcChatState.Ready) return;
            attemptedThisPlay = true;
            bool accepted = this.SendCommand(new SendNpcChatCommand(message));
            Debug.Log("AI 学习入口：本次手动发送 " + (accepted ? "已接受。" : "未接受，请先定位错误。"));
        }

        [ContextMenu("3. Clear Chat (No API Call)")]
        public void ClearChat()
        {
            if (CanOperate()) this.SendCommand<ClearNpcChatCommand>();
        }

        [ContextMenu("4. End Chat (No API Call)")]
        public void EndChat()
        {
            if (CanOperate()) this.SendCommand<EndNpcChatCommand>();
        }

        private bool CanOperate()
        {
            return Application.isPlaying && isActiveAndEnabled && chatSystem != null;
        }

        private void OnSnapshotChanged(NpcChatSnapshot snapshot)
        {
            state = snapshot.State;
            historyCount = snapshot.History.Count;
            welcomeMessage = snapshot.WelcomeMessage;
            lastReply = historyCount == 0 ? string.Empty : snapshot.History[historyCount - 1].NpcText;
            error = snapshot.Error;
            Debug.Log($"AI 交谈状态：{state}，成功轮数：{historyCount}，提示：{error}");
        }
    }
}
