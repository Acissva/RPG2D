using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class PlayerStatusHUD : RPG2DController
    {
        [SerializeField] private HealthBarView healthBarView;

        private IPlayerModel playerModel;

        private void Awake()
        {
            if(healthBarView == null)
            {
                healthBarView = GetComponentInChildren<HealthBarView>();
            }
        }

        private void Start()
        {
            playerModel = this.GetModel<IPlayerModel>();

            RegisterUIEvent();
        }

        //注册UI更新事件
        private void RegisterUIEvent()
        {
            playerModel.HP.RegisterWithInitValue(OnHPChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnHPChanged(int value)
        {
            if (healthBarView != null)
            {
                float percent = (float)playerModel.HP.Value / playerModel.MaxHP.Value;
                healthBarView.Refresh(percent);
            }
        }



    }
}

