using QFramework;
using UnityEngine;

namespace RPG2D
{
    public interface IObjectPoolSystem : ISystem
    {
        void Preload(GameObject prefab, int count);

        /// <summary>
        /// 获取对象，出池
        /// </summary>
        GameObject Spawn(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null);

        T Spawn<T>(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null) where T : Component;

        /// <summary>
        /// 归还对象，入池
        /// </summary>
        void Despawn(GameObject instance);

        /// <summary>
        /// 释放对象：若对象为池对象，则DeSpawn，反之则Destroy
        /// </summary>
        /// <param name="instance"></param>
        void Release(GameObject instance);

        void Clear(GameObject prefab);

        void ClearAll();

        bool IsPooled(GameObject instance);
    }
}
