using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    /*
        PlayerController
            负责：读取输入意图，并把意图传给 PlayerMovement

        PlayerMovement
            负责：根据外部给的移动意图，计算最终速度并操作 Rigidbody2D
    */
    public class PlayerMovement : MonoBehaviour
    {
        private readonly List<IMovementEffect> movementEffects = new List<IMovementEffect>();

        private Rigidbody2D mRigidbody2D;

        private Vector2 moveDir;
        private float moveSpeed;

        private void Awake()
        {
            if (!TryGetComponent<Rigidbody2D>(out mRigidbody2D))
            {
                Debug.Log("Can not get Rigidbody2D component");
            }
        }

        public void SetMoveInput(Vector2 dir, float speed)
        {
            moveDir = dir;
            moveSpeed = speed;
        }

        public void ResetMotion()
        {
            moveDir = Vector2.zero;
            moveSpeed = 0f;
            movementEffects.Clear();

            if (mRigidbody2D == null)
            {
                return;
            }

            mRigidbody2D.velocity = Vector2.zero;
            mRigidbody2D.angularVelocity = 0f;
        }

        public void AddMovementEffect(IMovementEffect effect)
        {
            movementEffects.Add(effect);
        }

        private void FixedUpdate()
        {
            if (mRigidbody2D == null) return;

            var velocity = moveDir * moveSpeed;

            //foreach不允许遍历时删除元素，所以用for，for中倒着循环时删除元素比较安全
            for (int i = movementEffects.Count - 1; i >= 0; i--)
            {
                var effect = movementEffects[i];

                velocity += effect.GetVelocity(Time.fixedDeltaTime);

                if (effect.IsFinished)
                {
                    movementEffects.RemoveAt(i);
                }
            }

            mRigidbody2D.velocity = velocity;

        }
    }
}
