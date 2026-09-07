using System.Collections.Generic;

namespace RPG2D
{
    public class CombatStateSnapshot
    {
        public static CombatStateSnapshot Empty { get; } =
            new CombatStateSnapshot(false, 0, new List<CombatSourceRuntimeState>());

        public bool IsInCombat { get; }
        public int Revision { get; }
        public IReadOnlyList<CombatSourceRuntimeState> ActiveSources { get; }

        public CombatStateSnapshot(
            bool isInCombat,
            int revision,
            IReadOnlyList<CombatSourceRuntimeState> activeSources)
        {
            IsInCombat = isInCombat;
            Revision = revision;
            ActiveSources = activeSources ?? new List<CombatSourceRuntimeState>();
        }
    }
}
