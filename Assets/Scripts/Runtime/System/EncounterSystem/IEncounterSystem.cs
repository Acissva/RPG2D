using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public interface IEncounterSystem : ISystem
    {
        void BeginEncounter(
            string encounterId,
            EncounterContextType contextType,
            string contextId,
            IReadOnlyList<EnemyController> enemies);

        bool IsEncounterCleared(string encounterId);
        void EndEncounter(string encounterId);
    }

    public class EncounterSystem : AbstractSystem, IEncounterSystem
    {
        private IEncounterModel encounterModel;

        protected override void OnInit()
        {
            encounterModel = this.GetModel<IEncounterModel>();

            this.RegisterEvent<EnemyDeadEvent>(OnEnemyDead);
        }

        public void BeginEncounter(
            string encounterId,
            EncounterContextType contextType,
            string contextId,
            IReadOnlyList<EnemyController> enemies)
        {
            if (string.IsNullOrWhiteSpace(encounterId))
            {
                Debug.LogWarning("Can not begin encounter because EncounterId is empty.");
                return;
            }

            if (encounterModel.Encounters.ContainsKey(encounterId))
            {
                Debug.LogWarning($"Encounter already exists and will be replaced: {encounterId}");
                encounterModel.Encounters.Remove(encounterId);
            }

            List<int> aliveEnemyInstanceIds = new List<int>();

            if (enemies != null)
            {
                foreach (EnemyController enemy in enemies)
                {
                    if (enemy == null || enemy.Stats == null || !enemy.Stats.IsAlive)
                    {
                        continue;
                    }

                    int enemyInstanceId = enemy.gameObject.GetInstanceID();

                    if (enemyInstanceId != 0 && !aliveEnemyInstanceIds.Contains(enemyInstanceId))
                    {
                        aliveEnemyInstanceIds.Add(enemyInstanceId);
                    }
                }
            }

            EncounterRuntimeState state = new EncounterRuntimeState(
                encounterId,
                contextType,
                contextId,
                aliveEnemyInstanceIds);

            encounterModel.Encounters[encounterId] = state;
            IncreaseRevision();

            if (state.InitialEnemyCount <= 0)
            {
                Debug.LogWarning($"Encounter has no alive enemies: {encounterId}");
            }
        }

        public bool IsEncounterCleared(string encounterId)
        {
            if (string.IsNullOrWhiteSpace(encounterId))
            {
                return false;
            }

            return encounterModel.Encounters.TryGetValue(encounterId, out EncounterRuntimeState state)
                && state.IsCleared;
        }

        public void EndEncounter(string encounterId)
        {
            if (string.IsNullOrWhiteSpace(encounterId))
            {
                return;
            }

            if (encounterModel.Encounters.Remove(encounterId))
            {
                IncreaseRevision();
                Debug.Log($"Encounter end：{encounterId}");
            }
        }

        private void OnEnemyDead(EnemyDeadEvent enemyDeadEvent)
        {
            if (enemyDeadEvent.EnemyInstanceId == 0)
            {
                return;
            }

            bool changed = false;
            List<EncounterClearedEvent> clearedEvents = null;

            foreach (EncounterRuntimeState state in encounterModel.Encounters.Values)
            {
                if (state.IsCleared)
                {
                    continue;
                }

                if (!state.AliveEnemyInstanceIds.Remove(enemyDeadEvent.EnemyInstanceId))
                {
                    continue;
                }

                changed = true;

                if (state.AliveEnemyInstanceIds.Count <= 0)
                {
                    state.IsCleared = true;

                    if (clearedEvents == null)
                    {
                        clearedEvents = new List<EncounterClearedEvent>();
                    }

                    clearedEvents.Add(new EncounterClearedEvent
                    {
                        EncounterId = state.EncounterId,
                        ContextType = state.ContextType,
                        ContextId = state.ContextId,
                        InitialEnemyCount = state.InitialEnemyCount
                    });
                }
            }

            if (!changed)
            {
                return;
            }

            IncreaseRevision();

            if (clearedEvents == null)
            {
                return;
            }

            foreach (EncounterClearedEvent clearedEvent in clearedEvents)
            {
                this.SendEvent(clearedEvent);
                Debug.Log($"Encounter cleared: {clearedEvent.EncounterId}");
            }
        }

        private void IncreaseRevision()
        {
            encounterModel.EncounterRevision.Value++;
        }
    }
}
