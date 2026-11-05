using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class HttpRequestLearningProbe : RPG2DController
    {
        private IHttpRequestHandle requestHandle;

        [ContextMenu("Get Health")]
        public void GetHealth()
        {
            if (!CanSend())
            {
                return;
            }

            requestHandle?.Cancel();
            requestHandle = this.GetSystem<IHttpRequestSystem>().Get("http://127.0.0.1:5080/api/health", 5, 16 * 1024, OnCompleted);
        }

        [ContextMenu("POST JSON Echo")]
        public void PostJsonEcho()
        {
            if (!CanSend())
            {
                return;
            }

            string json = "{\"message\":\"你好，公共 HTTP\",\"protocolVersion\":1}";
            requestHandle?.Cancel();
            requestHandle = this.GetSystem<IHttpRequestSystem>().PostJson("http://127.0.0.1:5087/echo", json, 5, 16 * 1024, OnCompleted);
        }

        private bool CanSend()
        {
            if (!Application.isPlaying || !isActiveAndEnabled)
            {
                Debug.Log("请进入 Play 模式并启用本组件，再发送请求。");
                return false;
            }

            return true;
        }

        private void OnCompleted(HttpRequestResult result)
        {
            Debug.Log($"公共 HTTP 完成：Failure={result.Failure}，HTTP={result.StatusCode}，IsDone={requestHandle?.IsDone}\n{result.Body}");
        }

        private void OnDisable()
        {
            requestHandle?.Cancel();
            requestHandle = null;
        }
    }
}
