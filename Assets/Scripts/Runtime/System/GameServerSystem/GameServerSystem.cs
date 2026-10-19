using QFramework;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RPG2D
{
    public class GameServerSystem : AbstractSystem, IGameServerSystem
    {
        private IConfigSystem configSystem;
        private IRuntimeUpdateSystem runtimeUpdateSystem;
        private IGameServerModel model;
        private UnityWebRequest request;
        private GameServerDownloadHandler downloadHandler;
        private uint requestId;
        private bool updateRegistered;
        private bool shuttingDown;

        protected override void OnInit()
        {
            configSystem = this.GetSystem<IConfigSystem>();
            runtimeUpdateSystem = this.GetSystem<IRuntimeUpdateSystem>();
            model = this.GetModel<IGameServerModel>();
            Application.quitting += OnApplicationQuitting;
        }

        protected override void OnDeinit()
        {
            Application.quitting -= OnApplicationQuitting;
            shuttingDown = true;
            CancelRequest();
        }

        public bool RefreshNotice()
        {
            if (shuttingDown || request != null)
            {
                return false;
            }

            GameServerSettings settings = configSystem.GameServerSettings;

            if (settings == null)
            {
                model.Snapshot.Value = Failure("未配置服务端连接设置。", "GameServerSettings 未通过 ConfigSystem 加载。");
                return false;
            }

            if (!settings.TryGetNoticeUrl(out string noticeUrl, out string error))
            {
                model.Snapshot.Value = Failure("服务端连接设置无效。", error);
                return false;
            }

            requestId++;

            try
            {
                request = new UnityWebRequest(noticeUrl, UnityWebRequest.kHttpVerbGET);
                downloadHandler = new GameServerDownloadHandler();
                request.downloadHandler = downloadHandler;
                request.disposeDownloadHandlerOnDispose = true;
                request.timeout = settings.TimeoutSeconds;
                request.SetRequestHeader("Accept", "application/json");
                runtimeUpdateSystem.RegisterUpdate(OnUpdate);
                updateRegistered = true;
                request.SendWebRequest();
            }
            catch (Exception exception)
            {
                ReleaseRequest();
                model.Snapshot.Value = Failure("无法发起公告请求。", exception.ToString());
                return false;
            }

            model.Snapshot.Value = WithPreviousNotice(GameServerRequestState.Loading, string.Empty);
            return true;
        }

        public void CancelRequest()
        {
            requestId++;
            ReleaseRequest();

            if (!shuttingDown && model != null && model.Snapshot.Value.State == GameServerRequestState.Loading)
            {
                model.Snapshot.Value = WithPreviousNotice(GameServerRequestState.Idle, string.Empty);
            }
        }

        private void OnUpdate()
        {
            UnityWebRequest completingRequest = request;
            uint completingId = requestId;

            if (completingRequest == null || (!downloadHandler.TooLarge && !completingRequest.isDone))
            {
                return;
            }

            GameServerSnapshot result;

            try
            {
                result = ReadResponse(completingRequest, downloadHandler);
            }
            catch (Exception exception)
            {
                result = Failure("公告响应无法解析。", exception.ToString());
            }
            finally
            {
                if (ReferenceEquals(request, completingRequest))
                {
                    ReleaseRequest();
                }
            }

            if (!shuttingDown && completingId == requestId)
            {
                model.Snapshot.Value = result;
            }
        }

        private GameServerSnapshot ReadResponse(UnityWebRequest completedRequest, GameServerDownloadHandler handler)
        {
            if (handler.TooLarge)
            {
                return Failure("公告响应过大。", "响应正文超过 16 KiB。");
            }

            if (completedRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                return Failure("服务暂不可用或请求超时。", completedRequest.error);
            }

            if (completedRequest.responseCode != 200)
            {
                return Failure("服务返回错误状态。", $"HTTP {completedRequest.responseCode}: {completedRequest.error}");
            }

            if (completedRequest.result != UnityWebRequest.Result.Success)
            {
                return Failure("公告响应接收失败。", completedRequest.error);
            }

            string contentType = completedRequest.GetResponseHeader("Content-Type") ?? string.Empty;
            string mediaType = contentType.Split(';')[0].Trim();

            if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                return Failure("公告响应类型不正确。", "Content-Type: " + contentType);
            }

            string json = handler.ReadUtf8Text();

            if (string.IsNullOrWhiteSpace(json))
            {
                return Failure("公告响应为空。", "响应不包含有效 JSON 正文。");
            }

            GameServerNoticeResponse response = JsonUtility.FromJson<GameServerNoticeResponse>(json);

            if (response == null || response.protocolVersion != 1)
            {
                return Failure("公告协议版本不兼容。", "要求 protocolVersion = 1。");
            }

            if (string.IsNullOrWhiteSpace(response.title) || response.title.Length > 64 || string.IsNullOrWhiteSpace(response.content) || response.content.Length > 512)
            {
                return Failure("公告内容不符合协议。", "title 必须为 1～64、content 必须为 1～512 个 UTF-16 代码单元，且不能只含空白。");
            }

            return new GameServerSnapshot(GameServerRequestState.Ready, response.title, response.content, string.Empty);
        }

        private GameServerSnapshot WithPreviousNotice(GameServerRequestState state, string error)
        {
            GameServerSnapshot previous = model.Snapshot.Value;
            return new GameServerSnapshot(state, previous.Title, previous.Content, error);
        }

        private GameServerSnapshot Failure(string message, string diagnostic)
        {
            Debug.LogWarning("服务端公告：" + message + " " + diagnostic);
            return WithPreviousNotice(GameServerRequestState.Failed, message);
        }

        private void ReleaseRequest()
        {
            if (updateRegistered)
            {
                runtimeUpdateSystem.UnregisterUpdate(OnUpdate);
                updateRegistered = false;
            }

            UnityWebRequest releasingRequest = request;
            request = null;
            downloadHandler = null;

            if (releasingRequest == null)
            {
                return;
            }

            try
            {
                if (!releasingRequest.isDone)
                {
                    releasingRequest.Abort();
                }
            }
            finally
            {
                releasingRequest.Dispose();
            }
        }

        private void OnApplicationQuitting()
        {
            shuttingDown = true;
            CancelRequest();
        }
    }
}
