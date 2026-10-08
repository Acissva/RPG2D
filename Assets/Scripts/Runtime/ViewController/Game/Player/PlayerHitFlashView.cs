using QFramework;
using UnityEngine;

namespace RPG2D
{
    [DisallowMultipleComponent]
    public sealed class PlayerHitFlashView : RPG2DController
    {
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField, Min(0.01f)] private float flashDuration = 0.15f;

        private IPlayerModel playerModel;
        private Material originalMaterial;
        private Material runtimeMaterial;
        private IUnRegister hurtEventSubscription;
        private IUnRegister lifeStateSubscription;
        private bool isFlashing;
        private float elapsedTime;

        private void Awake()
        {
            playerModel = this.GetModel<IPlayerModel>();

            if (bodyRenderer == null)
            {
                Debug.LogError("PlayerHitFlashView 未配置身体 SpriteRenderer。", this);
                return;
            }

            originalMaterial = bodyRenderer.sharedMaterial;

            if (originalMaterial == null || !originalMaterial.HasProperty(FlashAmountId))
            {
                Debug.LogError("PlayerHitFlashView 身体材质缺少 _FlashAmount 属性。", this);
                return;
            }

            runtimeMaterial = new Material(originalMaterial);
            runtimeMaterial.name = $"{originalMaterial.name} (Player Runtime)";
            runtimeMaterial.SetFloat(FlashAmountId, 0f);
            bodyRenderer.sharedMaterial = runtimeMaterial;
        }

        private void OnEnable()
        {
            if (bodyRenderer == null || runtimeMaterial == null)
            {
                Debug.LogError("PlayerHitFlashView 未初始化有效身体材质。", this);
                return;
            }

            lifeStateSubscription = playerModel.LifeState.RegisterWithInitValue(OnLifeStateChanged);
            hurtEventSubscription = this.RegisterEvent<PlayerHurtEvent>(OnPlayerHurt);
        }

        private void OnDisable()
        {
            hurtEventSubscription?.UnRegister();
            hurtEventSubscription = null;
            lifeStateSubscription?.UnRegister();
            lifeStateSubscription = null;
            CancelFlash();
        }

        private void Update()
        {
            if (!isFlashing)
            {
                return;
            }

            if (bodyRenderer == null || playerModel.LifeState.Value != PlayerLifeState.Alive)
            {
                CancelFlash();
                return;
            }

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / Mathf.Max(0.01f, flashDuration);

            if (progress >= 1f)
            {
                CancelFlash();
                return;
            }

            WriteFlashAmount(1f - progress);
        }

        private void OnPlayerHurt(PlayerHurtEvent hurtEvent)
        {
            if (bodyRenderer == null || playerModel.LifeState.Value != PlayerLifeState.Alive)
            {
                return;
            }

            elapsedTime = 0f;
            isFlashing = true;
            WriteFlashAmount(1f);
        }

        private void OnLifeStateChanged(PlayerLifeState lifeState)
        {
            CancelFlash();
        }

        private void CancelFlash()
        {
            isFlashing = false;
            elapsedTime = 0f;
            WriteFlashAmount(0f);
        }

        private void WriteFlashAmount(float amount)
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            runtimeMaterial.SetFloat(FlashAmountId, amount);
        }

        private void OnDestroy()
        {
            if (bodyRenderer != null && runtimeMaterial != null && bodyRenderer.sharedMaterial == runtimeMaterial)
            {
                bodyRenderer.sharedMaterial = originalMaterial;
            }

            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
                runtimeMaterial = null;
            }
        }
    }
}
