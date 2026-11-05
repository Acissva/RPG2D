using QFramework;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RPG2D
{
    public sealed class HttpRequestSystem : AbstractSystem, IHttpRequestSystem
    {
        private readonly List<PendingRequest> activeRequests = new List<PendingRequest>();
        private readonly List<PendingRequest> updateSnapshot = new List<PendingRequest>();
        private IRuntimeUpdateSystem runtimeUpdateSystem;
        private bool updateRegistered;
        private bool shuttingDown;

        protected override void OnInit()
        {
            runtimeUpdateSystem = this.GetSystem<IRuntimeUpdateSystem>();
            runtimeUpdateSystem.RegisterUpdate(OnUpdate);
            updateRegistered = true;
            Application.quitting += OnApplicationQuitting;
        }

        protected override void OnDeinit()
        {
            Application.quitting -= OnApplicationQuitting;
            Shutdown();
        }

        public IHttpRequestHandle Get(string url, int timeoutSeconds, int maxBodyBytes, Action<HttpRequestResult> onCompleted)
        {
            return SendRequest(url, UnityWebRequest.kHttpVerbGET, null, timeoutSeconds, maxBodyBytes, onCompleted);
        }

        public IHttpRequestHandle PostJson(string url, string json, int timeoutSeconds, int maxBodyBytes, Action<HttpRequestResult> onCompleted)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            byte[] uploadBody = new UTF8Encoding(false, true).GetBytes(json);
            return SendRequest(url, UnityWebRequest.kHttpVerbPOST, uploadBody, timeoutSeconds, maxBodyBytes, onCompleted);
        }

        private IHttpRequestHandle SendRequest(string url, string method, byte[] uploadBody, int timeoutSeconds, int maxBodyBytes, Action<HttpRequestResult> onCompleted)
        {
            Uri address = ValidateRequest(url, timeoutSeconds, maxBodyBytes, onCompleted);

            if (shuttingDown || !updateRegistered)
            {
                throw new InvalidOperationException("公共 HTTP 系统当前未运行。");
            }

            PendingRequest pending = new PendingRequest { Callback = onCompleted };
            pending.Handle = new HttpRequestHandle(() => CancelRequest(pending));
            activeRequests.Add(pending);

            try
            {
                pending.Request = new UnityWebRequest(address.AbsoluteUri, method);
                pending.Request.disposeDownloadHandlerOnDispose = false;
                pending.Request.disposeUploadHandlerOnDispose = false;
                pending.Handler = new LimitedDownloadHandler(maxBodyBytes);
                pending.Request.downloadHandler = pending.Handler;

                if (uploadBody != null)
                {
                    pending.UploadHandler = new UploadHandlerRaw(uploadBody);
                    pending.Request.uploadHandler = pending.UploadHandler;
                    pending.Request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
                }

                pending.Request.timeout = timeoutSeconds;
                pending.Request.redirectLimit = 0;
                pending.Request.SetRequestHeader("Accept", "application/json");
                pending.Request.SendWebRequest();
            }
            catch (Exception exception)
            {
                pending.StartFailure = new HttpRequestResult(HttpRequestFailure.StartFailed, 0, string.Empty, string.Empty, exception.ToString());
                ReleaseNative(pending);
            }

            return pending.Handle;
        }

        private static Uri ValidateRequest(string url, int timeoutSeconds, int maxBodyBytes, Action<HttpRequestResult> onCompleted)
        {
            if (onCompleted == null)
            {
                throw new ArgumentNullException(nameof(onCompleted));
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri address)
                || (address.Scheme != Uri.UriSchemeHttp && address.Scheme != Uri.UriSchemeHttps)
                || string.IsNullOrWhiteSpace(address.Host)
                || !string.IsNullOrEmpty(address.UserInfo)
                || !string.IsNullOrEmpty(address.Fragment))
            {
                throw new ArgumentException("请求地址必须是无用户凭据和片段的 HTTP / HTTPS 绝对地址。", nameof(url));
            }

            if (timeoutSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), "请求超时必须是正数秒。");
            }

            if (maxBodyBytes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBodyBytes), "响应正文上限必须大于 0。");
            }

            return address;
        }

        private void OnUpdate()
        {
            if (shuttingDown || activeRequests.Count == 0)
            {
                return;
            }

            updateSnapshot.Clear();
            updateSnapshot.AddRange(activeRequests);

            try
            {
                foreach (PendingRequest pending in updateSnapshot)
                {
                    if (shuttingDown)
                    {
                        break;
                    }

                    if (!activeRequests.Contains(pending))
                    {
                        continue;
                    }

                    HttpRequestResult result;

                    try
                    {
                        if (pending.StartFailure != null)
                        {
                            result = pending.StartFailure;
                        }
                        else
                        {
                            if (!pending.Handler.TooLarge && !pending.Request.isDone)
                            {
                                continue;
                            }

                            result = ReadResponse(pending);
                        }
                    }
                    catch (Exception exception)
                    {
                        result = new HttpRequestResult(HttpRequestFailure.DataProcessingError, 0, string.Empty, string.Empty, exception.ToString());
                    }

                    CompleteRequest(pending, result);
                }
            }
            finally
            {
                updateSnapshot.Clear();
            }
        }

        private static HttpRequestResult ReadResponse(PendingRequest pending)
        {
            UnityWebRequest request = pending.Request;
            long statusCode = request.responseCode;
            string contentType = request.GetResponseHeader("Content-Type");

            if (pending.Handler.TooLarge)
            {
                return new HttpRequestResult(HttpRequestFailure.BodyTooLarge, statusCode, contentType, string.Empty, $"响应正文超过 {pending.Handler.MaxBodyBytes} 字节。");
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                return new HttpRequestResult(HttpRequestFailure.ConnectionError, statusCode, contentType, string.Empty, request.error);
            }

            if (request.result != UnityWebRequest.Result.Success && request.result != UnityWebRequest.Result.ProtocolError)
            {
                return new HttpRequestResult(HttpRequestFailure.DataProcessingError, statusCode, contentType, string.Empty, request.error);
            }

            try
            {
                return new HttpRequestResult(HttpRequestFailure.None, statusCode, contentType, pending.Handler.ReadUtf8Text(), request.error);
            }
            catch (DecoderFallbackException exception)
            {
                return new HttpRequestResult(HttpRequestFailure.InvalidUtf8, statusCode, contentType, string.Empty, exception.Message);
            }
        }

        private void CompleteRequest(PendingRequest pending, HttpRequestResult result)
        {
            if (!activeRequests.Remove(pending))
            {
                return;
            }

            Action<HttpRequestResult> callback = pending.Callback;
            pending.Callback = null;
            ReleaseNative(pending);
            pending.Handle.Complete();

            if (shuttingDown || callback == null)
            {
                return;
            }

            try
            {
                callback(result);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void CancelRequest(PendingRequest pending)
        {
            if (!activeRequests.Remove(pending))
            {
                return;
            }

            pending.Callback = null;
            ReleaseNative(pending);
        }

        private static void ReleaseNative(PendingRequest pending)
        {
            UnityWebRequest request = pending.Request;
            LimitedDownloadHandler handler = pending.Handler;
            UploadHandlerRaw uploadHandler = pending.UploadHandler;
            pending.Request = null;
            pending.Handler = null;
            pending.UploadHandler = null;

            if (request != null)
            {
                ReleaseSafely(() =>
                {
                    if (!request.isDone)
                    {
                        request.Abort();
                    }
                });
                ReleaseSafely(request.Dispose);
            }

            if (handler != null)
            {
                ReleaseSafely(handler.Dispose);
            }

            if (uploadHandler != null)
            {
                ReleaseSafely(uploadHandler.Dispose);
            }
        }

        private static void ReleaseSafely(Action release)
        {
            try
            {
                release();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void Shutdown()
        {
            if (shuttingDown)
            {
                return;
            }

            shuttingDown = true;

            if (updateRegistered)
            {
                runtimeUpdateSystem.UnregisterUpdate(OnUpdate);
                updateRegistered = false;
            }

            while (activeRequests.Count > 0)
            {
                activeRequests[activeRequests.Count - 1].Handle.Cancel();
            }
        }

        private void OnApplicationQuitting()
        {
            Shutdown();
        }

        private sealed class PendingRequest
        {
            public UnityWebRequest Request;
            public LimitedDownloadHandler Handler;
            public UploadHandlerRaw UploadHandler;
            public HttpRequestHandle Handle;
            public Action<HttpRequestResult> Callback;
            public HttpRequestResult StartFailure;
        }
    }
}
