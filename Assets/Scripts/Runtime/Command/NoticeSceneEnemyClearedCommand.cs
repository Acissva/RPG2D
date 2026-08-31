using QFramework;

namespace RPG2D
{
    public class NoticeSceneEnemyClearedCommand : AbstractCommand
    {
        private string sceneId;
        private int initialEnemyCount;

        public NoticeSceneEnemyClearedCommand(string sceneId, int initialEnemyCount)
        {
            this.sceneId = sceneId;
            this.initialEnemyCount = initialEnemyCount;
        }

        protected override void OnExecute()
        {
            this.SendEvent(new SceneEnemyClearedEvent
            {
                SceneId = sceneId,
                ClearedEnemyCount = initialEnemyCount
            });
        }
    }
}