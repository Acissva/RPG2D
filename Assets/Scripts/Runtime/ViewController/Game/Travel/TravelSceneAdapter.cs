using System;
using UnityEngine;

namespace RPG2D
{
    public sealed class TravelSceneAdapter : ITravelSceneAdapter
    {
        public bool TryPlacePlayerAtSpawnPoint(string spawnPointId, out string reason)
        {
            PlayerSpawnPoint targetSpawnPoint = null;
            PlayerSpawnPoint[] spawnPoints = UnityEngine.Object.FindObjectsOfType<PlayerSpawnPoint>();

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
            Physics2D.SyncTransforms();

            reason = string.Empty;
            return true;
        }
    }
}
