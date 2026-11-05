using QFramework;
using System;

namespace RPG2D
{
    public interface IHttpRequestSystem : ISystem
    {
        IHttpRequestHandle Get(string url, int timeoutSeconds, int maxBodyBytes, Action<HttpRequestResult> onCompleted);
        IHttpRequestHandle PostJson(string url, string json, int timeoutSeconds, int maxBodyBytes, Action<HttpRequestResult> onCompleted);
    }
}
