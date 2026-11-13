using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public sealed class NpcChatSystem : AbstractSystem, INpcChatSystem
    {
        private IConfigSystem configSystem;
        private IHttpRequestSystem httpRequestSystem;
        private INpcChatModel model;
        private IHttpRequestHandle requestHandle;
        private IUnRegister sceneRegistration;
        private IUnRegister deathRegistration;
        // 本地状态的代次。每次发送、清空或结束都会改变，旧回调不能写回新状态
        private ulong generation;
        // 当前在途请求的关联编号
        private string activeRequestId = string.Empty;
        private bool shuttingDown;

        protected override void OnInit()
        {
            configSystem = this.GetSystem<IConfigSystem>();
            httpRequestSystem = this.GetSystem<IHttpRequestSystem>();
            model = this.GetModel<INpcChatModel>();
            sceneRegistration = this.RegisterEvent<SceneLoadStartedEvent>(_ => EndChat());
            deathRegistration = this.RegisterEvent<PlayerDiedEvent>(_ => EndChat());
            Application.quitting += Shutdown;
        }

        protected override void OnDeinit()
        {
            Application.quitting -= Shutdown;
            sceneRegistration?.UnRegister();
            deathRegistration?.UnRegister();
            sceneRegistration = null;
            deathRegistration = null;
            Shutdown();
        }

        public bool BeginChat(string npcId)
        {
            if (shuttingDown || model.Snapshot.Value.State != NpcChatState.Closed) return false;
            NpcChatConfig config = configSystem.NpcChatConfig;
            if (config == null || !config.TryValidate(out _) || !string.Equals(config.NpcId, npcId, StringComparison.Ordinal)) return false;
            InvalidateRequest();
            model.Snapshot.Value = new NpcChatSnapshot(NpcChatState.Ready, config.NpcId, config.DisplayName, config.WelcomeMessage, Array.Empty<NpcChatTurn>(), string.Empty, string.Empty);
            return true;
        }

        public bool SendMessage(string message)
        {
            if (shuttingDown) return false;
            NpcChatSnapshot previous = model.Snapshot.Value;
            if (previous.State == NpcChatState.Closed || previous.State == NpcChatState.Waiting) return false;
            if (!NpcChatProtocol.TryCreateRequest(previous.NpcId, message, previous.History, out string requestId, out string json, out string error))
            {
                model.Snapshot.Value = WithState(previous, NpcChatState.Failed, previous.History, previous.PendingText, error);
                return false;
            }

            GameServerSettings server = configSystem.GameServerSettings;
            NpcChatSettings settings = configSystem.NpcChatSettings;
            if (server == null || settings == null || !server.TryValidate(out Uri serverRoot, out _) || !settings.TryValidate(out _))
            {
                model.Snapshot.Value = WithState(previous, NpcChatState.Failed, previous.History, message, "AI 交谈连接设置无效。");
                return false;
            }
            ulong startingGeneration = ++generation;
            activeRequestId = requestId;
            model.Snapshot.Value = WithState(previous, NpcChatState.Waiting, previous.History, message, string.Empty);
            if (!IsCurrent(startingGeneration, requestId, previous.NpcId)) return false;
            try
            {
                string url = new Uri(serverRoot, "api/npc-chat").AbsoluteUri;
                IHttpRequestHandle startedHandle = httpRequestSystem.PostJson(url, json, settings.TimeoutSeconds, NpcChatRules.MaxResponseBytes,
                    result => OnCompleted(startingGeneration, requestId, previous, message, result));
                if (IsCurrent(startingGeneration, requestId, previous.NpcId)) requestHandle = startedHandle;
                else startedHandle?.Dispose();
                return true;
            }
            catch (Exception)
            {
                if (IsCurrent(startingGeneration, requestId, previous.NpcId))
                {
                    activeRequestId = string.Empty;
                    model.Snapshot.Value = WithState(previous, NpcChatState.Failed, previous.History, message, "无法发起 AI 交谈请求。");
                }
                return false;
            }
        }

        public void ClearChat()
        {
            if (shuttingDown || model.Snapshot.Value.State == NpcChatState.Closed) return;
            NpcChatSnapshot previous = model.Snapshot.Value;
            InvalidateRequest();
            model.Snapshot.Value = WithState(previous, NpcChatState.Ready, Array.Empty<NpcChatTurn>(), string.Empty, string.Empty);
        }

        public void EndChat()
        {
            InvalidateRequest();
            if (!shuttingDown && model != null) model.Snapshot.Value = NpcChatSnapshot.Empty;
        }

        // 判断这条请求是否仍是当前正在等待的请求，是否还有资格把结果写入会话
        private bool IsCurrent(ulong completingGeneration, string requestId, string npcId)
        {
            NpcChatSnapshot snapshot = model.Snapshot.Value;
            return !shuttingDown
                && completingGeneration == generation
                && string.Equals(requestId, activeRequestId, StringComparison.Ordinal)
                && snapshot.State == NpcChatState.Waiting
                && string.Equals(snapshot.NpcId, npcId, StringComparison.Ordinal);
        }

        private void OnCompleted(ulong completingGeneration, string requestId, NpcChatSnapshot previous, string message, HttpRequestResult result)
        {
            if (!IsCurrent(completingGeneration, requestId, previous.NpcId)) return;
            requestHandle = null;
            activeRequestId = string.Empty;
            string error = ReadTransportError(result);
            NpcChatResponse response = null;
            if (error.Length == 0 && NpcChatProtocol.TryReadResponse(result.Body, requestId, previous.NpcId, out response, out error))
            {
                var history = new List<NpcChatTurn>(previous.History) { new NpcChatTurn(message, response.reply) };
                while (history.Count > NpcChatRules.MaxHistoryTurns) history.RemoveAt(0);
                model.Snapshot.Value = WithState(previous, NpcChatState.Ready, history, string.Empty, string.Empty);
                return;
            }
            model.Snapshot.Value = WithState(previous, NpcChatState.Failed, previous.History, message, error);
        }

        private static string ReadTransportError(HttpRequestResult result)
        {
            if (result == null) return "AI 交谈响应接收失败。";
            if (result.Failure == HttpRequestFailure.BodyTooLarge) return "AI 回复超过 16 KiB。";
            if (result.Failure == HttpRequestFailure.StartFailed) return "无法发起 AI 交谈请求。";
            if (result.Failure == HttpRequestFailure.ConnectionError) return "AI 服务暂不可用或请求超时。";
            if (result.StatusCode != 200)
            {
                switch (result.StatusCode)
                {
                    case 400: return "交谈内容不符合服务要求，请修改后手动发送。";
                    case 404: return "该 NPC 暂不支持 AI 交谈。";
                    case 413: return "交谈请求过大，请缩短内容。";
                    case 415: return "交谈请求格式不受支持。";
                    case 429: return "AI 交谈繁忙，请稍后手动再发。";
                    case 502: return "AI 服务返回了无效回复。";
                    case 503: return "AI 服务暂不可用，请稍后手动再发。";
                    case 504: return "AI 服务等待超时，请稍后手动再发。";
                    default: return "AI 交谈服务返回错误状态。";
                }
            }
            if (result.Failure == HttpRequestFailure.InvalidUtf8) return "AI 回复编码无效。";
            if (result.Failure != HttpRequestFailure.None) return "AI 交谈响应接收失败。";
            string mediaType = result.ContentType.Split(';')[0].Trim();
            if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase)) return "AI 回复类型不正确。";
            return string.Empty;
        }

        private static NpcChatSnapshot WithState(NpcChatSnapshot previous, NpcChatState state, IEnumerable<NpcChatTurn> history, string pendingText, string error)
        {
            return new NpcChatSnapshot(state, previous.NpcId, previous.DisplayName, previous.WelcomeMessage, history, pendingText, error);
        }

        private void InvalidateRequest()
        {
            generation++;
            activeRequestId = string.Empty;
            IHttpRequestHandle cancellingHandle = requestHandle;
            requestHandle = null;
            cancellingHandle?.Dispose();
        }

        private void Shutdown()
        {
            if (shuttingDown) return;
            shuttingDown = true;
            InvalidateRequest();
        }
    }
}
