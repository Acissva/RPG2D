using System;
using UnityEngine;

namespace RPG2D
{
    /// <summary>
    /// 前期美术验证使用，决定现阶段不考虑美术，故仅用于美术验证场景AnimeTest.Scene
    /// </summary>
    public class AnimePlayerController : MonoBehaviour
    {
        private Transform visual;
        private Animator mAnimator;
        private Rigidbody2D mRigidbody2D;

        private FaceDirection faceDirection;
        private Vector2 inputAxis;
        private Vector2 moveDirection;
        public float speed;

        private void Awake()
        {
            //获取挂载视觉类组件的节点
            visual = transform.Find("Visual");
            if(!visual.TryGetComponent<Animator>(out mAnimator))
            {
                Debug.LogError("Can not get Animator Component on Node: Player - Visual");
            }

            //自身挂载控制类组件
            if (!TryGetComponent<Rigidbody2D>(out mRigidbody2D))
            {
                Debug.LogError("Can not get Rigidbody2D Component on Node: Player");
            }
        }

        private void Update()
        {
            HandleInput();
            UpdateAnimation();
        }

        private void FixedUpdate()
        {
            Move();
        }

        /// <summary>
        /// 接收输入
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                faceDirection = FaceDirection.Up;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                faceDirection = FaceDirection.Down;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                faceDirection = FaceDirection.Left;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                faceDirection = FaceDirection.Right;
            }

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            inputAxis = new Vector2(h, v);
            moveDirection = inputAxis.normalized;
        }

        /// <summary>
        /// 更新动画
        /// </summary>
        private void UpdateAnimation()
        {
            if (mAnimator != null)
            {
                mAnimator.SetFloat("FaceDirection", (float)faceDirection);
            }

            if (mAnimator != null && mRigidbody2D != null)
            {
                mAnimator.SetFloat("InputAxis", inputAxis.magnitude);
            }
        }

        /// <summary>
        /// 移动
        /// </summary>
        private void Move()
        {
            mRigidbody2D.velocity = moveDirection * speed;
        }

    }

    public enum FaceDirection
    {
        Down = 0,
        Up = 1, 
        Left = 2, 
        Right = 3
    }

}


