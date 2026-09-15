using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class TimerSystem : AbstractSystem, ITimerSystem
    {
        //sealed 在 C# 中表示 “禁止继承”
        private sealed class TimerTask
        {
            public int Id;
            public float FinishSeconds;
            public Action Callback;
            public TimerScope Scope;
        }

        private readonly LinkedList<TimerTask> timerTasks = new LinkedList<TimerTask>();
        private readonly List<TimerTask> dueTasks = new List<TimerTask>();

        private IRuntimeUpdateSystem runtimeUpdateSystem;
        private int nextTimerId = 1;

        public float CurrentSeconds { get; private set; }

        protected override void OnInit()
        {
            runtimeUpdateSystem = this.GetSystem<IRuntimeUpdateSystem>();
            runtimeUpdateSystem.RegisterUpdate(OnUpdate);

            this.RegisterEvent<SceneLoadStartedEvent>(_ => CancelScope(TimerScope.Scene));

            CurrentSeconds = 0f;
        }

        public TimerHandle Schedule(float seconds, Action callback, TimerScope scope)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            int id = nextTimerId++;
            TimerHandle handle = new TimerHandle(id);

            timerTasks.AddLast(new TimerTask
            {
                Id = id,
                FinishSeconds = CurrentSeconds + Mathf.Max(0f, seconds),
                Callback = callback,
                Scope = scope
            });

            return handle;
        }

        public bool Cancel(TimerHandle handle)
        {
            if (!handle.IsValid)
            {
                return false;
            }

            LinkedListNode<TimerTask> currentNode = timerTasks.First;

            while (currentNode != null)
            {
                LinkedListNode<TimerTask> nextNode = currentNode.Next;
                TimerTask timerTask = currentNode.Value;

                if (timerTask.Id == handle.Id)
                {
                    timerTask.Callback = null;
                    timerTasks.Remove(currentNode);
                    return true;
                }

                currentNode = nextNode;
            }

            for (int i = 0; i < dueTasks.Count; i++)
            {
                TimerTask timerTask = dueTasks[i];

                if (timerTask.Id == handle.Id && timerTask.Callback != null)
                {
                    timerTask.Callback = null;
                    return true;
                }
            }

            return false;
        }

        public void CancelScope(TimerScope scope)
        {
            LinkedListNode<TimerTask> currentNode = timerTasks.First;

            while (currentNode != null)
            {
                LinkedListNode<TimerTask> nextNode = currentNode.Next;
                TimerTask timerTask = currentNode.Value;

                if (timerTask.Scope == scope)
                {
                    timerTask.Callback = null;
                    timerTasks.Remove(currentNode);
                }

                currentNode = nextNode;
            }

            for (int i = 0; i < dueTasks.Count; i++)
            {
                TimerTask timerTask = dueTasks[i];

                if (timerTask.Scope == scope)
                {
                    timerTask.Callback = null;
                }
            }
        }

        private void OnUpdate()
        {
            CurrentSeconds += Time.deltaTime;

            if (timerTasks.Count == 0)
            {
                return;
            }

            LinkedListNode<TimerTask> currentNode = timerTasks.First;

            while (currentNode != null)
            {
                LinkedListNode<TimerTask> nextNode = currentNode.Next;
                TimerTask timerTask = currentNode.Value;

                if (CurrentSeconds >= timerTask.FinishSeconds)
                {
                    timerTasks.Remove(currentNode);
                    dueTasks.Add(timerTask);
                }

                currentNode = nextNode;
            }

            for (int i = 0; i < dueTasks.Count; i++)
            {
                TimerTask timerTask = dueTasks[i];
                Action callback = timerTask.Callback;
                timerTask.Callback = null;

                if (callback == null)
                {
                    continue;
                }

                try
                {
                    callback();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            dueTasks.Clear();
        }
    }
}
