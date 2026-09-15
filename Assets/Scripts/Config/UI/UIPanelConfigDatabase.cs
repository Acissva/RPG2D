using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    [CreateAssetMenu(menuName = "RPG2D/Config/UI/Panel Config Database", fileName = "UIPanelConfigDatabase")]
    public class UIPanelConfigDatabase : ScriptableObject
    {
        [SerializeField] private List<UIPanelConfig> panelConfigs = new();

        private Dictionary<UIPanelId, UIPanelConfig> configMap;

        public bool TryGetConfig(UIPanelId panelId, out UIPanelConfig config)
        {
            BuildConfigMapIfNeeded();
            return configMap.TryGetValue(panelId, out config);
        }

        private void BuildConfigMapIfNeeded()
        {
            if (configMap != null)
            {
                return;
            }

            panelConfigs ??= new List<UIPanelConfig>();
            configMap = new Dictionary<UIPanelId, UIPanelConfig>();

            for (int i = 0; i < panelConfigs.Count; i++)
            {
                UIPanelConfig config = panelConfigs[i];

                if (config == null)
                {
                    Debug.LogError($"{nameof(UIPanelConfigDatabase)} 的 PanelConfigs[{i}] 为空。", this);
                    continue;
                }

                if (config.PanelId == UIPanelId.None)
                {
                    Debug.LogError($"{nameof(UIPanelConfigDatabase)} 中存在 PanelId=None 的配置。", config);
                    continue;
                }

                if (configMap.ContainsKey(config.PanelId))
                {
                    Debug.LogError($"{nameof(UIPanelConfigDatabase)} 中存在重复配置：{config.PanelId}。", config);
                    continue;
                }

                configMap.Add(config.PanelId, config);
            }
        }

        private void OnValidate()
        {
            configMap = null;
            BuildConfigMapIfNeeded();
        }
    }
}

