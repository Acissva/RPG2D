using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class DungeonRoomDoorController : RPG2DController
    {
        [SerializeField]
        private string lockRoomId = "room_dungeon_001_combat_001";

        [SerializeField]
        private Collider2D blockingCollider;

        [SerializeField]
        private GameObject lockedVisual;

        private IDungeonSystem dungeonSystem;

        private void Awake()
        {
            dungeonSystem = this.GetSystem<IDungeonSystem>();

            IDungeonModel dungeonModel =
                this.GetModel<IDungeonModel>();

            dungeonModel.DungeonRevision
                .RegisterWithInitValue(_ => RefreshState())
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void RefreshState()
        {
            bool isLocked =
                dungeonSystem.IsRoomLocked(lockRoomId);

            if (blockingCollider != null)
            {
                blockingCollider.enabled = isLocked;
            }

            if (lockedVisual != null)
            {
                lockedVisual.SetActive(isLocked);
            }
        }
    }
}
