using QFramework;
using System.Collections.Generic;

namespace RPG2D
{
    public interface IQuestModel : IModel
    {
        BindableProperty<string> TrackedQuestId { get; }
        /// <summary>
        /// 任务修订版本号，用于判断任务状态是否已经发生变化（一般用于 UI 刷新）
        /// </summary>
        BindableProperty<int> QuestRevision { get; }
        /// <summary>
        /// 已接取的任务
        /// </summary>
        Dictionary<string, QuestLogEntry> QuestLog { get; }
    }

    public class QuestModel : AbstractModel, IQuestModel
    {
        public BindableProperty<string> TrackedQuestId { get; } = new BindableProperty<string>(string.Empty);

        public BindableProperty<int> QuestRevision { get; } = new BindableProperty<int>(0);

        public Dictionary<string, QuestLogEntry> QuestLog { get; } = new Dictionary<string, QuestLogEntry>();

        protected override void OnInit()
        {

        }
    }
}
