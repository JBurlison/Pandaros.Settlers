using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pandaros.Settlers.Entities;
using NPC;

namespace Pandaros.Settlers.Chance
{
    public class SettelerEvaluation : ISpawnSettlerEvaluator
    {
        public string Name => "SettelerEvaluation";

        public double SpawnChance(Players.Player p, Colony c, PlayerState state)
        {
            double chance = .4;
            var remainingBeds = state.ColonistCount - BedBlockTracker.GetCount(p);

            if (remainingBeds < 1)
                chance -= 0.1;

            if (remainingBeds > state.MaxPerSpawn)
                chance += 0.1;

            var hoursofFood = Stockpile.GetStockPile(p).TotalFood / c.FoodUsePerHour;

            if (hoursofFood < 24)
                chance -= 0.4;
            else
                chance += 0.2;

            if (JobTracker.GetCount(p) > SettlerManager.MIN_PERSPAWN)
                chance += .4;

            return chance;
        }
    }
}
