using QFramework;
using System.Collections.Generic;

namespace RPG2D
{
    public interface ICombatStateModel : IModel
    {
        bool IsInCombat { get; set; }
        Dictionary<string, CombatSourceRuntimeState> ActiveSources { get; }
        BindableProperty<int> CombatRevision { get; }
    }

    public class CombatStateModel : AbstractModel, ICombatStateModel
    {
        public bool IsInCombat { get; set; }

        public Dictionary<string, CombatSourceRuntimeState> ActiveSources { get; } =
            new Dictionary<string, CombatSourceRuntimeState>();

        public BindableProperty<int> CombatRevision { get; } =
            new BindableProperty<int>(0);

        protected override void OnInit()
        {
        }
    }
}
