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

            configMap = new Dictionary<UIPanelId, UIPanelConfig>();

            foreach (var config in panelConfigs)
            {
                if (config == null) continue;

                if (config.PanelId == UIPanelId.None)
                {
                    Debug.LogError($"{nameof(UIPanelConfigDatabase)} 中存在 PanelId=None 的配置", this);
                    continue;
                }

                if (configMap.ContainsKey(config.PanelId))
                {
                    Debug.LogError($"{nameof(UIPanelConfigDatabase)} 中存在重复配置：{config.PanelId}", this);
                    continue;
                }

                configMap.Add(config.PanelId, config);
            }
        }
    }
}

