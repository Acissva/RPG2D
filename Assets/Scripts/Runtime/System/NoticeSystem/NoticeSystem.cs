using QFramework;
using System;
using UnityEngine;

namespace RPG2D
{
    public class NoticeSystem : AbstractSystem, INoticeSystem
    {
        private IConfigSystem configSystem;
        private IHttpRequestSystem httpRequestSystem;
        private INoticeModel model;
        private IHttpRequestHandle requestHandle;
        private uint requestId;
        private bool shuttingDown;

        protected override void OnInit()
        {
            configSystem = this.GetSystem<IConfigSystem>();
            httpRequestSystem = this.GetSystem<IHttpRequestSystem>();
            model = this.GetModel<INoticeModel>();
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
            if (shuttingDown || requestHandle != null)
            {
                return false;
            }

            uint startingId = ++requestId;
            GameServerSettings settings = configSystem.GameServerSettings;

            if (settings == null)
            {
                Publish(startingId, Failure("未配置服务端连接设置。", "GameServerSettings 未通过 ConfigSystem 加载。"));
                return false;
            }

            if (!settings.TryValidate(out Uri serverRoot, out string error))
            {
                Publish(startingId, Failure("服务端连接设置无效。", error));
                return false;
            }

            try
            {
                string noticeUrl = new Uri(serverRoot, "api/notice").AbsoluteUri;
                requestHandle = httpRequestSystem.Get(noticeUrl, settings.TimeoutSeconds, 16 * 1024, result => OnCompleted(startingId, result));
            }
            catch (Exception exception)
            {
                Publish(startingId, Failure("无法发起公告请求。", exception.ToString()));
                return false;
            }

            Publish(startingId, WithPreviousNotice(NoticeRequestState.Loading, string.Empty));
            return true;
        }

        public void CancelRequest()
        {
            uint cancellingId = ++requestId;
            IHttpRequestHandle cancellingHandle = requestHandle;
            requestHandle = null;
            cancellingHandle?.Dispose();

            if (!shuttingDown && model != null && model.Snapshot.Value.State == NoticeRequestState.Loading)
            {
                Publish(cancellingId, WithPreviousNotice(NoticeRequestState.Idle, string.Empty));
            }
        }

        private void OnCompleted(uint completingId, HttpRequestResult transportResult)
        {
            if (shuttingDown || completingId != requestId)
            {
                return;
            }

            requestHandle = null;
            NoticeSnapshot result;

            try
            {
                result = ReadResponse(transportResult);
            }
            catch (Exception exception)
            {
                result = Failure("公告响应无法解析。", exception.ToString());
            }

            Publish(completingId, result);
        }

        private NoticeSnapshot ReadResponse(HttpRequestResult response)
        {
            if (response.Failure == HttpRequestFailure.BodyTooLarge)
            {
                return Failure("公告响应过大。", "响应正文超过 16 KiB。");
            }

            if (response.Failure == HttpRequestFailure.ConnectionError)
            {
                return Failure("服务暂不可用或请求超时。", response.Error);
            }

            if (response.Failure == HttpRequestFailure.StartFailed)
            {
                return Failure("无法发起公告请求。", response.Error);
            }

            if (response.StatusCode != 200)
            {
                return Failure("服务返回错误状态。", $"HTTP {response.StatusCode}: {response.Error}");
            }

            if (response.Failure != HttpRequestFailure.None && response.Failure != HttpRequestFailure.InvalidUtf8)
            {
                return Failure("公告响应接收失败。", response.Error);
            }

            string mediaType = response.ContentType.Split(';')[0].Trim();

            if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                return Failure("公告响应类型不正确。", "Content-Type: " + response.ContentType);
            }

            if (response.Failure == HttpRequestFailure.InvalidUtf8)
            {
                return Failure("公告响应无法解析。", response.Error);
            }

            string json = response.Body;

            if (string.IsNullOrWhiteSpace(json))
            {
                return Failure("公告响应为空。", "响应不包含有效 JSON 正文。");
            }

            NoticeResponse notice = JsonUtility.FromJson<NoticeResponse>(json);

            if (notice == null || notice.protocolVersion != 1)
            {
                return Failure("公告协议版本不兼容。", "要求 protocolVersion = 1。");
            }

            if (string.IsNullOrWhiteSpace(notice.title) || notice.title.Length > 64 || string.IsNullOrWhiteSpace(notice.content) || notice.content.Length > 512)
            {
                return Failure("公告内容不符合协议。", "title 必须为 1～64、content 必须为 1～512 个 UTF-16 代码单元，且不能只含空白。");
            }

            return new NoticeSnapshot(NoticeRequestState.Ready, notice.title, notice.content, string.Empty);
        }

        private void Publish(uint completingId, NoticeSnapshot result)
        {
            if (!shuttingDown && completingId == requestId)
            {
                model.Snapshot.Value = result;
            }
        }

        private NoticeSnapshot WithPreviousNotice(NoticeRequestState state, string error)
        {
            NoticeSnapshot previous = model.Snapshot.Value;
            return new NoticeSnapshot(state, previous.Title, previous.Content, error);
        }

        private NoticeSnapshot Failure(string message, string diagnostic)
        {
            Debug.LogWarning("服务端公告：" + message + " " + diagnostic);
            return WithPreviousNotice(NoticeRequestState.Failed, message);
        }

        private void OnApplicationQuitting()
        {
            shuttingDown = true;
            CancelRequest();
        }
    }
}
