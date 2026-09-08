using UnityEngine;

namespace RPG2D
{
    [DisallowMultipleComponent]
    public sealed class PlayerSpawnPoint : MonoBehaviour
    {
        [Tooltip("场景内稳定且唯一的出生点 ID")]
        [SerializeField] private string spawnPointId = string.Empty;

        public string SpawnPointId =>
            string.IsNullOrWhiteSpace(spawnPointId)
                ? string.Empty
                : spawnPointId.Trim();

        private void OnValidate()
        {
            spawnPointId = SpawnPointId;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.DrawLine(
                transform.position,
                transform.position + Vector3.up * 0.6f);
        }
    }
}
