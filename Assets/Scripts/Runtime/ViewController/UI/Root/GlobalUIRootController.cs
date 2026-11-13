namespace RPG2D
{
    public class GlobalUIRootController : UIRootController
    {
        private static GlobalUIRootController instance;

        protected override void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            DontDestroyOnLoad(gameObject);

            base.Awake();

            EnsureSceneTransitionPresenter();
            EnsureDialoguePresenter();
            EnsureNpcChatPresenter();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (instance == this)
            {
                instance = null;
            }
        }

        private void EnsureSceneTransitionPresenter()
        {
            SceneTransitionPresenter presenter = GetComponent<SceneTransitionPresenter>();

            if (presenter == null)
            {
                presenter = gameObject.AddComponent<SceneTransitionPresenter>();
            }

            presenter.Initialize();
        }

        private void EnsureDialoguePresenter()
        {
            DialoguePresenter presenter = GetComponent<DialoguePresenter>();

            if (presenter == null)
            {
                presenter = gameObject.AddComponent<DialoguePresenter>();
            }

            presenter.Initialize();
        }

        private void EnsureNpcChatPresenter()
        {
            NpcChatPresenter presenter = GetComponent<NpcChatPresenter>();
            if (presenter == null) presenter = gameObject.AddComponent<NpcChatPresenter>();
            presenter.Initialize();
        }
    }
}
