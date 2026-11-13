using QFramework;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RPG2D
{
    public sealed class NpcChatPanelLearningProbe : RPG2DController
    {
        private INpcChatSystem chatSystem;
        private INpcChatModel model;

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            chatSystem = this.GetSystem<INpcChatSystem>();
            model = this.GetModel<INpcChatModel>();
        }

        private void OnDisable()
        {
            chatSystem?.EndChat();
            chatSystem = null;
            model = null;
        }

        [ContextMenu("1. Begin Panel (No API Call)")]
        public void BeginPanel()
        {
            if (!CanOperate()) return;
            this.SendCommand(new BeginNpcChatCommand(NpcChatRules.GuideNpcId));
        }

        [ContextMenu("2. Preview Long History (No API Call)")]
        public void PreviewLongHistory()
        {
            if (!CanOperate()) return;
            NpcChatSnapshot previous = model.Snapshot.Value;
            if (previous.State != NpcChatState.Ready || previous.History.Count != 0 || previous.PendingText.Length != 0) return;
            var history = new List<NpcChatTurn>();
            for (int turn = 1; turn <= NpcChatRules.MaxHistoryTurns; turn++)
            {
                var reply = new StringBuilder($"【本地排版样本 {turn}，不是模型回复】\n");
                for (int paragraph = 1; paragraph <= 6; paragraph++)
                {
                    reply.Append($"第 {paragraph} 段：这是一段用于观察中文自动换行与滚动区域的本地文字。请滚动查看完整问答，确认底部内容没有被裁切，清空后应恢复欢迎状态。\n");
                }
                reply.Append("纯文本检查：<b>这不是加粗</b>，字面量 \\n 也不会自动变成换行。");
                history.Add(new NpcChatTurn($"第 {turn} 轮本地布局问题", reply.ToString()));
            }

            // 仅学习夹具写入本地布局样本；Waiting 用于禁用发送，不存在 HTTP 请求。
            model.Snapshot.Value = new NpcChatSnapshot(NpcChatState.Waiting, previous.NpcId, previous.DisplayName, previous.WelcomeMessage, history, "仅本地布局预览，请滚动观察。", "本地布局预览，没有网络请求；点击清空退出。");
        }

        [ContextMenu("3. Clear Preview (No API Call)")]
        public void ClearPreview()
        {
            if (CanOperate()) this.SendCommand<ClearNpcChatCommand>();
        }

        [ContextMenu("4. End Panel (No API Call)")]
        public void EndPanel()
        {
            if (CanOperate()) this.SendCommand<EndNpcChatCommand>();
        }

        private bool CanOperate()
        {
            return Application.isPlaying && isActiveAndEnabled && chatSystem != null && model != null;
        }
    }
}
