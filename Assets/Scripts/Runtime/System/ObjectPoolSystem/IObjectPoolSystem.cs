using QFramework;
using UnityEngine;

namespace RPG2D
{
    public interface IObjectPoolSystem : ISystem
    {
        /// <summary>
        /// 获取对象，出池
        /// </summary>
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null);

        /// <summary>
        /// 归还对象，入池
        /// </summary>
        void Despawn(GameObject instance);
    }
}
