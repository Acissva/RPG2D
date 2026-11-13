using System;
using UnityEngine;

namespace RPG2D
{
    [CreateAssetMenu(fileName = "GameServerSettings", menuName = "RPG2D/Config/Network/Game Server Settings")]
    public class GameServerSettings : ScriptableObject
    {
        [SerializeField] private string baseUrl = "http://localhost:5080";
        [SerializeField, Range(1, 30)] private int timeoutSeconds = 5;

        public string BaseUrl => baseUrl;
        public int TimeoutSeconds => timeoutSeconds;

        public bool TryValidate(out Uri baseUri, out string error)
        {
            baseUri = null;
            error = string.Empty;

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) || string.IsNullOrWhiteSpace(uri.Host))
            {
                error = "服务器地址必须是有效的 HTTP 或 HTTPS 绝对地址";
                return false;
            }

            if (uri.AbsolutePath != "/" || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) || !string.IsNullOrEmpty(uri.UserInfo))
            {
                error = "服务根地址只填写协议、主机和端口。";
                return false;
            }

            if (timeoutSeconds < 1 || timeoutSeconds > 30)
            {
                error = "请求超时必须为 1～30 秒。";
                return false;
            }

            baseUri = uri;
            return true;
        }
    }
}
