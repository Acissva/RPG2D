using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public class UIDebugLogger : RPG2DController
    {
        private static UIDebugLogger instance;

        [SerializeField] private bool logPanelEvents = true;
        [SerializeField] private bool logInputBlocked = true;
        [SerializeField] private bool onlyLogTopInputPanelChanged = false;

        [SerializeField] private bool logSnapshotAfterPanelEvent = false;

        [Tooltip("F9")]
        [SerializeField] private bool logSnapshotOnHotkey = true;

        [SerializeField] private bool logDetailedSnapshot = false;

        [Tooltip("UI 面板状态不一致自动警告")]
        [SerializeField] private bool warnStateMismatch = true;


        private IUIModel uiModel;
        private IUISystem uiSystem;
        private IInputReader inputReader;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            DontDestroyOnLoad(gameObject);

            Init();
        }

        private void Init()
        {
            uiModel = this.GetModel<IUIModel>();
            uiSystem = this.GetSystem<IUISystem>();
            inputReader = this.GetSystem<IRPG2DInputSystem>().InputReader;

            this.RegisterEvent<UIPanelOpenedEvent>(OnPanelOpened)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<UIPanelClosedEvent>(OnPanelClosed)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            uiModel.InputBlocked.RegisterWithInitValue(OnInputBlockedChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Update()
        {
            if (!logSnapshotOnHotkey)
            {
                return;
            }

            if (!inputReader.HasAction(InputActionType.UIDebugSnapshot))
            {
                return;
            }

            inputReader.ConsumeAction(InputActionType.UIDebugSnapshot);
            LogSnapshotNow();
        }

        private void OnPanelOpened(UIPanelOpenedEvent e)
        {
            if (!logPanelEvents)
            {
                return;
            }

            if (onlyLogTopInputPanelChanged && !e.IsTopInputPanelChanged)
            {
                return;
            }

            Debug.Log(
                $"[UI] Open Panel={e.PanelId}, Group={e.PanelGroup}, Layer={e.Layer}, " +
                $"TopInput={e.PreviousTopInputPanelId}->{e.CurrentTopInputPanelId}, " +
                $"TopInputChanged={e.IsTopInputPanelChanged}, InputBlocked={uiModel.InputBlocked.Value}");

            LogSnapshot();
        }

        private void OnPanelClosed(UIPanelClosedEvent e)
        {
            if (!logPanelEvents)
            {
                return;
            }

            if (onlyLogTopInputPanelChanged && !e.IsTopInputPanelChanged)
            {
                return;
            }

            Debug.Log(
                $"[UI] Close Panel={e.PanelId}, Group={e.PanelGroup}, Layer={e.Layer}, " +
                $"TopInput={e.PreviousTopInputPanelId}->{e.CurrentTopInputPanelId}, " +
                $"TopInputChanged={e.IsTopInputPanelChanged}, InputBlocked={uiModel.InputBlocked.Value}");

            LogSnapshot();
        }

        private void OnInputBlockedChanged(bool blocked)
        {
            if (!logInputBlocked)
            {
                return;
            }

            Debug.Log($"[UI] InputBlocked={blocked}, TopInputPanel={uiModel.TopInputPanelId.Value}");
        }

        private void LogSnapshot()
        {
            if (!logSnapshotAfterPanelEvent)
            {
                return;
            }

            LogSnapshotNow();
        }

        private void LogSnapshotNow()
        {
            var snapshot = uiSystem.GetStatusSnapshot();

            Debug.Log(
                $"[UI Snapshot] Roots={snapshot.RootCount}, " +
                $"Registered={snapshot.RegisteredPanelCount}, " +
                $"Opened={snapshot.OpenedPanelCount}, " +
                $"TopInput={snapshot.TopInputPanelId}, " +
                $"InputBlocked={snapshot.InputBlocked}, " +
                $"RegisteredPanels={string.Join(", ", snapshot.RegisteredPanelIds)}, " +
                $"OpenedPanels={string.Join(", ", snapshot.OpenedPanelIds)}");

            if (!logDetailedSnapshot)
            {
                if (warnStateMismatch)
                {
                    WarnStateMismatch(snapshot);
                }

                return;
            }

            LogPanelInfos("Registered", snapshot.RegisteredPanelInfos);
            LogPanelInfos("OpenedStack", snapshot.OpenedPanelInfos);

            if (warnStateMismatch)
            {
                WarnStateMismatch(snapshot);
            }

            LogAutoRepairInfos(snapshot);
        }

        private void LogPanelInfos(string title, List<UIPanelStatusInfo> panelInfos)
        {
            if (panelInfos == null || panelInfos.Count <= 0)
            {
                Debug.Log($"[UI Snapshot Detail] {title}: Empty");
                return;
            }

            for (int i = 0; i < panelInfos.Count; i++)
            {
                var info = panelInfos[i];
                Debug.Log(
                    $"[UI Snapshot Detail] {title}[{i}] " +
                    $"Panel={info.PanelId}, " +
                    $"Group={info.PanelGroup}, " +
                    $"Layer={info.Layer}, " +
                    $"Cache={info.CacheMode}, " +
                    $"SystemOpen={info.IsOpen}, " +
                    $"GameObjectActive={info.IsGameObjectActive}, " +
                    $"TopInput={info.IsTopInputPanel}, " +
                    $"CloseOnBack={info.CloseOnBack}, " +
                    $"BlockInput={info.BlockInput}, " +
                    $"SortOrder={info.SortOrder}, " +
                    $"Object={info.GameObjectName}, " +
                    $"Parent={info.ParentName}");
            }
        }

        private void WarnStateMismatch(UIStatusSnapshot snapshot)
        {
            foreach (var info in snapshot.RegisteredPanelInfos)
            {
                if (!info.HasStateMismatch)
                {
                    continue;
                }

                Debug.LogWarning(
                    $"[UI State Mismatch] Panel={info.PanelId}, " +
                    $"SystemOpen={info.IsOpen}, " +
                    $"GameObjectActive={info.IsGameObjectActive}, " +
                    $"Object={info.GameObjectName}, " +
                    $"Parent={info.ParentName}, " +
                    $"Reason={info.StateMismatchReason}");
            }
        }

        private void LogAutoRepairInfos(UIStatusSnapshot snapshot)
        {
            if (snapshot.AutoRepairInfos == null || snapshot.AutoRepairInfos.Count <= 0)
            {
                return;
            }

            foreach (var info in snapshot.AutoRepairInfos)
            {
                Debug.LogWarning(
                    $"[UI Auto Repair] Panel={info.PanelId}, " +
                    $"Reason={info.Reason}, " +
                    $"Repair={info.RepairAction}, " +
                    $"WasSystemOpen={info.WasSystemOpen}, " +
                    $"WasGameObjectActive={info.WasGameObjectActive}, " +
                    $"Object={info.GameObjectName}, " +
                    $"Parent={info.ParentName}");
            }
        }
    }
}

