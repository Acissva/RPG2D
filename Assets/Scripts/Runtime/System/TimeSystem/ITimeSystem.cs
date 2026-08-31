using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public interface ITimeSystem : ISystem
    {
        float CurrentSeconds { get; }
        void AddDelayTask(float seconds, Action OnDelayFinish);
    }

    public class DelayTask
    {
        public float Seconds { get; set; }
        public Action OnFinish { get; set; }

        public float StartSeconds { get; set; }
        public float FinishSeconds { get; set; }
        public DelayTaskState State { get; set; }
    }

    public enum DelayTaskState
    {
        NoStart,
        Started,
        Finish
    }

    public class TimeSystem : AbstractSystem, ITimeSystem
    {
        private IRuntimeUpdateSystem runtimeUpdateSystem;

        protected override void OnInit()
        {
            runtimeUpdateSystem = this.GetSystem<IRuntimeUpdateSystem>();

            runtimeUpdateSystem.RegisterUpdate(OnUpdate);

            CurrentSeconds = 0;
        }

        private void OnUpdate()
        {
            CurrentSeconds += Time.deltaTime;

            if (mDelayTasks.Count > 0)
            {
                var currentNode = mDelayTasks.First;

                while (currentNode != null)
                {
                    var nextNode = currentNode.Next;
                    var delayTask = currentNode.Value;

                    if (delayTask.State == DelayTaskState.NoStart)
                    {
                        delayTask.State = DelayTaskState.Started;
                        delayTask.StartSeconds = CurrentSeconds;
                        delayTask.FinishSeconds = CurrentSeconds + delayTask.Seconds;
                    }
                    else if (delayTask.State == DelayTaskState.Started)
                    {
                        if (CurrentSeconds >= delayTask.FinishSeconds)
                        {
                            delayTask.State = DelayTaskState.Finish;
                            delayTask.OnFinish();

                            delayTask.OnFinish = null;
                            mDelayTasks.Remove(currentNode);
                        }
                    }

                    currentNode = nextNode;
                }
            }
        }

        public float CurrentSeconds { get; private set; }

        private LinkedList<DelayTask> mDelayTasks = new LinkedList<DelayTask>();

        public void AddDelayTask(float seconds, Action OnDelayFinish)
        {
            var delayTask = new DelayTask()
            {
                Seconds = seconds,
                OnFinish = OnDelayFinish,
                State = DelayTaskState.NoStart,
            };

            mDelayTasks.AddLast(delayTask);
        }

    }


}

