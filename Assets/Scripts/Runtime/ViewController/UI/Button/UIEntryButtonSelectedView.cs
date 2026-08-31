using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace RPG2D
{
    [RequireComponent(typeof(Image))]
    public class UIEntryButtonSelectedView : RPG2DController
    {
        [SerializeField] private UIPanelId targetPanelId;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(0.7f, 0.9f, 1f, 1f);

        private Image image;
        private IUISystem uiSystem;

        private void Awake()
        {
            image = GetComponent<Image>();
            uiSystem = this.GetSystem<IUISystem>();

            this.RegisterEvent<UIPanelOpenedEvent>(OnPanelOpened).UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<UIPanelClosedEvent>(OnPanelClosed).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void Start()
        {
            Refresh();
        }

        private void OnPanelOpened(UIPanelOpenedEvent e)
        {
            Refresh();
        }

        private void OnPanelClosed(UIPanelClosedEvent e)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (image == null) return;

            bool selected = uiSystem.IsPanelOpen(targetPanelId);
            image.color = selected ? selectedColor : normalColor;
        }


    }
}
