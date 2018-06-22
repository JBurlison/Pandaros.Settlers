using Pandaros.Settlers.Extender;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Jobs.Roaming
{
    public interface IRoamingJobObjective : INameable
    {
        float WorkTime { get; }
        ushort ItemIndex { get; }
        Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks { get; }
        string ObjectiveCategory { get; }

        void DoWork(Players.Player player, RoamingJobState state);
    }

    public interface IRoamingJobObjectiveAction : INameable
    {
        float TimeToPreformAction { get; }
        string AudoKey { get; }
        ushort ObjectiveLoadEmptyIcon { get; }
        ushort PreformAction(Players.Player player, RoamingJobState state);
    }
}