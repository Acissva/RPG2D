using UnityEngine;

namespace RPG2D
{
    [CreateAssetMenu(
        fileName = "PlayerConfig",
        menuName = "RPG2D/Config/Game/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField, Min(0)] private int initialHP = 5;
        [SerializeField, Min(1)] private int maxHP = 5;
        [SerializeField, Min(0f)] private float moveSpeed = 5f;

        public int InitialHP => initialHP;
        public int MaxHP => maxHP;
        public float MoveSpeed => moveSpeed;

        private void OnValidate()
        {
            if (maxHP < 1) maxHP = 1;
            if (initialHP < 0) initialHP = 0;
            if (moveSpeed < 0) moveSpeed = 0;

            if (initialHP > maxHP)
            {
                initialHP = maxHP;
            }
        }

    }
}

