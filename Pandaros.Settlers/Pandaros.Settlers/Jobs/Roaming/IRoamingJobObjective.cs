using Pandaros.Settlers.Extender;
using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Jobs.Roaming
{
    public interface IRoamingJobObjective : INameable
    {
        float WorkTime { get; }
        ItemId ItemIndex { get; }
        Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks { get; }
        string ObjectiveCategory { get; }

        void DoWork(Colony colony, RoamingJobState state);
    }

    public interface IRoamingJobObjectiveAction : INameable
    {
        float TimeToPreformAction { get; }
        string AudioKey { get; }
        ItemId ObjectiveLoadEmptyIcon { get; }
        ItemId PreformAction(Colony colony, RoamingJobState state);
    }
}