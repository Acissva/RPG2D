using QFramework;
using System;
using UnityEngine;

namespace RPG2D
{
    public interface IRuntimeUpdateSystem : ISystem
    {
        void RegisterUpdate(Action onUpdate);
        void UnregisterUpdate(Action onUpdate);
    }

    public class RuntimeUpdateSystem : AbstractSystem, IRuntimeUpdateSystem
    {
        private GameObject updateObj;
        private RuntimeUpdateBehaviour updateBehaviour;

        protected override void OnInit()
        {
            updateObj = new GameObject(nameof(RuntimeUpdateBehaviour));
            updateObj.transform.SetParent(RPG2D.FrameworkGenerateItemsParent);

            updateBehaviour = updateObj.AddComponent<RuntimeUpdateBehaviour>();
        }

        public void RegisterUpdate(Action onUpdate)
        {
            if(updateBehaviour != null)
            {
                updateBehaviour.OnUpdate += onUpdate;
            }
        }

        public void UnregisterUpdate(Action onUpdate)
        {
            if (updateBehaviour != null)
            {
                updateBehaviour.OnUpdate -= onUpdate;
            }
        }

    }

    public class RuntimeUpdateBehaviour : MonoBehaviour
    {
        public event Action OnUpdate;

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        private void OnDestroy()
        {
            OnUpdate = null;
        }
    }
}

