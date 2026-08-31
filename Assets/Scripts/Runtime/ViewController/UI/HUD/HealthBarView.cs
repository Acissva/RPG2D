using UnityEngine;

namespace RPG2D
{
    public class HealthBarView : MonoBehaviour
    {
        [SerializeField] private RectTransform fillMask;
        [SerializeField] private RectTransform delayFillMask;

        [SerializeField, Min(0.01f)] private float delayDuration = 2f;

        private float delayStartValue;
        private float delayTargetValue;
        private float delayElapsed;
        private bool isDelayChanging;

        [SerializeField] private bool autoHide = true;
        /// <summary>
        /// 多长时间后自动隐藏
        /// </summary>
        [Tooltip("多长时间后自动隐藏")]
        [SerializeField, Min(0)] private float autoHideDelayTime = 1f;
        private float autoHideTimer = 1f;

        private void Awake()
        {
            if (fillMask == null)
            {
                var fillMaskTrans = transform.Find("FillMask");
                if (fillMaskTrans != null)
                {
                    fillMask = fillMaskTrans.GetComponent<RectTransform>();
                }
            }

            if (delayFillMask == null)
            {
                var delayFillMaskTrans = transform.Find("DelayFillMask");
                if (delayFillMaskTrans != null)
                {
                    delayFillMask = delayFillMaskTrans.GetComponent<RectTransform>();
                }
            }

            if (fillMask == null)
            {
                Debug.LogError($"{nameof(HealthBarView)} 缺少 fillMask 引用", this);
                enabled = false;
                return;
            }

            if (delayFillMask == null)
            {
                Debug.LogError($"{nameof(HealthBarView)} 缺少 delayFillMask 引用", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (isDelayChanging)
            {
                delayElapsed += Time.deltaTime;

                float t = Mathf.Clamp01(delayElapsed / delayDuration);
                float value = Mathf.Lerp(delayStartValue, delayTargetValue, t);

                delayFillMask.anchorMax = new Vector2(value, delayFillMask.anchorMax.y);

                if (t >= 1f)
                {
                    delayFillMask.anchorMax = new Vector2(delayTargetValue, delayFillMask.anchorMax.y);
                    isDelayChanging = false;
                }
            }

            AutoHide();
        }

        public void Refresh(float percent)
        {
            percent = Mathf.Clamp01(percent);

            fillMask.anchorMax = new Vector2(percent, fillMask.anchorMax.y);

            delayStartValue = delayFillMask.anchorMax.x;
            delayTargetValue = percent;
            delayElapsed = 0f;

            isDelayChanging = !Mathf.Approximately(delayStartValue, delayTargetValue);

            if (!isDelayChanging)
            {
                delayFillMask.anchorMax = new Vector2(delayTargetValue, delayFillMask.anchorMax.y);
            }

            Show();
        }

        private void AutoHide()
        {
            if (isDelayChanging) return;

            if (autoHide == false)
            {
                if (!gameObject.activeSelf)
                {
                    Show();
                }
                return;
            }

            if (autoHideTimer < 0f)
            {
                Hide();
                return;
            }

            autoHideTimer -= Time.deltaTime;
        }

        public void SetAutoHide(bool autoHide, float autoHideDelayTime)
        {
            this.autoHide = autoHide;

            if(autoHideDelayTime >= 0f)
            {
                this.autoHideDelayTime = autoHideDelayTime;
            }
            else
            {
                this.autoHideDelayTime = 1f;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);

            autoHideTimer = autoHideDelayTime >= 0 ? autoHideDelayTime : 0f;
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            autoHideTimer = autoHideDelayTime >= 0 ? autoHideDelayTime : 0f;
        }

    }
}

