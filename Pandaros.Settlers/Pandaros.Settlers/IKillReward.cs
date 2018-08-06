using System;
using System.Collections.Generic;

namespace Pandaros.Settlers
{
    public interface IKillReward
    {
        Players.Player OriginalGoal { get; }
        string LootTableName { get; }
    }
}