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

        void DoWork(Colony colony, RoamingJobState state);
    }

    public interface IRoamingJobObjectiveAction : INameable
    {
        float TimeToPreformAction { get; }
        string AudoKey { get; }
        ushort ObjectiveLoadEmptyIcon { get; }
        ushort PreformAction(Colony colony, RoamingJobState state);
    }
}