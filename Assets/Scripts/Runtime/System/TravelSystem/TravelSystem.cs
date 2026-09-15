using QFramework;
using System;
using UnityEngine;

namespace RPG2D
{
    public class TravelSystem : AbstractSystem, ITravelSystem
    {
        private readonly ITravelSceneAdapter sceneAdapter;
        private ISceneSystem sceneSystem;

        private TravelRequest pendingRequest;
        private string fromSceneName = string.Empty;

        public bool IsTraveling { get; private set; }

        public TravelSystem(ITravelSceneAdapter sceneAdapter)
        {
            this.sceneAdapter = sceneAdapter ?? throw new ArgumentNullException(nameof(sceneAdapter));
        }

        protected override void OnInit()
        {
            sceneSystem = this.GetSystem<ISceneSystem>();

            this.RegisterEvent<SceneLoadCompletedEvent>(OnSceneLoadCompleted);

            this.RegisterEvent<SceneLoadFailedEvent>(OnSceneLoadFailed);
        }

        public bool Travel(TravelRequest request)
        {
            if (IsTraveling)
            {
                Debug.LogWarning("已有迁移正在执行，拒绝新的迁移请求。");
                return false;
            }

            if (sceneSystem.IsLoading)
            {
                Debug.LogWarning("SceneSystem 正在加载场景，拒绝迁移请求。");
                return false;
            }

            //TravelRequest 是 struct，属于值类型，不是引用类型
            //这里采用复制而不是重新调用构造方法，主要是为了保留同一个 OperationId (GUID)
            TravelRequest normalizedRequest = request;      //这行会复制一份完整的 TravelRequest
            normalizedRequest.TargetSceneName = request.TargetSceneName?.Trim();
            normalizedRequest.TargetSpawnPointId = request.TargetSpawnPointId?.Trim();

            if (normalizedRequest.OperationId == Guid.Empty)
            {
                Debug.LogError("迁移请求失败：OperationId 无效。");
                return false;
            }

            if (string.IsNullOrEmpty(normalizedRequest.TargetSceneName))
            {
                Debug.LogError("迁移请求失败：目标场景名称为空。");
                return false;
            }

            if (string.IsNullOrEmpty(normalizedRequest.TargetSpawnPointId))
            {
                Debug.LogError("迁移请求失败：目标出生点 ID 为空。");
                return false;
            }

            string currentSceneName = sceneSystem.CurrentSceneName;

            if (string.IsNullOrWhiteSpace(currentSceneName))
            {
                Debug.LogError("迁移请求失败：无法获取当前场景名称。");
                return false;
            }

            pendingRequest = normalizedRequest;
            fromSceneName = currentSceneName;
            IsTraveling = true;

            this.SendEvent(new TravelStartedEvent
            {
                OperationId = pendingRequest.OperationId,
                FromSceneName = fromSceneName,
                TargetSceneName = pendingRequest.TargetSceneName,
                TargetSpawnPointId = pendingRequest.TargetSpawnPointId
            });

            if (string.Equals(currentSceneName, pendingRequest.TargetSceneName, StringComparison.Ordinal))
            {
                CompleteTravelAtSpawnPoint();
            }
            else
            {
                bool accepted = sceneSystem.LoadScene(pendingRequest.TargetSceneName, pendingRequest.OperationId);

                if (!accepted)
                {
                    FailTravel("SceneSystem 拒绝了场景加载请求。");
                }
            }

            return true;
        }

        private void OnSceneLoadCompleted(SceneLoadCompletedEvent e)
        {
            if (!IsTraveling)
            {
                return;
            }

            if (e.OperationId != pendingRequest.OperationId)
            {
                return;
            }

            if (!string.Equals(
                    e.SceneName,
                    pendingRequest.TargetSceneName,
                    StringComparison.Ordinal))
            {
                FailTravel($"场景加载完成事件与迁移目标不一致：{e.SceneName}");
                return;
            }

            CompleteTravelAtSpawnPoint();
        }

        private void OnSceneLoadFailed(SceneLoadFailedEvent e)
        {
            if (!IsTraveling)
            {
                return;
            }

            if (e.OperationId != pendingRequest.OperationId)
            {
                return;
            }

            if (!string.Equals(e.ToSceneName, pendingRequest.TargetSceneName, StringComparison.Ordinal))
            {
                FailTravel($"场景加载失败事件与迁移目标不一致：{e.ToSceneName}");
                return;
            }

            FailTravel(e.Reason);
        }

        private void CompleteTravelAtSpawnPoint()
        {
            if (!string.Equals(sceneSystem.CurrentSceneName, pendingRequest.TargetSceneName, StringComparison.Ordinal))
            {
                FailTravel($"当前场景不是迁移目标：{sceneSystem.CurrentSceneName}");
                return;
            }

            if (!sceneAdapter.TryPlacePlayerAtSpawnPoint(pendingRequest.TargetSpawnPointId, out string reason))
            {
                FailTravel(reason);
                return;
            }

            TravelRequest completedRequest = pendingRequest;
            string completedFromSceneName = fromSceneName;

            ClearPendingTravel();

            Debug.Log(
                $"迁移完成：{completedFromSceneName} -> " +
                $"{completedRequest.TargetSceneName}，" +
                $"出生点：{completedRequest.TargetSpawnPointId}");

            this.SendEvent(new TravelCompletedEvent
            {
                OperationId = completedRequest.OperationId,
                FromSceneName = completedFromSceneName,
                SceneName = completedRequest.TargetSceneName,
                SpawnPointId = completedRequest.TargetSpawnPointId
            });
        }

        private void FailTravel(string reason)
        {
            TravelRequest failedRequest = pendingRequest;
            string failedFromSceneName = fromSceneName;

            ClearPendingTravel();

            string failureReason = string.IsNullOrWhiteSpace(reason)
                ? "未知原因"
                : reason;

            Debug.LogError(
                $"迁移失败：{failedFromSceneName} -> " +
                $"{failedRequest.TargetSceneName}，原因：{failureReason}");

            this.SendEvent(new TravelFailedEvent
            {
                OperationId = failedRequest.OperationId,
                FromSceneName = failedFromSceneName,
                TargetSceneName = failedRequest.TargetSceneName,
                TargetSpawnPointId = failedRequest.TargetSpawnPointId,
                Reason = failureReason
            });
        }

        private void ClearPendingTravel()
        {
            pendingRequest = default;
            fromSceneName = string.Empty;
            IsTraveling = false;
        }
    }
}
