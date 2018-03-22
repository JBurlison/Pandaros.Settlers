using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    public interface IKillReward
    {
        Players.Player OriginalGoal { get; }
        Dictionary<ushort, int> KillRewards { get; }
    }
}
