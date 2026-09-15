using QFramework;

namespace RPG2D
{
    public interface ICombatStateSystem : ISystem
    {
        bool IsInCombat { get; }
        bool HasCombatSource(string sourceId);
        CombatStateSnapshot GetSnapshot();
        void ReportSourceEngaged(string sourceId, string sourceType = "", string contextId = "");
        void ReportSourceDisengaged(string sourceId);
        void ClearCombatState();
    }
}
