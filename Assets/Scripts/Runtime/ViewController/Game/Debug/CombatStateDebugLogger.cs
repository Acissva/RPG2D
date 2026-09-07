using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class CombatStateDebugLogger : RPG2DController
    {
        private static CombatStateDebugLogger instance;

        [SerializeField] private bool logCombatStateChanged = true;

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

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Init()
        {
            this.RegisterEvent<CombatStartedEvent>(OnCombatStarted)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<CombatEndedEvent>(OnCombatEnded)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnCombatStarted(CombatStartedEvent e)
        {
            if (!logCombatStateChanged)
            {
                return;
            }

            Debug.Log(
                $"进入战斗：SourceId={e.SourceId}, SourceType={e.SourceType}, ContextId={e.ContextId}, ActiveSourceCount={e.ActiveSourceCount}");
        }

        private void OnCombatEnded(CombatEndedEvent e)
        {
            if (!logCombatStateChanged)
            {
                return;
            }

            Debug.Log(
                $"退出战斗：SourceId={e.SourceId}, SourceType={e.SourceType}, ContextId={e.ContextId}, ActiveSourceCount={e.ActiveSourceCount}");
        }
    }
}
