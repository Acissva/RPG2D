using QFramework;
using System.Collections.Generic;

namespace RPG2D
{
    public class CombatStateSystem : AbstractSystem, ICombatStateSystem
    {
        private ICombatStateModel combatStateModel;

        public bool IsInCombat => combatStateModel.IsInCombat;

        protected override void OnInit()
        {
            combatStateModel = this.GetModel<ICombatStateModel>();

            this.RegisterEvent<SceneLoadStartedEvent>(_ =>
            {
                ClearCombatState();
            });
        }

        public bool HasCombatSource(string sourceId)
        {
            string normalizedSourceId = NormalizeSourceId(sourceId);

            if (string.IsNullOrWhiteSpace(normalizedSourceId))
            {
                return false;
            }

            return combatStateModel.ActiveSources.ContainsKey(normalizedSourceId);
        }

        public CombatStateSnapshot GetSnapshot()
        {
            List<CombatSourceRuntimeState> activeSources = new();

            foreach (CombatSourceRuntimeState source in combatStateModel.ActiveSources.Values)
            {
                if (source == null)
                {
                    continue;
                }

                activeSources.Add(CloneSourceState(source));
            }

            return new CombatStateSnapshot(
                combatStateModel.IsInCombat,
                combatStateModel.CombatRevision.Value,
                activeSources);
        }

        public void ReportSourceEngaged(string sourceId, string sourceType = "", string contextId = "")
        {
            string normalizedSourceId = NormalizeSourceId(sourceId);

            if (string.IsNullOrWhiteSpace(normalizedSourceId))
            {
                return;
            }

            if (combatStateModel.ActiveSources.TryGetValue(
                    normalizedSourceId,
                    out CombatSourceRuntimeState existingSource))
            {
                if (UpdateSourceState(existingSource, sourceType, contextId))
                {
                    IncreaseRevision();
                }

                return;
            }

            bool wasInCombat = combatStateModel.IsInCombat;

            CombatSourceRuntimeState sourceState = new CombatSourceRuntimeState
            {
                SourceId = normalizedSourceId,
                SourceType = NormalizeOptionalText(sourceType),
                ContextId = NormalizeOptionalText(contextId)
            };

            combatStateModel.ActiveSources[normalizedSourceId] = sourceState;
            RefreshCombatState();
            IncreaseRevision();

            if (!wasInCombat && combatStateModel.IsInCombat)
            {
                this.SendEvent(new CombatStartedEvent
                {
                    SourceId = sourceState.SourceId,
                    SourceType = sourceState.SourceType,
                    ContextId = sourceState.ContextId,
                    ActiveSourceCount = combatStateModel.ActiveSources.Count
                });
            }
        }

        public void ReportSourceDisengaged(string sourceId)
        {
            string normalizedSourceId = NormalizeSourceId(sourceId);

            if (string.IsNullOrWhiteSpace(normalizedSourceId))
            {
                return;
            }

            if (!combatStateModel.ActiveSources.TryGetValue(
                    normalizedSourceId,
                    out CombatSourceRuntimeState sourceState))
            {
                return;
            }

            bool wasInCombat = combatStateModel.IsInCombat
                || combatStateModel.ActiveSources.Count > 0;

            combatStateModel.ActiveSources.Remove(normalizedSourceId);
            RefreshCombatState();
            IncreaseRevision();

            if (wasInCombat && !combatStateModel.IsInCombat)
            {
                this.SendEvent(new CombatEndedEvent
                {
                    SourceId = sourceState.SourceId,
                    SourceType = sourceState.SourceType,
                    ContextId = sourceState.ContextId,
                    ActiveSourceCount = combatStateModel.ActiveSources.Count
                });
            }
        }

        public void ClearCombatState()
        {
            bool wasInCombat = combatStateModel.IsInCombat
                || combatStateModel.ActiveSources.Count > 0;

            if (!wasInCombat)
            {
                return;
            }

            combatStateModel.ActiveSources.Clear();
            combatStateModel.IsInCombat = false;
            IncreaseRevision();

            this.SendEvent(new CombatEndedEvent
            {
                SourceId = string.Empty,
                SourceType = string.Empty,
                ContextId = string.Empty,
                ActiveSourceCount = 0
            });
        }

        private void RefreshCombatState()
        {
            combatStateModel.IsInCombat = combatStateModel.ActiveSources.Count > 0;
        }

        private void IncreaseRevision()
        {
            combatStateModel.CombatRevision.Value++;
        }

        private static bool UpdateSourceState(
            CombatSourceRuntimeState source,
            string sourceType,
            string contextId)
        {
            if (source == null)
            {
                return false;
            }

            bool changed = false;
            string normalizedSourceType = NormalizeOptionalText(sourceType);
            string normalizedContextId = NormalizeOptionalText(contextId);

            if (source.SourceType != normalizedSourceType)
            {
                source.SourceType = normalizedSourceType;
                changed = true;
            }

            if (source.ContextId != normalizedContextId)
            {
                source.ContextId = normalizedContextId;
                changed = true;
            }

            return changed;
        }

        private static CombatSourceRuntimeState CloneSourceState(CombatSourceRuntimeState source)
        {
            return new CombatSourceRuntimeState
            {
                SourceId = source.SourceId,
                SourceType = source.SourceType,
                ContextId = source.ContextId
            };
        }

        private static string NormalizeSourceId(string sourceId)
        {
            return sourceId == null ? string.Empty : sourceId.Trim();
        }

        private static string NormalizeOptionalText(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }
}
