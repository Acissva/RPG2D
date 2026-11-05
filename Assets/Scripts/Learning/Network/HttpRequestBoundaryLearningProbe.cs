using QFramework;
using System.Collections;
using UnityEngine;

namespace RPG2D
{
    public class HttpRequestBoundaryLearningProbe : RPG2DController
    {
        private const string Root = "http://127.0.0.1:5087";
        private IHttpRequestHandle requestHandle;
        private Coroutine routine;
        private int passedCount;
        private int totalCount;

        [ContextMenu("Run HTTP Boundary Matrix")]
        public void RunMatrix()
        {
            if (!Application.isPlaying || !isActiveAndEnabled)
            {
                Debug.Log("请进入 Play 模式并启用本组件，再运行边界观察。");
                return;
            }

            if (routine != null)
            {
                Debug.Log("边界观察正在进行，请等待汇总。");
                return;
            }

            routine = StartCoroutine(RunCases());
        }

        private IEnumerator RunCases()
        {
            passedCount = 0;
            totalCount = 0;
            yield return Check("空正文", "/boundary/empty", 3, 8, HttpRequestFailure.None, 200, string.Empty);
            yield return Check("恰好8字节", "/boundary/exact", 3, 8, HttpRequestFailure.None, 200, "12345678");
            yield return Check("声明长度超限", "/boundary/declared-over", 3, 8, HttpRequestFailure.BodyTooLarge, 200, string.Empty);
            yield return Check("无长度累计超限", "/boundary/chunked-over", 3, 8, HttpRequestFailure.BodyTooLarge, 200, string.Empty);
            yield return Check("UTF8分段", "/boundary/utf8-split", 3, 6, HttpRequestFailure.None, 200, "你好");
            yield return Check("无效UTF8", "/boundary/invalid-utf8", 3, 8, HttpRequestFailure.InvalidUtf8, 200, string.Empty);
            yield return Check("HTTP503完整正文", "/boundary/http-503", 3, 256, HttpRequestFailure.None, 503, "{\"error\":\"sample_unavailable\"}");
            yield return Check("不解析JSON", "/boundary/invalid-json", 3, 64, HttpRequestFailure.None, 200, "{broken");
            yield return Check("请求超时", "/delay/3000/health", 1, 256, HttpRequestFailure.ConnectionError, -1, string.Empty);
            Debug.Log($"HTTP 边界汇总：{(passedCount == totalCount ? "PASS" : "FAIL")}，通过={passedCount}/{totalCount}");
            routine = null;
        }

        private IEnumerator Check(string name, string path, int timeoutSeconds, int maxBodyBytes, HttpRequestFailure expectedFailure, long expectedStatus, string expectedBody)
        {
            totalCount++;
            int callbackCount = 0;
            bool returned = false;
            bool resultMatches = false;
            requestHandle = this.GetSystem<IHttpRequestSystem>().Get(Root + path, timeoutSeconds, maxBodyBytes, result =>
            {
                callbackCount++;
                bool statusMatches = expectedStatus < 0 || result.StatusCode == expectedStatus;
                resultMatches = returned && requestHandle.IsDone && result.Failure == expectedFailure && statusMatches && result.Body == expectedBody;
                Debug.Log($"HTTP 边界结果：{name}，Failure={result.Failure}，HTTP={result.StatusCode}，IsDone={requestHandle.IsDone}，Body={result.Body}，Error={result.Error}");
            });
            returned = true;
            float deadline = Time.realtimeSinceStartup + timeoutSeconds + 3f;

            while (!requestHandle.IsDone && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            yield return null;
            bool passed = requestHandle.IsDone && callbackCount == 1 && resultMatches;

            if (passed)
            {
                passedCount++;
            }

            Debug.Log($"HTTP 边界项：{(passed ? "PASS" : "FAIL")}，{name}，回调={callbackCount}");
            requestHandle.Dispose();
            requestHandle = null;
        }

        private void OnDisable()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            requestHandle?.Dispose();
            requestHandle = null;
        }
    }
}
