using QFramework;
using System.Collections;
using UnityEngine;

namespace RPG2D
{
    public class HttpRequestConcurrencyLearningProbe : RPG2DController
    {
        private const string RequestABody = "{\"request\":\"A\",\"message\":\"并行请求\"}";
        private IHttpRequestHandle requestA;
        private IHttpRequestHandle requestB;
        private Coroutine observation;
        private int aCallbacks;
        private int bCallbacks;
        private bool aSuccess;
        private bool bSuccess;
        private bool aWasPending;

        [ContextMenu("Parallel Complete Both")]
        public void CompleteBoth()
        {
            StartPair(false);
        }

        [ContextMenu("Parallel Cancel A")]
        public void CancelA()
        {
            StartPair(true);
        }

        private void StartPair(bool cancelA)
        {
            if (!Application.isPlaying || !isActiveAndEnabled)
            {
                Debug.Log("请进入 Play 模式并启用本组件，再开始并存请求。");
                return;
            }

            if (observation != null)
            {
                Debug.Log("上一组还在观察，请等待本组汇总后再开始。");
                return;
            }

            requestA?.Dispose();
            requestB?.Dispose();
            aCallbacks = 0;
            bCallbacks = 0;
            aSuccess = false;
            bSuccess = false;
            aWasPending = false;
            IHttpRequestSystem http = this.GetSystem<IHttpRequestSystem>();
            requestA = http.PostJson("http://127.0.0.1:5087/delay/3000/echo", RequestABody, 5, 16 * 1024, result =>
            {
                aCallbacks++;
                aSuccess = result.Failure == HttpRequestFailure.None && result.StatusCode == 200 && result.Body == RequestABody;
                Debug.Log($"并存 A 完成：Failure={result.Failure}，HTTP={result.StatusCode}，IsDone={requestA?.IsDone}，Body={result.Body}");
            });
            requestB = http.Get("http://127.0.0.1:5087/delay/500/health", 5, 16 * 1024, result =>
            {
                bCallbacks++;
                bSuccess = result.Failure == HttpRequestFailure.None && result.StatusCode == 200 && result.Body.Contains("\"service\":\"RPG2D_HTTP_LEARNING_HOST\"");
                Debug.Log($"并存 B 完成：Failure={result.Failure}，HTTP={result.StatusCode}，IsDone={requestB?.IsDone}");

                if (cancelA)
                {
                    aWasPending = requestA != null && !requestA.IsDone;
                    requestA?.Cancel();
                    requestA?.Dispose();
                    Debug.Log($"只取消 A：取消前仍活动={aWasPending}，A.IsDone={requestA?.IsDone}，B.IsDone={requestB?.IsDone}");
                }
            });
            Debug.Log($"并存开始：模式={(cancelA ? "CancelA" : "CompleteBoth")}，A.IsDone={requestA.IsDone}，B.IsDone={requestB.IsDone}");
            observation = StartCoroutine(Observe(cancelA));
        }

        private IEnumerator Observe(bool cancelA)
        {
            yield return new WaitForSecondsRealtime(6f);
            bool aExpected = cancelA ? aWasPending && aCallbacks == 0 : aCallbacks == 1 && aSuccess;
            bool passed = aExpected && bCallbacks == 1 && bSuccess && requestA.IsDone && requestB.IsDone;
            Debug.Log($"并存汇总：{(passed ? "PASS" : "FAIL")}，模式={(cancelA ? "CancelA" : "CompleteBoth")}，A回调={aCallbacks}，B回调={bCallbacks}，A.IsDone={requestA.IsDone}，B.IsDone={requestB.IsDone}");

            if (!passed)
            {
                Debug.LogError("本组没有满足预期，请保留日志，由 Codex 核对宿主、时序和实际结果。");
            }

            requestA.Dispose();
            requestB.Dispose();
            observation = null;
        }

        private void OnDisable()
        {
            if (observation != null)
            {
                StopCoroutine(observation);
                observation = null;
            }

            requestA?.Dispose();
            requestB?.Dispose();
            requestA = null;
            requestB = null;
        }
    }
}
