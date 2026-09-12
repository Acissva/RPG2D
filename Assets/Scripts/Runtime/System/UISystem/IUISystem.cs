using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPG2D
{
    public interface IUISystem : ISystem
    {
        void RegisterRoot(UIRootController root);
        void UnregisterRoot(UIRootController root);
        Transform GetRoot(UILayer layer);

        bool RegisterPanel(UIPanelController panel);
        void UnregisterPanel(UIPanelController panel);

        void OpenPanel(UIPanelId panelId, bool closeOtherPanels = true);
        void ClosePanel(UIPanelId panelId);
        void TogglePanel(UIPanelId panelId, bool closeOtherPanels = true);

        void CloseAllPanels();

        void ShowConfirmPopup(string message, Action onConfirm);

        bool IsPanelOpen(UIPanelId panelId);

        UIStatusSnapshot GetStatusSnapshot();

        void CleanupInvalidUIReferences();
    }

    public class UISystem : AbstractSystem, IUISystem
    {
        private readonly List<UIRootController> roots = new();
        private readonly Dictionary<UIPanelId, UIPanelController> panels = new();
        private readonly List<UIPanelId> openedPanelStack = new();
        private readonly List<UIPanelId> lastPublishedOpenedPanelStack = new();

        private IUIModel uiModel;
        private IAssetSystem assetSystem;
        private IConfigSystem configSystem;
        private IInputReader inputReader;
        private IRuntimeUpdateSystem runtimeUpdateSystem;

        private ConfirmPopupController confirmPopup;

        private readonly List<UIAutoRepairInfo> autoRepairInfos = new();
        private const int MaxAutoRepairInfoCount = 20;

        protected override void OnInit()
        {
            uiModel = this.GetModel<IUIModel>();
            assetSystem = this.GetSystem<IAssetSystem>();
            configSystem = this.GetSystem<IConfigSystem>();
            inputReader = this.GetSystem<IRPG2DInputSystem>().InputReader;
            runtimeUpdateSystem = this.GetSystem<IRuntimeUpdateSystem>();

            runtimeUpdateSystem.RegisterUpdate(OnUpdate);
        }

        private void OnUpdate()
        {
            if (!inputReader.HasAction(InputActionType.UIBack))
            {
                return;
            }

            HandleUIBack();
        }

        public void RegisterRoot(UIRootController root)
        {
            if (root == null || roots.Contains(root)) return;

            roots.Add(root);
        }

        public void UnregisterRoot(UIRootController root)
        {
            if (root == null) return;

            roots.Remove(root);
        }

        public Transform GetRoot(UILayer layer)
        {
            CleanupInvalidRoots();

            for (int i = roots.Count - 1; i >= 0; i--)
            {
                var root = roots[i];

                if (root == null)
                {
                    continue;
                }

                var targetRoot = root.GetRoot(layer);

                if (targetRoot != null)
                {
                    return targetRoot;
                }
            }

            return null;
        }

        public bool RegisterPanel(UIPanelController panel)
        {
            if (panel == null)
            {
                return false;
            }

            if (!EnsurePanelInitialized(panel))
            {
                return false;
            }

            if (panels.TryGetValue(panel.PanelId, out var oldPanel))
            {
                if (oldPanel != null && oldPanel != panel)
                {
                    Debug.LogError($"重复注册 UI 面板：{panel.PanelId}，已拒绝注册新面板。旧面板：{oldPanel.name}，新面板：{panel.name}");
                    return false;
                }

                if (oldPanel == null)
                {
                    panels.Remove(panel.PanelId);
                }
            }

            if (panel is ConfirmPopupController popup)
            {
                confirmPopup = popup;
            }

            panels[panel.PanelId] = panel;

            if (panel.IsOpen)
            {
                MarkPanelOpen(panel.PanelId);
            }
            else
            {
                openedPanelStack.Remove(panel.PanelId);
            }

            RefreshUIState();
            return true;
        }

        public void UnregisterPanel(UIPanelController panel)
        {
            if (panel == null || panel.PanelId == UIPanelId.None)
            {
                return;
            }

            if (panels.TryGetValue(panel.PanelId, out var currentPanel) && currentPanel == panel)
            {
                panels.Remove(panel.PanelId);
                openedPanelStack.Remove(panel.PanelId);
                RefreshUIState();
            }
        }

        private bool EnsurePanelInitialized(UIPanelController panel)
        {
            if (panel == null)
            {
                return false;
            }

            if (panel.IsInitialized)
            {
                return true;
            }

            if (panel.PanelId == UIPanelId.None)
            {
                Debug.LogError($"注册 UI 面板失败：{panel.name} 的 PanelId 不能为 None", panel);
                return false;
            }

            var panelConfigDatabase = configSystem.UIPanelConfigDatabase;

            if (panelConfigDatabase == null)
            {
                Debug.LogError($"注册 UI 面板失败：ConfigSystem 没有加载 UIPanelConfigDatabase，PanelId={panel.PanelId}", panel);
                return false;
            }

            if (!panelConfigDatabase.TryGetConfig(panel.PanelId, out var config) || config == null)
            {
                Debug.LogError($"注册 UI 面板失败：UIPanelConfigDatabase 中找不到 {panel.PanelId} 的配置", panel);
                return false;
            }

            return panel.Initialize(config);
        }

        public void OpenPanel(UIPanelId panelId, bool closeOtherPanels = true)
        {
            CleanupInvalidPanels();

            if (!TryGetOrLoadPanel(panelId, out var panel))
            {
                return;
            }

            if (closeOtherPanels)
            {
                CloseOtherPanelsInSameGroup(panelId, panel.PanelGroup);
            }

            if (openedPanelStack.Contains(panelId))
            {
                RefreshUIState();
                return;
            }

            var previousTopInputPanelId = uiModel.TopInputPanelId.Value;

            ApplySortOrder(panel);

            panel.Open();
            MarkPanelOpen(panelId);
            RefreshUIState();

            var currentTopInputPanelId = uiModel.TopInputPanelId.Value;

            this.SendEvent(new UIPanelOpenedEvent
            {
                PanelId = panelId,
                PanelGroup = panel.PanelGroup,
                Layer = panel.Layer,
                PreviousTopInputPanelId = previousTopInputPanelId,
                CurrentTopInputPanelId = uiModel.TopInputPanelId.Value,
                IsTopInputPanelChanged = previousTopInputPanelId != currentTopInputPanelId,
            });
        }

        private void CloseOtherPanelsInSameGroup(UIPanelId targetPanelId, UIPanelGroup targetGroup)
        {
            if (targetGroup == UIPanelGroup.None) return;

            var panelIds = new List<UIPanelId>(openedPanelStack);

            for (int i = panelIds.Count - 1; i >= 0; i--)
            {
                var openedPanelId = panelIds[i];

                if (openedPanelId == targetPanelId)
                {
                    continue;
                }

                if (!panels.TryGetValue(openedPanelId, out var openedPanel) || openedPanel == null)
                {
                    continue;
                }

                if (openedPanel.PanelGroup == targetGroup)
                {
                    ClosePanel(openedPanelId);
                }
            }
        }

        public void ClosePanel(UIPanelId panelId)
        {
            CleanupInvalidPanels();

            if (!panels.TryGetValue(panelId, out var panel) || panel == null)
            {
                return;
            }

            if (!openedPanelStack.Contains(panelId))
            {
                if (panel.IsOpen)
                {
                    panel.Close();
                }

                RefreshUIState();
                return;
            }

            var previousTopInputPanelId = uiModel.TopInputPanelId.Value;
            var panelGroup = panel.PanelGroup;
            var layer = panel.Layer;

            panel.Close();
            openedPanelStack.Remove(panelId);

            if (panel.CacheMode == UIPanelCacheMode.DestroyOnClose)
            {
                panels.Remove(panelId);
                Object.Destroy(panel.gameObject);
            }

            RefreshUIState();

            var currentTopInputPanelId = uiModel.TopInputPanelId.Value;

            this.SendEvent(new UIPanelClosedEvent
            {
                PanelId = panelId,
                PanelGroup = panelGroup,
                Layer = layer,
                PreviousTopInputPanelId = previousTopInputPanelId,
                CurrentTopInputPanelId = currentTopInputPanelId,
                IsTopInputPanelChanged = previousTopInputPanelId != currentTopInputPanelId,
            });
        }

        public void TogglePanel(UIPanelId panelId, bool closeOtherPanels = true)
        {
            if (IsPanelOpen(panelId))
            {
                ClosePanel(panelId);
            }
            else
            {
                OpenPanel(panelId, closeOtherPanels);
            }
        }

        public void CloseAllPanels()
        {
            CleanupInvalidPanels();

            //因为 ClosePanel() 会修改 openedPanelStack，所以 CloseAllPanels() 里要先复制一份
            var panelIds = new List<UIPanelId>(openedPanelStack);

            for (int i = panelIds.Count - 1; i >= 0; i--)
            {
                ClosePanel(panelIds[i]);
            }

            RefreshUIState();
        }

        public bool IsPanelOpen(UIPanelId panelId)
        {
            RefreshUIState();
            return openedPanelStack.Contains(panelId);
        }

        //记录面板的打开行为（将最新的面板放到列表末尾）
        private void MarkPanelOpen(UIPanelId panelId)
        {
            openedPanelStack.Remove(panelId);
            openedPanelStack.Add(panelId);
        }

        private void RefreshUIState()
        {
            CleanupInvalidPanels();

            UIPanelId topInputPanelId = FindTopInputPanelId();
            bool inputBlocked = HasInputBlockingPanel();

            bool stateChanged = topInputPanelId != uiModel.TopInputPanelId.Value
                || inputBlocked != uiModel.InputBlocked.Value
                || HasOpenedPanelStackChanged();

            if (!stateChanged)
            {
                return;
            }

            lastPublishedOpenedPanelStack.Clear();
            lastPublishedOpenedPanelStack.AddRange(openedPanelStack);

            uiModel.TopInputPanelId.Value = topInputPanelId;
            uiModel.InputBlocked.Value = inputBlocked;
            uiModel.UIRevision.Value++;
        }

        public void ShowConfirmPopup(string message, Action onConfirm)
        {
            OpenPanel(UIPanelId.ConfirmPopup, false);

            if (confirmPopup == null)
            {
                Debug.LogWarning("未注册 ConfirmPopupController");
                return;
            }

            confirmPopup.SetContent(message, onConfirm);
        }

        private bool HasInputBlockingPanel()
        {
            for (int i = openedPanelStack.Count - 1; i >= 0; i--)
            {
                var panelId = openedPanelStack[i];

                if (!panels.TryGetValue(panelId, out var panel) || panel == null)
                {
                    continue;
                }

                if (panel.BlockInput)
                {
                    return true;
                }
            }

            return false;
        }

        private void CleanupInvalidRoots()
        {
            for (int i = roots.Count - 1; i >= 0; i--)
            {
                if (roots[i] == null)
                {
                    roots.RemoveAt(i);
                }
            }
        }

        private void CleanupInvalidPanels()
        {
            var invalidPanelIds = new List<UIPanelId>();

            foreach (var pair in panels)
            {
                if (pair.Value == null)
                {
                    invalidPanelIds.Add(pair.Key);
                }
            }

            foreach (var panelId in invalidPanelIds)
            {
                RecordAutoRepair(new UIAutoRepairInfo
                {
                    PanelId = panelId,
                    Reason = "panels 中的面板引用为空或已被销毁",
                    RepairAction = "从 panels 和 openedPanelStack 中移除",
                    WasSystemOpen = openedPanelStack.Contains(panelId),
                    WasGameObjectActive = false,
                    GameObjectName = string.Empty,
                    ParentName = string.Empty
                });

                panels.Remove(panelId);
                openedPanelStack.Remove(panelId);
            }

            for (int i = openedPanelStack.Count - 1; i >= 0; i--)
            {
                var panelId = openedPanelStack[i];

                if (!panels.TryGetValue(panelId, out var panel) || panel == null)
                {
                    RecordAutoRepair(new UIAutoRepairInfo
                    {
                        PanelId = panelId,
                        Reason = "openedPanelStack 中存在未注册或已销毁的面板",
                        RepairAction = "从 openedPanelStack 中移除",
                        WasSystemOpen = true,
                        WasGameObjectActive = false,
                        GameObjectName = string.Empty,
                        ParentName = string.Empty
                    });

                    openedPanelStack.RemoveAt(i);

                    continue;
                }

                if (!panel.IsOpen)
                {
                    RecordAutoRepair(new UIAutoRepairInfo
                    {
                        PanelId = panelId,
                        Reason = "UISystem 认为面板已打开，但 GameObject 未激活",
                        RepairAction = "从 openedPanelStack 中移除",
                        WasSystemOpen = true,
                        WasGameObjectActive = false,
                        GameObjectName = panel.gameObject.name,
                        ParentName = panel.transform.parent != null ? panel.transform.parent.name : string.Empty
                    });

                    openedPanelStack.RemoveAt(i);
                }
            }
        }

        private void ApplySortOrder(UIPanelController targetPanel)
        {
            if (targetPanel == null) return;

            Transform parent = targetPanel.transform.parent;

            if (parent == null) return;

            int targetIndex = 0;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                var panel = child.GetComponent<UIPanelController>();

                if (panel == null || panel == targetPanel) continue;

                if (panel.SortOrder <= targetPanel.SortOrder)
                {
                    targetIndex = child.GetSiblingIndex() + 1;
                }
            }

            targetPanel.transform.SetSiblingIndex(targetIndex);
        }

        private UIPanelController LoadPanelFromConfig(UIPanelConfig config)
        {
            if (config == null)
            {
                Debug.LogError("加载 UI 面板失败：UIPanelConfig 为空");
                return null;
            }

            if (config.PanelId == UIPanelId.None)
            {
                Debug.LogError("加载 UI 面板失败：PanelId 不能为 None");
                return null;
            }

            if (string.IsNullOrWhiteSpace(config.PrefabPath))
            {
                Debug.LogError($"加载 UI 面板失败：{config.PanelId} 的 PrefabPath 为空");
                return null;
            }

            if (panels.TryGetValue(config.PanelId, out var existingPanel) && existingPanel != null)
            {
                return existingPanel;
            }

            GameObject prefab = assetSystem.Load<GameObject>(config.PrefabPath);

            if (prefab == null)
            {
                Debug.LogError($"加载 UI 面板失败：找不到资源 {config.PrefabPath}");
                return null;
            }

            Transform root = GetRoot(config.Layer);

            if (root == null)
            {
                Debug.LogError($"加载 UI 面板失败：找不到 Layer={config.Layer} 对应的 Root");
                return null;
            }

            GameObject panelObj = Object.Instantiate(prefab, root);
            panelObj.name = prefab.name;

            UIPanelController panel = panelObj.GetComponent<UIPanelController>();

            if (panel == null)
            {
                Debug.LogError($"加载 UI 面板失败：Prefab {config.PrefabPath} 缺少 UIPanelController");
                Object.Destroy(panelObj);
                return null;
            }

            if (!panel.Initialize(config))
            {
                Object.Destroy(panelObj);
                return null;
            }

            panel.Close();

            if (!RegisterPanel(panel))
            {
                Object.Destroy(panelObj);
                return null;
            }

            return panel;
        }

        private bool TryGetOrLoadPanel(UIPanelId panelId, out UIPanelController panel)
        {
            CleanupInvalidPanels();

            if (panels.TryGetValue(panelId, out panel) && panel != null)
            {
                return true;
            }

            panel = null;

            var panelConfigDatabase = configSystem.UIPanelConfigDatabase;

            if (panelConfigDatabase == null)
            {
                Debug.LogWarning($"未注册 UI 面板：{panelId}，且 ConfigSystem 没有加载 UIPanelConfigDatabase");
                return false;
            }

            if (!panelConfigDatabase.TryGetConfig(panelId, out var config) || config == null)
            {
                Debug.LogWarning($"未注册 UI 面板：{panelId}，且配置库中找不到对应配置");
                return false;
            }

            panel = LoadPanelFromConfig(config);

            return panel != null;
        }

        public UIStatusSnapshot GetStatusSnapshot()
        {
            CleanupInvalidRoots();
            RefreshUIState();

            var snapshot = new UIStatusSnapshot
            {
                TopInputPanelId = uiModel.TopInputPanelId.Value,
                InputBlocked = uiModel.InputBlocked.Value,
                RootCount = roots.Count,
                RegisteredPanelCount = panels.Count,
                OpenedPanelCount = openedPanelStack.Count,
                RegisteredPanelIds = new List<UIPanelId>(panels.Keys),
                OpenedPanelIds = new List<UIPanelId>(openedPanelStack),
                AutoRepairInfos = new List<UIAutoRepairInfo>(autoRepairInfos),
            };

            foreach (var pair in panels)
            {
                var panel = pair.Value;

                if (panel == null) continue;

                snapshot.RegisteredPanelInfos.Add(CreatePanelStatusInfo(panel));
            }

            foreach (var panelId in openedPanelStack)
            {
                if (!panels.TryGetValue(panelId, out var panel) || panel == null) continue;

                snapshot.OpenedPanelInfos.Add(CreatePanelStatusInfo(panel));
            }

            snapshot.RegisteredPanelIds.Sort();     // Sort() 会按枚举背后的数字从小到大排
            snapshot.RegisteredPanelInfos.Sort((a, b) => a.PanelId.CompareTo(b.PanelId));   //因为 UIPanelStatusInfo 是一个类，C# 不知道默认该按什么排序，所以需要告诉它排序方法
            // OpenedPanelIds 不应该排序，因为它代表打开栈顺序，最后一个就是顶部面板

            return snapshot;
        }

        private UIPanelStatusInfo CreatePanelStatusInfo(UIPanelController panel)
        {
            bool isOpen = openedPanelStack.Contains(panel.PanelId);
            bool isGameObjectActive = panel.IsOpen;
            bool hasStateMismatch = isOpen != isGameObjectActive;

            string mismatchReason = string.Empty;

            if (hasStateMismatch)
            {
                mismatchReason = isOpen
                    ? "UISystem 认为面板已打开，但 GameObject 未激活"
                    : "GameObject 已激活，但 UISystem 未记录为打开";
            }

            return new UIPanelStatusInfo
            {
                PanelId = panel.PanelId,
                PanelGroup = panel.PanelGroup,
                Layer = panel.Layer,
                CacheMode = panel.CacheMode,

                //这是 UI 管理系统的“业务状态”，表示：UISystem 认为这个面板是打开的
                IsOpen = openedPanelStack.Contains(panel.PanelId),
                //这是面板自身的“显示状态”，正常情况下，如果所有开关都只走 UISystem.OpenPanel / ClosePanel，这两个值应该一致
                /*特殊情况：
                    某个脚本直接 panel.Open()
                    某个脚本直接 panel.Close()
                    某个脚本直接 gameObject.SetActive(false)
                    场景初始化或销毁过程中状态没同步
                 */
                //分开写可以帮助发现“系统状态”和“实际显示状态”是否偏离
                IsGameObjectActive = panel.IsOpen,
                IsTopInputPanel = uiModel.TopInputPanelId.Value == panel.PanelId,
                CloseOnBack = panel.CloseOnBack,
                BlockInput = panel.BlockInput,

                HasStateMismatch = hasStateMismatch,
                StateMismatchReason = mismatchReason,

                SortOrder = panel.SortOrder,
                GameObjectName = panel.gameObject.name,
                ParentName = panel.transform.parent != null ? panel.transform.parent.name : string.Empty,
            };
        }

        private void RecordAutoRepair(UIAutoRepairInfo info)
        {
            if (info == null)
            {
                return;
            }

            autoRepairInfos.Add(info);

            while (autoRepairInfos.Count > MaxAutoRepairInfoCount)
            {
                autoRepairInfos.RemoveAt(0);
            }
        }

        public void CleanupInvalidUIReferences()
        {
            CleanupInvalidRoots();
            RefreshUIState();       // RefreshUIState() 内部已经调用 CleanupInvalidPanels()
        }

        private UIPanelId FindTopInputPanelId()
        {
            UIPanelController topInputPanel = null;
            int topOpenedIndex = -1;

            for (int i = 0; i < openedPanelStack.Count; i++)
            {
                UIPanelId panelId = openedPanelStack[i];

                if (!panels.TryGetValue(panelId, out UIPanelController panel) || panel == null)
                {
                    continue;
                }

                if (!panel.BlockInput && !panel.CloseOnBack)
                {
                    continue;
                }

                if (topInputPanel == null || HasHigherInputPriority(panel, i, topInputPanel, topOpenedIndex))
                {
                    topInputPanel = panel;
                    topOpenedIndex = i;
                }
            }

            return topInputPanel == null ? UIPanelId.None : topInputPanel.PanelId;
        }

        private static bool HasHigherInputPriority(UIPanelController candidate, int candidateOpenedIndex, UIPanelController current, int currentOpenedIndex)
        {
            if (candidate.Layer != current.Layer)
            {
                return (int)candidate.Layer > (int)current.Layer;
            }

            if (candidate.SortOrder != current.SortOrder)
            {
                return candidate.SortOrder > current.SortOrder;
            }

            return candidateOpenedIndex > currentOpenedIndex;
        }

        private void HandleUIBack()
        {
            UIPanelId topInputPanelId = uiModel.TopInputPanelId.Value;

            if (topInputPanelId == UIPanelId.None)
            {
                return;
            }

            inputReader.ConsumeAction(InputActionType.UIBack);

            if (!panels.TryGetValue(topInputPanelId, out UIPanelController panel) || panel == null || !panel.CloseOnBack)
            {
                return;
            }

            ClosePanel(topInputPanelId);
        }

        private bool HasOpenedPanelStackChanged()
        {
            if (openedPanelStack.Count != lastPublishedOpenedPanelStack.Count)
            {
                return true;
            }

            for (int i = 0; i < openedPanelStack.Count; i++)
            {
                if (openedPanelStack[i] != lastPublishedOpenedPanelStack[i])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
