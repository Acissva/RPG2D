using QFramework;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPG2D
{
    public sealed class ObjectPoolSystem : AbstractSystem, IObjectPoolSystem
    {
        private const string RootName = "[RPG2D ObjectPool]";

        // Prefab -> Queue<Instance>
        private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();
        // Instance -> Prefab
        private readonly Dictionary<GameObject, GameObject> instanceToPrefab = new();
        private readonly HashSet<GameObject> inactiveInstances = new();

        private Transform root;

        protected override void OnInit()
        {
            this.RegisterEvent<SceneLoadStartedEvent>(_ => ClearAll());
        }

        public void Preload(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0)
            {
                return;
            }

            EnsurePool(prefab);

            for (int i = 0; i < count; i++)
            {
                GameObject instance = CreateInstance(prefab);
                pools[prefab].Enqueue(instance);
                inactiveInstances.Add(instance);
            }
        }

        public GameObject Spawn(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            if (prefab == null)
            {
                return null;
            }

            EnsurePool(prefab);

            GameObject instance = Pop(prefab) ?? CreateInstance(prefab);

            inactiveInstances.Remove(instance);

            instance.transform.SetParent(parent, false);
            instance.transform.SetPositionAndRotation(position, rotation);

            NotifySpawned(instance);
            instance.SetActive(true);

            return instance;
        }

        public T Spawn<T>(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null) where T : Component
        {
            GameObject instance = Spawn(prefab, position, rotation, parent);
            return instance != null ? instance.GetComponent<T>() : null;
        }

        public void Despawn(GameObject instance)
        {
            if (instance == null || inactiveInstances.Contains(instance))
            {
                return;
            }

            if (!instanceToPrefab.TryGetValue(instance, out GameObject prefab))
            {
                return;
            }

            EnsurePool(prefab);

            NotifyDespawned(instance);

            instance.SetActive(false);
            instance.transform.SetParent(root, false);

            inactiveInstances.Add(instance);
            pools[prefab].Enqueue(instance);
        }

        public void Release(GameObject instance)
        {
            if (!instanceToPrefab.TryGetValue(instance, out GameObject prefab))
            {
                Object.Destroy(instance);
                return;
            }

            Despawn(instance);
        }

        public void Clear(GameObject prefab)
        {
            if (prefab == null || !pools.TryGetValue(prefab, out Queue<GameObject> pool))
            {
                return;
            }

            while (pool.Count > 0)
            {
                DestroyPooledInstance(pool.Dequeue());
            }

            pools.Remove(prefab);
        }

        public void ClearAll()
        {
            foreach (Queue<GameObject> pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    DestroyPooledInstance(pool.Dequeue());
                }
            }

            pools.Clear();
            inactiveInstances.Clear();
            instanceToPrefab.Clear();
            root = null;
        }

        public bool IsPooled(GameObject instance)
        {
            return instance != null && instanceToPrefab.ContainsKey(instance);
        }

        private void EnsurePool(GameObject prefab)
        {
            EnsureRoot();

            if (!pools.ContainsKey(prefab))
            {
                pools.Add(prefab, new Queue<GameObject>());
            }
        }

        private GameObject Pop(GameObject prefab)
        {
            Queue<GameObject> pool = pools[prefab];

            while (pool.Count > 0)
            {
                GameObject instance = pool.Dequeue();
                inactiveInstances.Remove(instance);

                if (instance != null)
                {
                    return instance;
                }
            }

            return null;
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            EnsureRoot();

            GameObject instance = Object.Instantiate(prefab, root);
            instance.SetActive(false);
            instanceToPrefab[instance] = prefab;

            return instance;
        }

        private void EnsureRoot()
        {
            if (root != null)
            {
                return;
            }

            GameObject rootObject = GameObject.Find(RootName);

            if (rootObject == null)
            {
                rootObject = new GameObject(RootName);
                //Object.DontDestroyOnLoad(rootObject);
            }

            root = rootObject.transform;
        }

        private void DestroyPooledInstance(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            inactiveInstances.Remove(instance);
            instanceToPrefab.Remove(instance);
            Object.Destroy(instance);
        }

        private static void NotifySpawned(GameObject instance)
        {
            IPoolable[] poolables = instance.GetComponentsInChildren<IPoolable>(true);

            foreach (IPoolable poolable in poolables)
            {
                poolable.OnSpawned();
            }
        }

        private static void NotifyDespawned(GameObject instance)
        {
            IPoolable[] poolables = instance.GetComponentsInChildren<IPoolable>(true);

            foreach (IPoolable poolable in poolables)
            {
                poolable.OnDespawned();
            }
        }
    }
}
