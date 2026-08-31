using UnityEngine;

namespace RPG2D
{
    public interface IMovementEffect
    {
        bool IsFinished {  get; }
        Vector2 GetVelocity(float fixedDeltaTime);
    }

    public class KnockbackMovementEffect : IMovementEffect
    {
        private Vector2 velocity;
        private float timer;
        private readonly float drag;

        public bool IsFinished => timer <= 0f;

        public KnockbackMovementEffect(
            Vector2 direction,
            float force,
            float duration,
            float drag)
        {
            velocity = direction.normalized * force;
            timer = duration;
            this.drag = drag;
        }

        public Vector2 GetVelocity(float fixedDeltaTime)
        {
            if(timer <= 0f)
            {
                return Vector2.zero;
            }

            timer -= fixedDeltaTime;

            var currentVelocity = velocity;

            velocity = Vector2.Lerp(velocity, Vector2.zero, drag * fixedDeltaTime);

            return currentVelocity;
        }
    }


}

