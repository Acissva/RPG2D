using QFramework;
using UnityEngine;

namespace RPG2D
{
    public interface IAssetSystem : ISystem
    {
        T Load<T>(string assetKey) where T : Object;
    }

    //TODO:当前底层仍使用 Resources，后续可在这里统一替换为 Addressables 或 AssetBundle。
    public class AssetSystem : AbstractSystem, IAssetSystem
    {
        protected override void OnInit()
        {
        }

        public T Load<T>(string assetKey) where T : Object
        {
            if (string.IsNullOrWhiteSpace(assetKey))
            {
                Debug.LogError($"加载资源失败：assetKey 为空，Type={typeof(T).Name}");
                return null;
            }

            return Resources.Load<T>(assetKey);
        }
    }
}
