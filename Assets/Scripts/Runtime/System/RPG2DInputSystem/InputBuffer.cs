using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    /// <summary>
    /// 输入缓存类
    /// </summary>
    public class InputBuffer
    {
        /// <summary>
        /// 记录某个动作在哪一帧被按下
        /// </summary>
        private readonly Dictionary<InputActionType, int> pressedFrames = new();
        /// <summary>
        /// 记录某个动作是否已经被业务逻辑消费过
        /// </summary>
        private readonly HashSet<InputActionType> consumedActions = new();

        /// <summary>
        /// 缓存多少帧
        /// </summary>
        private readonly int bufferFrames;

        public InputBuffer(int bufferFrames = 1)
        {
            this.bufferFrames = bufferFrames;
        }


        /// <summary>
        /// 当检测到按键按下时调用
        /// </summary>
        public void Press(InputActionType action)
        {
            pressedFrames[action] = Time.frameCount;
            consumedActions.Remove(action);
        }

        /// <summary>
        /// 判断指定的输入现在是否还有效
        /// </summary>
        public bool Has(InputActionType action)
        {
            //这个动作有没有被按下过？
            if (!pressedFrames.TryGetValue(action, out int pressedFrame))
            {
                return false;
            }

            //有没有已经被消费？
            if (consumedActions.Contains(action))
            {
                return false;
            }

            //当前帧距离按下帧数是否还在 bufferFrames 范围内？
            return Time.frameCount - pressedFrame < bufferFrames;
        }

        public bool Consume(InputActionType action)
        {
            if (!Has(action))
            {
                return false;
            }

            consumedActions.Add(action);
            return true;
        }

        public void ClearExpired()
        {
            //因为不能在 foreach 遍历 Dictionary 时直接删除其中元素
            //所以先缓存到 过时列表
            List<InputActionType> expired = null;

            foreach (var pair in pressedFrames)
            {
                if(Time.frameCount - pair.Value > bufferFrames)
                {
                    if(expired == null)
                    {
                        expired = new List<InputActionType>();
                    }
                    expired.Add(pair.Key);
                }
            }

            if(expired == null) return;

            foreach (var action in expired)
            {
                pressedFrames.Remove(action);
                consumedActions.Remove(action);
            }
        }
        
    }
}