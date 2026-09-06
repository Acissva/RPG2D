using System.Collections.Generic;

namespace RPG2D
{
    public class EncounterRuntimeState
    {
        public string EncounterId { get; }
        /// <summary>
        /// 这场 Encounter 属于哪种具体业务上下文
        /// </summary>
        public EncounterContextType ContextType { get; }
        /// <summary>
        /// 这场 Encounter 属于哪个具体业务上下文
        /// </summary>
        public string ContextId { get; }
        public int InitialEnemyCount { get; }
        public HashSet<int> AliveEnemyInstanceIds { get; }
        public bool IsCleared { get; set; }

        public EncounterRuntimeState(
            string encounterId,
            EncounterContextType contextType,
            string contextId,
            IEnumerable<int> aliveEnemyInstanceIds)
        {
            EncounterId = encounterId;
            ContextType = contextType;
            ContextId = contextId;
            AliveEnemyInstanceIds = new HashSet<int>(aliveEnemyInstanceIds);
            InitialEnemyCount = AliveEnemyInstanceIds.Count;
            IsCleared = InitialEnemyCount <= 0;
        }
    }
}
