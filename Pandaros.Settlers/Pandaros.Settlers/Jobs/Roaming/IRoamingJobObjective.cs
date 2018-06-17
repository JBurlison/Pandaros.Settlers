using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Jobs.Roaming
{
    public interface IRoamingJobObjective
    {
        string Name { get; }
        float WorkTime { get; }
        ushort ItemIndex { get; }
        Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks { get; }
        string ObjectiveCategory { get; }

        void DoWork(Players.Player player, RoamingJobState state);
    }

    public interface IRoamingJobObjectiveAction
    {
        string Name { get; }
        float TimeToPreformAction { get; }
        string AudoKey { get; }
        ushort ObjectiveLoadEmptyIcon { get; }
        ushort PreformAction(Players.Player player, RoamingJobState state);
    }
}