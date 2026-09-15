using UnityEngine;
using UnityEngine.Rendering;

namespace RPG2D
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class YSortRenderer2D : MonoBehaviour
    {
        private const int MinSortingOrder = -30000;
        private const int MaxSortingOrder = 30000;

        private static readonly Renderer[] EmptyRenderers = new Renderer[0];
        private static readonly int[] EmptyOrders = new int[0];

        [Header("Sort Point")]
        [SerializeField] private Transform sortingPoint;
        [SerializeField, Min(1)] private int orderPerUnit = 10;
        [SerializeField] private int orderOffset = 0;

        [Header("Update")]
        [SerializeField] private bool updateEveryFrame = true;
        [SerializeField] private bool applyInEditMode = true;

        [Header("Renderer Cache")]
        [SerializeField] private bool preferSortingGroup = true;
        [SerializeField] private bool includeChildRenderers = true;
        [SerializeField] private bool includeInactiveChildren = false;

        private SortingGroup sortingGroup;
        private Renderer[] renderers = EmptyRenderers;
        private int[] relativeRendererOrders = EmptyOrders;
        private int lastAppliedOrder = int.MinValue;

        public int CurrentSortOrder => lastAppliedOrder;

        private Transform EffectiveSortingPoint => sortingPoint != null ? sortingPoint : transform;

        private void Reset()
        {
            sortingPoint = transform;
            RefreshCache();
            ApplySorting(true);
        }

        private void Awake()
        {
            if (sortingPoint == null)
            {
                sortingPoint = transform;
            }

            RefreshCache();
            ApplySorting(true);
        }

        private void OnEnable()
        {
            if (sortingPoint == null)
            {
                sortingPoint = transform;
            }

            RefreshCache();
            ApplySorting(true);
        }

        private void OnValidate()
        {
            orderPerUnit = Mathf.Max(1, orderPerUnit);

            if (sortingPoint == null)
            {
                sortingPoint = transform;
            }

            RefreshCache();
            ApplySorting(true);
        }

        private void OnTransformChildrenChanged()
        {
            RefreshCache();
            ApplySorting(true);
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                if (applyInEditMode)
                {
                    ApplySorting(false);
                }

                return;
            }

            if (updateEveryFrame)
            {
                ApplySorting(false);
            }

        }

        public void ForceRefresh()
        {
            RefreshCache();
            lastAppliedOrder = int.MinValue;
            ApplySorting(true);
        }

        private void RefreshCache()
        {
            sortingGroup = preferSortingGroup ? GetComponent<SortingGroup>() : null;

            if (sortingGroup != null)
            {
                renderers = EmptyRenderers;
                relativeRendererOrders = EmptyOrders;
                return;
            }

            renderers = includeChildRenderers ? GetComponentsInChildren<Renderer>(includeInactiveChildren)
                : GetComponents<Renderer>();

            if (renderers == null || renderers.Length == 0)
            {
                renderers = EmptyRenderers;
                relativeRendererOrders = EmptyOrders;
                return;
            }

            int referenceOrder = renderers[0] != null ? renderers[0].sortingOrder : 0;
            relativeRendererOrders = new int[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                relativeRendererOrders[i] = renderers[i] != null
                    ? renderers[i].sortingOrder - referenceOrder
                    : 0;
            }
        }

        private void ApplySorting(bool force)
        {
            int order = CalculateSortingOrder();

            if (!force && order == lastAppliedOrder)
            {
                return;
            }

            if (sortingGroup != null)
            {
                sortingGroup.sortingOrder = order;
            }
            else
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] == null)
                    {
                        continue;
                    }

                    renderers[i].sortingOrder = ClampSortingOrder(order + relativeRendererOrders[i]);
                }
            }

            lastAppliedOrder = order;
        }

        private int CalculateSortingOrder()
        {
            float rawOrder = -EffectiveSortingPoint.position.y * orderPerUnit + orderOffset;
            return ClampSortingOrder(Mathf.RoundToInt(rawOrder));
        }

        private static int ClampSortingOrder(int value)
        {
            return Mathf.Clamp(value, MinSortingOrder, MaxSortingOrder);
        }

    }
}

