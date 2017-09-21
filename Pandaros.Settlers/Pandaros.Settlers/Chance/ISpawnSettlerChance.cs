using Pandaros.Settlers.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Chance
{
    public interface ISpawnSettlerEvaluator
    {
        string Name { get; }

        double SpawnChance(Players.Player p, Colony c, PlayerState state);
    }
}
