using QFramework;
using System;
using UnityEngine;

namespace RPG2D
{
    public interface ITravelSystem : ISystem
    {
        bool IsTraveling { get; }

        /// <returns>返回 true，只表示请求已被接受，不代表玩家已经到达</returns>
        bool Travel(TravelRequest request);
    }

    public class TravelSystem : AbstractSystem, ITravelSystem
    {
        private ISceneSystem sceneSystem;

        private TravelRequest pendingRequest;
        private string fromSceneName = string.Empty;

        public bool IsTraveling { get; private set; }

        protected override void OnInit()
        {
            sceneSystem = this.GetSystem<ISceneSystem>();

            this.RegisterEvent<SceneLoadCompletedEvent>(
                OnSceneLoadCompleted);

            this.RegisterEvent<SceneLoadFailedEvent>(
                OnSceneLoadFailed);
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

            var normalizedRequest = new TravelRequest(
                request.TargetSceneName?.Trim(),
                request.TargetSpawnPointId?.Trim());

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
                FromSceneName = fromSceneName,
                TargetSceneName = pendingRequest.TargetSceneName,
                TargetSpawnPointId = pendingRequest.TargetSpawnPointId
            });

            if (string.Equals(
                    currentSceneName,
                    pendingRequest.TargetSceneName,
                    StringComparison.Ordinal))
            {
                CompleteTravelAtSpawnPoint();
            }
            else
            {
                sceneSystem.LoadScene(pendingRequest.TargetSceneName);
            }

            return true;
        }

        private void OnSceneLoadCompleted(SceneLoadCompletedEvent e)
        {
            if (!IsTraveling)
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

            if (!string.Equals(
                    e.ToSceneName,
                    pendingRequest.TargetSceneName,
                    StringComparison.Ordinal))
            {
                FailTravel($"场景加载失败事件与迁移目标不一致：{e.ToSceneName}");
                return;
            }

            FailTravel(e.Reason);
        }

        private void CompleteTravelAtSpawnPoint()
        {
            if (!string.Equals(
                    sceneSystem.CurrentSceneName,
                    pendingRequest.TargetSceneName,
                    StringComparison.Ordinal))
            {
                FailTravel($"当前场景不是迁移目标：{sceneSystem.CurrentSceneName}");
                return;
            }

            if (!TryPlacePlayerAtSpawnPoint(
                    pendingRequest.TargetSpawnPointId,
                    out string reason))
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
                FromSceneName = completedFromSceneName,
                SceneName = completedRequest.TargetSceneName,
                SpawnPointId = completedRequest.TargetSpawnPointId
            });
        }

        private bool TryPlacePlayerAtSpawnPoint(string spawnPointId, out string reason)
        {
            PlayerSpawnPoint targetSpawnPoint = null;
            PlayerSpawnPoint[] spawnPoints =
                UnityEngine.Object.FindObjectsOfType<PlayerSpawnPoint>();

            foreach (PlayerSpawnPoint spawnPoint in spawnPoints)
            {
                if (!string.Equals(spawnPoint.SpawnPointId, spawnPointId, StringComparison.Ordinal))
                {
                    continue;
                }

                if (targetSpawnPoint != null)
                {
                    reason = $"场景中存在重复的出生点 ID：{spawnPointId}";
                    return false;
                }

                targetSpawnPoint = spawnPoint;
            }

            if (targetSpawnPoint == null)
            {
                reason = $"场景中找不到出生点：{spawnPointId}";
                return false;
            }

            GameObject player;

            try
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }
            catch (UnityException e)
            {
                reason = $"查找 Player 失败：{e.Message}";
                return false;
            }

            if (player == null)
            {
                reason = "场景中找不到带有 Player Tag 的玩家对象。";
                return false;
            }

            Vector3 targetPosition = targetSpawnPoint.transform.position;

            if (player.TryGetComponent(out PlayerMovement playerMovement))
            {
                playerMovement.ResetMotion();
            }

            if (player.TryGetComponent(out Rigidbody2D playerRigidbody))
            {
                playerRigidbody.velocity = Vector2.zero;
                playerRigidbody.angularVelocity = 0f;
                playerRigidbody.position = targetPosition;
            }

            player.transform.position = targetPosition;

            //立即把 GameObject 的 Transform 位置、旋转等变化同步给 2D 物理系统
            Physics2D.SyncTransforms();

            reason = string.Empty;
            return true;
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
