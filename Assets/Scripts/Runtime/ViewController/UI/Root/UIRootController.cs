using QFramework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPG2D
{
    /// <summary>
    /// 用于自动获取场景中的面板，以及控制面板的初始状态（挂载到UI根节点）
    /// </summary>
    public class UIRootController : RPG2DController
    {
        private readonly List<UIPanelController> registeredPanels = new();

        [SerializeField] private List<UIPanelId> defaultOpenPanels = new();

        [SerializeField] private UIPanelConfigDatabase panelConfigDatabase;

        private IUISystem uiSystem;

        [SerializeField] private bool autoCreateMissingRoots = true;
        [SerializeField] private bool warnMissingRoots = true;

        [SerializeField] private Transform hudRoot;
        [SerializeField] private Transform toolbarRoot;
        [SerializeField] private Transform mainPanelRoot;
        [SerializeField] private Transform dialogueRoot;
        [SerializeField] private Transform popupRoot;
        [SerializeField] private Transform toastRoot;
        [SerializeField] private Transform loadingRoot;

        private const string HUDName = "HUD";
        private const string ToolbarName = "Toolbar";
        private const string MainPanelName = "MainPanel";
        private const string DialogueName = "Dialogue";
        private const string PopupName = "Popup";
        private const string ToastName = "Toast";
        private const string LoadingName = "Loading";

        protected virtual void Awake()
        {
            EnsureRoots();

            uiSystem = this.GetSystem<IUISystem>();
            uiSystem.RegisterRoot(this);

            if (panelConfigDatabase != null)
            {
                uiSystem.SetPanelConfigDatabase(panelConfigDatabase);
            }

            var panels = GetComponentsInChildren<UIPanelController>(true);

            foreach (var panel in panels)
            {
                if (!panel.ValidateConfig())
                {
                    continue;
                }

                uiSystem.RegisterPanel(panel);
                registeredPanels.Add(panel);
            }

            CloseRegisteredPanels();

            foreach (var panelId in defaultOpenPanels)
            {
                uiSystem.OpenPanel(panelId, false);
            }
        }

        protected virtual void OnDestroy()
        {
            if (uiSystem == null)
            {
                return;
            }

            foreach (var panel in registeredPanels)
            {
                uiSystem.UnregisterPanel(panel);
            }

            registeredPanels.Clear();
            uiSystem.UnregisterRoot(this);
        }

        private void CloseRegisteredPanels()
        {
            foreach (var panel in registeredPanels)
            {
                if (panel == null)
                {
                    continue;
                }

                uiSystem.ClosePanel(panel.PanelId);
            }
        }

        public Transform GetRoot(UILayer layer)
        {
            switch (layer)
            {
                case UILayer.HUD:
                    return hudRoot;

                case UILayer.Toolbar:
                    return toolbarRoot;

                case UILayer.MainPanel:
                    return mainPanelRoot;

                case UILayer.Dialogue:
                    return dialogueRoot;

                case UILayer.Popup:
                    return popupRoot;

                case UILayer.Toast:
                    return toastRoot;

                case UILayer.Loading:
                    return loadingRoot;

                default:
                    return transform;
            }
        }

        private Transform EnsureRoot(Transform currentRoot, string name, UILayer layer)
        {
            if (currentRoot != null)
            {
                SyncCanvasSorting(currentRoot, layer);
                return currentRoot;
            }

            Transform existingRoot = FindRoot(name);

            if (existingRoot != null)
            {
                SyncCanvasSorting(existingRoot, layer);
                return existingRoot;
            }

            if (!autoCreateMissingRoots)
            {
                if (warnMissingRoots)
                {
                    Debug.LogWarning($"{nameof(UIRootController)} 缺少 Root：{name}", this);
                }
                return null;
            }

            Transform canvasTrans;
            Transform existingCanvas = transform.Find(name + "Canvas");
            if (existingCanvas != null)
            {
                canvasTrans = existingCanvas;
            }
            else
            {
                canvasTrans = CreateCanvas(transform, name + "Canvas", (int)layer);
            }

            Transform rootObjTrans = CreateRoot(canvasTrans, name + "Root");
            SyncCanvasSorting(rootObjTrans, layer);

            return rootObjTrans;
        }

        private void EnsureRoots()
        {
            hudRoot = EnsureRoot(hudRoot, HUDName, UILayer.HUD);
            toolbarRoot = EnsureRoot(toolbarRoot, ToolbarName, UILayer.Toolbar);
            mainPanelRoot = EnsureRoot(mainPanelRoot, MainPanelName, UILayer.MainPanel);
            dialogueRoot = EnsureRoot(dialogueRoot, DialogueName, UILayer.Dialogue);
            popupRoot = EnsureRoot(popupRoot, PopupName, UILayer.Popup);
            toastRoot = EnsureRoot(toastRoot, ToastName, UILayer.Toast);
            loadingRoot = EnsureRoot(loadingRoot, LoadingName, UILayer.Loading);
        }

        private Transform CreateCanvas(Transform parent, string name, int sortingOrder)
        {
            var canvasObj = new GameObject(name, typeof(RectTransform));
            canvasObj.transform.SetParent(parent, false);

            var rectTransform = canvasObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;

            var canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            canvasScaler.scaleFactor = 1f;
            canvasScaler.referencePixelsPerUnit = 100f;

            canvasObj.AddComponent<GraphicRaycaster>();

            return canvasObj.transform;
        }

        private Transform CreateRoot(Transform parent, string name)
        {
            var rootObj = new GameObject(name, typeof(RectTransform));
            rootObj.transform.SetParent(parent, false);

            var rectTransform = rootObj.GetComponent<RectTransform>();

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            return rootObj.transform;
        }

        private Transform FindRoot(string name)
        {
            Transform existingRoot = transform.Find(name + "Root");
            if (existingRoot != null) return existingRoot;

            return transform.Find(name + "Canvas/" + name + "Root");
        }

        private void SyncCanvasSorting(Transform root, UILayer layer)
        {
            if (root == null || layer == UILayer.None)
            {
                return;
            }

            Canvas canvas = root.GetComponentInParent<Canvas>();

            if (canvas == null)
            {
                return;
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = (int)layer;
        }
    }
}

