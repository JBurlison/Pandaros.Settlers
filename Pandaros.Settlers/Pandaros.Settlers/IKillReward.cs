using System;
using System.Collections.Generic;

namespace Pandaros.Settlers
{
    public interface IKillReward
    {
        Colony OriginalGoal { get; }
        string LootTableName { get; }
    }
}