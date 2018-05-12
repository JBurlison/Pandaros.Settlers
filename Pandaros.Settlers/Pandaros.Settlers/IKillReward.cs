using System.Collections.Generic;

namespace Pandaros.Settlers
{
    public interface IKillReward
    {
        Players.Player OriginalGoal { get; }
        Dictionary<ushort, int> KillRewards { get; }
    }
}