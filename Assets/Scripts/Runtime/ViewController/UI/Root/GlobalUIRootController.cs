namespace RPG2D
{
    public class GlobalUIRootController : UIRootController
    {
        private static GlobalUIRootController instance;

        protected override void Awake()
        {
            if(instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            DontDestroyOnLoad(gameObject);

            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(instance == this)
            {
                instance = null;
            }
        }

    }
}

