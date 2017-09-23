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
            double chance = .5;
            var remainingBeds =  BedBlockTracker.GetCount(p) - state.ColonistCount;

            if (remainingBeds < 1)
                chance -= 0.1;

            if (remainingBeds >= state.MaxPerSpawn)
                chance += 0.1;

            var hoursofFood = Stockpile.GetStockPile(p).TotalFood / c.FoodUsePerHour;

            if (hoursofFood < 48)
                chance -= 0.4;
            else
                chance += 0.2;

            if (JobTracker.GetCount(p) > state.MaxPerSpawn)
                chance += .4;
            else
                chance -= .2;

            if (state.Difficulty != GameDifficulty.Easy)
                if (c.InSiegeMode || 
                c.LastSiegeModeSpawn != 0 &&
                Pipliz.Time.SecondsSinceStartDouble - c.LastSiegeModeSpawn > TimeSpan.FromMinutes(5).TotalSeconds)
                chance -= 0.4;

            return chance;
        }
    }
}
