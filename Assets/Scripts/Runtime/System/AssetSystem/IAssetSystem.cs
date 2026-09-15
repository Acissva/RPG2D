using QFramework;
using UnityEngine;

namespace RPG2D
{
    public interface IAssetSystem : ISystem
    {
        T Load<T>(string assetKey) where T : Object;
    }
}
